#!/usr/bin/env python
import itertools

from showMovie.defish import create_fisher
import cv2
import numpy as np

THRESHOLD = int(255 * 0.7)

# Observed framing of the camera I used
right_crop = 178+10
left_crop = 216+10
diameter = 1920 - (left_crop + right_crop)
vshift = 40
top_margin = ((diameter - 1080) / 2) + vshift
bottom_margin = ((diameter - 1080) / 2) - vshift

class Pipeline(object):
    def __init__(self, defish):
        self.prev = None
        self.flowstate = None
        if defish:
            self.defisher = create_fisher((diameter,diameter), (1080,1080))
        else:
            self.defisher = None

    def process(self, img):
        # Simplify to grayscale for processing
        img = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)

        if self.defisher:
            img = img[0:1080, left_crop:-right_crop]
            img = cv2.copyMakeBorder(img, top_margin, bottom_margin, 0, 0, cv2.BORDER_CONSTANT)
            # cv2.imwrite("border.jpg", img)

            img = self.defisher.unwarp(img)
            # cv2.imwrite("defished.jpg", img)
            img = cv2.resize(img, (img.shape[1]/2, img.shape[0]/2))

        ### Analysis (in greyscale)
        img = morph_cleanup(img)
        contours = find_contours(img)

        # flowimg, self.flowstate = compute_flow(self.prev, img, self.flowstate)

        ### Drawing (in colour)
        img = cv2.cvtColor(img, cv2.COLOR_GRAY2BGR)
        draw_contours(img, contours)

        cv2.imshow("debug", img)
        return img


def morph_cleanup(img):
    ### Morphological cleanup
    # http://docs.opencv.org/3.0-beta/doc/py_tutorials/py_imgproc/py_morphological_ops/py_morphological_ops.html
    # http://stackoverflow.com/questions/29104091/morphological-reconstruction-in-opencv
    morph_kernel = np.ones((5, 5), np.uint8)

    # img = cv2.erode(img, morph_kernel)
    # img = cv2.dilate(img, morph_kernel)

    # Morph open to remove noise
    img = cv2.morphologyEx(img, cv2.MORPH_OPEN, morph_kernel, iterations=2)

    # Morph close to fill dark holes
    img = cv2.morphologyEx(img, cv2.MORPH_CLOSE, morph_kernel, iterations=3)
    return img

def find_contours(img):
    edges = cv2.Canny(img, 20, 40)
    edges = cv2.GaussianBlur(edges, (5, 5), 0)
    # img = edges

    contours, hchy = cv2.findContours(edges, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)
    return contours

def draw_contours(img, contours):
    # Draw and fill all contours
    # cv2.drawContours(img, contours, -1, (0, 255, 0), -1)
    colors = itertools.cycle(itertools.product(*([(0, 255)] * 3)))
    for i, c in enumerate(contours):
        cv2.drawContours(img, contours, i, next(colors), -1)

def local_max(img):
    kernel = np.ones((40, 40), np.uint8)
    mask = cv2.dilate(img, kernel)
    result = cv2.compare(img, mask, cv2.CMP_GE)
    return result
