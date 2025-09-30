import cv2

cap = cv2.VideoCapture(1, cv2.CAP_AVFOUNDATION)  
if not cap.isOpened():
    print("Kunne ikke åbne kameraet")
    exit()

while True:
    ret, frame = cap.read()
    if not ret:
        print("Kunne ikke læse fra kameraet")
        break

    cv2.imshow("Testkamera", frame)
    if cv2.waitKey(1) & 0xFF == 27:  # ESC
        break

cap.release()
cv2.destroyAllWindows()
