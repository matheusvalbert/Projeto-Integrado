import os
import sys
import io
import time
import _thread
import dropbox
import socket
import time
from PIL import Image
import cv2
import numpy as np
import glob
import shutil
import gspread
from oauth2client.service_account import ServiceAccountCredentials

def sendToDropbox(clientDropbox, sem):
    f = open("./sem" + str(sem) + ".jpg", 'rb')
    clientDropbox.files_upload(f.read(), "/sem" + str(sem) + ".jpg", mode=dropbox.files.WriteMode.overwrite)
    f.close()
    os.remove("./sem" + str(sem) + ".jpg")

def colocarTabela(sheet, tempoAberto, nCarros, sem):
    row = [tempoAberto, sem, nCarros]
    sheet.insert_row(row, 1)

def calculoTempo(historico, nCarros, sem):
    med = int((nCarros + historico[sem - 1])/2)
    historico[sem - 1] = nCarros
    tempo = med + 6
    return historico, tempo

def detectar(net, sem):
    numberCars = []
    images_path = glob.glob(r"./sem" + str(sem) + ".jpg")
    print('numero semaforo: ', sem)
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
                center_x = int(detection[0] * width)
                center_y = int(detection[1] * height)
                w = int(detection[2] * width)
                h = int(detection[3] * height)
                x = int(center_x - w / 2)
                y = int(center_y - h / 2)
                boxes.append([x, y, w, h])
                confidences.append(float(confidence))
                class_ids.append(class_id)

        indexes = cv2.dnn.NMSBoxes(boxes, confidences, 0.5, 0.4)
        numberCars = len(indexes)
        for i in range(len(boxes)):
            if i in indexes:
                x, y, w, h = boxes[i]
                cv2.rectangle(img, (x, y), (x + w, y + h), (255,0,0), 2)
        cv2.imwrite(img_path, img)

    return numberCars

def receberFotos(sock):

    client, addr = sock.accept()
    message = 'f'
    client.send(message.encode())

    time.sleep(1)

    res = ''
    resp = client.recv(100).decode()

    time.sleep(1)

    return client

def enviarTempo(client, tempoAberto):

    tempo = str(tempoAberto)
    resp = ''

    client.send(tempo.encode())

    time.sleep(1)

    resp = client.recv(100).decode()

    time.sleep(1)

    client.close()

def main():

    sem = 1

    historico = [0, 0]

    net = cv2.dnn.readNet("car.weights", "car.cfg")

    token = "AMByKjptTBsAAAAAAAAAAbxSTnRA7LChw4IAj_Kvp5UC1XoJdy6MZw0Nr4ehxaLe"; # Necessário criar conta no dropbox e gerar token de acesso
    clientDropbox = dropbox.Dropbox(token)

    scope = ['https://spreadsheets.google.com/feeds', 'https://www.googleapis.com/auth/drive']
    creds = ServiceAccountCredentials.from_json_keyfile_name('client_secret.json', scope)
    clientDrive = gspread.authorize(creds)
    sheet = clientDrive.open("Dados_Semaforos").sheet1

    sock = socket.socket()
    sock1 = socket.socket()

    sock.bind(('0.0.0.0', 80))
    sock.listen(0)

    sock1.bind(('0.0.0.0', 81))
    sock1.listen(0)

    while True:
        if sem == 1:
            client = receberFotos(sock)
        else:
            client1 = receberFotos(sock1)
        nCarros = detectar(net, sem)
        print('numero de carros: ', nCarros)
        historico, tempoAberto = calculoTempo(historico, 1, sem)
        print('tempo Aberto: ', tempoAberto)
        colocarTabela(sheet, tempoAberto, nCarros, sem)
        sendToDropbox(clientDropbox, sem)
        if sem == 1:
            enviarTempo(client, tempoAberto)
        else:
            enviarTempo(client1, tempoAberto)
        if sem == 1:
            sem = 2
        else:
            sem = 1

    sock.close()
    sock1.close()

main()
