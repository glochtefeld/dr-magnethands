using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Magnethands.Menus.Main
{
    /// <summary>
    /// Adds servers to be saved to the server list if it is a
    /// valid IPv4 address.
    /// </summary>
    public class OnlineServerList : MonoBehaviour
    {
        public TMPro.TMP_InputField ipInput;
        public GameObject serverPrefab;
        public GameObject serverListContent;

        // See notes below script for further explanation
        private bool CheckIp()
        {
            string ip = ipInput.text;
            if (Regex.IsMatch(ip, "(\\d{1,3}\\.?){4}"))
            {
                string[] temp = ip.Split('.');
                foreach (var num in temp)
                {
                    int.TryParse(num, out int parsed);
                    if (parsed > 255)
                        return false;
                }
                return true;
            }
            else
                return false;
        }

        public void AddServerToList()
        {
            if (CheckIp())
            {
                // Copy string to variable
                var item = Instantiate(serverPrefab, serverListContent.transform);
                var serverListingIp = item.transform.GetChild(0)
                    .GetComponent<TMPro.TMP_Text>();
                serverListingIp.text = ipInput.text;
                ipInput.text = "";
            }
        }
    }
}
/* CheckIP(): You might be wondering, won't the IPValidator do this 
 * step for us? TMP_InputValidators only work on a single character
 * at a time, not the whole string, and only while the string is 
 * being typed. Making sure it's valid is a different step. The regex
 * is checking that groups of 1-3 digits appear 4 times and are 
 * separated by periods. */