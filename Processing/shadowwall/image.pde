// Image processing methods
//=====================================================================================

final float THRESHOLD = 0.7;

// Erosion/dilation sequence. Positive for dilation, negative for erosion.
final int[] EROSION_DILATION = {-2, 1};

PImage processFrame(PImage frame, boolean flip) {
  frame = centerCrop(frame, WidthInPixels, HeightInPixels);
  opencv.loadImage(frame);
  //opencv.gray();
  opencv.useGray();
  
  thresholdImage();

  //opencv.findCannyEdges(20,75);
  if (flip) { opencv.flip(OpenCV.HORIZONTAL); }
  PImage snapshot = opencv.getSnapshot();
  //computeBlobs(snapshot);
  return snapshot;
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

// Draws blobs into the snapshot
void computeBlobs(PImage snapshot) {
  blobDetector.computeBlobs(snapshot.pixels);
  println("Blobs:", blobDetector.getBlobNb());
  color hilite = #ff0000;
  for (int i = 0; i < blobDetector.getBlobNb(); ++i) {
    Blob b = blobDetector.getBlob(i);
    //println("blob at", b.x, b.y); 
    int cx = (int)(b.x * snapshot.width);
    int cy = (int)(b.y * snapshot.height);
    int w = (int)(b.w * snapshot.width);
    int h = (int)(b.h * snapshot.height);
    for (int x = cx - (w/2); x < cx + (w/2); ++x) {
      snapshot.set(x, cy, hilite);
    }
    for (int y = cy - (h/2); y < cy + (h/2); ++y) {
      snapshot.set(cx, y, hilite);
    }
  }
}