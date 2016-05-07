#!/usr/bin/env python
# -*- coding: utf-8 -*-
import numpy as np
import cv2
import serial
import glob
import time
from MyRectangle import MyRectangle
import helperFunctions as hp

cv2.namedWindow("preview")
vc = cv2.VideoCapture()

MaximumNumberOfPorts = 24
target_frame_rate = 30

led_serial = []
led_image = []
led_area = []
led_layout = []

panel_width_in_pixels = 180
panel_height_in_pixels = 120
idx_dummy_columns = np.array([147, 151, 155, 159])


# ask a Teensy board for its LED configuration, and set up the info for it.
def serial_configure(port_name, port_num):
    if port_num >= MaximumNumberOfPorts:
        print 'Too many serial ports, please increase maxPorts'
        return

    print 'Port name ' + port_name

    led_serial.append(serial.Serial(port_name, timeout=1))
    if led_serial[port_num] is None:
        print 'portName: ', port_name, ' returned null'
        return

    time.sleep(50 / 1000.0)  # sleep for 50ms

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
    print 'Panel: ', port_num, ' cols: ', params[0], ' rows: ', params[1]
    rect = MyRectangle((int(params[5]), int(params[6])), int(params[7]), int(params[8]))
    led_area.append(rect)

    print 'xoff: ', params[5], ' yoff: ', params[6], ' width: ', params[7], '%, height: ', params[8], '%'

    led_layout.append(int(params[2]))
    print 'laout: ', params[2]


def initialise_serial_ports():
    ports = glob.glob('/dev/tty.usbmodem*')
    print 'Serial Ports: '
    print ports
    total_num_ports = 0

    for idx, port in enumerate(ports):
        serial_configure(port, idx)
        total_num_ports += 1

    print led_serial
    return total_num_ports


def close_all_ports(num_ports):
    for i in range(0, num_ports):
        led_serial[i].close()


# int initialiseProcessingPipeline():
#   opencv = new OpenCV(this, WidthInPixels, HeightInPixels);
#   println("Using color?", opencv.getUseColor());
#   //opencv.useGray();
#
#   blobDetector = new BlobDetection(WidthInPixels, HeightInPixels);
#   //BlobDetection.setConstants(5, 20, 60);
#   blobDetector.setThreshold(0.5);
#   blobDetector.setPosDiscrimination(true); // find highlights, not lowlights
# }

def send_frame_to_led_panels(frame, num_ports):
    # Write the frame to panels
    [height, width, depth] = frame.shape
    for teensy_idx in range(0, num_ports):
        # copy a portion of the movie's image to the LED image
        xoffset = led_area[teensy_idx].x
        yoffset = led_area[teensy_idx].y
        twidth = led_area[teensy_idx].width
        theight = led_area[teensy_idx].height

        print 'xoffset: ' + str(xoffset)
        print 'yoffset: ' + str(yoffset)
        print 'width: ' + str(twidth)
        print 'height: ' + str(theight)

        print 'start width: ' + str(xoffset) + ' end width: ' + str(xoffset + twidth)
        print 'start height: ' + str(yoffset) + ' end height: ' + str(yoffset + theight)

        print frame.shape

        # determine what portion of frame to send to given Teensy:
        # led_image[teensy_idx] = np.copy(frame[xoffset:xoffset+theight,yoffset:yoffset+twidth,:])
        led_image[teensy_idx] = np.copy(frame[yoffset:yoffset + theight, xoffset:xoffset + twidth, :])
        # convert the LED image to raw data byte[]
        print 'led_image[teensy_idx] ' + str(led_image[teensy_idx].shape)
        led_data = hp.image_to_data(led_image[teensy_idx], led_layout)

        # send byte data to Teensys:
        if teensy_idx == 0:
            led_data[0] = '*' # first Teensy is the frame sync master
            usec = int((1000000.0 / target_frame_rate) * 0.75)
            led_data[1] = (usec) & 0xff # request the frame sync pulse
            led_data[2] = (usec >> 8) & 0xff # at 75% of the frame time
        else:
            led_data[0] = '%' # others sync to the master board
            led_data[1] = 0
            led_data[2] = 0

        # and finally send the raw data to the LEDs
        print led_serial[teensy_idx]
        print led_data
        led_serial[teensy_idx].write(bytearray(led_data))

def main():
    number_of_ports_in_use = initialise_serial_ports()
    print number_of_ports_in_use

    hp.initialise_gamma_table()
    #    print hp.gamma_table

    # now capture frames from webcam:
    cv2.namedWindow("preview")
    vc = cv2.VideoCapture(0)

    if vc.isOpened():  # try to get the first frame
        rval, frame = vc.read()
    else:
        rval = False

    print 'Frame shape: ' + str(frame.shape)

    frame_count = 3
    while rval and frame_count > 0:
        cv2.imshow("preview", frame)
        rval, frame = vc.read()
        cv2.imwrite('orig_frame.png', frame)
        # resize frame to exactly be the dimensions of LED panel
        new_frame = hp.resize(frame, panel_width_in_pixels, panel_height_in_pixels, idx_dummy_columns)
        new_frame = cv2.flip(new_frame, 1)
        cv2.imwrite('new_frame.png', new_frame)

        send_frame_to_led_panels(new_frame, number_of_ports_in_use)
        key = cv2.waitKey(20)
        if key == 27:  # exit on ESC
            break
        frame_count -= 1

    cv2.destroyWindow("preview")
    close_all_ports(number_of_ports_in_use)


if __name__ == "__main__":
    main()
