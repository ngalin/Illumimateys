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
from concurrent.futures import ThreadPoolExecutor
from showMovie.imgproc import Pipeline, BackgroundRejecterMog, BackgroundRejecterAvg
import check_panel_time
from imgproc import draw_text

FAKE_SERIAL = False
DEFISH = True

MAX_NUM_PORTS = 24
TARGET_FRAME_RATE = 30
TEENSY_SYNC = False

CAPTURE_SIZE = (1920, 1080)
PREVIEW_SIZE = (CAPTURE_SIZE[0]/2, CAPTURE_SIZE[1]/2)

led_serial = []
led_image = []
led_area = []
led_layout = []

PANEL_WIDTH = 180
PANEL_HEIGHT = 120
DUMMY_COL_INDICES = list(range(24, 24+16, 4))

send_executor = ThreadPoolExecutor(1)
is_writing = False

FAKE_PORTS = ['/fake/1', '/fake/2', '/fake/3', '/fake/4', '/fake/5', '/fake/6']
class FakeSerial(object):
    def readline(self):
        return '120,32,0,0,0,152,0,32,120,0,0,0'
    def write(self, b):
        pass
    def close(self):
        pass

# ask a Teensy board for its LED configuration, and set up the info for it.
def serial_configure(port_name, port_num, fake=False):
    if port_num >= MAX_NUM_PORTS:
        print 'Too many serial ports, please increase maxPorts'
        return
    print 'Port name ' + port_name

    port = FakeSerial() if fake else serial.Serial(port_name, timeout=1)
    led_serial.append(port)
    if led_serial[port_num] is None:
        print 'portName: ', port_name, ' returned null'
        return

    time.sleep(500 / 1000.0)
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

def initialise_serial_ports(fake=False):
    ports = FAKE_PORTS if fake else glob.glob('/dev/ttyACM*')#''/dev/tty.usbmodem*')
    print 'Serial Ports: '
    print ports
    idx = -1

    for idx, port in enumerate(ports):
        serial_configure(port, idx, fake=fake)
    return idx + 1

def close_all_ports(num_ports):
    for i in range(0, num_ports):
        led_serial[i].close()

def send_frame_to_led_panels(frame, num_ports, show_debug=False):
    # Resize to exact dimensions of panels, adding in dummy columns
    frame = hp.resize(frame, PANEL_WIDTH, PANEL_HEIGHT, DUMMY_COL_INDICES)
    if show_debug: cv2.imshow("panels", frame)

    if is_writing:
        return # drop the frame that the panels can't keep up with

    # Write the frame to panels
    for teensy_idx in range(0, num_ports):
        # copy a portion of the movie's image to the LED image
        xoffset = led_area[teensy_idx].x
        yoffset = led_area[teensy_idx].y
        twidth = led_area[teensy_idx].width
        theight = led_area[teensy_idx].height

        # determine what portion of frame to send to given Teensy:
        led_image[teensy_idx] = frame[yoffset:yoffset + theight, xoffset:xoffset + twidth, :]
        led_data = hp.image_to_data_fast(led_image[teensy_idx], led_layout[teensy_idx])
        # verify_led_data(teensy_idx, led_data)

        # send byte data to Teensys:
        if (not TEENSY_SYNC) or teensy_idx == 0:
            led_data[0] = '*'  # first Teensy is the frame sync master
            usec = int((1000000.0 / TARGET_FRAME_RATE) * 0.75)
            led_data[1] = (usec) & 0xff  # request the frame sync pulse
            led_data[2] = (usec >> 8) & 0xff  # at 75% of the frame time
        else:
            led_data[0] = '%'  # others sync to the master board
            led_data[1] = 0
            led_data[2] = 0

        def write(idx, data):
            global is_writing
            is_writing = True
            led_serial[idx].write(data)
            is_writing = False
        # write(teensy_idx, bytes(led_data))
        send_executor.submit(write, teensy_idx, bytes(led_data))

def verify_led_data(teensy_idx, led_data):
    if teensy_idx > 4: return
    led_data_orig = hp.image_to_data_original(led_image[teensy_idx], led_layout[teensy_idx])
    if led_data != led_data_orig:
        print(repr(led_data_orig))
        print(repr(led_data))
        # raise AssertionError

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

# Notes:
# - consider reading in grayscale frame straight away

def main(argv):
    filename = None
    if argv:
        filename = argv[0]

    needs_release = False
    if filename:
        cap = open_file(filename)
    else:
        cap = open_camera()
        needs_release = True
    if not cap.isOpened:
        print "Failed to open capture"
        return

    print "Initialising pipeline"
    pipeline = Pipeline(DEFISH, bg=BackgroundRejecterAvg())

    print "Initialising serial ports"
    num_ports = initialise_serial_ports(fake=FAKE_SERIAL)
    print "Initialised", num_ports, "ports"

    # Open a preview window
    cv2.namedWindow("capture")
    cv2.namedWindow("debug")
    cv2.namedWindow("debug2")
    cv2.namedWindow("panels")

    # Run until no more frames
    run(cap, filename, num_ports, pipeline)

    cv2.destroyWindow("capture")
    cv2.destroyWindow("debug")
    cv2.destroyWindow("debug2")
    cv2.destroyWindow("panels")
    if needs_release:
        cap.release() #release camera

    # in case of some crash - set all LEDs to black before closing ports to Teensys
    send_black_frame(num_ports)
    time.sleep(1)
    send_black_frame(num_ports)
    send_executor.shutdown(wait=True)
    close_all_ports(num_ports)


def run(cap, filename, num_ports, pipeline):
    tstart = time.time()
    have_frame, frame = cap.read()
    framecount = 1
    # need to first draw all black frame:
    send_black_frame(num_ports)
    while have_frame:
        show_debug = framecount % (TARGET_FRAME_RATE/2) == 0
        if show_debug: cv2.imshow("capture", cv2.resize(frame, PREVIEW_SIZE))

        tprocstart = time.time()
        frame = pipeline.process(frame, show_debug)
        frame = cv2.flip(frame, 1)
        #draw_text(frame)
        tprocend = time.time()

        if check_panel_time.good_time_to_play():
            send_frame_to_led_panels(frame, num_ports, show_debug)
        else:
            send_black_frame(num_ports)

        key = cv2.waitKey(1)
        if key == 27:  # exit on ESC
            break

        tend = time.time()
        if show_debug:
            proc_duration = (tprocend - tprocstart)
            send_duration = (tend - tprocend)
            duration = (tend - tstart)
            print "Frame took", duration * 1000, "ms,", proc_duration * 1000, "proc,", \
                send_duration * 1000, "send,", (1 / duration), "fps"
        tstart = time.time()
        have_frame, frame = cap.read()
        framecount += 1
        if filename and not have_frame:
            framecount = 0
            cap = open_file(filename)
            have_frame, frame = cap.read()
    return cap


def send_black_frame(num_ports):
    black_frame = np.zeros((184, 120, 3), np.uint8)
    black_frame[:] = (0, 0, 0)
    send_frame_to_led_panels(black_frame, num_ports)

if __name__ == "__main__":
    main(sys.argv[1:])
