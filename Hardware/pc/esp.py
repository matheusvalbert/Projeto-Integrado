from ftplib import FTP_TLS
import socket
from random import seed
from random import randint
import shutil
import os
import time

def tempoSemaforo(client):
    resp = ''
    while resp == '':
        print('esperando receber tempo')
        resp = client.recv(100).decode()
        time.sleep(0.5)
    print('Resposta '+resp)
    print('Verde')
    time.sleep(int(resp))
    print('Amarelo')
    time.sleep(4)
    print('Vermelho')
    charac = 't'
    client.send(charac.encode())

def confirmarEnvio(client):
    charac = 'e'
    client.send(charac.encode())
    print('Enviado char')
    time.sleep(0.1)
    tempoSemaforo(client)

def enviarImagem(ftp, client):
    num = randint(1, 4)
    fileName = 'image'+str(num)+'.jpg'
    print('Enviada imagem '+fileName)
    fileName2 = 'sem2.jpg'
    shutil.copyfile(fileName,fileName2)
    file = open(fileName2,'rb')
    ftp.storbinary('STOR '+fileName2, file)
    file.close()
    os.remove('sem2.jpg')
    time.sleep(0.1)
    confirmarEnvio(client)
    
def conexaoSocket(ftp):
    while True:
        client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        client.connect(('18.231.4.103', 81))
        while True:
            resp = client.recv(100).decode()
            if(resp == 'f'):
                print('f eh a resposta')
                time.sleep(0.1)
                enviarImagem(ftp, client)
                client.close()
                break

def main():
    ftp = FTP_TLS('18.231.4.103')
    ftp.sendcmd('USER valbert')
    ftp.sendcmd('PASS 12345678')
    print(ftp.getwelcome())
    print(ftp.cwd('fotos'))
    ftp.set_pasv(False)
    print('conectado')
    conexaoSocket(ftp)
    ftp.sendcmd('QUIT')

seed(1)
main()
