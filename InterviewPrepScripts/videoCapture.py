import numpy as np
import cv2
import time

start = time.time()
end = start + 3 #show video for three seconds - I do this to make sure your stream doesn't get stuffed up by a bad exit. Remove in future.

cap = cv2.VideoCapture(0)

#while time.time() < end: #
while(True):
    # Capture frame-by-frame
    ret, frame = cap.read()
    if frame == None:
        continue
    b,g,r = cv2.split(frame)
    b_new = cv2.resize(b,(10,10))
    g_new = cv2.resize(g,(10,10))
    r_new = cv2.resize(r,(10,10))
    out = cv2.merge((b_new,g_new,r_new))
    cv2.imshow('frame',out)
    if cv2.waitKey(1) & 0xFF == ord('q'):
        cap.release()
        cv2.destroyAllWindows()
        break

# When everything done, release the capture
cap.release()
cv2.destroyAllWindows()
