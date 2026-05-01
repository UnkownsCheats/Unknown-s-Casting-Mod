using UnityEngine;

namespace UnknownCasting.Core
{
    public class MotionBlurEffect : MonoBehaviour
    {
        public static MotionBlurEffect Instance { get; private set; }

        public static bool Enabled { get; set; } = false;
        public static float Strength { get; set; } = 0.5f;
        public static bool BlurOnPlayer { get; set; } = true;

        private Material blurMaterial;
        private Camera targetCamera;
        private RenderTexture previousFrame;
        private RenderTexture tempTexture;

        private int playerLayer;
        private int defaultCullingMask;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            playerLayer = LayerMask.NameToLayer("GorillaTag");
            if (playerLayer == -1)
            {
                playerLayer = LayerMask.NameToLayer("Player");
            }
            if (playerLayer == -1)
            {
                playerLayer = 0;
            }

            Shader blurShader = Shader.Find("Hidden/BlurEffectConeTap");
            if (blurShader == null)
            {
                blurShader = Shader.Find("Hidden/SimpleMotionBlur");
            }
            if (blurShader == null)
            {
                blurShader = CreateSimpleBlurShader();
            }

            if (blurShader != null)
            {
                blurMaterial = new Material(blurShader);
                blurMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            targetCamera = GetComponent<Camera>();
            if (targetCamera != null)
            {
                defaultCullingMask = targetCamera.cullingMask;
            }
        }

        void OnDestroy()
        {
            if (previousFrame != null)
            {
                previousFrame.Release();
                Destroy(previousFrame);
            }
            if (tempTexture != null)
            {
                tempTexture.Release();
                Destroy(tempTexture);
            }
            if (blurMaterial != null)
            {
                Destroy(blurMaterial);
            }
            if (Instance == this)
            {
                Instance = null;
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!Enabled || blurMaterial == null || targetCamera == null)
            {
                if (targetCamera != null)
                {
                    Graphics.Blit(source, destination);
                }
                return;
            }

            UpdateCullingMask();

            if (previousFrame == null || previousFrame.width != source.width || previousFrame.height != source.height)
            {
                if (previousFrame != null) previousFrame.Release();
                if (tempTexture != null) tempTexture.Release();

                previousFrame = new RenderTexture(source.width, source.height, source.depth, RenderTextureFormat.ARGB32);
                tempTexture = new RenderTexture(source.width, source.height, source.depth, RenderTextureFormat.ARGB32);
                previousFrame.Create();
                tempTexture.Create();
            }

            blurMaterial.SetFloat("_Strength", Strength);

            Graphics.Blit(source, previousFrame, blurMaterial);
            Graphics.Blit(previousFrame, destination, blurMaterial);
        }

        private void UpdateCullingMask()
        {
            if (targetCamera == null) return;

            int newCullingMask;
            if (BlurOnPlayer)
            {
                newCullingMask = defaultCullingMask;
            }
            else
            {
                newCullingMask = defaultCullingMask & ~(1 << playerLayer);
            }

            if (targetCamera.cullingMask != newCullingMask)
            {
                targetCamera.cullingMask = newCullingMask;
            }
        }

        private Shader CreateSimpleBlurShader()
        {
            try
            {
                Shader shader = Shader.Find("Hidden/BlurEffectConeTap");
                return shader;
            }
            catch
            {
                return null;
            }
        }

        public static void Toggle()
        {
            Enabled = !Enabled;
            if (Instance != null && Instance.targetCamera != null)
            {
                Instance.targetCamera.cullingMask = Instance.defaultCullingMask;
            }
        }
    }
}
