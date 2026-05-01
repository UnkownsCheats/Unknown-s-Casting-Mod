using GorillaNetworking;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Reflection;

namespace UnknownCasting.Core.MicToggle
{
    public class MicToggle : MonoBehaviour
    {
        public static MicToggle Instance { get; private set; }
        public bool micEnabled = true;

        private GameObject gorillaComputer;
        private Image micStatusImage;
        private GameObject canvasObj;
        private Texture2D micOnTexture;
        private Texture2D micOffTexture;

        void Awake()
        {
            Instance = this;
            Debug.Log("[MicToggle] Instance created");
        }

        void Start()
        {
            Debug.Log("[MicToggle] Start called");
            FindGorillaComputer();
            LoadTextures();
            CreateStatusImage();
            Debug.Log("[MicToggle] Setup complete, micEnabled: " + micEnabled);
        }

        void Update()
        {
            UpdateMicStatus();
        }

        public void FindGorillaComputer()
        {
            gorillaComputer = GameObject.Find("GorillaComputer");
        }

        private void LoadTextures()
        {
            // Load textures using the same method as the main plugin
            micOnTexture = LoadTextureFromAssembly("UnknownCasting.Core.MicToggle.MicOn.png");
            micOffTexture = LoadTextureFromAssembly("UnknownCasting.Core.MicToggle.MicOff.png");

            // Check if textures loaded successfully
            if (micOnTexture == null)
            {
                Debug.LogError("MicOn.png not found in embedded resources!");
                // Create fallback textures
                micOnTexture = CreateFallbackTexture(Color.green, "ON");
            }
            if (micOffTexture == null)
            {
                Debug.LogError("MicOff.png not found in embedded resources!");
                // Create fallback textures
                micOffTexture = CreateFallbackTexture(Color.red, "OFF");
            }
        }

        private Texture2D LoadTextureFromAssembly(string path)
        {
            try
            {
                Texture2D texture = new Texture2D(2, 2);
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
                if (stream != null)
                {
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, (int)stream.Length);
                    texture.LoadImage(buffer);
                    texture.Apply();
                    return texture;
                }
                return null;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load texture {path}: {e.Message}");
                return null;
            }
        }

        private Texture2D CreateFallbackTexture(Color color, string text)
        {
            Texture2D texture = new Texture2D(64, 64);
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            // Add simple text (this is a basic fallback)
            if (text == "ON")
            {
                // Draw a green circle with white checkmark
                for (int x = 20; x < 44; x++)
                {
                    for (int y = 20; y < 44; y++)
                    {
                        if (Mathf.Pow(x - 32, 2) + Mathf.Pow(y - 32, 2) < 144) // Circle
                        {
                            texture.SetPixel(x, y, Color.green);
                        }
                    }
                }
            }
            else
            {
                // Draw a red circle with white X
                for (int x = 20; x < 44; x++)
                {
                    for (int y = 20; y < 44; y++)
                    {
                        if (Mathf.Pow(x - 32, 2) + Mathf.Pow(y - 32, 2) < 144) // Circle
                        {
                            texture.SetPixel(x, y, Color.red);
                        }
                    }
                }
            }

            texture.Apply();
            return texture;
        }

