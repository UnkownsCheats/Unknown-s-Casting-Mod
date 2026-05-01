using UnknownCasting.Core;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

namespace Dev
{
    public static class AutoSpec
    {
        public static bool AutoPilotEnabled = false;
        private static VRRig currentTarget = null;
        private static float lastTargetUpdateTime = 0f;
        private static float baseTargetUpdateInterval = 2f;
        private static float immediateSwitchCooldown = 0.5f;
        private static float lastImmediateSwitchTime = 0f;

        private static Dictionary<VRRig, Vector3> previousPositions = new Dictionary<VRRig, Vector3>();
        private static Dictionary<VRRig, float> movementSpeeds = new Dictionary<VRRig, float>();

        public static float x, y = .6f, z = 2;
        public static bool followhead = false;
        public static bool FP = false;
        public static bool TPFront = false;
        public static float smoothing = 0.10f;
        public static bool enableSmoothing = true;
        public static int smoothingType = 1;

        private static Vector3 velocity;
        private static Vector4 rotationVelocity;

        public static void ToggleAutoPilot()
        {
            AutoPilotEnabled = !AutoPilotEnabled;
            currentTarget = null;
            lastTargetUpdateTime = Time.time;
            previousPositions.Clear();
            movementSpeeds.Clear();

            if (AutoPilotEnabled)
            {
                FindBestTarget();
            }
            else
            {
                if (CameraUpdater.cam != null)
                {
                    CameraUpdater.rig = GorillaTagger.Instance?.offlineVRRig;
                }
            }
        }

        public static void Update()
        {
            if (!AutoPilotEnabled) return;

            UpdateMovementTracking();

            if (currentTarget != null && IsLavaPerson(currentTarget) &&
                Time.time - lastImmediateSwitchTime > immediateSwitchCooldown)
            {
                Debug.Log($"Current target {GetPlayerName(currentTarget)} became lava, switching immediately!");
                FindBestTarget(true);
                lastImmediateSwitchTime = Time.time;
                lastTargetUpdateTime = Time.time;
                return;
            }

            if (currentTarget == null || !IsValidTarget(currentTarget))
            {
                if (Time.time - lastTargetUpdateTime > 0.2f)
                {
                    FindBestTarget();
                    lastTargetUpdateTime = Time.time;
                }
                return;
            }

            float dynamicUpdateInterval = CalculateDynamicUpdateInterval(currentTarget);

            if (Time.time - lastTargetUpdateTime > dynamicUpdateInterval)
            {
                FindBestTarget();
                lastTargetUpdateTime = Time.time;
            }

            if (currentTarget != null)
            {
                CameraUpdater.rig = currentTarget;
            }
        }

        private static float CalculateDynamicUpdateInterval(VRRig target)
        {
            if (target == null) return baseTargetUpdateInterval;

            float distanceToLava = GetDistanceToNearestLavaMonkey(target.transform.position);
            int chasingCount = GetLavaMonkeysChasingCount(target);
            float movementSpeed = GetMovementSpeed(target);

            float interval = baseTargetUpdateInterval;

            if (chasingCount > 0)
            {
                interval += chasingCount * 0.3f;

                if (distanceToLava < 15f)
                {
                    float proximityFactor = (15f - distanceToLava) / 15f; 
                    interval += proximityFactor * 1f; 
                }

                if (distanceToLava < 10f && movementSpeed > 3f)
                {
                    interval += Mathf.Clamp(movementSpeed * 0.1f, 0f, 0.5f);
                }

                if (chasingCount >= 2 && distanceToLava < 8f && movementSpeed > 4f)
                {
                    interval += 1f;
                }
            }
            else
            {
                if (distanceToLava > 20f)
                {
                    float distanceFactor = (distanceToLava - 20f) / 30f;
                    interval -= Mathf.Clamp(distanceFactor * 1.0f, 0f, 1.0f);
                }

                if (distanceToLava > 25f && movementSpeed < 2f)
                {
                    interval -= 0.3f;
                }
            }

            interval = Mathf.Clamp(interval, 0.5f, 3f);

            return interval;
        }

