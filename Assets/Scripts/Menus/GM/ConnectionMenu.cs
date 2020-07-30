using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Magnethands.Menus.GM
{
    /// <summary>
    /// Controller class for the Host's connection view.
    /// </summary>
    public class ConnectionMenu : MonoBehaviour
    {
        public GameObject connectedUserPrefab;
        public GameObject content;
        private Dictionary<int,GameObject> users = 
            new Dictionary<int, GameObject>();

        public void AddUser(int id)
        {
            Debug.Log(id);
            var user = Instantiate(connectedUserPrefab, content.transform);
            var userInfo = user.GetComponent<ConnectedUser>();
            userInfo.userName.text = id.ToString();
            userInfo.connectionId = id;
            users.Add(id, user);
        }

        public void RemoveUser(int id)
        {
            try
            {
                var user = users[id];
                users.Remove(id);
                Destroy(user);
            }
            catch
            {
                Debug.LogError("No user with that name found");
            }
        }

        public void ChangeName(int id, string to)
        {
            try
            {
                var user = users[id];
                var userInfo = user.GetComponent<ConnectedUser>();
                userInfo.userName.text = to;
            }
            catch
            {
                Debug.LogError("No user with that name found");
            }
        }

        public void IncrementUserSuggestions(int id)
        {
            var user = users[id];
            user.GetComponent<ConnectedUser>().AddSuggestion();
        }

        public int CountUsers() => users.Count;
    }
}
