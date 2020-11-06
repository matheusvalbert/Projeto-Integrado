import os
import sys
import io
import time
import _thread
import dropbox
import zipfile as ZipFile
import zipfile
from PIL import Image
import cv2
import numpy as np
import glob
import shutil
import gspread
from oauth2client.service_account import ServiceAccountCredentials

def colocarTabela(sheet, tempoAberto, nCarros, flag):
    maxDetected = max(nCarros)
    row = [tempoAberto, flag, maxDetected]
    sheet.insert_row(row, 1)

def calculoTempo(historico, nCarros, flag):
    maxCarros = max(nCarros)
    med = int((maxCarros + historico[flag - 1])/2)
    historico[flag - 1] = maxCarros
    tempo = med + 6
    return historico, tempo

def zipdir(path, ziph):
    for root, dirs, files in os.walk(path):
        for file in files:
            ziph.write(os.path.join(root, file), file)

def zip(client, n, flag):
    print("flag" + str(flag))
    os.remove("./Captures/Captures"+str(flag)+".zip")
    zipf = zipfile.ZipFile('Detected'+str(flag)+'.zip', 'w', zipfile.ZIP_DEFLATED)
    zipdir('./Captures'+str(flag)+'/', zipf)
    zipf.close()
    f = open("./Detected"+str(flag)+".zip", 'rb')
    client.files_upload(f.read(), "/Detected"+str(flag)+".zip", mode=dropbox.files.WriteMode.overwrite)
    f.close()
    os.remove('./Detected'+str(flag)+'.zip')

def detectar(net, flag):
    numberCars = []
    images_path = glob.glob(r"./Captures"+str(flag) + "/*.jpg")
    layer_names = net.getLayerNames()
    output_layers = [layer_names[i[0] - 1] for i in net.getUnconnectedOutLayers()]
    images_path = sorted(images_path)
    for img_path in images_path:
        img = cv2.imread(img_path)
        img = cv2.resize(img, None, fx=0.4, fy=0.4)
        height, width, channels = img.shape

        # Detecta os objetos
        blob = cv2.dnn.blobFromImage(img, 0.00392, (416, 416), (0, 0, 0), True, crop=False)

        net.setInput(blob)
        outs = net.forward(output_layers)

        # Mostra as informações na tela
        class_ids = []
        confidences = []
        boxes = []
        for out in outs:
            for detection in out:
                scores = detection[5:]
                class_id = np.argmax(scores)
                confidence = scores[class_id]
               # print("espaco 0 " + str(detection[0] * width))
               # print("espaco 1 " +str(detection[1] * height))
               # print("espaco 2 " +str(detection[2] * width))
               # print("espaco 3 " +str(detection[3] * height))
                center_x = int(detection[0] * width)
                center_y = int(detection[1] * height)
                w = int(detection[2] * width)
                if detection[3] == float("inf") or detection[3] == float("-inf"):
                    print("Infinito")
                    h = 10
                else:
                    h = int(detection[3] * height)
                x = int(center_x - w / 2)
                y = int(center_y - h / 2)
                boxes.append([x, y, w, h])
                confidences.append(float(confidence))
                class_ids.append(class_id)

        indexes = cv2.dnn.NMSBoxes(boxes, confidences, 0.5, 0.4)
        numberCars.append(len(indexes))
        for i in range(len(boxes)):
            if i in indexes:
                x, y, w, h = boxes[i]
                cv2.rectangle(img, (x, y), (x + w, y + h), (255,0,0), 2)
        cv2.imwrite(img_path, img)

    return numberCars


def unzip(client, flag):

    with zipfile.ZipFile("./Captures/Captures" + str(flag) + ".zip", 'r') as zip_ref:
        fileNames = zip_ref.namelist()
        size = len(fileNames)
        for numerocam in range(size):
            data = zip_ref.read(fileNames[numerocam])
            image = Image.open(io.BytesIO(data))
            path = 'Captures'+str(flag) +'/img-' + str(numerocam + 1)+'.jpg'
            image.save(path)

def getImagesFromDropbox(client, flag):

    if os.path.isdir("./Captures") == False:
        os.mkdir("./Captures")
    while True:
        if flag == 1:
            try:
                f = client.files_download_to_file("./Captures/Captures1.zip", "/Captures1.zip")        
                #with open("./Captures/Captures.zip", "wb") as file:
                #    file.write(f.content)
                client.files_delete("/Captures1.zip")
                time.sleep(1)
                print("Recebido arquivo da flag 1")
                break
            except:
                time.sleep(1)
        else:
            try:
                f = client.files_download_to_file("./Captures/Captures2.zip", "/Captures2.zip")        
                #with open("./Captures/Captures.zip", "wb") as file:
                #    file.write(f.content)
                client.files_delete("/Captures2.zip")
                time.sleep(1)
                print("Recebido arquivo da flag 2")
                break
            except:
                time.sleep(1)

def threadInit(client, net, sheet, flag, lock):
    historico = [0, 0]
    while True:
        getImagesFromDropbox(client, flag)
        unzip(client, flag)
        lock.acquire();
        nCarros = detectar(net, flag)
        lock.release();
        print(nCarros)
        historico, tempoAberto = calculoTempo(historico, nCarros, flag)
        colocarTabela(sheet, tempoAberto, nCarros, flag)
        zip(client, len(nCarros), flag)
        nCarros.clear()
        
        
def main():

    net = cv2.dnn.readNet("car.weights", "car.cfg")
    
    token = ""; # Necessário criar conta no dropbox e gerar token de acesso
    client = dropbox.Dropbox(token)
    lock = _thread.allocate_lock()
    scope = ['https://spreadsheets.google.com/feeds', 'https://www.googleapis.com/auth/drive']
    creds = ServiceAccountCredentials.from_json_keyfile_name('client_secret.json', scope)
    clientDrive = gspread.authorize(creds)
    sheet = clientDrive.open("Dados_Semaforos").sheet1
    try:
            _thread.start_new_thread( threadInit,(client, net, sheet, 1, lock))
            _thread.start_new_thread( threadInit,(client, net, sheet, 2, lock))
    except:
            print("Error: unable to start thread")

    while True:
        pass

main()
