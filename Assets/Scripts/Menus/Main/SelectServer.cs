using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Magnethands.Game;

namespace Magnethands.Menus.Main
{
    /// <summary>
    /// Attached to the ServerListPrefab object, this turns the panel 
    /// into a button and makes clicking it attempt to connect to the
    /// IP address specified.
    /// </summary>
    public class SelectServer : MonoBehaviour
    {
        public TMPro.TMP_Text ipAddress;
        private void Start()
        {
            gameObject.GetComponent<Button>().onClick.AddListener(
                () => Player.Instance.StartClient(ipAddress.text));
        }
    }
}