        private static void UpdateMovementTracking()
        {
            List<VRRig> allRigs = GetAllVRRigs();
            float deltaTime = Time.deltaTime;

            foreach (VRRig rig in allRigs)
            {
                if (rig == null || rig.transform == null) continue;

                if (previousPositions.ContainsKey(rig))
                {
                    Vector3 currentPos = rig.transform.position;
                    Vector3 previousPos = previousPositions[rig];
                    float distanceMoved = Vector3.Distance(currentPos, previousPos);
                    float instantSpeed = distanceMoved / Mathf.Max(deltaTime, 0.016f);

                    if (movementSpeeds.ContainsKey(rig))
                    {
                        movementSpeeds[rig] = Mathf.Lerp(movementSpeeds[rig], instantSpeed, 0.3f);
                    }
                    else
                    {
                        movementSpeeds[rig] = instantSpeed;
                    }

                    previousPositions[rig] = currentPos;
                }
                else
                {
                    previousPositions[rig] = rig.transform.position;
                }
            }
        }

        private static void FindBestTarget(bool forceSwitch = false)
        {
            List<VRRig> allRigs = GetAllVRRigs();
            VRRig bestTarget = null;
            float bestScore = float.MinValue;
            VRRig localPlayerRig = GetLocalPlayerRig();

            foreach (VRRig rig in allRigs)
            {
                if (rig == null || rig.transform == null) continue;

                if (rig == localPlayerRig) continue;

                if (IsValidTarget(rig) && !IsLavaPerson(rig))
                {
                    float score = CalculateTargetScore(rig);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestTarget = rig;
                    }
                }
            }

            if (bestTarget != null)
            {
                bool shouldSwitch = forceSwitch ||
                                  currentTarget == null ||
                                  !IsValidTarget(currentTarget) ||
                                  IsLavaPerson(currentTarget) ||
                                  bestScore > CalculateTargetScore(currentTarget) + GetSwitchThreshold(currentTarget);

                if (shouldSwitch && bestTarget != currentTarget)
                {
                    float oldInterval = CalculateDynamicUpdateInterval(currentTarget);
                    float newInterval = CalculateDynamicUpdateInterval(bestTarget);

                    currentTarget = bestTarget;
                    int chasingCount = GetLavaMonkeysChasingCount(currentTarget);
                    float movementSpeed = GetMovementSpeed(currentTarget);
                    float distanceToLava = GetDistanceToNearestLavaMonkey(currentTarget.transform.position);

                    Debug.Log($"Now spectating: {GetPlayerName(currentTarget)} " +
                             $"(Chased by: {chasingCount} lava, " +
                             $"Speed: {movementSpeed:F1}m/s, " +
                             $"Lava Distance: {distanceToLava:F1}m, " +
                             $"Switch Delay: {newInterval:F1}s, " +
                             $"Score: {bestScore:F1})");
                }
            }
            else if (currentTarget != null)
            {
                if (!IsValidTarget(currentTarget) || IsLavaPerson(currentTarget))
                {
                    currentTarget = null;
                }
            }
        }

        private static float GetSwitchThreshold(VRRig currentTarget)
        {
            if (currentTarget == null) return 1.5f;

            float distanceToLava = GetDistanceToNearestLavaMonkey(currentTarget.transform.position);
            int chasingCount = GetLavaMonkeysChasingCount(currentTarget);

            float baseThreshold = 1.5f;

            if (chasingCount > 0)
            {
                baseThreshold += chasingCount * 0.3f;

                if (distanceToLava < 10f)
                {
                    baseThreshold += (10f - distanceToLava) * 0.1f;
                }
            }

            return Mathf.Clamp(baseThreshold, 1.5f, 3f);
        }

