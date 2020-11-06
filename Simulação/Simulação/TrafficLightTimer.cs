using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System;
using System.IO;
using System.IO.Compression;

using Dropbox.Api;
using Dropbox.Api.Files;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Services;
using Google.Apis.Sheets.v4.Data;

public class TrafficLightTimer : MonoBehaviour
{
    public int groupNumber;
    public Light TL1_light;
    public Light TL2_light;
    public Light TL3_light;
    public GameObject TL1_block;
    public GameObject TL2_block;
    public GameObject TL3_block;
    public GameObject TL1_Camera;
    public GameObject TL2_Camera;
    public GameObject TL3_Camera;
    private Save_Screen TL1_Script;
    private Save_Screen TL2_Script;
    private Save_Screen TL3_Script;

    private Thread receiveThread;
    private Thread sendThread;
    private int TL1Stage = 1;
    private int TL2Stage = 0;
    private int TL3Stage = 1;
	public int fps;

    private double groupTIme = 10;
    private double timer = 10;
    private bool photoUpdated = false; 

    private string token = ""; // Necessário criar conta no dropbox e gerar token de acesso

    private static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
    private static readonly string SpreadsheetId = "1KPY4pYHvqjLIzbpnNU17qQbOVI6-IgZ_5vb_x042yB8";
    private static SheetsService service;

    void Start (){  
        using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
        {
            var serviceInitializer = new BaseClientService.Initializer
            {
                HttpClientInitializer = GoogleCredential.FromStream(stream).CreateScoped(Scopes)
            };
            service = new SheetsService(serviceInitializer);
        }
		Application.targetFrameRate = fps;
        Init();
    }

    private void Init(){
        print ("Simulation Initialized");

        sendThread = new Thread (new ThreadStart(sendImages));
        sendThread.IsBackground = true;
        sendThread.Start();

        receiveThread = new Thread (new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void sendImages(){
        int nFiles;

        if(!Directory.Exists("./Conjunto"+groupNumber)){
            Directory.CreateDirectory("./Conjunto"+groupNumber);
        }

        nFiles = Directory.GetFiles("./Conjunto"+groupNumber).Length;

        if( (nFiles == 3 && groupNumber == 1) || (nFiles == 2 && groupNumber == 2) ){
            if(File.Exists("./Captures"+groupNumber+".zip")){
                deleteFile("./Captures"+groupNumber+".zip");
            }
            ZipFile.CreateFromDirectory("./Conjunto"+groupNumber, "./Captures"+groupNumber+".zip");
        
            foreach (String file in Directory.GetFiles("./Conjunto"+groupNumber)){
                File.Delete(file);
            }

            using (var client = new DropboxClient(token)){
                var mStream = new MemoryStream(File.ReadAllBytes("./Captures"+groupNumber+".zip"));
                var updated = client.Files.UploadAsync("/Captures"+groupNumber+".zip", WriteMode.Overwrite.Instance, body: mStream);
                updated.Wait();
                Debug.Log("Conjunto"+groupNumber+": Successful Upload\n");
            }
        }
    }

    private void deleteFile(String path){
        if(File.Exists(path)){
            File.Delete(path);
        }
        return;
    }

    private void takePhoto() {
        if((timer - 3) <= 0 && !photoUpdated) {
            TL1_Camera.GetComponent<Save_Screen>().takePhoto();
            TL2_Camera.GetComponent<Save_Screen>().takePhoto();
            if (groupNumber == 1) {
                TL3_Camera.GetComponent<Save_Screen>().takePhoto();
            }
            sendImages();
            photoUpdated = true;
        }
    }

    private void ReceiveData(){
        SpreadsheetsResource.ValuesResource.GetRequest request;
        IList<IList<object>> values;
        ValueRange response;
        int lastCount = 0;
        int count;

        while(true) {

            request = service.Spreadsheets.Values.Get(SpreadsheetId, "Semaforos!A:C");
            response = request.Execute();
            
            if(response.Values != null && response.Values[0].Count != 0){
                count = Convert.ToInt16(response.Values.Count);
                if ( count != lastCount ){
                    values = response.Values;
                    if (Convert.ToString(values[0][1]) == Convert.ToString(groupNumber)){
                        groupTIme = Convert.ToDouble(values[0][0]);
                        print("groupTime"+groupNumber+": "+groupTIme);
                    }
                    lastCount = count;
                }
            }

            Thread.Sleep(3000);
        }
    }

    private void changeStage() {
        if (timer <= 0) {
            TL1Stage = (TL1Stage == 2) ? 0 : 1;
            TL2Stage = (TL2Stage == 2) ? 0 : 1;
            if(groupNumber == 1){
                TL3Stage = (TL3Stage == 2) ? 0 : 1;
            }
            timer = groupTIme;
            photoUpdated = false;
        }
        else if(timer <= 3 && timer > 0){
            TL1Stage = (TL1Stage == 1) ? 2 : TL1Stage;
            TL2Stage = (TL2Stage == 1) ? 2 : TL2Stage;
            if(groupNumber == 1){
                TL3Stage = (TL3Stage == 1) ? 2 : TL3Stage;
            }
        }
    }

    private void Update(){
        timer = timer - Time.deltaTime;
        takePhoto();
        setGreen();
        setYellow();
        setRed();
        changeStage();
    }

    private void setGreen(){

        if(TL1Stage == 1){
            TL1_light.color = UnityEngine.Color.green;
            TL1_block.SetActive(false);
        }
        if(TL2Stage == 1){
            TL2_light.color = UnityEngine.Color.green;
            TL2_block.SetActive(false);
        }
        if(groupNumber == 1){
            if(TL3Stage == 1){
                TL3_light.color = UnityEngine.Color.green;
                TL3_block.SetActive(false);
            }
        }
    }

    private void setYellow(){

        if(TL1Stage == 2){
            TL1_light.color = UnityEngine.Color.yellow;
            TL1_block.SetActive(false);
        }
        if(TL2Stage == 2){
            TL2_light.color = UnityEngine.Color.yellow;
            TL2_block.SetActive(false);
        }
        if(groupNumber == 1){
            if(TL3Stage == 2){
                TL3_light.color = UnityEngine.Color.yellow;
                TL3_block.SetActive(false);
            }
        }
    }

    private void setRed(){

        if(TL1Stage == 0){
            TL1_light.color = UnityEngine.Color.red;
            TL1_block.SetActive(true);
        }
        if(TL2Stage == 0){
            TL2_light.color = UnityEngine.Color.red;
            TL2_block.SetActive(true);
        }
        if(groupNumber == 1){
            if(TL3Stage == 0){
                TL3_light.color = UnityEngine.Color.red;
                TL3_block.SetActive(true);
            }
        }
    }
}
