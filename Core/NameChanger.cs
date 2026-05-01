using GorillaNetworking;
using Photon.Pun;
using UnityEngine;

namespace Stats.Core
{
    public class NameChanger
    {
        public static void ChangeName(string Name)
        {
            GorillaComputer.instance.currentName = Name;
            PhotonNetwork.LocalPlayer.NickName = Name;
            GorillaComputer.instance.savedName = Name;
            PlayerPrefs.SetString("playerName", Name);
            PlayerPrefs.Save();
        }

    }
}