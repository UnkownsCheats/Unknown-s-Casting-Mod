using UnityEngine;

namespace UnknownCasting.Core
{
    public class FpsUnLock : MonoBehaviour
    {
        public static bool FpsUnlocked { get; private set; } = false;
        private static int originalTargetFPS = -1;
        private static int originalVSyncCount = -1;

        public static void ToggleFPSUnlock()
        {
            FpsUnlocked = !FpsUnlocked;

            if (FpsUnlocked)
            {
                originalTargetFPS = Application.targetFrameRate;
                originalVSyncCount = QualitySettings.vSyncCount;

                Application.targetFrameRate = 10000; 
                QualitySettings.vSyncCount = 0;

            }
            else
            {
                if (originalTargetFPS != -1)
                    Application.targetFrameRate = originalTargetFPS;

                if (originalVSyncCount != -1)
                    QualitySettings.vSyncCount = originalVSyncCount;

                
            }
        }

        public static void SetFPSUnlock(bool enabled)
        {
            if (enabled != FpsUnlocked)
            {
                ToggleFPSUnlock();
            }
        }

        void OnDestroy()
        {
            if (FpsUnlocked)
            {
                if (originalTargetFPS != -1)
                    Application.targetFrameRate = originalTargetFPS;

                if (originalVSyncCount != -1)
                    QualitySettings.vSyncCount = originalVSyncCount;

                FpsUnlocked = false;
            }
        }
    }
}