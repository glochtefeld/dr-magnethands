using Magnethands.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Magnethands.Menus.GM
{
    public class GetNewPowers : MonoBehaviour
    {
        private PowerDispersion ps = Networking.Server.Instance.ps;
        public void GetPowers()
        {
            Player.Instance.ClearPowers();
            gameObject.GetComponent<AssignPowers>().ClearPowers();
            for (int i = 0; i < 4; i++)
            {
                Player.Instance.AddPower(ps.PopItem());
            }
            Player.Instance.SetPowers();
        }
    }
}