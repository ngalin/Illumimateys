#!/usr/bin/env python

import cv2
import numpy as np

FISH_FOV = 180.0

class Defisher(object):
    def __init__(self, src_size, dst_size, xmap, ymap):
        self.src_size = src_size
        self.dst_size = dst_size
        self.xmap = xmap
        self.ymap = ymap

    def unwarp(self, img):
        assert img.shape[0] == self.src_size[1]
        assert img.shape[1] == self.src_size[0]
        output = cv2.remap(img, self.xmap, self.ymap, cv2.INTER_LINEAR)
        return output


def create_fisher(src_size, dst_size, hfovd=FISH_FOV, vfovd=FISH_FOV):
    Ws, Hs = src_size
    Wd, Hd = dst_size
    # Build the fisheye mapping
    map_x = np.zeros((Hd, Wd), np.float32)
    map_y = np.zeros((Hd, Wd), np.float32)
    vfov = (vfovd / 180.0) * np.pi
    hfov = (hfovd / 180.0) * np.pi
    vstart = ((180.0 - vfovd) / 180.00) * np.pi / 2.0
    hstart = ((180.0 - hfovd) / 180.00) * np.pi / 2.0
    count = 0
    # need to scale to changed range from our
    # smaller cirlce traced by the fov
    xmax = np.sin(np.pi / 2.0) * np.cos(vstart)
    xmin = np.sin(np.pi / 2.0) * np.cos(vstart + vfov)
    xscale = xmax - xmin
    xoff = xscale / 2.0
    zmax = np.cos(hstart)
    zmin = np.cos(hfov + hstart)
    zscale = zmax - zmin
    zoff = zscale / 2.0
    # Fill in the map, this is slow but
    # we could probably speed it up
    # since we only calc it once, whatever
    for y in range(0, int(Hd)):
        theta = hstart + (hfov * ((float(y) / float(Hd))))
        zp = ((np.cos(theta)) + zoff) / zscale  #
        for x in range(0, int(Wd)):
            count = count + 1
            phi = vstart + (vfov * ((float(x) / float(Wd))))
            xp = ((np.sin(theta) * np.cos(phi)) + xoff) / zscale  #
            xS = Ws - (xp * Ws)
            yS = Hs - (zp * Hs)
            map_x.itemset((y, x), int(xS))
            map_y.itemset((y, x), int(yS))

    return Defisher(src_size, dst_size, map_x, map_y)
