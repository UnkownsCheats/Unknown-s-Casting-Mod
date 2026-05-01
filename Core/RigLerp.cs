using GorillaNetworking;
using Photon.Pun;
using UnityEngine;

namespace UnknownCasting.Core
{
    public static class RigLerp
    {
        // Rig lerp values for remote players (0-100)
        public static float LerpValue = 20f;
        public static float targetLerpValue = 20f;

        private static bool lerpValueInitialized = false;
        private static float updateRate = 0.15f;
        private static float nextUpdateTime;

        public static void Update()
        {
            // Smooth lerp value transitions
            if (!lerpValueInitialized || Mathf.Abs(targetLerpValue - LerpValue) > 0.01f)
            {
                LerpValue = Mathf.Lerp(LerpValue, targetLerpValue, Time.deltaTime * 8f);
                if (Mathf.Abs(targetLerpValue - LerpValue) <= 0.01f)
                {
                    LerpValue = targetLerpValue;
                    lerpValueInitialized = true;
                }
            }
        }

        public static void LateUpdate()
        {
            float actualLerp = LerpValue / 100f;

            try
            {
                var manager = GorillaGameManager.instance;
                if (manager != null)
                {
                    foreach (var player in PhotonNetwork.PlayerList)
                    {
                        var vrrig = manager.FindPlayerVRRig(player);
                        if (vrrig != null && vrrig != GorillaTagger.Instance?.offlineVRRig)
                        {
                            vrrig.lerpValueBody = actualLerp;
                            vrrig.lerpValueFingers = actualLerp;
                        }
                    }
                }
            }
            catch { }
        }
    }
}
