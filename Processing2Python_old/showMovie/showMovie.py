import cv2

cv2.namedWindow("preview")
vc = cv2.VideoCapture(0)

if vc.isOpened():  # try to get the first frame
    rval, frame = vc.read()
else:
    rval = False

[width,height,depth] = frame.shape
print width
print height
print depth

while rval:
    cv2.imshow("preview", frame)
    rval, frame = vc.read()
    frame = cv2.flip(frame, 1)
    key = cv2.waitKey(20)
    if key == 27:  # exit on ESC
        break

cv2.destroyWindow("preview")
