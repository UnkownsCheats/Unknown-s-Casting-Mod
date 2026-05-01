using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using GorillaLocomotion;
using GorillaNetworking;

namespace UnknownCasting.Core
{
    public class HandCam : MonoBehaviour
    {
        public static HandCam Instance { get; private set; }
        
        private GameObject handCamRoot;
        private GameObject mainCube;
        private GameObject cameraCylinder;
        private GameObject leftHandle;
        private GameObject rightHandle;
        
        private List<GameObject> buttons = new List<GameObject>();
        private List<Renderer> buttonRenderers = new List<Renderer>();
        
        private GameObject currentCameraObject;
        private Camera currentCamera;
        
        private bool isHeld = false;
        private Transform holdingHand;
        
        private int currentViewMode = 0;
        private float targetFOV = 110f;
        
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        
        private float buttonPressCooldown = 0.2f;
        private float lastButtonPressTime;
        
        private static readonly string[] viewModeLabels = { "FOV", "FP", "3RD", "HAND" };
        
        public bool handCamEnabled = false;
        
        void Awake()
        {
            Instance = this;
        }
        
        void Start()
        {
            CreateHandCam();
        }
        
        void Update()
        {
            if (!handCamEnabled) return;
            
            HandleTriggerInput();
            HandleViewModeButtons();
            UpdateCameraAttachment();
            UpdateButtonColors();
        }
        
        private void CreateHandCam()
        {
            handCamRoot = new GameObject("HandCamRoot");
            DontDestroyOnLoad(handCamRoot);
            handCamRoot.SetActive(false);
            
            mainCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mainCube.name = "MainCube";
            mainCube.transform.SetParent(handCamRoot.transform);
            mainCube.transform.localPosition = Vector3.zero;
            mainCube.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            ApplyCubeMaterial(mainCube, new Color(0.15f, 0.15f, 0.15f));
            
            cameraCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cameraCylinder.name = "CameraCylinder";
            cameraCylinder.transform.SetParent(handCamRoot.transform);
            cameraCylinder.transform.localPosition = new Vector3(0, 0, 0);
            cameraCylinder.transform.localEulerAngles = new Vector3(90, 0, 0);
            cameraCylinder.transform.localScale = new Vector3(0.15f, 0.3f, 0.15f);
            ApplyCubeMaterial(cameraCylinder, Color.black);
            
            CreateHandles();
            CreateButtons();
            
            CreateHandCamCamera();
            
            originalPosition = handCamRoot.transform.position;
            originalRotation = handCamRoot.transform.rotation;
        }
        
        private void CreateHandles()
        {
            leftHandle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leftHandle.name = "LeftHandle";
            leftHandle.transform.SetParent(handCamRoot.transform);
            leftHandle.transform.localPosition = new Vector3(-0.35f, 0, 0);
            leftHandle.transform.localEulerAngles = new Vector3(0, 0, 90);
            leftHandle.transform.localScale = new Vector3(0.05f, 0.2f, 0.05f);
            ApplyCubeMaterial(leftHandle, new Color(0.3f, 0.3f, 0.3f));
            
            rightHandle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rightHandle.name = "RightHandle";
            rightHandle.transform.SetParent(handCamRoot.transform);
            rightHandle.transform.localPosition = new Vector3(0.35f, 0, 0);
            rightHandle.transform.localEulerAngles = new Vector3(0, 0, 90);
            rightHandle.transform.localScale = new Vector3(0.05f, 0.2f, 0.05f);
            ApplyCubeMaterial(rightHandle, new Color(0.3f, 0.3f, 0.3f));
            
            GameObject leftHandleGrip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            leftHandleGrip.name = "LeftHandleGrip";
            leftHandleGrip.transform.SetParent(handCamRoot.transform);
            leftHandleGrip.transform.localPosition = new Vector3(-0.35f, -0.15f, 0);
            leftHandleGrip.transform.localEulerAngles = new Vector3(0, 0, 90);
            leftHandleGrip.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
            ApplyCubeMaterial(leftHandleGrip, new Color(0.5f, 0.5f, 0.5f));
            
            GameObject rightHandleGrip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rightHandleGrip.name = "RightHandleGrip";
            rightHandleGrip.transform.SetParent(handCamRoot.transform);
            rightHandleGrip.transform.localPosition = new Vector3(0.35f, -0.15f, 0);
            rightHandleGrip.transform.localEulerAngles = new Vector3(0, 0, 90);
            rightHandleGrip.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
            ApplyCubeMaterial(rightHandleGrip, new Color(0.5f, 0.5f, 0.5f));
        }
        
        private void CreateButtons()
        {
            float[] xOffsets = { -0.12f, 0.12f, -0.12f, 0.12f };
            float[] yOffsets = { 0.22f, 0.22f, -0.22f, -0.22f };
            Color[] buttonColors = { Color.green, Color.cyan, Color.yellow, Color.magenta };
            
            for (int i = 0; i < 4; i++)
            {
                GameObject button = GameObject.CreatePrimitive(PrimitiveType.Cube);
                button.name = "Button_" + viewModeLabels[i];
                button.transform.SetParent(mainCube.transform);
                button.transform.localPosition = new Vector3(xOffsets[i], yOffsets[i], 0.51f);
                button.transform.localScale = new Vector3(0.08f, 0.08f, 0.02f);
                ApplyCubeMaterial(button, buttonColors[i]);
                
                buttons.Add(button);
                buttonRenderers.Add(button.GetComponent<Renderer>());
            }
        }
        
