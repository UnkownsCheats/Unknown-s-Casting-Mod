using UnityEngine;

namespace UnknownCasting.Core
{
    public class AntiLag
    {
        public static void OnPlayerInit()
        {
            Application.targetFrameRate = 144;
            QualitySettings.SetQualityLevel(1);
            QualitySettings.antiAliasing = 0;
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.particleRaycastBudget = 0;
            QualitySettings.pixelLightCount = 0;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            QualitySettings.realtimeReflectionProbes = false;
            QualitySettings.lodBias = 0;
            QualitySettings.pixelLightCount = 0;
            QualitySettings.realtimeReflectionProbes = false;
            QualitySettings.enableLODCrossFade = false;
            QualitySettings.maximumLODLevel = 0;
            foreach (Camera cam in Camera.allCameras)
            {
                cam.allowHDR = false;
                cam.focusDistance = 0;
                cam.farClipPlane = 50;
                cam.focusDistance = 1;
                cam.allowHDR = false;
            }
        }
    }
}