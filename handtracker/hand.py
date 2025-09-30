import cv2
import numpy as np
import time
import pyautogui
from rtmlib import Hand, draw_skeleton

# Indstillinger
wVideo, hVideo = 640, 480
frameR = 100          # bevægelsesramme
smoothing = 7
pyautogui.FAILSAFE = False  # undgå at programmet stopper ved skærmhjørner

# Skærmstørrelse
wScr, hScr = pyautogui.size()

# Smoothing-variabler
xloc, yloc = 0, 0
clocx, clocy = 0, 0

# FPS
ptime = 0

# States (adskilt!)
tap_clicked      = False   # “index+middle” klik en gang
left_hold_down   = False   # “thumb+index” hold venstre klik
right_down  = False   # “thumb+middle”  højre klik
scroll_anchor_y  = None    # referencepunkt til scroll-gesture

# Hånd-tracker
hand_tracker = Hand(backend='onnxruntime', device='cpu')

# Find et kamera der virker
cap = None
for i in range(3):
    test = cv2.VideoCapture(i, cv2.CAP_AVFOUNDATION)
    if test.isOpened():
        ret, frm = test.read()
        if ret:
            print(f"Bruger kamera index {i}")
            cap = test
            cap.set(cv2.CAP_PROP_FRAME_WIDTH, wVideo)
            cap.set(cv2.CAP_PROP_FRAME_HEIGHT, hVideo)
            break
        test.release()

if cap is None:
    print("Kunne ikke finde noget fungerende kamera")
    exit()

while True:
    ret, frame = cap.read(0)
    if not ret:
        print("Kunne ikke læse fra kameraet")
        break

    # Hænder
    kpts, scores = hand_tracker(frame)
    if len(kpts) > 0:
        hand = kpts[0]  # (21, 2) float

        # laver skelet
        frame = draw_skeleton(frame, kpts, scores)

        # Nyttige punkter
        ix, iy = hand[8].astype(int)   # index tip
        mx, my = hand[12].astype(int)  # middle tip
        tx, ty = hand[4].astype(int)   # thumb tip

        # Finger-up logik (alle 5)
        fingers = [0, 0, 0, 0, 0]
        # Tommel (simpel højre-hånd antagelse; men vi bruger egentlig distance til pinch, så denne er ikke kritisk)
        if hand[4][0] > hand[3][0]:
            fingers[0] = 1
        # Index/middle/ring/pinky: tip over PIP
        if hand[8][1]  < hand[6][1]:   fingers[1] = 1
        if hand[12][1] < hand[10][1]:  fingers[2] = 1
        if hand[16][1] < hand[14][1]:  fingers[3] = 1
        if hand[20][1] < hand[18][1]:  fingers[4] = 1

        # Dynamiske tærskler baseret på håndens størrelse
        xs, ys = hand[:,0], hand[:,1]
        diag = float(np.hypot(xs.max() - xs.min(), ys.max() - ys.min()))
        tap_thr   = max(15.0, 0.12 * diag)  # index+middle klik-tærskel
        pinch_thr = max(20.0, 0.18 * diag)  # thumb+index hold-tærskel

        # Tegn bevægelsesramme
        cv2.rectangle(frame, (frameR, frameR), (wVideo - frameR, hVideo - frameR), (255, 0, 255), 2)

        # Mus-flyt: KUN index oppe (ingen andre fingre)
        if fingers[1] == 1 and fingers[2] == 0 and fingers[3] == 0 and fingers[4] == 0:
            index_x = np.interp(ix, (frameR, wVideo - frameR), (0, wScr))
            index_y = np.interp(iy, (frameR, hVideo - frameR), (0, hScr))

            clocx = xloc + (index_x - xloc) / smoothing
            clocy = yloc + (index_y - yloc) / smoothing

            pyautogui.moveTo(wScr - clocx, clocy)
            cv2.circle(frame, (ix, iy), 15, (255, 0, 255), cv2.FILLED)
            xloc, yloc = clocx, clocy

        # ventser klik EN gang: index + middle tæt på hinanden
        tap_dist = float(np.linalg.norm(hand[8] - hand[12]))
        if (fingers[1] == 1 and fingers[2] == 1) and (tap_dist < tap_thr) and (not tap_clicked):
            pyautogui.click()
            tap_clicked = True

        # Reset når gestussen ikke længere er aktiv
        if not (fingers[1] == 1 and fingers[2] == 1 and tap_dist < tap_thr):
            tap_clicked = False
        
        # HOLD venstre klik: thumb + index pinch
        pinch_dist_left = float(np.linalg.norm(hand[4] - hand[8]))
        pinch_left = pinch_dist_left < pinch_thr
        if pinch_left and not left_hold_down:
            pyautogui.mouseDown()
            left_hold_down = True
        elif not pinch_left and left_hold_down:
            pyautogui.mouseUp()
            left_hold_down = False

        # højre klik: thumb + middle pinch
        pinch_dist_right = float(np.linalg.norm(hand[4] - hand[12]))
        pinch_right = pinch_dist_right < pinch_thr
        if pinch_right and not right_down:
            pyautogui.mouseDown(button='right')
            right_down = True
        elif not pinch_right and right_down:
            pyautogui.mouseUp(button='right')
            right_down = False
        
        # hold index, middle og ring finger nede for at rolle musen (scroll) (op og ned)
        if fingers[1] == 1 and fingers[2] == 1 and fingers[3] == 1:
            current_hand_y = float(hand[9][1])
            scroll_thr = max(8.0, 0.04 * diag)
            if scroll_anchor_y is None:
                scroll_anchor_y = current_hand_y
            delta = scroll_anchor_y - current_hand_y
            if abs(delta) > scroll_thr:
                pyautogui.scroll(1 if delta > 0 else -1)
                scroll_anchor_y = current_hand_y
        else:
            scroll_anchor_y = None

       
        # Debug overlay 
        cv2.putText(frame, f"tap:{tap_dist:.1f}/{tap_thr:.1f}  pinchL:{pinch_dist_left:.1f}/{pinch_thr:.1f}  pinchR:{pinch_dist_right:.1f}/{pinch_thr:.1f}",
        (20, 90), cv2.FONT_HERSHEY_PLAIN, 1.2, (0, 255, 0), 1)
        cv2.circle(frame, (tx, ty), 6, (0, 255, 0), cv2.FILLED)
        cv2.circle(frame, (ix, iy), 6, (0, 255, 0), cv2.FILLED)
        cv2.line(frame, (tx, ty), (ix, iy), (0, 255, 0), 2)
        

    # FPS
    ctime = time.time()
    fps = 1 / (ctime - ptime) if ctime != ptime else 0
    ptime = ctime
    cv2.putText(frame, str(int(fps)), (20, 50), cv2.FONT_HERSHEY_PLAIN, 3, (255, 0, 0), 3)

    # Vis vindue
    cv2.imshow("Virtual Mouse | Made by Benner and Willy", frame)
    if cv2.waitKey(1) & 0xFF == 27:  # ESC
        break

# Luk ned
if left_hold_down:
    try:
        pyautogui.mouseUp()
    except Exception:
        pass
if right_down:
    try:
        pyautogui.mouseUp(button='right')
    except Exception:
        pass
cap.release()
cv2.destroyAllWindows()