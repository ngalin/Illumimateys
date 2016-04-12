/*  OctoWS2811 movie2serial.pde - Transmit video data to 1 or more
      Teensy 3.0 boards running OctoWS2811 VideoDisplay.ino
    http://www.pjrc.com/teensy/td_libs_OctoWS2811.html
    Copyright (c) 2013 Paul Stoffregen, PJRC.COM, LLC

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
*/

// Imports
//=====================================================================================

import blobDetection.*;
import gab.opencv.*;
import processing.video.*;
import processing.video.Movie;
import processing.serial.*;
import java.awt.Rectangle;

// Constants
//=====================================================================================
// Macbook Pro ix 1280x1024. Set these to match camera's native resolution.
int CameraWidth = 1280;
int CameraHeight = 720;
int TargetFrameRate = 30;

// The panel is 180w x 120h, i.e. 3:2. Using double resolution for processing/display.
int ResolutionMultiple = 1;
int WidthInPixels = 180 * ResolutionMultiple;
int HeightInPixels = 120 * ResolutionMultiple;
float PanelAspect = WidthInPixels / (float)HeightInPixels;

// Must be an absolute path. If this file can't be found, will open a capture instead.
//String MovieFileName = "/tmp/shadowwall.avi";
String MovieFileName = "/Users/srdjankrstic/Programming/PhilipsCircle_10secs.avi";
//String MovieFileName = "/non-existent_file.avi";

// Variables
//=====================================================================================

// Only one of these two sources will be non-null, depending on whether the movie file is found.
Movie movieStream;
Capture processingStream;

// Processing
OpenCV opencv;
BlobDetection blobDetector;
PImage lastRenderFrame;

// LED panels
int MaximumNumberOfPorts = 24;
Serial[] ledSerial = new Serial[MaximumNumberOfPorts];     // each port's actual Serial port
Rectangle[] ledArea = new Rectangle[MaximumNumberOfPorts]; // the area of the movie each port gets, in % (0-100)
boolean[] ledLayout = new boolean[MaximumNumberOfPorts];   // layout of rows, true = even is left->right
PImage[] ledImage = new PImage[MaximumNumberOfPorts];      // image sent to each port

float gamma = 1.7;
int[] gammaTable = new int[256];

int numberOfPortsInUse = 0;
int errorCount = 0;

// Setup methods
//=====================================================================================

void settings() {
  // Create window to display the video stream.
  // Double height for a debug display below the main frame.
  size(WidthInPixels, HeightInPixels * 2);
}

void setup() {
  frameRate(TargetFrameRate);
  initialiseSerialPorts();
  initialiseGammaTable();
  initialiseVideoStream();
  initialiseProcessingPipeline();
}
 
// Called to render the screen - on this computer, not the LED panel.
void draw() {
  // Show the frame on screen
  int xPosition = 0, yPosition = 0; 
  if (lastRenderFrame != null) {
    image(lastRenderFrame, xPosition, yPosition);
  } else {
    //print ("Movie not ready yet");
    return;
  }
  
  // Copy the frame from the display window to send to panels.
  PImage frame = get(0, 0, WidthInPixels, HeightInPixels);
  sendFrameToLedPanels(frame);

  drawPanelData();

  if (frameCount % TargetFrameRate == 0) {
    println("Frame rate:", frameRate);
  }
}

void drawPanelData() {
  // then display what was most recently sent to the LEDs
  // by displaying all the images for each port.
  for (int i = 0; i < numberOfPortsInUse; i++) {
    // compute the intended size of the entire LED array
    int xsize = percentageInverse(ledImage[i].width, ledArea[i].width);
    int ysize = percentageInverse(ledImage[i].height, ledArea[i].height);
    
    // computer this image's position within it
    int xloc =  percentage(xsize, ledArea[i].x);
    int yloc =  percentage(ysize, ledArea[i].y);
    
    // show what should appear on the LEDs (this layout is probably wrong)
    image(ledImage[i], WidthInPixels - xsize / 2 + xloc, HeightInPixels + yloc);
  }
}

void initialiseSerialPorts() {
  String[] list = Serial.list();
  delay(20);
  println("Serial Ports:");
  println(join(list, "\n"));
  //serialConfigure("/dev/ttyACM0");  // change these to your port names
  //serialConfigure("/dev/ttyACM1");
  //serialConfigure("/dev/tty.usbmodem1350351");
  serialConfigure("/dev/tty.usbmodem1562921");

  if (errorCount > 0) exit();
}

void initialiseGammaTable() {
  for (int i = 0; i < 256; i++) {
    gammaTable[i] = (int)(pow((float)i / 255.0, gamma) * 255.0 + 0.5);
  }
}

void initialiseVideoStream() {  
  // Open file or camera
  File movieFile = new File(MovieFileName);
  if (movieFile.exists()) {
    movieStream = new Movie(this, MovieFileName);
    movieStream.loop();
  } else {
    print("Can't find " + MovieFileName);
    // Note that the parameters to capture must be compatible with the camera; not all parameters are
    // accepted and a mismatch distorts the video.
    processingStream = new Capture(this, CameraWidth, CameraHeight, TargetFrameRate);
    processingStream.start();
  }
}

void initialiseProcessingPipeline() {
  opencv = new OpenCV(this, WidthInPixels, HeightInPixels);
  blobDetector = new BlobDetection(WidthInPixels, HeightInPixels);
  //BlobDetection.setConstants(5, 20, 60);
  blobDetector.setThreshold(0.5);
  blobDetector.setPosDiscrimination(true); // find highlights, not lowlights
}

// Processing methods
//=====================================================================================
 
// Called every time a new frame is available to read.
void movieEvent(Movie movieFrame) {
  int m = millis();
  movieFrame.read();
  lastRenderFrame = processFrame(movieFrame, false);
  if (frameCount % TargetFrameRate == 0) {
    println("movieEvent took", (millis() - m), "ms");
  }
}

void captureEvent(Capture capture) {
  int m = millis();
  capture.read();
  lastRenderFrame = processFrame(capture, true);
  if (frameCount % TargetFrameRate == 0) {
    println("captureEvent took", (millis() - m), "ms");
  }
}

// respond to mouse clicks as pause/play
//boolean isPlaying = true;
//void mousePressed() {
//  if (movieStream == null) return;
//  if (isPlaying) {
//    movieStream.pause();
//    isPlaying = false;
//  } else {
//    movieStream.play();
//    isPlaying = true;
//  }
//}