using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System;
using System.Text;


public class TCPConnection : MonoBehaviour
{

    public TcpClient tcpClient;
    public Thread connectionThread;

    public TopPlatform tpScript;

    public bool stopThread = false;

    public float[] legLengthsArray = new float[6];
    public GameObject topPlatformObj;

    // Start is called before the first frame update
    void Start()
    {
        connectionThread = new Thread(ConnectionHandler);
        connectionThread.Start();

    }

    void ConnectionHandler(){
        //connect client to server
        tcpClient = new TcpClient();
        tcpClient.Connect(IPAddress.Parse("127.0.0.1"),4844);

        Byte[] buffer = new Byte[1024];

        try
        {
            NetworkStream stream = tcpClient.GetStream();
            Debug.Log("[UnityVisualSim] client is connected!");
            Debug.Log("[UnityVisualSim] Waiting for data...");
            
            //(thread)blocking while loop which checks for data in stream
            int len;
            while ((len = stream.Read(buffer,0,buffer.Length)) > 0 && !stopThread)
            {
                Debug.Log("[UnityVisualSim] Data received! len=" + len);
                //store incomming data inside byte arr
                var incommingData = new Byte[len];
                Array.Copy(buffer, 0,incommingData,0,len);
                string incommingDataString = Encoding.UTF8.GetString(incommingData);

                StewartplatformData dataInJson = JsonUtility.FromJson<StewartplatformData>(incommingDataString);
                
                string legsData = string.Join(", ", dataInJson.legs);
                string result = $@"
                {{
                    ""orientation"": {{
                        ""yaw"": {dataInJson.orientation.yaw},
                        ""roll"": {dataInJson.orientation.roll},
                        ""pitch"": {dataInJson.orientation.pitch}
                    }},
                    ""legs"": [{legsData}]
                }}";
                Debug.Log("[UnityVisualSim] Received data: \n" + result);
                // test om te kijken of legs wel floats worden

                for (int i = 0; i < dataInJson.legs.Length; i++)
                {
                    Debug.Log($"Leg {i + 1}: {dataInJson.legs[i]}");
                }
                legLengthsArray[5] = dataInJson.legs[0] /100;       
                legLengthsArray[0] = dataInJson.legs[1] / 100;
                legLengthsArray[1] = dataInJson.legs[2] / 100;
                legLengthsArray[2] = dataInJson.legs[3]/ 100;
                legLengthsArray[3] = dataInJson.legs[4]/ 100;
                legLengthsArray[4] = dataInJson.legs[5]/ 100;

                // !!! check here values and call change color if needed of leg
                for (int i = 0; i < legLengthsArray.Length; i++)
                {
                    Debug.Log((Mathf.Abs(legLengthsArray[i])*100)-988);
                    if((Mathf.Abs(legLengthsArray[i])*100)-988 > 200|| (Mathf.Abs(legLengthsArray[i])*100)-988< -200)   {
                        topPlatformObj.GetComponent<TopPlatform>().MakeLegRed(i);
                        continue;
                    }else{
                        topPlatformObj.GetComponent<TopPlatform>().MakeLegNormal(i);
                    }
                }
                topPlatformObj.GetComponent<TopPlatform>().adjustLegs();
            }
            
            Debug.Log("[UnityVisualSim] Stopped client, now turning off!");
            stream.Close();
            tcpClient.Close();
             Debug.Log("[UnityVisualSim] Everthing closed!");
        }
        catch (System.Exception ex)
        {
            Debug.Log("[UnityVisualSim] No connection!");
            tcpClient.Close();
            Debug.Log("[UnityVisualSim] " + ex.ToString());
            throw;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

[System.Serializable]
public class Orientation
{
    public float yaw;
    public float roll;
    public float pitch;
}

[System.Serializable]
public class StewartplatformData
{
    public Orientation orientation;
    public float[] legs;
}
