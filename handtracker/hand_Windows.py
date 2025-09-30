# Virtual Mouse med Hånd-Tracking - til Windows

import cv2
import numpy as np
import time
import pyautogui
from rtmlib import Hand, draw_skeleton

# Indstillinger
wVideo, hVideo = 640, 360   # Mindre ramme = hurtigere 
frameR = 100
smoothing = 7
pyautogui.FAILSAFE = False

# Skærmstørrelse
wScr, hScr = pyautogui.size()

# Smoothing-variabler
xloc, yloc = 0, 0
clocx, clocy = 0, 0

# FPS
ptime = 0

# States
tap_clicked = False
hold_down = False
right_hold_down = False
middle_click = False
last_middle_click = 0  # timestamp for middle click

# Hånd-tracker
hand_tracker = Hand(backend='onnxruntime', device='cpu')

# Kamera
cap = cv2.VideoCapture(0)
cap.set(cv2.CAP_PROP_FRAME_WIDTH, wVideo)
cap.set(cv2.CAP_PROP_FRAME_HEIGHT, hVideo)
if not cap.isOpened():
    print("Kunne ikke åbne kameraet")
    exit()

frame_count = 0
while True:
    ret, frame = cap.read()
    if not ret:
        break

    # Hænder
    kpts, scores = hand_tracker(frame)
    if len(kpts) > 0:
        hand = kpts[0]

        frame = draw_skeleton(frame, kpts, scores)

        ix, iy = hand[8].astype(int)
        mx, my = hand[12].astype(int)
        tx, ty = hand[4].astype(int)

        # Fingre
        fingers = [0, 0, 0, 0, 0]
        if hand[4][0] > hand[3][0]: fingers[0] = 1   # Tommelfinger
        if hand[8][1]  < hand[6][1]:  fingers[1] = 1 # Pegefinger
        if hand[12][1] < hand[10][1]: fingers[2] = 1 # Langfinger
        if hand[16][1] < hand[14][1]: fingers[3] = 1 # Ringfinger
        if hand[20][1] < hand[18][1]: fingers[4] = 1 # Lillefinger

        # Dynamiske tærskler
        xs, ys = hand[:,0], hand[:,1]
        diag = float(np.hypot(xs.max() - xs.min(), ys.max() - ys.min()))
        tap_thr   = max(15.0, 0.12 * diag)
        pinch_thr = max(20.0, 0.18 * diag)

        # Mus-flyt – throttle to every 2nd frame
        if fingers == [0,1,0,0,0]:
            index_x = np.interp(ix, (frameR, wVideo-frameR), (0, wScr))
            index_y = np.interp(iy, (frameR, hVideo-frameR), (0, hScr))
            clocx = xloc + (index_x - xloc) / smoothing
            clocy = yloc + (index_y - yloc) / smoothing

            if frame_count % 2 == 0:  # Kun hver anden frame
                pyautogui.moveTo(wScr - clocx, clocy, _pause=False)

            cv2.circle(frame, (ix, iy), 15, (255, 0, 255), cv2.FILLED)
            xloc, yloc = clocx, clocy

        # Hold venstre klik
        tap_dist = float(np.linalg.norm(hand[8] - hand[12]))
        if (fingers[1] == 1 and fingers[2] == 1) and (tap_dist < tap_thr) and (not hold_down):
            pyautogui.click(button='left', _pause=False)
            hold_down = True
        if not (fingers[1] == 1 and fingers[2] == 1 and tap_dist < tap_thr):
            hold_down = False

        # Venstre klik
        pinch_dist = float(np.linalg.norm(hand[4] - hand[8]))
        pinch = pinch_dist < pinch_thr
        if pinch and not tap_clicked:
            pyautogui.mouseDown(button='left', _pause=False)
            tap_clicked = True
        elif not pinch and tap_clicked:
            pyautogui.mouseUp(button='left', _pause=False)
            tap_clicked = False

        # Højre klik hold
        pinch_dist_right = float(np.linalg.norm(hand[4] - hand[12]))
        pinch_right = pinch_dist_right < pinch_thr
        if pinch_right and not right_hold_down:
            pyautogui.mouseDown(button='right', _pause=False)
            right_hold_down = True
        elif not pinch_right and right_hold_down:
            pyautogui.mouseUp(button='right', _pause=False)
            right_hold_down = False

        # Scroll klik – non-blocking throttle
        if fingers[1:5] == [1,1,1,1] and fingers[0] == 0:
            if time.time() - last_middle_click > 0.3:
                pyautogui.click(button='middle', _pause=False)
                last_middle_click = time.time()

        # Scroll op/ned
        if fingers[1:4] == [1,1,1] and fingers[0] == 0 and fingers[4] == 0:
            current_hand_y = int(hand[9][1])
            mid_screen_y = hVideo // 2
            if current_hand_y < mid_screen_y:
                pyautogui.scroll(75)
            else:
                pyautogui.scroll(-75)

    # FPS
    ctime = time.time()
    fps = 1 / (ctime - ptime) if ctime != ptime else 0
    ptime = ctime
    cv2.putText(frame, str(int(fps)), (20, 50),
                cv2.FONT_HERSHEY_PLAIN, 2, (255, 0, 0), 2)

    cv2.imshow("Windows Virtual Mouse | Optimized", frame)
    if cv2.waitKey(1) & 0xFF == 27:
        break

    frame_count += 1

# Cleanup
if hold_down:
    pyautogui.mouseUp(button="left", _pause=False)
cap.release()
cv2.destroyAllWindows()
