import numpy as np
import cv2
from time import sleep
import serial


#ser = serial.Serial('/dev/ttyUSB0', 115200, timeout=1)
ser = serial.Serial('/dev/ttyACM0', 115200, timeout=1)
cap = cv2.VideoCapture(0)

NUM_LEDS = 100
SEND_SERIAL = True

while 1:
    ret,frame = cap.read()
    if frame == None:
        continue
#    b,g,r = cv2.split(frame)
    r,g,b = cv2.split(frame)
    b_new = cv2.resize(b,(10,10))
    g_new = cv2.resize(g,(10,10))
    r_new = cv2.resize(r,(10,10))
    out = cv2.merge((b_new,g_new,r_new))
    cv2.imshow('frame',out)
    b_send = np.array(b_new.flatten())
    g_send = np.array(g_new.flatten())
    r_send = np.array(r_new.flatten())
    
    if SEND_SERIAL:
        tdata = ser.read()
        if tdata == 'a':
            print 'got an a!'
            #send frame
            for i in range(0,NUM_LEDS):
                colorBytes = (b_send[i]/2,g_send[i]/2,r_send[i]/2)
#                colorBytes = (255-b_send[i],255-g_send[i],255-r_send[i])
                ser.write(np.uint8(colorBytes))
                continue

#    for i in range(0,NUM_LEDS):
#        print np.uint8(b_send[i])
#        print np.uint8(g_send[i])
#        print np.uint8(r_send[i])

    if cv2.waitKey(1) & 0xFF == ord('q'):
#        cap.release()
#        cv2.destroyAllWindows()
        break

cap.release()
cv2.destroyAllWindows()



