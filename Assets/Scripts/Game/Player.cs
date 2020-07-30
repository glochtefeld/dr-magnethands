using Magnethands.Menus;
using Magnethands.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Magnethands.Game
{
    /// <summary>
    /// Singleton class which controls all other behaviour.
    /// </summary>
    public class Player : MonoBehaviour
    {
        public State currentState;
        public static bool isGameMaster;
        public static Player Instance { set; get; }

        private List<string> powers = new List<string>();
        
        void Start()
        {
            currentState = State.MainMenu;
            if (Instance == null)
            {
                DontDestroyOnLoad(gameObject);
                Instance = this;
            }
            else
                Destroy(gameObject);

        }

        public void StartServer()
        {
            isGameMaster = true;
            currentState = State.HSuggestAllowNew;
            var client = GameObject.Find("/Client");
            client.GetComponent<Client>().ShutDown();
            DestroyImmediate(client);
            SceneManager.LoadScene("Scenes/GameMasterView");
            GameObject.Find("/Server").GetComponent<Server>().Initialize();
        }

        public void StartClient(string ip)
        {
            isGameMaster = false;
            currentState = State.CConnectedSending;
            DestroyImmediate(GameObject.Find("/Server")); 
            var client = GameObject.Find("/Client").GetComponent<Client>();
            client.ServerIp = ip;
            client.AttemptConnection();
        }

        public void AddPower(string power)
        {
            powers.Add(power);
            Debug.Log("Added Power");
        }

        public void SetPowers()
        {
            var powerText = GameObject.Find("/Canvases")
                .GetComponent<AssignPowers>();
            foreach (var power in powers)
            {
                powerText.AddPower(power);
            }
        }

        public void ClearPowers()
        {
            powers = new List<string>();
        }
   
    }
}