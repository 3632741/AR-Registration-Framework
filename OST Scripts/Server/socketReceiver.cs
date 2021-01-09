/*
 
    -----------------------
    UDP-Receive(send to)
    -----------------------
    // [url]http://msdn.microsoft.com/de-de/library/bb979228.aspx#ID0E3BAC[/url]


// > receive
// 127.0.0.1 : 8051

// send
// nc -u 127.0.0.1 8051

*/
using UnityEngine;
using System.Collections;
 
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Globalization;

public class socketReceiver : MonoBehaviour
{
    
    // receiving Thread
    Thread receiveThread;

    // udpclient object
    UdpClient client;

    // public
    // public string IP = "127.0.0.1"; default local
    [Header("Port used to connect to Kinect Device")]
    public int port; // define > init

    
    [Header("Kinect Tracked Position")]
    public float xKinectTracked = 0;
    public float yKinectTracked = 0;
    public float zKinectTracked = 0;
    // infos
    string lastReceivedUDPPacket = "";
    string allReceivedUDPPackets = ""; // clean up this from time to time!


    // start from shell
    private static void Main()
    {
        socketReceiver receiveObj = new socketReceiver();
        receiveObj.init();

        string text = "";
        do
        {
            text = Console.ReadLine();
        }
        while (!text.Equals("exit"));
    }
    // start from unity3d
    public void Start()
    {

        init();
    }

    // OnGUI
    void OnGUI()
    {/*
        Rect rectObj = new Rect(40, 10, 200, 400);
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        GUI.Box(rectObj, "# UDPReceive\n127.0.0.1 " + port + " #\n"
                    + "shell> nc -u 127.0.0.1 : " + port + " \n"
                    + "\nLast Packet: \n" + lastReceivedUDPPacket
                    + "\n\nAll Messages: \n" + allReceivedUDPPackets
                , style);
                */
    }

    // init
    private void init()
    {
        // Endpunkt definieren, von dem die Nachrichten gesendet werden.
        print("UDPSend.init()");

        // define port
        port = 8888;

        // status
        print("Sending to 127.0.0.1 : " + port);
        print("Test-Sending to this Port: nc -u 127.0.0.1  " + port + "");


        // ----------------------------
        // Abhören
        // ----------------------------
        // Lokalen Endpunkt definieren (wo Nachrichten empfangen werden).
        // Einen neuen Thread für den Empfang eingehender Nachrichten erstellen.
        receiveThread = new Thread(
            new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();

    }

    // receive thread
    private void ReceiveData()
    {

        client = new UdpClient(port);
        while (true)
        {

            try
            {
                // Bytes empfangen.
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);

                // Bytes mit der UTF8-Kodierung in das Textformat kodieren.
                string text = Encoding.UTF8.GetString(data);
                string[] coordinates = text.Split(' ');
                float x = float.Parse(coordinates[1], CultureInfo.InvariantCulture);
                float y = float.Parse(coordinates[3], CultureInfo.InvariantCulture);
                float z = float.Parse(coordinates[5], CultureInfo.InvariantCulture);
                xKinectTracked = -x;
                yKinectTracked = y;
                zKinectTracked = z;
                // Den abgerufenen Text anzeigen.
                print(">> " + text);

                // latest UDPpacket
                lastReceivedUDPPacket = text;

                // ....
                allReceivedUDPPackets = allReceivedUDPPackets + text;

            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    // getLatestUDPPacket
    // cleans up the rest
    public string getLatestUDPPacket()
    {
        allReceivedUDPPackets = "";
        return lastReceivedUDPPacket;
    }
}