        private static VRRig GetLocalPlayerRig()
        {
            try
            {
                return GorillaTagger.Instance?.offlineVRRig;
            }
            catch
            {
                return null;
            }
        }

        private static bool IsValidTarget(VRRig rig)
        {
            if (rig == null) return false;
            if (rig.transform == null) return false;
            if (rig.headMesh == null) return false;

            VRRig localPlayer = GetLocalPlayerRig();
            if (rig == localPlayer) return false;

            return true;
        }

        private static float CalculateTargetScore(VRRig rig)
        {
            if (rig == null || rig.transform == null) return float.MinValue;

            int chasingCount = GetLavaMonkeysChasingCount(rig);
            float chasingScore = chasingCount * 25f;

            float movementSpeed = GetMovementSpeed(rig);
            float movementScore = CalculateMovementScore(rig, movementSpeed);

            float distanceToLava = GetDistanceToNearestLavaMonkey(rig.transform.position);
            float proximityScore = CalculateProximityScore(distanceToLava, movementSpeed);

            float chaserProximityScore = CalculateProximityToChasers(rig);

            float currentTargetBonus = (rig == currentTarget) ? 8f : 0f;

            float randomFactor = Random.Range(-0.5f, 0.5f);

            return chasingScore + movementScore + proximityScore + chaserProximityScore + currentTargetBonus + randomFactor;
        }

        private static float CalculateMovementScore(VRRig rig, float movementSpeed)
        {
            float baseMovementScore = movementSpeed * 12f;

            int chasingCount = GetLavaMonkeysChasingCount(rig);
            if (chasingCount > 0 && movementSpeed > 2f)
            {
                baseMovementScore += (movementSpeed * 5f) + (chasingCount * 3f);
            }

            float accelerationBonus = CalculateAccelerationBonus(rig);
            baseMovementScore += accelerationBonus;

            return Mathf.Clamp(baseMovementScore, 0f, 100f);
        }

        private static float CalculateProximityScore(float distanceToLava, float movementSpeed)
        {
            float baseProximity = Mathf.Max(0f, 15f - distanceToLava) * 2f;

            if (distanceToLava < 10f && movementSpeed > 3f)
            {
                baseProximity += (10f - distanceToLava) * 3f + (movementSpeed * 4f);
            }

            if (distanceToLava < 5f)
            {
                baseProximity += 15f;
                if (movementSpeed > 4f)
                {
                    baseProximity += 10f;
                }
            }

            return baseProximity;
        }

        private static float CalculateAccelerationBonus(VRRig rig)
        {
            try
            {
                if (movementSpeeds.ContainsKey(rig) && previousPositions.ContainsKey(rig))
                {
                    float currentSpeed = movementSpeeds[rig];

                    if (currentSpeed > 4f)
                    {
                        return currentSpeed * 0.5f;
                    }
                }
            }
            catch { }

            return 0f;
        }

        private static float GetMovementSpeed(VRRig rig)
        {
            if (rig == null) return 0f;

            if (movementSpeeds.ContainsKey(rig))
            {
                return movementSpeeds[rig];
            }
            return 0f;
        }

        private static int GetLavaMonkeysChasingCount(VRRig targetRig)
        {
            if (targetRig == null || targetRig.transform == null) return 0;

            int chasingCount = 0;
            List<VRRig> allRigs = GetAllVRRigs();
            VRRig localPlayerRig = GetLocalPlayerRig();

            foreach (VRRig rig in allRigs)
            {
                if (rig == localPlayerRig) continue;

                if (rig != null && rig.transform != null && IsLavaPerson(rig))
                {
                    if (IsChasingPlayer(rig, targetRig))
                    {
                        chasingCount++;
                    }
                }
            }

            return chasingCount;
        }