        private void CreateHandCamCamera()
        {
            currentCameraObject = new GameObject("HandCamCamera");
            currentCameraObject.transform.SetParent(handCamRoot.transform);
            currentCameraObject.transform.localPosition = new Vector3(0, 0, 0.3f);
            currentCameraObject.transform.localRotation = Quaternion.identity;
            
            currentCamera = currentCameraObject.AddComponent<Camera>();
            currentCamera.fieldOfView = 110f;
            currentCamera.nearClipPlane = 0.03f;
            currentCamera.farClipPlane = 1000f;
            currentCamera.cullingMask = -1;
            currentCamera.enabled = false;
            
            AudioListener listener = currentCameraObject.AddComponent<AudioListener>();
            listener.enabled = false;
        }
        
        private void ApplyCubeMaterial(GameObject obj, Color color)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                var shader = Shader.Find("Standard") ?? Shader.Find("Legacy Shaders/Diffuse");
                if (shader != null)
                {
                    Material mat = new Material(shader);
                    mat.color = color;
                    renderer.material = mat;
                }
            }
        }
        
        private void HandleTriggerInput()
        {
            VRRig localRig = GorillaTagger.Instance?.offlineVRRig;
            if (localRig == null) return;
            
            bool rightTriggerPressed = localRig.rightIndex?.triggerValue > 0.3f;
            bool leftTriggerPressed = localRig.leftIndex?.triggerValue > 0.3f;
            
            if (rightTriggerPressed || leftTriggerPressed)
            {
                isHeld = true;
                
                Transform handTransform = localRig.rightHandTransform;
                if (leftTriggerPressed)
                {
                    handTransform = localRig.leftHandTransform;
                }
                
                if (handTransform != null)
                {
                    handCamRoot.transform.position = handTransform.position + handTransform.forward * 0.3f;
                    handCamRoot.transform.rotation = handTransform.rotation;
                }
            }
            else
            {
                isHeld = false;
                holdingHand = null;
            }
        }
        
        private void HandleViewModeButtons()
        {
            if (Time.time - lastButtonPressTime < buttonPressCooldown) return;
            
            VRRig localRig = GorillaTagger.Instance?.offlineVRRig;
            if (localRig == null) return;
            
            bool rightIndexPressed = localRig.rightIndex?.triggerValue > 0.3f;
            bool leftIndexPressed = localRig.leftIndex?.triggerValue > 0.3f;
            
            if (!isHeld) return;
            
            if (rightIndexPressed || leftIndexPressed)
            {
                CheckButtonPress();
                lastButtonPressTime = Time.time;
            }
        }
        
        private void CheckButtonPress()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                if (buttons[i] == null) continue;
                
                Vector3 buttonWorldPos = buttons[i].transform.position;
                float dist = Vector3.Distance(buttonWorldPos, Camera.main.transform.position);
                
                if (dist < 0.5f)
                {
                    PressButton(i);
                    break;
                }
            }
        }
        
        private void PressButton(int buttonIndex)
        {
            switch (buttonIndex)
            {
                case 0:
                    targetFOV = (targetFOV == 110f) ? 70f : 110f;
                    if (CameraUpdater.cam != null)
                    {
                        CameraUpdater.FOV = targetFOV;
                    }
                    break;
                    
                case 1:
                    CameraUpdater.FP = !CameraUpdater.FP;
                    CameraUpdater.followhead = CameraUpdater.FP;
                    break;
                    
                case 2:
                    CameraUpdater.FP = false;
                    CameraUpdater.followhead = false;
                    break;
                    
                case 3:
                    ToggleHandCamActive();
                    break;
            }
        }
        
        private void ToggleHandCamActive()
        {
            handCamEnabled = !handCamEnabled;
            
            if (handCamRoot != null)
            {
                handCamRoot.SetActive(handCamEnabled);
            }
            
            if (handCamEnabled)
            {
                if (CameraUpdater.camObject != null)
                {
                    CameraUpdater.camObject.SetActive(false);
                }
            }
            else
            {
                if (CameraUpdater.camObject != null)
                {
                    CameraUpdater.camObject.SetActive(true);
                }
            }
        }
        
        private void UpdateCameraAttachment()
        {
            if (isHeld && holdingHand != null)
            {
                if (currentCamera != null)
                {
                    currentCamera.enabled = true;
                }
            }
            else
            {
                if (currentCamera != null)
                {
                    currentCamera.enabled = false;
                }
            }
        }
        
        private void UpdateButtonColors()
        {
            if (buttonRenderers.Count < 4) return;
            
            Color[] activeColors = new Color[]
            {
                targetFOV == 110f ? Color.green : Color.gray,
                CameraUpdater.FP ? Color.green : Color.gray,
                !CameraUpdater.FP && !CameraUpdater.followhead ? Color.green : Color.gray,
                handCamEnabled ? Color.green : Color.gray
            };
            
            for (int i = 0; i < buttonRenderers.Count; i++)
            {
                if (buttonRenderers[i] != null)
                {
                    buttonRenderers[i].material.color = activeColors[i];
                }
            }
        }
        
        public void SetHandCamEnabled(bool enabled)
        {
            handCamEnabled = enabled;
            
            if (handCamRoot != null)
            {
                handCamRoot.SetActive(enabled);
            }
        }
        
        public bool IsHandCamEnabled()
        {
            return handCamEnabled;
        }
        
        void OnDestroy()
        {
            if (handCamRoot != null)
            {
                Destroy(handCamRoot);
            }
        }
    }
}