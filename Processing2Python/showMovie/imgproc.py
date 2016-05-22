#!/usr/bin/env python
import itertools
import random
from collections import Counter

from showMovie.defish import create_fisher
import cv2
import numpy as np
from showMovie.helperFunctions import make_gamma_table
from perspective_transform import four_point_transform

THRESHOLD = int(255 * 0.7)
MIN_CONTOUR_DIMENSION = 20 # In debug window pixels

MORPH_KERNEL = np.ones((3, 3), np.uint8)

COLOURS = colors = itertools.cycle([
    (255, 100, 100),
    (255, 255, 100),
    (255, 100, 255),
    (100, 255, 100),
    (255, 100, 255),
    (100, 100, 255),
])

# Observed framing of the camera I used
RAW_HEIGHT = 1080
left_crop = 196+10 # Left black region to discard
right_crop = 208+10 # Right black region to discard
diameter = 1920 - (left_crop + right_crop) # Remaining horizontal diameter of projection
vshift = 74 # Amount to vertically shift downwards to recenter
top_margin = ((diameter - RAW_HEIGHT) / 2) + vshift # Top margin to add to create a square
bottom_margin = ((diameter - RAW_HEIGHT) / 2) - vshift # Bottom margin to add

# Processing time can be reduced by shrinking this, but it makes no difference until sending to
# teensys is faster than ~50 ms
DEFISHED_SIZE = 1080
DEFISHED_TOP_MARGIN = 308 # These are measured from post-fisheye image
DEFISHED_BOTTOM_MARGIN = 209

# Length of the longer (top) side of pespective rect
CROP_WIDTH = (DEFISHED_SIZE * 0.580)
# Length of the shorter (bottom) side
PERSPECTIVE_WIDTH = int(CROP_WIDTH * 0.840)

class Pipeline(object):
    def __init__(self, defish, bg=None):
        """
        :param defish: whether to defish
        """
        if defish:
            self.defisher = create_fisher((diameter,diameter), (DEFISHED_SIZE, DEFISHED_SIZE))
        else:
            self.defisher = None
        self.bg = bg
        self.prev_drawn = None
        self.prev_grey_img = None
        self.flowstate = None

    def process(self, img, show_debug=False):
        # Simplify to grayscale for processing
        img = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        is_color = False

        # Correct fisheye projection
        if self.defisher:
            img = img[0:RAW_HEIGHT, left_crop:-right_crop]
            img = cv2.copyMakeBorder(img, top_margin, bottom_margin, 0, 0, cv2.BORDER_CONSTANT)
            img = self.defisher.unwarp(img)

        img = correct_perspective(img)
        if self.bg:
            img = self.bg.process(img, show_debug)

        ### Analysis (in greyscale)
        # if show_debug: cv2.imshow("debug2", cv2.threshold(img, 12, 255, cv2.THRESH_BINARY)[1])
        img = morph_cleanup(img)

        # Threshold before contours
        # img = simple_threshold(img) #, or...
        img = adaptive_threshold(img)

        contours = find_contours(img)

        ### Drawing (in colour)
        if not is_color:
            img = cv2.cvtColor(img, cv2.COLOR_GRAY2BGR)
        img = draw_contours(img, contours, self.prev_drawn)
        self.prev_drawn = img
        # draw_rectangles(img, contours) #better identifies individual contours - allows to easily detect 'bottom' of contour for future 'moving down the screen'
        # bottom_of_contour(img, contours)

        if show_debug or True: cv2.imshow("debug", img)
        return img


class BackgroundRejecterMog(object):
    def __init__(self):
        # length_history = 100
        # number_gaussian_mixtures = 6
        # background_ratio = 0.9
        # noise_strength_sigma = 1
        self.fgbg = cv2.BackgroundSubtractorMOG()#history=200, nmixtures=6, backgroundRatio=0.1, noiseSigma=1)

    def process(self, frame, show_debug=False):
        fgmask = self.fgbg.apply(frame)
        if show_debug: cv2.imshow("bg", fgmask)
        frame = frame & fgmask
        return frame

class BackgroundRejecterAvg(object):
    def __init__(self, frame=None):
        self.avg = np.float32(frame) if frame else None

    def process(self, frame, show_debug=False):
        if self.avg is None:
            self.avg = np.float32(frame)

        cv2.accumulateWeighted(frame, self.avg, 0.003)
        res = cv2.convertScaleAbs(self.avg)
        if show_debug:
            cv2.imshow("bg", res)

        # Method 1: reject by subtraction. Avoids hard boundaries, only works well when background is dark.
        res = np.minimum(res, frame)
        frame = frame - res

        # Method 2: reject by masking. Leaves more information but creates hard "glow" boundaries at threshold
        # mask = np.abs(frame - res) > 10
        # frame = np.where(mask, frame, 0)
        return frame


def correct_perspective(img):
    # Crop and correct perspective
    topy = DEFISHED_TOP_MARGIN + 232  # Approx horizon - change if camera moves
    boty = DEFISHED_SIZE - DEFISHED_BOTTOM_MARGIN
    topleftx = (DEFISHED_SIZE - CROP_WIDTH) / 2
    toprightx = DEFISHED_SIZE - topleftx
    botleftx = (DEFISHED_SIZE - PERSPECTIVE_WIDTH) / 2
    botrightx = DEFISHED_SIZE - botleftx
    # (TL, TR, BR, BL)
    pts = np.array([(topleftx, topy), (toprightx, topy), (botrightx, boty), (botleftx, boty)], dtype="float32")
    img = four_point_transform(img, pts)
    return img

