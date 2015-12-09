import numpy as np
import cv2
from time import sleep
import serial


#ser = serial.Serial('/dev/ttyUSB0', 115200, timeout=1)
ser = serial.Serial('/dev/ttyACM1', 115200, timeout=1)
byteR = (255,0,0)
byteG = (0,255,0)
byteB = (0,0,255)

NUM_LEDS = 100

j = 0;
while 1:
    tdata = ser.read()
    if tdata == 'a':
        print 'got an a!'
        print j
        if j == 0:
            for i in range(0,NUM_LEDS):
                ser.write(np.uint8(byteR))
                ser.write(np.uint8(byteR))
                ser.write(np.uint8(byteR))
            j = 1
            continue
        if j == 1:
            for i in range(0,NUM_LEDS):
                ser.write(np.uint8(byteG))
                ser.write(np.uint8(byteG))
                ser.write(np.uint8(byteG))
            j = 2
            continue
        if j == 2:
            for i in range(0,NUM_LEDS):
                ser.write(np.uint8(byteB))
                ser.write(np.uint8(byteB))
                ser.write(np.uint8(byteB))
            j = 0
            continue



