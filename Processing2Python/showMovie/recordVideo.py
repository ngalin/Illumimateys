import cv2


INTERNAL_CAMERA = 0
EXTERNAL_CAMERA = 1

filename = 'testRecordVideo.mp4'

cap = cv2.VideoCapture(INTERNAL_CAMERA)
frame_width = cap.get(cv2.cv.CV_CAP_PROP_FRAME_WIDTH)
frame_height = cap.get(cv2.cv.CV_CAP_PROP_FRAME_HEIGHT)
frame_rate = 30
fourcc = cv2.cv.CV_FOURCC(*'mp4v')
out = cv2.VideoWriter(filename,fourcc,frame_rate, (int(1280),int(1024)))

while (cap.isOpened()):
    have_frame, frame = cap.read()
    if have_frame:
        out.write(frame)
        cv2.imshow('video frame',frame)
    else:
        break

    key = cv2.waitKey(1)
    if key == 27:  # exit on ESC
        break


cap.release()
out.release()
cv2.destroyAllWindows()


