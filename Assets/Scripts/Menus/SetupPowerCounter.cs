using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Magnethands.Menus
{
    /// <summary>
    /// Adds powers to global power pool for both GM and players.
    /// </summary>
    public class SetupPowerCounter : MonoBehaviour
    {
        private int _powerCount;
        public TMPro.TMP_Text counter;
        public TMPro.TMP_InputField input;
        public Button submitButton;

        public void SuggestPower()
        {
            var power = input.text;
            if (string.IsNullOrEmpty(power))
                return;

            if (Game.Player.isGameMaster)
            {
                Networking.Server.Instance.CurrentPowers++;
                Networking.Server.Instance.ps.AddPowerToPool(power);
            }
            else
            {
                Networking.Client.Instance.SuggestPower(power);
                Debug.Log("Sending power");
            }
            _powerCount++;
            counter.text = $"Submit ({_powerCount}/8)";
            input.text = "";
            if (_powerCount > 7)
                LockInput();
        }

        private void LockInput()
        {
            submitButton.interactable = false;
            counter.text = "Please wait";
        }
    }
}