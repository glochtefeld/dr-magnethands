using UnityEngine;

namespace Magnethands.Networking
{
    public struct LANConnectionInfo
    {
        public string ipAddress;
        public string name;
        public float ttl;

        public LANConnectionInfo(string ip, string data)
        {
            ipAddress = ip;
            name = data;
            ttl = Time.time + 5f;
        }
    }
}
