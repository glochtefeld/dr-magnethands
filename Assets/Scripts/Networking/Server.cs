using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;
using Magnethands.Menus.GM;
using Magnethands.Networking.Operations;
using System.Net;
using System.Net.Sockets;
using Magnethands.Game;
using Magnethands.Menus;

namespace Magnethands.Networking
{
    public class Server : MonoBehaviour
    {
        public static Server Instance { private set; get; }
        private const int MAX_USER = 8;
        private const int PORT = 42000;
        private const int BROADCAST_PORT = 42001;
        private const int BYTE_SIZE = 1024;
        private const int KEY = 0;
        private const int VERSION = 1;
        private const int SUB_VERSION = 1;
        private string broadcastData = "SERVER";
        private byte[] msgBuffer;
        private byte reliableChannel;
        private byte unreliableChannel;
        private int hostId;
        private bool isStarted = false;
        private bool isBroadcasting = false;
        public PowerDispersion ps = new PowerDispersion();

        private int currentConnections = 0;
        public int CurrentPowers { set; get; }
        private bool playing;


        #region Monobehaviour 
        private void Start()
        {
            msgBuffer = new byte[broadcastData.Length * sizeof(char)];
            System.Buffer.BlockCopy(
                broadcastData.ToCharArray(),
                0,
                msgBuffer,
                0,
                msgBuffer.Length);

            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Update()
        {
            CheckIfBroadcasting();
            CheckIncomingMessage();
            CheckIfEveryoneIsReady();
        }

        private void CheckIfEveryoneIsReady()
        {
            if (!isStarted || playing)
                return;
            Debug.Log($"CurrentConnections: {currentConnections} " +
                $"CurrentPowers: {CurrentPowers} " +
                $"Target: {(currentConnections *8) + 8}");

            if (currentConnections > 0
                && CurrentPowers >= ((currentConnections * 8) + 8))
                AssignPowersAndStartGame();
        }
        #endregion

#pragma warning disable CS0618

        #region Broadcasting
        private void CheckIfBroadcasting()
        {
            if (NewConnections.AllowConnections && !isBroadcasting)
                Broadcast();
            else if (!NewConnections.AllowConnections && isBroadcasting)
                StopBroadcast();
        }
        private void Broadcast()
        {
            if (!isStarted)
                return;
            isBroadcasting = true;
            NetworkTransport.StartBroadcastDiscovery(
                hostId,
                BROADCAST_PORT,
                KEY,
                VERSION,
                SUB_VERSION,
                msgBuffer,
                msgBuffer.Length,
                1000,
                out byte error);
            if ((NetworkError)error != NetworkError.Ok)
                Debug.LogError($"Network Error: {(NetworkError)error}");
        }

        private void StopBroadcast()
        {
            if (!isStarted)
                return;
            isBroadcasting = false;
            NetworkTransport.StopBroadcastDiscovery();
        }
        #endregion

        public void Initialize()
        {
            NetworkTransport.Init();
            ConnectionConfig cc = new ConnectionConfig();
            reliableChannel = cc.AddChannel(QosType.ReliableSequenced);
            unreliableChannel = cc.AddChannel(QosType.Unreliable);
            HostTopology topology = new HostTopology(cc, MAX_USER);

            // Server exclusive code
            hostId = NetworkTransport.AddHost(topology, PORT);
            isStarted = true;
            Debug.Log($"Started server on port {PORT}");
        }

        public void ShutDown()
        {
            isStarted = false;
            playing = false;
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
                    AddUserToList(connectionId);
                    currentConnections++;
                    break;
                case NetworkEventType.DataEvent:
                    Debug.Log("Data recieved");
                    BinaryFormatter formatter = new BinaryFormatter();
                    MemoryStream ms = new MemoryStream(recievedBuffer);
                    NetMsg msg = (NetMsg)formatter.Deserialize(ms);
                    OnData(connectionId, msg);
                    break;
                case NetworkEventType.DisconnectEvent:
                    Debug.Log($"User {connectionId} has disconnected");
                    RemoveUserFromList(connectionId);
                    currentConnections--;
                    break;
                default:
                case NetworkEventType.BroadcastEvent:
                    Debug.Log("Unexpected network event type");
                    break;
            }
        }

        private void AddUserToList(int connectionId)
        {
            var userList = GameObject.Find("/Canvases")
                .GetComponent<ConnectionMenu>();
            userList.AddUser(connectionId);
        }
        
        private void RemoveUserFromList(int connectionId)
        {
            var userList = GameObject.Find("/Canvases")
                .GetComponent<ConnectionMenu>();
            userList.RemoveUser(connectionId);
        }
#pragma warning restore CS0618

        private void OnData(int connectionId, NetMsg msg)
        {
            switch (msg.Code)
            {
                case (byte)Operation.Power:
                    Debug.Log("Adding power to pool");
                    AddPowerToPool(connectionId, msg);
                    break;
            }
        }

        private void AddPowerToPool(int connId, NetMsg msg)
        {
            CurrentPowers++;
            NetPower np = (NetPower)msg;
            ps.AddPowerToPool(np.Suggestion);
            GameObject.Find("/Canvases").GetComponent<ConnectionMenu>()
                .IncrementUserSuggestions(connId);
        }

#pragma warning disable CS0618
        private void SendToClient(int recHostId, int connId, NetMsg msg)
        {
            byte[] buffer = new byte[BYTE_SIZE];
            var formatter = new BinaryFormatter();
            var ms = new MemoryStream(buffer);
            formatter.Serialize(ms, msg);
            NetworkTransport.Send(
                hostId,
                connId,
                reliableChannel,
                buffer,
                BYTE_SIZE,
                out byte error);
            if ((NetworkError)error != NetworkError.Ok)
                Debug.LogError($"Network Error: {(NetworkError)error}");
        }
#pragma warning restore CS0618
        
        public void AssignPowersAndStartGame()
        {
            ps.Randomize();
            int totalNumOfPowers = ps.Count();
            
            // Send to GM for first pick
            for (int i = 0; i < 4; i++)
            {
                string item = ps.PopItem();
                Debug.Log($"Sending {item} to GM");
                Player.Instance.AddPower(item);
            }

            NetAllPowersIn napi = new NetAllPowersIn();
            for (int connId = 1; connId <= currentConnections; connId++)
            {
                for (int j = 0; j < 4; j++)
                {
                    string item = ps.PopItem();
                    Debug.Log($"Sending {item} to {connId}");
                    NetPower np = new NetPower { Suggestion = item };
                    SendToClient(0, connId, np);
                }
                SendToClient(0, connId, napi);
            }
            Debug.Log("Finished Sending Items");
            // Switch GM canvas
            var canvases = GameObject.Find("/Canvases").GetComponent<CanvasSwitcher>();
            Player.Instance.SetPowers();
            canvases.SwitchCanvas(canvases[1]);
            Debug.Log("Switched GM Canvas");
            // Stop checking if players are ready
            playing = true;
            Debug.Log("Stopping player ready check");
            // Stop broadcasting
            canvases.GetComponent<NewConnections>().ToggleConnections(false);
            Debug.Log("Stopping broadcast");
        }
    }
}