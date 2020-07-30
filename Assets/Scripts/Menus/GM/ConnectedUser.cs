using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Magnethands.Menus.GM
{
    /// <summary>
    /// Used in the ConnectedUserPrefab to update how many suggestions 
    /// have been made by the user.
    /// </summary>
    public class ConnectedUser : MonoBehaviour
    {
        public TMPro.TMP_Text userName;
        public TMPro.TMP_Text suggestions;
        public int connectionId;
        private int _currentSuggestionCount;

        private void Update()
        {
            suggestions.text = _currentSuggestionCount.ToString() + "/8";
        }

        public void AddSuggestion()
        {
            _currentSuggestionCount++;
        }
    }
}