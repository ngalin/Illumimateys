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
int WidthInPixels = 360;
int HeightInPixels = 240;
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
PImage lastRenderFrame;
ArrayList<Contour> contours;

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
  // Create debug window to display the video stream.
  size(WidthInPixels, HeightInPixels);
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
  
  // then try to show what was most recently sent to the LEDs
  // by displaying all the images for each port.
  for (int i = 0; i < numberOfPortsInUse; i++) {
    // compute the intended size of the entire LED array
    int xsize = percentageInverse(ledImage[i].width, ledArea[i].width);
    int ysize = percentageInverse(ledImage[i].height, ledArea[i].height);
    
    // computer this image's position within it
    int xloc =  percentage(xsize, ledArea[i].x);
    int yloc =  percentage(ysize, ledArea[i].y);
    
    // show what should appear on the LEDs
    image(ledImage[i], 240 - xsize / 2 + xloc, 10 + yloc);
  }
  
  if (frameCount % TargetFrameRate == 0) {
    println("Frame rate:", frameRate);
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
    // It's tempting to set the parameters to capture here, but not all sizes are supported and
    // it distorts the video
    processingStream = new Capture(this, CameraWidth, CameraHeight, TargetFrameRate);
    processingStream.start();
  }
}

void initialiseProcessingPipeline() {
  //if (processingStream == null) return;
  opencv = new OpenCV(this, WidthInPixels, HeightInPixels);
}

// Processing methods
//=====================================================================================
 
// Called every time a new frame is available to read.
void movieEvent(Movie movieFrame) {
  int m = millis();
  movieFrame.read();
  lastRenderFrame = processFrame(movieFrame);
  sendFrameToLedPanels(lastRenderFrame);
  if (frameCount % TargetFrameRate == 0) {
    println("movieEvent took", (millis() - m), "ms");
  }
}

void captureEvent(Capture capture) {
  int m = millis();
  capture.read();
  lastRenderFrame = processFrame(capture);
  sendFrameToLedPanels(lastRenderFrame);
  if (frameCount % TargetFrameRate == 0) {
    println("captureEvent took", (millis() - m), "ms");
  }
}

PImage processFrame(PImage frame) {
  frame = centerCrop(frame, WidthInPixels, HeightInPixels);
  opencv.loadImage(frame);
  //opencv.gray();
  opencv.useGray();
  opencv.threshold((int)(255 * 0.7));

  //int thresholdBlockSize = 16; int thresholdConstant = 1;
  //opencv.adaptiveThreshold(thresholdBlockSize+1, thresholdConstant);
 
  //opencv.findCannyEdges(20,75);
  
  PImage newFrame = opencv.getSnapshot();
  return newFrame;
}

// Center-crops the largest area possible from frame and resizes
// to width:height.
PImage centerCrop(PImage frame, int width, int height) {
  assert frame.width >= width && frame.height >= height;
  float frameAspect = frame.width / (float)frame.height;
  float cropAspect = width / (float)height;
  //println("frame", frameAspect, "panel", PanelAspect);
  int cropWidth, cropHeight;
  if (frameAspect >= cropAspect) { // frame is wide
   cropWidth = (int)(frame.height * cropAspect);
   cropHeight = frame.height;
  } else { // frame is tall
   cropWidth = frame.width;
   cropHeight = (int)(frame.width / cropAspect);
  }
  
  int cropX = (frame.width - cropWidth) / 2;
  int cropY = (frame.height - cropHeight) / 2;
  //println("frame", frame.width, frame.height, "cropw", cropWidth, "croph", cropHeight, "at", cropX, cropY);
  frame = frame.get(cropX, cropY, cropWidth, cropHeight);
  frame.resize(width, height);
  return frame;
}

// Processing methods
//=====================================================================================

void sendFrameToLedPanels(PImage frame) {
  // Write the frame to panels  
  for (int i=0; i < numberOfPortsInUse; i++) {    
    // copy a portion of the movie's image to the LED image
    int xoffset = percentage(frame.width, ledArea[i].x);
    int yoffset = percentage(frame.height, ledArea[i].y);
    int xwidth =  percentage(frame.width, ledArea[i].width);
    int yheight = percentage(frame.height, ledArea[i].height);
    ledImage[i].copy(frame, xoffset, yoffset, xwidth, yheight,
                     0, 0, ledImage[i].width, ledImage[i].height);
    // convert the LED image to raw data
    byte[] ledData =  new byte[(ledImage[i].width * ledImage[i].height * 3) + 3];
    image2data(ledImage[i], ledData, ledLayout[i]);
    if (i == 0) {
      ledData[0] = '*';  // first Teensy is the frame sync master
      int usec = (int)((1000000.0 / TargetFrameRate) * 0.75);
      ledData[1] = (byte)(usec);   // request the frame sync pulse
      ledData[2] = (byte)(usec >> 8); // at 75% of the frame time
    } else {
      ledData[0] = '%';  // others sync to the master board
      ledData[1] = 0;
      ledData[2] = 0;
    }
    // send the raw data to the LEDs  :-)
    ledSerial[i].write(ledData); 
  }
}

