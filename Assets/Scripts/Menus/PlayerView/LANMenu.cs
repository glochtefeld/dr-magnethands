using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Magnethands.Networking;
using System.Linq;

namespace Magnethands.Menus.PlayerView
{
    public class LANMenu : MonoBehaviour
    {
        public GameObject serverPrefab;
        public GameObject content;
        private Dictionary<GameObject, LANConnectionInfo> conns =
            new Dictionary<GameObject, LANConnectionInfo>();



        public void AddToList(LANConnectionInfo info)
        {
            foreach (var conn in conns)
            {
                var key = conn.Key;
                if (conns[key].ipAddress == info.ipAddress)
                {
                    conns[key] = info;
                    return;
                }
            }

            var instance = Instantiate(serverPrefab, content.transform);
            var ip = instance.transform.GetChild(0)
                .GetComponent<TMPro.TMP_Text>();
            var name = instance.transform.GetChild(1)
                .GetComponent<TMPro.TMP_Text>();
            ip.text = info.ipAddress;
            name.text = info.name;
            conns.Add(instance, info);
        }

        private void Awake()
        {
            StartCoroutine(CleanupList());
        }

        private IEnumerator CleanupList()
        {
            while (true)
            {
                var keys = conns.Keys.ToList();
                foreach (var key in keys)
                {
                    if (conns[key].ttl <= Time.time)
                    {
                        conns.Remove(key);
                        Destroy(key);
                    }
                }
                yield return new WaitForSeconds(5f);
            }
        }
    }
}