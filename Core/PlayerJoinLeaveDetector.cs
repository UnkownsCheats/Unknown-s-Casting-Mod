using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace UnknownCasting.Core
{
    public class PlayerJoinLeaveDetector : MonoBehaviour
    {
        public static PlayerJoinLeaveDetector Instance;
        
        private HashSet<int> knownPlayers = new HashSet<int>();
        
        void Awake()
        {
            Instance = this;
            Debug.Log("[PlayerJoinLeave] Detector initialized");
        }
        
        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
        
        void Update()
        {
            DetectPlayers();
        }
        
        private void DetectPlayers()
        {
            try
            {
                VRRig[] allRigs = FindObjectsOfType<VRRig>();
                var localRig = GorillaTagger.Instance?.offlineVRRig;
                
                foreach (var rig in allRigs)
                {
                    if (rig == null || rig == localRig) continue;
                    
                    int actorID = GetActorID(rig);
                    if (actorID < 0) continue;
                    
                    if (!knownPlayers.Contains(actorID))
                    {
                        knownPlayers.Add(actorID);
                        string playerName = rig.playerText1?.text ?? "Unknown";
                        OnPlayerJoinedProxy(actorID, playerName, rig.playerColor);
                    }
                }
                
                if (PhotonNetwork.InRoom)
                {
                    List<int> toRemove = new List<int>();
                    foreach (int id in knownPlayers)
                    {
                        bool found = false;
                        foreach (var rig in allRigs)
                        {
                            if (rig == null) continue;
                            if (GetActorID(rig) == id)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            toRemove.Add(id);
                        }
                    }
                    
                    foreach (int id in toRemove)
                    {
                        knownPlayers.Remove(id);
                        OnPlayerLeftProxy(id);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PlayerJoinLeave] Detect Error: {ex.Message}");
            }
        }
        
        private int GetActorID(VRRig rig)
        {
            try
            {
                var prop = typeof(VRRig).GetProperty("actorNumber", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop != null)
                    return (int)prop.GetValue(rig);
            }
            catch { }
            return -1;
        }
        
        private void OnPlayerJoinedProxy(int actorID, string playerName, Color playerColor)
        {
            Debug.Log($"[PlayerJoinLeave] Player joined: {playerName} (ActorID: {actorID})");
            
            if (ToastNotification.Instance != null)
                ToastNotification.Instance.OnPlayerJoined(playerName);
            
            if (PlayerStatsManager.Instance != null)
                PlayerStatsManager.Instance.OnPlayerJoin(actorID, playerName, playerColor);
        }
        
        private void OnPlayerLeftProxy(int actorID)
        {
            Debug.Log($"[PlayerJoinLeave] Player left (ActorID: {actorID})");
            
            if (ToastNotification.Instance != null)
                ToastNotification.Instance.OnPlayerLeft("Player");
            
            if (PlayerStatsManager.Instance != null)
                PlayerStatsManager.Instance.OnPlayerLeave(actorID);
        }
        
        public void ClearKnownPlayers()
        {
            knownPlayers.Clear();
        }
    }
}