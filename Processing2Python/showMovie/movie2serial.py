#!/usr/bin/env python
# -*- coding: utf-8 -*-
import sys

import numpy as np
import cv2
import serial
import glob
import time
from MyRectangle import MyRectangle
import helperFunctions as hp
from showMovie.imgproc import Pipeline

MAX_NUM_PORTS = 24
TARGET_FRAME_RATE = 30

CAPTURE_SIZE = (1920, 1080)
PREVIEW_SIZE = (CAPTURE_SIZE[0]/4, CAPTURE_SIZE[1]/4)

led_serial = []
led_image = []
led_area = []
led_layout = []

PANEL_WIDTH = 180
PANEL_HEIGHT = 120
DUMMY_COL_INDICES = list(range(24, 24+16, 4))

# ask a Teensy board for its LED configuration, and set up the info for it.
def serial_configure(port_name, port_num):
    if port_num >= MAX_NUM_PORTS:
        print 'Too many serial ports, please increase maxPorts'
        return

    print 'Port name ' + port_name

    led_serial.append(serial.Serial(port_name, timeout=1))
    if led_serial[port_num] is None:
        print 'portName: ', port_name, ' returned null'
        return

    time.sleep(500 / 1000.0)  # sleep for 50ms

    led_serial[port_num].write('?')

    line = led_serial[port_num].readline()
    print line

    if line is None:
        print 'Serial port ' + port_name + ' is not responding.'
        print 'Is it really a Teensy running VideoDisplay?'
        return

    params = line.split(",")
    if len(params) != 12:
        print 'Error: port ' + port_name + ' did not respond to LED config query'
        return

    # only store the info and increase numPorts if Teensy responds properly
    led_image.append(np.zeros((int(params[0]), int(params[1]), 3), np.uint8))
    # Note: rows and cols are according to the teensy, which is configured to be mounted rotated π/2
    #print 'Panel: ', port_num, ' cols: ', params[0], ' rows: ', params[1]
    rect = MyRectangle((int(params[5]), int(params[6])), int(params[7]), int(params[8]))
    led_area.append(rect)

    #print 'xoff: ', params[5], ' yoff: ', params[6], ' width: ', params[7], '%, height: ', params[8], '%'

    led_layout.append(int(params[2]))

def initialise_serial_ports():
    ports = glob.glob('/dev/tty.usbmodem*')
    print 'Serial Ports: '
    print ports
    idx = -1

    for idx, port in enumerate(ports):
        serial_configure(port, idx)
    return idx + 1

def close_all_ports(num_ports):
    for i in range(0, num_ports):
        led_serial[i].close()

def send_frame_to_led_panels(frame, num_ports):
    # Resize to exact dimensions of panels, adding in dummy columns
    frame = hp.resize(frame, PANEL_WIDTH, PANEL_HEIGHT, DUMMY_COL_INDICES)
    cv2.imshow("panels", frame)

    # Write the frame to panels
    for teensy_idx in range(0, num_ports):
        # copy a portion of the movie's image to the LED image
        xoffset = led_area[teensy_idx].x
        yoffset = led_area[teensy_idx].y
        twidth = led_area[teensy_idx].width
        theight = led_area[teensy_idx].height

        # determine what portion of frame to send to given Teensy:
        led_image[teensy_idx] = np.copy(frame[yoffset:yoffset + theight, xoffset:xoffset + twidth, :])
        led_data = hp.image_to_data(led_image[teensy_idx], led_layout[teensy_idx])

        # send byte data to Teensys:
       # if teensy_idx == 0:
        led_data[0] = '*'  # first Teensy is the frame sync master
        usec = int((1000000.0 / TARGET_FRAME_RATE) * 0.75)
        led_data[1] = (usec) & 0xff  # request the frame sync pulse
        led_data[2] = (usec >> 8) & 0xff  # at 75% of the frame time
        # else:
        #     led_data[0] = '%'  # others sync to the master board
        #     led_data[1] = 0
        #     led_data[2] = 0

        led_serial[teensy_idx].write(bytes(led_data))

def open_camera():
    print "Opening capture from camera at", CAPTURE_SIZE
    cap = cv2.VideoCapture(0)
    cap.set(cv2.cv.CV_CAP_PROP_FPS, 30)
    cap.set(cv2.cv.CV_CAP_PROP_FRAME_WIDTH, CAPTURE_SIZE[0])
    cap.set(cv2.cv.CV_CAP_PROP_FRAME_HEIGHT, CAPTURE_SIZE[1])
    # cap.set(cv2.cv.CV_CAP_PROP_GAIN, 1)
    # cap.set(cv2.cv.CV_CAP_PROP_EXPOSURE, 1)
    return cap

def open_file(path):
    print "Opening capture from", path
    cap = cv2.VideoCapture(path)
    cap.set(cv2.cv.CV_CAP_PROP_FPS, 30)
    return cap

def main(argv):
    filename = None
    if argv:
        filename = argv[0]

    defish = False
    if filename:
        cap = open_file(filename)
    else:
        cap = open_camera()
        defish = True
    if not cap.isOpened:
        print "Failed to open capture"
        return

    print "Initialising pipeline"
    pipeline = Pipeline(defish)

    print "Initialising serial ports"
    num_ports = initialise_serial_ports()
    print "Initialised", num_ports, "ports"

    # Open a preview window
    cv2.namedWindow("preview")
    cv2.namedWindow("panels")
    cv2.namedWindow("debug")

    tstart = time.time()
    have_frame, frame = cap.read()
    framecount = 1
    while have_frame:
        # frame = cv2.imread("/Users/alex/Desktop/shadowwall-test-1.png")
        preview_frame = cv2.resize(frame, PREVIEW_SIZE)
        cv2.imshow("preview", preview_frame)

        frame = pipeline.process(frame)
        send_frame_to_led_panels(frame, num_ports)

        key = cv2.waitKey(1)
        if key == 27:  # exit on ESC
            break

        tend = time.time()
        if framecount % TARGET_FRAME_RATE == 0:
            duration = (tend - tstart)
            print "Frame took", duration * 1000, "ms,", (1/duration), "fps"
        tstart = time.time()
        have_frame, frame = cap.read()
        framecount += 1
        if filename and not have_frame:
            framecount = 0
            cap = open_file(filename)
            have_frame, frame = cap.read()


    cv2.destroyWindow("preview")
    cv2.destroyWindow("panels")
    close_all_ports(num_ports)


if __name__ == "__main__":
    main(sys.argv[1:])
