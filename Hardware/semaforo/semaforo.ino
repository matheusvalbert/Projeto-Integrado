#include <WiFi.h>
#include <WiFiClient.h>   
#include "esp_camera.h"
#include "soc/soc.h"
#include "soc/rtc_cntl_reg.h"
#include "driver/rtc_io.h"
#include "ESP32_FTPClient.h"

const char* WIFI_SSID = "2G";
const char* WIFI_PASS = "Valbert12345678?";

const uint16_t port = 80;
const char * host = "18.231.4.103";

char* ftp_server = "18.231.4.103";
char* ftp_user = "valbert";
char* ftp_pass = "12345678";
char* ftp_path = "/fotos/";

ESP32_FTPClient ftp (ftp_server,ftp_user,ftp_pass, 5000, 2);

#define PWDN_GPIO_NUM     32
#define RESET_GPIO_NUM    -1
#define XCLK_GPIO_NUM      0
#define SIOD_GPIO_NUM     26
#define SIOC_GPIO_NUM     27

#define Y9_GPIO_NUM       35
#define Y8_GPIO_NUM       34
#define Y7_GPIO_NUM       39
#define Y6_GPIO_NUM       36
#define Y5_GPIO_NUM       21
#define Y4_GPIO_NUM       19
#define Y3_GPIO_NUM       18
#define Y2_GPIO_NUM        5
#define VSYNC_GPIO_NUM    25
#define HREF_GPIO_NUM     23
#define PCLK_GPIO_NUM     22

#define LED_VERMELHO      15
#define LED_AMARELO       14
#define LED_VERDE          2

camera_config_t config;

void setup() {

  pinMode(LED_VERMELHO,OUTPUT);
  pinMode(LED_AMARELO,OUTPUT);
  pinMode(LED_VERDE,OUTPUT);

  digitalWrite(LED_VERMELHO,HIGH);
  digitalWrite(LED_AMARELO,LOW);
  digitalWrite(LED_VERDE,LOW);

  WRITE_PERI_REG(RTC_CNTL_BROWN_OUT_REG, 0);

  Serial.begin(115200);

  WiFi.begin(WIFI_SSID, WIFI_PASS);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.println("...");
  }

  Serial.print("WiFi connected with IP: ");
  Serial.println(WiFi.localIP());

  initCamera();

  ftp.OpenConnection();
}

void loop() {

  WiFiClient client;

  if (client.connect(host, port)) {

    if(aguardarEnvioFoto(client) == true) {
      delay(100);
      enviarFoto();
      delay(100);
      confirmarEnvio(client);
      delay(100);
      tempoAberto(client);
      delay(100);
      fimFechamento(client);
      delay(100);
    }
  }
}

void confirmarEnvio(WiFiClient client) {

  if (client) {

    while (client.connected()) {

        client.write('e');
        Serial.println("Enviado");
        return;
    }
  }
}

void fimFechamento(WiFiClient client) {

  if (client) {

    while (client.connected()) {

        client.write('t');
        Serial.println("Fechado");
        return;
    }
  }
}

void tempoAberto(WiFiClient client) {

  String tempo = "";

  if (client) {

    while (client.connected()) {

      while (client.available()>0) {

          char num = client.read();
          tempo += num;
      }
       if(!tempo.equals("")) {
        
        Serial.println(tempo);
        int tempoInt = tempo.toInt();

        digitalWrite(LED_VERMELHO,LOW);
        digitalWrite(LED_AMARELO,LOW);
        digitalWrite(LED_VERDE,HIGH);
        
        delay(tempoInt*1000);

        digitalWrite(LED_VERMELHO,LOW);
        digitalWrite(LED_AMARELO,HIGH);
        digitalWrite(LED_VERDE,LOW);

        delay(4000);

        digitalWrite(LED_VERMELHO,HIGH);
        digitalWrite(LED_AMARELO,LOW);
        digitalWrite(LED_VERDE,LOW);
        
        return;
       }
    }
  }
}

bool aguardarEnvioFoto(WiFiClient client) {

  char enviarFoto;

   if (client) {

    while (client.connected()) {

      while (client.available()>0) {

        enviarFoto = client.read();
        if(enviarFoto == 'f') {
          
          Serial.println("enviando Foto");
          return true;
        }
      }
    }
  }

  return false;
}

void initCamera() {
  
  config.ledc_channel = LEDC_CHANNEL_0;
  config.ledc_timer = LEDC_TIMER_0;
  config.pin_d0 = Y2_GPIO_NUM;
  config.pin_d1 = Y3_GPIO_NUM;
  config.pin_d2 = Y4_GPIO_NUM;
  config.pin_d3 = Y5_GPIO_NUM;
  config.pin_d4 = Y6_GPIO_NUM;
  config.pin_d5 = Y7_GPIO_NUM;
  config.pin_d6 = Y8_GPIO_NUM;
  config.pin_d7 = Y9_GPIO_NUM;
  config.pin_xclk = XCLK_GPIO_NUM;
  config.pin_pclk = PCLK_GPIO_NUM;
  config.pin_vsync = VSYNC_GPIO_NUM;
  config.pin_href = HREF_GPIO_NUM;
  config.pin_sscb_sda = SIOD_GPIO_NUM;
  config.pin_sscb_scl = SIOC_GPIO_NUM;
  config.pin_pwdn = PWDN_GPIO_NUM;
  config.pin_reset = RESET_GPIO_NUM;
  config.xclk_freq_hz = 20000000;
  config.pixel_format = PIXFORMAT_JPEG; 
  
  if(psramFound()) {
    
    config.frame_size = FRAMESIZE_UXGA;//FRAMESIZE_UXGA; // FRAMESIZE_ + QVGA|CIF|VGA|SVGA|XGA|SXGA|UXGA
    config.jpeg_quality = 10;
    config.fb_count = 2;
  }
  else {
    
    config.frame_size = FRAMESIZE_UXGA;
    config.jpeg_quality = 12;
    config.fb_count = 1;
  }
  
  esp_err_t err = esp_camera_init(&config);
  if (err != ESP_OK) {
    
    Serial.printf("Camera init failed with error 0x%x", err);
    return;
  }
}

void enviarFoto() {
      
  camera_fb_t * fb = NULL;
  
  fb = esp_camera_fb_get();  
  if(!fb) {
    
    Serial.println("Camera capture failed");
    return;
  }

  ftp.ChangeWorkDir(ftp_path);
  ftp.InitFile("Type I");

  String nombreArchivo = "sem1.jpg";
  Serial.println("Subiendo "+nombreArchivo);
  int str_len = nombreArchivo.length() + 1; 
 
  char char_array[str_len];
  nombreArchivo.toCharArray(char_array, str_len);
  
  ftp.NewFile(char_array);
  ftp.WriteData( fb->buf, fb->len );
  ftp.CloseFile();

  esp_camera_fb_return(fb); 
}
