using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Magnethands.Menus
{
    /// <summary>
    /// Adds the powers to the buttons in a player's scene.
    /// </summary>
    public class AssignPowers : MonoBehaviour
    {
        public TMPro.TMP_Text[] powers = new TMPro.TMP_Text[4];
        private int lastAddedIndex = 0;

        public void AddPower(string power)
        {
            if (lastAddedIndex > powers.Length)
                return;
            powers[lastAddedIndex].text = power;
            lastAddedIndex++;
        }

        public void ClearPowers()
        {
            lastAddedIndex = 0;
        }
    }
}