def morph_cleanup(img):
    ### Morphological cleanup
    # http://docs.opencv.org/3.0-beta/doc/py_tutorials/py_imgproc/py_morphological_ops/py_morphological_ops.html
    # http://stackoverflow.com/questions/29104091/morphological-reconstruction-in-opencv

    # img = cv2.erode(img, morph_kernel)
    # img = cv2.dilate(img, morph_kernel)

    # Morph open to remove noise
    img = cv2.morphologyEx(img, cv2.MORPH_OPEN, MORPH_KERNEL, iterations=1)

    # Morph close to fill dark holes
    img = cv2.morphologyEx(img, cv2.MORPH_CLOSE, MORPH_KERNEL, iterations=2)

    # Erode to define edges
    # img = cv2.erode(img, MORPH_KERNEL, iterations=2)

    # For cool fuzzy edge-style shadow, use gradient
    # img = cv2.morphologyEx(img, cv2.MORPH_GRADIENT, MORPH_KERNEL)

    return img

def simple_threshold(img):
    # Ghibli style
    thresh = 12
    ret, img = cv2.threshold(img, thresh, 255, cv2.THRESH_BINARY)
    return img

def adaptive_threshold(img):
    # Simple low threshold first to remove some noise
    # ret, img = cv2.threshold(img, 5, 255, cv2.THRESH_BINARY)

    thresh_size = 111
    thresh_c = -2
    img = cv2.adaptiveThreshold(img, 255, cv2.ADAPTIVE_THRESH_MEAN_C, cv2.THRESH_BINARY, thresh_size, thresh_c)
    return img

def find_contours(img):
    #when doing edge detection, remove/denoise image first, then apply Canny
    # img = cv2.GaussianBlur(img, (5, 5), 0)
    # edges = cv2.Canny(img, 100, 200)

    # Contours appropriate for filling with colour
    # edges = cv2.Canny(img, 5, 15)
    # edges = cv2.GaussianBlur(edges, (5, 5), 0) #consider blurring again, after edge detection
    # contours, hchy = cv2.findContours(edges, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)

    contours, hch = cv2.findContours(img, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
    # Discard too-small contours
    def is_big_enough(cnt):
        x, y, w, h = cv2.boundingRect(cnt)
        return w > MIN_CONTOUR_DIMENSION or h > MIN_CONTOUR_DIMENSION
    contours = [c for c in contours if is_big_enough(c)]
    return contours

def compute_flow(previmg, img, prevflow):
    pyramidScale = 0.5 # 0.5 is each layer half the size
    pyramidLevels = 3
    windowSize = 10 # higher is faster, robust, blurrier
    iterations = 3
    polySize = 5 # typically 5 or 7
    polySigma = 1.2 # suggested 5->1.1, 7->1.5
    gaussian = False

    if previmg is None:
        return None, None

    flags = 0
    if gaussian:
        flags |= cv2.OPTFLOW_FARNEBACK_GAUSSIAN
    if prevflow is not None:
        flags |= cv2.OPTFLOW_USE_INITIAL_FLOW

    flow = cv2.calcOpticalFlowFarneback(previmg, img,
        pyramidScale, pyramidLevels, windowSize, iterations,
        polySize, polySigma, flags, prevflow)

    magMax = 8
    mag, ang = cv2.cartToPolar(flow[..., 0], flow[..., 1])
    mag = np.clip(mag, 0, magMax)

    hsv = np.zeros(img.shape+(3,), 'uint8')
    hsv[..., 0] = ang * 180 / np.pi / 2  # hue is angle
    hsv[..., 1] = 255  # full saturation
    hsv[..., 2] = mag * (255 / magMax)  # value is magnitude
    rgb = cv2.cvtColor(hsv, cv2.COLOR_HSV2BGR)

    return flow, rgb

def draw_contours(img, contours, prev_drawn=None):
    # Draw and fill all contours
    drawn = np.zeros_like(img)
    for ctrIdx, ctr in enumerate(contours):
        color = (0, 0, 0)
        if prev_drawn is not None:
            # Randomly sample the bounding box of this contour in the previous image and use the most
            # common colour.
            x, y, w, h = cv2.boundingRect(ctr)
            prev_box = prev_drawn[y:y+h, x:x+w]
            # cv2.rectangle(drawn, (x, y), (x + w, y + h), (0, 255, 0), 1)
            color_counts = Counter()
            for _ in range(12): #Make this bigger for more domination by larger blobs
                xx = random.randrange(w)
                yy = random.randrange(h)
                color_counts[tuple(map(int, prev_box[yy, xx]))] += 1
            color_counts[(0, 0, 0)] = 0 # Don't choose black if possible
            counted = sorted(((count, color) for color, count in color_counts.items()), reverse=True)
            color = counted[0][1]
        if color == (0, 0, 0):
            color = next(COLOURS)
        cv2.drawContours(drawn, contours, ctrIdx, color, thickness=-1) # -1 to fill
    return drawn

def local_max(img):
    kernel = np.ones((40, 40), np.uint8)
    mask = cv2.dilate(img, kernel)
    result = cv2.compare(img, mask, cv2.CMP_GE)
    return result

def draw_rectangles(img, contours):
    #draw the bounding rectangles around contours
    for i, ctr in enumerate(contours):
        x, y, w, h = cv2.boundingRect(ctr)
        cv2.rectangle(img, (x, y), (x + w, y + h), (0, 255, 0), 2)

def bottom_of_contour(img, contours):
    bottom_x = []
    bottom_y = []
    #get the bounding rectangles around contours
    for i, ctr in enumerate(contours):
        x, y, w, h = cv2.boundingRect(ctr)
        bottom_x.append(x + w/2)
        bottom_y.append(y + h)
        cv2.circle(img,(int(bottom_x[i]),int(bottom_y[i])),3,(255,0,0))

    return zip(bottom_x, bottom_y)
