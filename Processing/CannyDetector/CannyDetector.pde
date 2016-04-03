import gab.opencv.*;
import processing.video.*;
import java.awt.*;

Capture video;
OpenCV opencv;
PImage src, canny, scharr, sobel;

void setup() {
  size(640, 480);
  video = new Capture(this, 640/2, 480/2);
  opencv = new OpenCV(this, 640/2, 480/2);

  opencv.loadCascade(OpenCV.CASCADE_FRONTALFACE);  

  video.start();
}

void draw() {
  scale(2);
  opencv.loadImage(video);
  opencv.findCannyEdges(20,75);
  canny = opencv.getSnapshot();
  
  //image(video, 0, 0 );
  image(canny, 0,0);
}

void captureEvent(Capture c) {
  c.read();
}