        private static bool IsChasingPlayer(VRRig chaser, VRRig target)
        {
            if (chaser == null || target == null || chaser.transform == null || target.transform == null)
                return false;

            float distance = Vector3.Distance(chaser.transform.position, target.transform.position);

            if (distance < 20f)
            {
                Vector3 directionToTarget = (target.transform.position - chaser.transform.position).normalized;
                float dotProduct = Vector3.Dot(chaser.transform.forward, directionToTarget);

                return dotProduct > -0.3f || distance < 8f;
            }

            return false;
        }

        private static float CalculateProximityToChasers(VRRig targetRig)
        {
            if (targetRig == null || targetRig.transform == null) return 0f;

            float totalProximity = 0f;
            int validChasers = 0;
            List<VRRig> allRigs = GetAllVRRigs();
            VRRig localPlayerRig = GetLocalPlayerRig();

            foreach (VRRig rig in allRigs)
            {
                if (rig == localPlayerRig) continue;

                if (rig != null && rig.transform != null && IsLavaPerson(rig) && IsChasingPlayer(rig, targetRig))
                {
                    float distance = Vector3.Distance(targetRig.transform.position, rig.transform.position);
                    float proximity = Mathf.Max(0f, 15f - distance) * 0.8f;
                    totalProximity += proximity;
                    validChasers++;
                }
            }

            return validChasers > 0 ? totalProximity : 0f;
        }

        private static float GetDistanceToNearestLavaMonkey(Vector3 position)
        {
            float minDistance = float.MaxValue;
            List<VRRig> allRigs = GetAllVRRigs();
            VRRig localPlayerRig = GetLocalPlayerRig();

            foreach (VRRig rig in allRigs)
            {
                if (rig == null || rig.transform == null) continue;
                if (rig == localPlayerRig) continue;

                if (IsLavaPerson(rig))
                {
                    float distance = Vector3.Distance(position, rig.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                    }
                }
            }

            return minDistance == float.MaxValue ? 100f : minDistance;
        }

        private static bool IsLavaPerson(VRRig rig)
        {
            if (rig == null) return false;

            VRRig localPlayerRig = GetLocalPlayerRig();
            if (rig == localPlayerRig) return false;

            try
            {
                if (rig.mainSkin != null && rig.mainSkin.material != null)
                {
                    string matName = rig.mainSkin.material.name.ToLower();
                    return matName.Contains("infected") || matName.Contains("lava") || matName.Contains("tagged");
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private static List<VRRig> GetAllVRRigs()
        {
            List<VRRig> rigs = new List<VRRig>();

            try
            {
                rigs.AddRange(GetVRRigsFromParent());

                if (GorillaTagger.Instance != null && GorillaTagger.Instance.offlineVRRig != null)
                {
                    if (!rigs.Contains(GorillaTagger.Instance.offlineVRRig))
                    {
                        rigs.Add(GorillaTagger.Instance.offlineVRRig);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error getting VRRigs: {e.Message}");
            }

            return rigs;
        }

        private static List<VRRig> GetVRRigsFromParent()
        {
            var result = new List<VRRig>();
            
            // First add local player
            var localRig = GorillaTagger.Instance?.offlineVRRig;
            if (localRig != null) result.Add(localRig);

            // Then add all networked players using GorillaGameManager
            try
            {
                var manager = GorillaGameManager.instance;
                if (manager != null)
                {
                    foreach (var player in PhotonNetwork.PlayerList)
                    {
                        if (player == PhotonNetwork.LocalPlayer) continue;
                        
                        var vrrig = manager.FindPlayerVRRig(player);
                        if (vrrig != null) result.Add(vrrig);
                    }
                }
            }
            catch { }
            return result;
        }

        private static string GetPlayerName(VRRig rig)
        {
            if (rig == null) return "Unknown";

            VRRig localPlayerRig = GetLocalPlayerRig();
            if (rig == localPlayerRig) return "Local Player";

            if (rig.playerText1 != null && !string.IsNullOrEmpty(rig.playerText1.text))
                return rig.playerText1.text;
            return "Unnamed Player";
        }
    }
}