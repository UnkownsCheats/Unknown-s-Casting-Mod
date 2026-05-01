using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

namespace UnknownCasting.Core
{
    public class PlayerStatsManager : MonoBehaviour
    {
        public static PlayerStatsManager Instance;
        
        private Dictionary<int, PlayerStats> playerStatsMap = new Dictionary<int, PlayerStats>();
        
        public class PlayerStats
        {
            public string playerName;
            public int actorID;
            public int tags = 0;
            public int timesTagged = 0;
            public int kills = 0;
            public int deaths = 0;
            public float timePlayed = 0f;
            public DateTime joinTime;
            public string tag = "";
            public Color playerColor = Color.white;
        }
        
        private bool showStatsTags = true;
        private float statsTagOffset = 0.8f;
        private float statsTagSize = 0.5f;
        
        private Dictionary<VRRig, TextMeshPro> statsTagMap = new Dictionary<VRRig, TextMeshPro>();
        
        private Font silkScreen;
        
        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        void Update()
        {
            UpdateStatsTags();
            UpdateTimePlayed();
        }
        
        public void OnPlayerJoin(int actorID, string playerName, Color playerColor)
        {
            if (!playerStatsMap.ContainsKey(actorID))
            {
                playerStatsMap[actorID] = new PlayerStats
                {
                    playerName = playerName,
                    actorID = actorID,
                    joinTime = DateTime.Now,
                    playerColor = playerColor
                };
            }
            else
            {
                playerStatsMap[actorID].playerName = playerName;
                playerStatsMap[actorID].playerColor = playerColor;
            }
        }
        
        public void OnPlayerLeave(int actorID)
        {
            if (playerStatsMap.ContainsKey(actorID))
            {
                playerStatsMap.Remove(actorID);
            }
        }
        
        public void OnPlayerTagged(int actorID)
        {
            if (playerStatsMap.ContainsKey(actorID))
            {
                playerStatsMap[actorID].tags++;
            }
        }
        
        public void OnPlayerWasTagged(int actorID)
        {
            if (playerStatsMap.ContainsKey(actorID))
            {
                playerStatsMap[actorID].timesTagged++;
            }
        }
        
        public void OnPlayerKill(int actorID)
        {
            if (playerStatsMap.ContainsKey(actorID))
            {
                playerStatsMap[actorID].kills++;
            }
        }
        
        public void OnPlayerDeath(int actorID)
        {
            if (playerStatsMap.ContainsKey(actorID))
            {
                playerStatsMap[actorID].deaths++;
            }
        }
        
        public void SetPlayerTag(int actorID, string tag)
        {
            if (playerStatsMap.ContainsKey(actorID))
            {
                playerStatsMap[actorID].tag = tag;
            }
        }
        
        public PlayerStats GetStats(int actorID)
        {
            if (playerStatsMap.ContainsKey(actorID))
                return playerStatsMap[actorID];
            return null;
        }
        
        public PlayerStats GetStats(string playerName)
        {
            foreach (var stats in playerStatsMap.Values)
            {
                if (stats.playerName == playerName)
                    return stats;
            }
            return null;
        }
        
        public List<PlayerStats> GetAllStats()
        {
            return new List<PlayerStats>(playerStatsMap.Values);
        }
        
        public void SetShowStatsTags(bool show)
        {
            showStatsTags = show;
            if (!show)
            {
                DestroyAllStatsTags();
            }
        }
        
        public void SetStatsTagOffset(float offset)
        {
            statsTagOffset = offset;
        }
        
        public void SetStatsTagSize(float size)
        {
            statsTagSize = size;
        }
        