        private void CreateStatusImage()
        {
            // Create canvas if it doesn't exist
            if (canvasObj == null)
            {
                canvasObj = new GameObject("MicStatusCanvas");
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Create or find image object
            GameObject imageObj = GameObject.Find("MicStatusImage");
            if (imageObj == null)
            {
                imageObj = new GameObject("MicStatusImage");
                imageObj.transform.SetParent(canvasObj.transform);
                micStatusImage = imageObj.AddComponent<Image>();
            }
            else
            {
                micStatusImage = imageObj.GetComponent<Image>();
                if (micStatusImage == null)
                {
                    micStatusImage = imageObj.AddComponent<Image>();
                }
            }

            // Set initial sprite
            UpdateStatusDisplay();

            micStatusImage.preserveAspect = true;

            // Set up the rect transform
            RectTransform rectTransform = micStatusImage.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.pivot = new Vector2(1, 0);
            rectTransform.anchoredPosition = new Vector2(-10, 10);
            rectTransform.sizeDelta = new Vector2(32, 32);
        }

        public void ToggleMicrophone()
        {
            micEnabled = !micEnabled;

            try
            {
                GorillaComputer gc = GameObject.Find("GorillaComputer")?.GetComponent<GorillaComputer>();
                if (gc != null)
                {
                    gc.pttType = micEnabled ? "ALL CHAT" : "OFF";
                    
                    var gcType = gc.GetType();
                    foreach (var field in gcType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                    {
                        if (field.Name.ToLower().Contains("muted"))
                        {
                            field.SetValue(gc, !micEnabled);
                        }
                    }
                }
                
                GorillaTagger gt = GorillaTagger.Instance;
                if (gt != null)
                {
                    var gtType = gt.GetType();
                    foreach (var field in gtType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                    {
                        if (field.Name.ToLower().Contains("mic") || field.Name.ToLower().Contains("voice"))
                        {
                            if (field.FieldType == typeof(bool))
                            {
                                field.SetValue(gt, micEnabled);
                            }
                        }
                    }
                }
                
                VRRig localRig = GorillaTagger.Instance?.offlineVRRig;
                if (localRig != null)
                {
                    var rigType = localRig.GetType();
                    
                    foreach (var field in rigType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                    {
                        if (field.Name.ToLower().Contains("mic") || field.Name.ToLower().Contains("voice"))
                        {
                            if (field.FieldType == typeof(bool))
                            {
                                field.SetValue(localRig, micEnabled);
                            }
                            else if (field.FieldType == typeof(AudioSource) && !micEnabled)
                            {
                                var audioSrc = field.GetValue(localRig) as AudioSource;
                                if (audioSrc != null)
                                {
                                    audioSrc.mute = true;
                                }
                            }
                        }
                    }
                    
                    foreach (var prop in rigType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                    {
                        if (prop.Name.ToLower().Contains("mic") || prop.Name.ToLower().Contains("voice"))
                        {
                            if (prop.PropertyType == typeof(bool))
                            {
                                prop.SetValue(localRig, micEnabled);
                            }
                        }
                    }
                    
                    if (!micEnabled && localRig.gameObject != null)
                    {
                        foreach (var comp in localRig.gameObject.GetComponentsInChildren<AudioSource>())
                        {
                            comp.mute = true;
                        }
                    }
                    else if (micEnabled && localRig.gameObject != null)
                    {
                        foreach (var comp in localRig.gameObject.GetComponentsInChildren<AudioSource>())
                        {
                            comp.mute = false;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("[MicToggle] Error: " + e.Message);
            }

            UpdateStatusDisplay();
        }

        private void UpdateMicStatus()
        {
            if (gorillaComputer != null && micStatusImage != null)
            {
                bool currentStatus = gorillaComputer.GetComponent<GorillaComputer>().pttType == "ALL CHAT";
                if (currentStatus != micEnabled)
                {
                    micEnabled = currentStatus;
                    UpdateStatusDisplay();
                }
            }
        }

        private void UpdateStatusDisplay()
        {
            if (micStatusImage != null && micOnTexture != null && micOffTexture != null)
            {
                if (micEnabled)
                {
                    micStatusImage.sprite = Sprite.Create(micOnTexture,
                        new Rect(0, 0, micOnTexture.width, micOnTexture.height),
                        new Vector2(0.5f, 0.5f));
                    micStatusImage.color = Color.white;
                }
                else
                {
                    micStatusImage.sprite = Sprite.Create(micOffTexture,
                        new Rect(0, 0, micOffTexture.width, micOffTexture.height),
                        new Vector2(0.5f, 0.5f));
                    micStatusImage.color = Color.white;
                }
            }
        }

        // Method to toggle the entire mic toggle system on/off
        public void SetMicToggleEnabled(bool enabled)
        {
            if (canvasObj != null)
            {
                canvasObj.SetActive(enabled);
            }
        }

        // Cleanup when destroyed
        void OnDestroy()
        {
            if (canvasObj != null)
            {
                Destroy(canvasObj);
            }
        }
    }
}