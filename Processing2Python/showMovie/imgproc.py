#!/usr/bin/env python
from showMovie.defish import create_fisher
import cv2
import numpy as np

THRESHOLD = int(255 * 0.7)
MIN_CONTOUR_AREA = 10

# Observed framing of the camera I used
right_crop = 178 + 10
left_crop = 216 + 10
diameter = 1920 - (left_crop + right_crop)
vshift = 40
top_margin = ((diameter - 1080) / 2) + vshift
bottom_margin = ((diameter - 1080) / 2) - vshift


def contours_to_plot(contours):
    contours_ = []

    for i in range(0, len(contours)):
        if cv2.contourArea(contours[i]) > 500:
            contours_.append(contours[i])

            # print cv2.contourArea(contours[i])

    return contours_


class Pipeline(object):
    def __init__(self, defish):
        if defish:
            self.defisher = create_fisher((diameter, diameter), (1080, 1080))
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
            img = cv2.resize(img, (img.shape[1] / 2, img.shape[0] / 2))

        # ok, img = cv2.threshold(img, THRESHOLD, 255, cv2.THRESH_BINARY)

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

        edges = cv2.Canny(img, 10, 70) #20,40
        #edges = cv2.GaussianBlur(edges, (7, 7), 0)
        edges = cv2.GaussianBlur(edges, (3, 3), 0)
        img = edges

        contours, hchy = cv2.findContours(edges, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_NONE)

        img = cv2.cvtColor(img, cv2.COLOR_GRAY2BGR)

        contours = contours_to_plot(contours)
        cv2.drawContours(img,contours, -1, (0,255,0), -1)
        #cv2.drawContours(img, contours[i], -1, (0,255,0), -1)


        # Convert back to colour (we'll replace this with colorising later.
        # img = cv2.cvtColor(img, cv2.COLOR_GRAY2BGR)

        #edges = cv2.cvtColor(edges, cv2.COLOR_GRAY2BGR)

        cv2.imshow("debug", img)
        return img


def local_max(img):
    kernel = np.ones((40, 40), np.uint8)
    mask = cv2.dilate(img, kernel)
    result = cv2.compare(img, mask, cv2.CMP_GE)
    return result