// image2data converts an image to OctoWS2811's raw data format.
// The number of vertical pixels in the image must be a multiple
// of 8.  The data array must be the proper size for the image.
void image2data(PImage image, byte[] data, boolean layout) {
  int offset = 3;
  int x, y, xbegin, xend, xinc, mask;
  int linesPerPin = image.height / 8;
  int pixel[] = new int[8];
  
  for (y = 0; y < linesPerPin; y++) {
    if ((y & 1) == (layout ? 0 : 1)) {
      // even numbered rows are left to right
      xbegin = 0;
      xend = image.width;
      xinc = 1;
    } else {
      // odd numbered rows are right to left
      xbegin = image.width - 1;
      xend = -1;
      xinc = -1;
    }
    for (x = xbegin; x != xend; x += xinc) {
      for (int i=0; i < 8; i++) {
        // fetch 8 pixels from the image, 1 for each pin
        pixel[i] = image.pixels[x + (y + linesPerPin * i) * image.width];
        pixel[i] = convert_RGB_2_GRB(pixel[i]);
       // pixel[i] = pixel[i] % 20; 
      //  pixel[i] = pixel[i] & 0x1F1F1F;
      pixel[i] = pixel[i] & 0xf0f0f0;
      pixel[i] = pixel[i] >> 4;
      
      }
      // convert 8 pixels to 24 bytes
      for (mask = 0x800000; mask != 0; mask >>= 1) {
        byte b = 0;
        for (int i=0; i < 8; i++) {
          if ((pixel[i] & mask) != 0) b |= (1 << i);
        }
        data[offset++] = b;
      }
    }
  } 
}

int convert_RGB_2_GRB(int colour) {
  int red = (colour & 0xFF0000) >> 16;
  int green = (colour & 0x00FF00) >> 8;
  int blue = (colour & 0x0000FF);
  
  red = gammaTable[red];
  green = gammaTable[green];
  blue = gammaTable[blue];
  
  return (green << 16) | (red << 8) | (blue);
}

// ask a Teensy board for its LED configuration, and set up the info for it.
void serialConfigure(String portName) {
  if (numberOfPortsInUse >= MaximumNumberOfPorts) {
    println("too many serial ports, please increase maxPorts");
    errorCount++;
    return;
  }
  try {
    ledSerial[numberOfPortsInUse] = new Serial(this, portName);
    if (ledSerial[numberOfPortsInUse] == null) throw new NullPointerException();
    ledSerial[numberOfPortsInUse].write('?');
  } catch (Throwable e) {
    println("Serial port " + portName + " does not exist or is non-functional");
    errorCount++;
    return;
  }
  delay(50);
  String line = ledSerial[numberOfPortsInUse].readStringUntil(10);
  if (line == null) {
    println("Serial port " + portName + " is not responding.");
    println("Is it really a Teensy 3.0 running VideoDisplay?");
    errorCount++;
    return;
  }
  String param[] = line.split(",");
  if (param.length != 12) {
    println("Error: port " + portName + " did not respond to LED config query");
    errorCount++;
    return;
  }
  // only store the info and increase numPorts if Teensy responds properly
  ledImage[numberOfPortsInUse] = new PImage(Integer.parseInt(param[0]), Integer.parseInt(param[1]), RGB);
  ledArea[numberOfPortsInUse] = new Rectangle(Integer.parseInt(param[5]), Integer.parseInt(param[6]),
                     Integer.parseInt(param[7]), Integer.parseInt(param[8]));
  ledLayout[numberOfPortsInUse] = (Integer.parseInt(param[5]) == 0);
  numberOfPortsInUse++;
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

// scale a number by a percentage, from 0 to 100
int percentage(int num, int percent) {
  double mult = percentageFloat(percent);
  double output = num * mult;
  return (int)output;
}

// scale a number by the inverse of a percentage, from 0 to 100
int percentageInverse(int num, int percent) {
  double div = percentageFloat(percent);
  double output = num / div;
  return (int)output;
}

// convert an integer from 0 to 100 to a float percentage
// from 0.0 to 1.0.  Special cases for 1/3, 1/6, 1/7, etc
// are handled automatically to fix integer rounding.
double percentageFloat(int percent) {
  if (percent == 33) return 1.0 / 3.0;
  if (percent == 17) return 1.0 / 6.0;
  if (percent == 14) return 1.0 / 7.0;
  if (percent == 13) return 1.0 / 8.0;
  if (percent == 11) return 1.0 / 9.0;
  if (percent ==  9) return 1.0 / 11.0;
  if (percent ==  8) return 1.0 / 12.0;
  return (double)percent / 100.0;
}