// Image processing methods
//=====================================================================================

final float THRESHOLD = 0.5;

// Erosion/dilation sequence. Positive for dilation, negative for erosion.
final int[] EROSION_DILATION = {-2, 1};

/**
 * Processes the argument image through OpenCV, returning the resulting image.
 */
PImage processFrame(PImage frame, boolean flip) {
  frame = centerCrop(frame, WidthInPixels, HeightInPixels);
  opencv.loadImage(frame);
  
  thresholdImage();

  //opencv.findCannyEdges(20,75);
  if (flip) { opencv.flip(OpenCV.HORIZONTAL); }
  PImage snapshot = opencv.getSnapshot();
  return frame;
  //return snapshot;
}

/**
 * Annotates the image already drawn to the render buffer in place
 */
void annotateImage() {
  annotateBlobs();
}

// Thresholds an image
void thresholdImage() {
  //int thresholdBlockSize = 16; int thresholdConstant = 1;
  //opencv.adaptiveThreshold(thresholdBlockSize+1, thresholdConstant);
   
  opencv.threshold((int)(255 * THRESHOLD));
  
  // Erode/dilate to remove noise
  for (int ed : EROSION_DILATION) {
   if (ed > 0) {
     for (int i = 0; i < ed; ++i) { opencv.dilate(); }
   } else  if (ed < 0) {
     for (int i = 0; i < -ed; ++i) { opencv.erode(); }
   }
  }
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

// Computes blobs over and then draws annotations into the frame
void annotateBlobs() {
  PImage frame = get(0, 0, WidthInPixels, HeightInPixels);
  blobDetector.computeBlobs(frame.pixels);
  println("Blobs:", blobDetector.getBlobNb());
  color hilite = #ff0000;
  for (int i = 0; i < blobDetector.getBlobNb(); ++i) {
    Blob b = blobDetector.getBlob(i);
    //println("blob at", b.x, b.y); 
    int cx = (int)(b.x * WidthInPixels);
    int cy = (int)(b.y * HeightInPixels);
    int w = (int)(b.w * WidthInPixels);
    int h = (int)(b.h * HeightInPixels);
    stroke(hilite);
    line(cx-w/2, cy, cx+w/2, cy);
    line(cx, cy-h/2, cx, cy+h/2);
  }
}