        private void UpdateStatsTags()
        {
            if (!showStatsTags) return;
            
            var vrrigs = GetVRRigs();
            Debug.Log($"[PlayerStats] UpdateStatsTags called, found {vrrigs.Count} rigs");
            
            foreach (var rig in vrrigs)
            {
                if (rig == null) continue;
                
                int actorID = GetActorID(rig);
                Debug.Log($"[PlayerStats] Rig: {(rig.playerText1?.text ?? "null")}, ActorID: {actorID}");
                if (actorID < 0) continue;
                
                if (!statsTagMap.ContainsKey(rig))
                {
                    CreateStatsTag(rig);
                }
                
                if (statsTagMap.TryGetValue(rig, out var statsTag) && statsTag != null)
                {
                    statsTag.transform.localPosition = Vector3.up * statsTagOffset;
                    statsTag.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
                    
                    PlayerStats stats = GetStats(actorID);
                    Debug.Log($"[PlayerStats] Stats for actorID {actorID}: {(stats != null ? stats.playerName + " tags=" + stats.tags : "null")}");
                    if (stats != null && !string.IsNullOrEmpty(stats.tag))
                    {
                        statsTag.text = $"[{stats.tag}]";
                        statsTag.color = Color.yellow;
                        statsTag.fontSize = statsTagSize;
                        statsTag.gameObject.SetActive(true);
                    }
                    else
                    {
                        statsTag.gameObject.SetActive(false);
                    }
                }
            }
            
            CleanupStatsTags();
        }
        
        private void CreateStatsTag(VRRig rig)
        {
            GameObject tagObj = new GameObject("StatsTag");
            tagObj.transform.SetParent(rig.transform);
            tagObj.transform.localPosition = Vector3.up * statsTagOffset;
            
            TextMeshPro tmp = tagObj.AddComponent<TextMeshPro>();
            tmp.fontSize = statsTagSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.yellow;
            tmp.enableWordWrapping = false;
            tmp.raycastTarget = false;
            
            statsTagMap[rig] = tmp;
        }
        
        private void CleanupStatsTags()
        {
            List<VRRig> toRemove = new List<VRRig>();
            foreach (var kvp in statsTagMap)
            {
                if (kvp.Key == null || kvp.Key.transform == null)
                {
                    if (kvp.Value != null)
                        UnityEngine.Object.Destroy(kvp.Value.gameObject);
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var rig in toRemove)
                statsTagMap.Remove(rig);
        }
        
        private void DestroyAllStatsTags()
        {
            foreach (var tag in statsTagMap.Values)
            {
                if (tag != null)
                    UnityEngine.Object.Destroy(tag.gameObject);
            }
            statsTagMap.Clear();
        }
        
        private void UpdateTimePlayed()
        {
            float delta = Time.deltaTime;
            foreach (var stats in playerStatsMap.Values)
            {
                stats.timePlayed += delta;
            }
        }
        
        private List<VRRig> GetVRRigs()
        {
            var result = new List<VRRig>();
            
            var localRig = GorillaTagger.Instance?.offlineVRRig;
            if (localRig != null) result.Add(localRig);
            
            try
            {
                // Try using GorillaGameManager if in room
                var manager = GorillaGameManager.instance;
                if (manager != null && PhotonNetwork.InRoom)
                {
                    foreach (var player in PhotonNetwork.PlayerList)
                    {
                        if (player == PhotonNetwork.LocalPlayer) continue;
                        
                        var vrrig = manager.FindPlayerVRRig(player);
                        if (vrrig != null) result.Add(vrrig);
                    }
                }
                else
                {
                    // In lobby - try to find all VRRigs in the scene
                    VRRig[] allRigs = FindObjectsOfType<VRRig>();
                    foreach (var rig in allRigs)
                    {
                        if (rig != null && rig != localRig && rig.gameObject != null)
                        {
                            result.Add(rig);
                        }
                    }
                }
            }
            catch { }
            
            return result;
        }
        
        private int GetActorID(VRRig rig)
        {
            try
            {
                var field = typeof(VRRig).GetProperty("actorNumber", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    return (int)field.GetValue(rig);
                }
            }
            catch { }
            
            return -1;
        }
        
        public void ResetAllStats()
        {
            playerStatsMap.Clear();
            DestroyAllStatsTags();
        }
        
        public string GetStatsSummary()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Player Stats ===");
            foreach (var stats in playerStatsMap.Values)
            {
                sb.AppendLine($"{stats.playerName}: Tags={stats.tags}, Kills={stats.kills}, Deaths={stats.deaths}");
            }
            return sb.ToString();
        }
    }
}