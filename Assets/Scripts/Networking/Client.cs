using Magnethands.Game;
using Magnethands.Menus;
using Magnethands.Menus.PlayerView;
using Magnethands.Networking.Operations;
using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Magnethands.Networking
{
    public class Client : MonoBehaviour
    {
        public static Client Instance { private set; get; }
        private const int MAX_USER = 8;
        private const int PORT = 42000;
        private const int BROADCAST_PORT = 42001;
        private const int BYTE_SIZE = 1024;
        private const int KEY = 0;
        private const int VERSION = 1;
        private const int SUB_VERSION = 1;
        public string ServerIp { set; get; }
        private byte reliableChannel;
        private byte unreliableChannel;
        private int connectionId;
        private int hostId;
        private bool isStarted = false;
        private bool isConnected = false;

#pragma warning disable CS0618
        private HostTopology topology;
#pragma warning restore CS0618 
        
        #region Monobehaviour
        private void Start()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Update()
        {
            CheckIncomingMessage();
        }

        #endregion

        // Client begins listening as soon as game is started
        public void Initialize()
        {
#pragma warning disable CS0618
            NetworkTransport.Init();
            ConnectionConfig cc = new ConnectionConfig();
            reliableChannel = cc.AddChannel(QosType.ReliableSequenced);
            unreliableChannel = cc.AddChannel(QosType.Unreliable);
            topology = new HostTopology(cc, MAX_USER);
            hostId = NetworkTransport.AddHost(topology, BROADCAST_PORT);
            if (hostId == -1)
            {
                Debug.LogError("Broadcast Listening failed");
                hostId = NetworkTransport.AddHost(topology, 0);
                if (hostId == -1)
                    Debug.LogError("That still didn't work");
            }

            NetworkTransport.SetBroadcastCredentials(
                hostId,
                KEY,
                VERSION,
                SUB_VERSION,
                out byte error);
            if ((NetworkError)error != NetworkError.Ok)
            {
                Debug.LogError($"Network Error: {(NetworkError)error}");
                return;
            }
            isStarted = true;
        }

        public void AttemptConnection()
        {
            
            // Client only code
            // If localhost is entered or own IP address, change port
            // and IP accordingly
            if (ServerIp == "127.0.0.1"
                || ServerIp == Dns.GetHostEntry(Dns.GetHostName())
                    .AddressList[0].ToString())
            {
                Debug.Log("Testing on localhost");
                hostId = NetworkTransport.AddHost(topology, 0);
                ServerIp = "127.0.0.1";
            }
            else
            {
                hostId = NetworkTransport.AddHost(topology, PORT);
            }
            connectionId = NetworkTransport.Connect(
                hostId,
                ServerIp,
                PORT,
                0,
                out byte error);
            if ((NetworkError)error != NetworkError.Ok)
            {
                Debug.LogError($"Network Error: {(NetworkError)error}");
                return;
            }
            isStarted = true;
            isConnected = true;
            Debug.Log($"Connected to {ServerIp}");
            // Will only change scene iff connection is successful
            SceneManager.LoadScene("Scenes/PlayerView");
        }

        public void ShutDown()
        {
            isStarted = false;
            NetworkTransport.Shutdown();
        }

        public void CheckIncomingMessage()
        {
            if (!isStarted)
                return;

            int recievingHostId;
            int connectionId;
            int channelId;

            byte[] recievedBuffer = new byte[BYTE_SIZE];
            int dataSize;
            byte error;

            NetworkEventType eventType = NetworkTransport.Receive(
                out recievingHostId,
                out connectionId,
                out channelId,
                recievedBuffer,
                BYTE_SIZE,
                out dataSize,
                out error);
            if ((NetworkError)error != NetworkError.Ok)
                Debug.LogError($"Network Error: {(NetworkError)error}");

            // Appropriately handle the network event
            switch (eventType)
            {
                case NetworkEventType.Nothing:
                    break;
                case NetworkEventType.ConnectEvent:
                    Debug.Log($"User={connectionId}, Host={hostId}");
                    break;
                case NetworkEventType.DataEvent:
                    Debug.Log("Data recieved");
                    BinaryFormatter formatter = new BinaryFormatter();
                    MemoryStream ms = new MemoryStream(recievedBuffer);
                    NetMsg msg = (NetMsg)formatter.Deserialize(ms);
                    OnData(connectionId, channelId, recievingHostId, msg);
                    break;
                case NetworkEventType.DisconnectEvent:
                    Debug.Log($"User {connectionId} has disconnected");
                    break;
                default:
                case NetworkEventType.BroadcastEvent:
                    ReadBroadcast();
                    break;
            }
        }

        private void ReadBroadcast()
        {
            if (isConnected)
                return;
            byte[] buffer = new byte[BYTE_SIZE];
            NetworkTransport.GetBroadcastConnectionMessage(
                hostId,
                buffer,
                BYTE_SIZE,
                out int recievedSize,
                out byte broadcastError);
            if ((NetworkError)broadcastError != NetworkError.Ok)
                Debug.LogError($"Network Error: {(NetworkError)broadcastError}");
            NetworkTransport.GetBroadcastConnectionInfo(
                hostId,
                out string senderAddr,
                out int port,
                out byte infoError);
            if ((NetworkError)infoError != NetworkError.Ok)
                Debug.LogError($"Network Error: {(NetworkError)infoError}");
            // Do things
            Debug.Log($"Got broadcast from {senderAddr}");
            
            // Deserialize server name
            var chars = new char[recievedSize * sizeof(char)];
            System.Buffer.BlockCopy(buffer, 0, chars, 0, recievedSize);
            string data = new string(chars);
            LANConnectionInfo info = new LANConnectionInfo(senderAddr, data);
            GameObject.Find("/Canvases").GetComponent<LANMenu>().AddToList(info);
            

        }
#pragma warning restore CS0618

        private void OnData(int connId, int channelId, int recHostId, NetMsg msg)
        {
            switch (msg.Code)
            {
                case (byte)Operation.Scene:
                    Debug.Log("Switching to new scene");
                    ChangeScene(msg);
                    break;
                case (byte)Operation.Power:
                    Debug.Log("Added power");
                    AddPower(msg);
                    break;
                case (byte)Operation.Art:
                    Debug.Log("Art recieved");
                    throw new NotImplementedException();
                case (byte)Operation.PowersIn:
                    Debug.Log("Starting Game");
                    StartGame();
                    break;
                case (byte)Operation.None:
                default:
                    break;
            }
        }

        private void StartGame()
        {
            if ("PlayerView" != SceneManager.GetActiveScene().name)
                return;

            var canvasParent = GameObject.Find("/Canvases")
                .GetComponent<CanvasSwitcher>();
            canvasParent.SwitchCanvas(canvasParent[1]);
            Player.Instance.SetPowers();
        }

        private void AddPower(NetMsg msg)
        {
            string power = ((NetPower)msg).Suggestion;
            Player.Instance.AddPower(power);
        }

        private void ChangeScene(NetMsg msg)
        {
            SceneManager.LoadScene(((NetChangeScene)msg).Scene);
        }

#pragma warning disable CS0618
        private void SendToServer(NetMsg msg)
        {
            byte[] buffer = new byte[BYTE_SIZE];
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(buffer);
            formatter.Serialize(ms, msg);
            NetworkTransport.Send(hostId,
                connectionId,
                reliableChannel,
                buffer,
                BYTE_SIZE,
                out byte error);
            if ((NetworkError)error != NetworkError.Ok)
                Debug.LogError($"Network Error: {(NetworkError)error}");
        }
#pragma warning restore CS0618

        public void SuggestPower(string power)
        {
            NetPower np = new NetPower { Suggestion = power };
            SendToServer(np);
        }
    }
}