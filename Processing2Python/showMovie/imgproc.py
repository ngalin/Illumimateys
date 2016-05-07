#!/usr/bin/env python
from showMovie.defish import create_fisher
import cv2

# Observed framing of the camera I used
right_crop = 178+10
left_crop = 216+10
diameter = 1920 - (left_crop + right_crop)
vshift = 40
top_margin = ((diameter - 1080) / 2) + vshift
bottom_margin = ((diameter - 1080) / 2) - vshift

class Pipeline(object):
    def __init__(self, defish):
        if defish:
            self.defisher = create_fisher((diameter,diameter), (1080,1080))
        else:
            self.defisher = None

    def process(self, img):
        if self.defisher:
            img = img[0:1080, left_crop:-right_crop]
            img = cv2.copyMakeBorder(img, top_margin, bottom_margin, 0, 0, cv2.BORDER_CONSTANT)
            # cv2.imwrite("border.jpg", img)

            img = self.defisher.unwarp(img)
            # cv2.imwrite("defished.jpg", img)
            img = cv2.resize(img, (img.shape[1]/2, img.shape[0]/2))

        cv2.imshow("debug", img)
        return img
