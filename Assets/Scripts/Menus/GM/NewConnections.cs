using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Magnethands.Menus.GM
{
    public class NewConnections : MonoBehaviour
    {
        public static bool AllowConnections { private set; get; } = true;
        public void ToggleConnections() => AllowConnections = !AllowConnections;
        public void ToggleConnections(bool state) => AllowConnections = state;
    }
}