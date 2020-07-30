using System.Collections.Generic;
using UnityEngine;

namespace Magnethands.Menus.GM
{
    /// <summary>
    /// Singleton class. Stores the powers sent to it, randomizes the
    /// order, and allows other classes to pop from the randomized stack.
    /// </summary>
    public class PowerDispersion
    {
        private List<string> _allPowers;
        public Stack<string> AllPowers { set; get; }
        

        public PowerDispersion()
        {
            _allPowers = new List<string>();
            AllPowers = new Stack<string>();
        }
        public void Randomize()
        {
            var rng = new System.Random();
            while (_allPowers.Count > 0)
            {
                var item = _allPowers[rng.Next(0, _allPowers.Count - 1)];
                Debug.Log($"Item:{item}");
                AllPowers.Push(item);
                _allPowers.Remove(item);
                Debug.Log($"Stack Count: {AllPowers.Count}");
            }
            
        }

        public void AddPowerToPool(string power) => _allPowers.Add(power);
        public int Count() => AllPowers.Count;
        public string PopItem()
        {
            if (AllPowers.Count > 0)
                return AllPowers.Pop();
            else
                return "";
        }
    }
}
