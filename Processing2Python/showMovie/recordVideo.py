import cv2


INTERNAL_CAMERA = 1
EXTERNAL_CAMERA = 0
PREVIEW_SIZE = (1920/2, 1080/2)

filename = 'testRecordVideo_MakerSpace_withoutFilters_lightsOff.mp4'

cap = cv2.VideoCapture(EXTERNAL_CAMERA)
cap.set(cv2.cv.CV_CAP_PROP_FPS, 30)
cap.set(cv2.cv.CV_CAP_PROP_FRAME_WIDTH, 1920)
cap.set(cv2.cv.CV_CAP_PROP_FRAME_HEIGHT, 1080)

frame_width = cap.get(cv2.cv.CV_CAP_PROP_FRAME_WIDTH)
frame_height = cap.get(cv2.cv.CV_CAP_PROP_FRAME_HEIGHT)
frame_rate = 30
fourcc = cv2.cv.CV_FOURCC(*'mp4v')
out = cv2.VideoWriter(filename,fourcc,frame_rate, (int(1920),int(1080)))

while (cap.isOpened()):
    have_frame, frame = cap.read()
    if have_frame:
        out.write(frame)
        #cv2.imshow('video frame',frame)
        cv2.imshow("video frame", cv2.resize(frame, PREVIEW_SIZE))
    else:
        break

    key = cv2.waitKey(1)
    if key == 27:  # exit on ESC
        break


cap.release()
out.release()
cv2.destroyAllWindows()


