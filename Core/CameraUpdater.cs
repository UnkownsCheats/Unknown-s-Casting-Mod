using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using GorillaNetworking;
using Photon.Pun;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;

namespace UnknownCasting.Core
{
    public static class CameraUpdater
    {
        public static GameObject camObject;
        public static Camera cam;
        public static AudioListener listener;
        public static RenderTexture renderTexture;

        // Store reference to main camera to restore it later
        private static Camera mainCamera;
        private static AudioListener mainListener;
        private static bool wasMainCameraEnabled;

        // VR mode detection - Disabled to force desktop mode
        public static bool isVRMode = false;
        private static bool vrCheckDone = false;
        
        // Manual override for VR mode - Disabled by default to force desktop mode
        public static bool forceVRMode = false;
        public static bool forceDesktopMode = true;

        public static float x, y = .6f, z = 2;
        public static bool followhead = false;
        public static bool FP = false;
        public static bool TPFront = false;
        public static bool hidePlayerCosmetics = false;
        public static float positionSmoothing = 0.5f;
        public static float rotationSmoothing = 0.5f;
        public static bool enableSmoothing = true;
        public static int smoothingType = 1;

        // Self-rig smoothing (Template-style)
        public static bool enableSelfRigSmoothing = false;
        public static float selfRigLerpAmount = 0f;
        private static bool localSmoothingReady = false;
        private static Vector3 smoothHeadPos;
        private static Quaternion smoothHeadRot;
        private static Quaternion smoothBodyRot;
        private static Vector3 smoothLeftHandPos;
        private static Quaternion smoothLeftHandRot;
        private static Vector3 smoothRightHandPos;
        private static Quaternion smoothRightHandRot;
        private static readonly Quaternion handOffset = Quaternion.Euler(180f, 180f, 0f);
        
        // Fly mode - allows WASD movement in mid-air
        public static bool flyMode = false;
        public static float flySpeed = 5f;

        public static float FOV = 115;
        public static float nearClip = 0.03f;

        public static VRRig rig;
        public static List<VRRig> players;

        private static Vector3 velocity;
        private static Vector4 rotationVelocity;

        public static bool isWASDMode = false;

        // Freecam smoothing
        private static Vector3 freecamVelocity;
        private static Vector4 freecamRotVelocity;

        private static float yaw, pitch;
        public static float moveSpeed = 10f;
        public static float lookSensitivity = 0.1f;

        private static GameObject cameraModel;

        // Auto-start timer
        private static float initTime;
        private static bool autoStartAttempted = false;

        // Manual camera controls
        private static bool manualControlsEnabled = false;
        private static float manualMoveSpeed = 0.1f;

        // Roll lock (anti-roll)
        public static bool rollLock = false;

        // Remote rig update rate throttling
        private static float updateRate = 0.15f;
        private static float nextUpdateTime;

        public static void StartCamera()
        {
            // Force desktop mode - VR mode detection disabled
            isVRMode = false;
            forceVRMode = false;
            forceDesktopMode = true;
            
            Debug.Log($"[UnknownCasting] Starting camera - VR mode: {isVRMode}, camObject: {camObject != null}, Camera.main: {Camera.main}");

            // Only create camera for desktop mode, like Loi Caster
            if (isVRMode)
            {
                Debug.Log("[UnknownCasting] VR mode detected - skipping camera creation (desktop only)");
                return;
            }

            if (camObject != null)
            {
                Debug.Log("[UnknownCasting] Destroying existing camera object");
                UnityEngine.Object.Destroy(camObject);
                camObject = null;
                cam = null;
                listener = null;
            }

            Debug.Log("[UnknownCasting] Creating new camera object");
            camObject = new GameObject("UnknownCastingCamera");
            cam = camObject.AddComponent<Camera>();
            listener = camObject.AddComponent<AudioListener>();

            MotionBlurEffect motionBlur = camObject.AddComponent<MotionBlurEffect>();

            // Desktop Mode: Create render texture camera like Loi Caster
            if (Camera.main == null)
            {
                Debug.Log("[UnknownCasting] No main camera found - using fallback");
                // Create a temporary camera if no main camera exists
                cam.cameraType = CameraType.Game;
                cam.cullingMask = -1; // Render everything
            }
            else
            {
                Debug.Log("[UnknownCasting] Found main camera - using its settings");
                cam.cameraType = CameraType.Game;
                cam.cullingMask = Camera.main.cullingMask;
                mainListener = Camera.main.GetComponent<AudioListener>();
                if (mainListener != null)
                {
                    mainListener.enabled = false;
                }
            }

            // Create render texture
            renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            cam.targetTexture = renderTexture;

            cam.depth = 0;
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.enabled = true;

            Debug.Log("[UnknownCasting] Desktop camera ready (rendering to texture)");

            GameObject shoulderCam = GameObject.Find("Shoulder Camera");
            if (shoulderCam != null)
                shoulderCam.SetActive(false);

            CreateCameraModel();
        }

        public static void ForceVRMode(bool force)
        {
            forceVRMode = force;
            forceDesktopMode = !force;
            StartCamera();
        }

        public static void ForceDesktopMode(bool force)
        {
            forceDesktopMode = force;
            forceVRMode = !force;
            StartCamera();
        }

        public static void StopCamera()
        {
            if (camObject != null)
            {
                UnityEngine.Object.Destroy(camObject);
                camObject = null;
                cam = null;
                listener = null;
            }

            // Clean up render texture
            if (renderTexture != null)
            {
                UnityEngine.Object.Destroy(renderTexture);
                renderTexture = null;
            }

            // Restore main camera audio listener
            if (mainListener != null)
            {
                mainListener.enabled = true;
            }
        }

        private static bool CheckIfVRMode()
        {
            // Check if XR devices are active - this is more reliable
            try
            {
                var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
                SubsystemManager.GetInstances(xrDisplaySubsystems);
                
                foreach (var subsystem in xrDisplaySubsystems)
                {
                    if (subsystem.running)
                    {
                        Debug.Log("[UnknownCasting] XR Display subsystem running - VR mode detected");
                        return true;
                    }
                }
            }
            catch (Exception ex) { Debug.Log($"[UnknownCasting] XR check error: {ex.Message}"); }

            // Check for head mounted device
            try
            {
                var inputDevices = new List<UnityEngine.XR.InputDevice>();
                UnityEngine.XR.InputDevices.GetDevicesWithCharacteristics(UnityEngine.XR.InputDeviceCharacteristics.HeadMounted, inputDevices);
                
                if (inputDevices.Count > 0)
                {
                    Debug.Log($"[UnknownCasting] Found {inputDevices.Count} head mounted devices - VR mode");
                    return true;
                }
            }
            catch (Exception ex) { Debug.Log($"[UnknownCasting] Input device check error: {ex.Message}"); }

            // Fallback: check if running with VR headset (environment variable or process check)
            try
            {
                // Check SteamVR runtime
                var steamVR = Type.GetType("Valve.VR.CVRSystem, SteamVR");
                if (steamVR != null)
                {
                    Debug.Log("[UnknownCasting] SteamVR detected - VR mode");
                    return true;
                }
            }
            catch { }

            Debug.Log("[UnknownCasting] No VR detected - desktop mode");
            return false;
        }

        private static void CreateCameraModel()
        {
            cameraModel = new GameObject("UnknownCastingCameraModel");
            cameraModel.transform.SetParent(camObject.transform);
            cameraModel.transform.localPosition = Vector3.zero;
            cameraModel.transform.localRotation = Quaternion.identity;

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "CameraBody";
            body.transform.SetParent(cameraModel.transform);
            body.transform.localPosition = new Vector3(0, 0, -0.3f);
            body.transform.localScale = new Vector3(0.4f, 0.25f, 0.5f);
            ApplyMaterial(body, new Color(0.1f, 0.1f, 0.1f));

            GameObject lens = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lens.name = "CameraLens";
            lens.transform.SetParent(cameraModel.transform);
            lens.transform.localPosition = new Vector3(0, 0, 0.05f);
            lens.transform.localEulerAngles = new Vector3(90, 0, 0);
            lens.transform.localScale = new Vector3(0.15f, 0.25f, 0.15f);
            ApplyMaterial(lens, Color.black);

            GameObject viewfinder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            viewfinder.name = "Viewfinder";
            viewfinder.transform.SetParent(cameraModel.transform);
            viewfinder.transform.localPosition = new Vector3(-0.2f, 0.1f, -0.4f);
            viewfinder.transform.localScale = new Vector3(0.1f, 0.05f, 0.15f);
            ApplyMaterial(viewfinder, new Color(0.15f, 0.15f, 0.15f));

            UnityEngine.Object.Destroy(body.GetComponent<Collider>());
            UnityEngine.Object.Destroy(lens.GetComponent<Collider>());
            UnityEngine.Object.Destroy(viewfinder.GetComponent<Collider>());
        }

        public static void Update()
        {
            // Record init time on first update
            if (initTime == 0f) initTime = Time.time;

            // Auto-start camera after 2 seconds if not started yet
            if (camObject == null && !autoStartAttempted && Time.time - initTime >= 2f)
            {
                StartCamera();
                autoStartAttempted = true;
            }

            // Only update camera if we're in desktop mode and camera exists
            if (!isVRMode && camObject != null)
            {
                RunCamera();
            }

            // Handle manual camera controls (only in desktop mode)
            if (!isVRMode)
            {
                HandleManualControls();
            }

            // Update rig lerp smoothing
            UnknownCasting.Core.RigLerp.Update();

            // Update self-rig smoothing (Template method)
            UpdateSelfRigSmoothing();

            if (!isVRMode)
            {
                UpdatePlayerList();
            }

            if (Dev.Plugin.freecam && !isVRMode)
            {
                FreecamControls();
            }

            if (cameraModel != null)
            {
                cameraModel.SetActive(Dev.Plugin.ShowCameraObj);
            }

            if (rig == GorillaTagger.Instance?.offlineVRRig)
            {
                return;
            }

            if (PhotonNetworkController.Instance != null)
            {
                PhotonNetworkController.Instance.disableAFKKick = true;
            }
        }

        static Quaternion ApplyRollLock(Quaternion rot)
        {
            if (!rollLock) return rot;
            Vector3 euler = rot.eulerAngles;
            euler.z = 0f;
            return Quaternion.Euler(euler);
        }

        private static void HandleManualControls()
        {
            if (!manualControlsEnabled || Keyboard.current == null) return;

            // Manual camera position controls
            if (Keyboard.current.wKey.isPressed)
                z -= manualMoveSpeed; // W moves camera forward (decreases Z)
            if (Keyboard.current.sKey.isPressed)
                z += manualMoveSpeed; // S moves camera backward (increases Z)
            if (Keyboard.current.aKey.isPressed)
                x -= manualMoveSpeed; // A moves camera left (decreases X)
            if (Keyboard.current.dKey.isPressed)
                x += manualMoveSpeed; // D moves camera right (increases X)
            if (Keyboard.current.eKey.isPressed)
                y += manualMoveSpeed; // E moves camera up (increases Y)
            if (Keyboard.current.qKey.isPressed)
                y -= manualMoveSpeed; // Q moves camera down (decreases Y)

            // Reset camera position
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                x = 0f;
                y = 0.6f;
                z = 2f;
            }
        }

        public static void SetManualControlsEnabled(bool enabled)
        {
            manualControlsEnabled = enabled;
        }

        public static bool GetManualControlsEnabled()
        {
            return manualControlsEnabled;
        }

        public static void SetManualMoveSpeed(float speed)
        {
            manualMoveSpeed = speed;
        }

        public static float GetManualMoveSpeed()
        {
            return manualMoveSpeed;
        }

        // Self-rig smoothing – virtual clone (original rig never modified)
        public static void UpdateSelfRigSmoothing()
        {
            GorillaTagger instance = GorillaTagger.Instance;
            VRRig vrrig = instance?.offlineVRRig;
            if (vrrig == null) return;

            if (!enableSelfRigSmoothing)
            {
                vrrig.enabled = true;
                localSmoothingReady = false;
                return;
            }

            vrrig.enabled = false;
            if (!localSmoothingReady)
            {
                SetupSmoothRig();
            }

            Transform head = instance.headCollider.transform;
            Transform left = instance.leftHandTransform;
            Transform right = instance.rightHandTransform;

            float amt = selfRigLerpAmount;
            if (amt >= 0f)
            {
                float factor = Mathf.Lerp(30f, 0.5f, amt / 5f);
                float dt = Time.deltaTime;
                smoothHeadPos = Vector3.Lerp(smoothHeadPos, head.position, dt * factor);
                smoothHeadRot = Quaternion.Slerp(smoothHeadRot, head.rotation, dt * factor);
                smoothBodyRot = Quaternion.Slerp(smoothBodyRot, Quaternion.Euler(0f, head.rotation.eulerAngles.y, 0f), dt * factor);
                smoothLeftHandPos = Vector3.Lerp(smoothLeftHandPos, left.position, dt * factor);
                smoothLeftHandRot = Quaternion.Slerp(smoothLeftHandRot, left.rotation, dt * factor);
                smoothRightHandPos = Vector3.Lerp(smoothRightHandPos, right.position, dt * factor);
                smoothRightHandRot = Quaternion.Slerp(smoothRightHandRot, right.rotation, dt * factor);
            }
            else
            {
                float chance = 1f - Mathf.Abs(amt) / 5.1f;
                if (UnityEngine.Random.value < chance)
                {
                    smoothHeadPos = head.position;
                    smoothHeadRot = head.rotation;
                    smoothBodyRot = Quaternion.Euler(0f, head.rotation.eulerAngles.y, 0f);
                    smoothLeftHandPos = left.position;
                    smoothLeftHandRot = left.rotation;
                    smoothRightHandPos = right.position;
                    smoothRightHandRot = right.rotation;
                }
            }

            vrrig.transform.position = smoothHeadPos - new Vector3(0f, 0.15f, 0f);
            vrrig.transform.rotation = smoothBodyRot;
            vrrig.head.rigTarget.rotation = smoothHeadRot;
            vrrig.leftHand.rigTarget.position = smoothLeftHandPos;
            vrrig.leftHand.rigTarget.rotation = smoothLeftHandRot * handOffset;
            vrrig.rightHand.rigTarget.position = smoothRightHandPos;
            vrrig.rightHand.rigTarget.rotation = smoothRightHandRot * handOffset;
        }

        private static void SetupSmoothRig()
        {
            var instance = GorillaTagger.Instance;
            if (instance == null) return;

            Transform transform = instance.headCollider.transform;
            Transform leftHandTransform = instance.leftHandTransform;
            Transform rightHandTransform = instance.rightHandTransform;

            smoothHeadPos = transform.position;
            smoothHeadRot = transform.rotation;
            smoothBodyRot = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
            smoothLeftHandPos = leftHandTransform.position;
            smoothLeftHandRot = leftHandTransform.rotation;
            smoothRightHandPos = rightHandTransform.position;
            smoothRightHandRot = rightHandTransform.rotation;

            localSmoothingReady = true;
        }

        private static void ApplyMaterial(GameObject obj, Color color)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            obj.GetComponent<MeshRenderer>().material = mat;
        }

        static void UpdatePlayerList()
        {
            if (Keyboard.current == null) return;

            var vrrigs = GetAllVRRigs();

            // Helper to check both digit row and numpad keys
            bool IsPressed(int digit)
            {
                return digit switch
                {
                    0 => Keyboard.current.digit0Key.wasPressedThisFrame || Keyboard.current.numpad0Key.wasPressedThisFrame,
                    1 => Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame,
                    2 => Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame,
                    3 => Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame,
                    4 => Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame,
                    5 => Keyboard.current.digit5Key.wasPressedThisFrame || Keyboard.current.numpad5Key.wasPressedThisFrame,
                    6 => Keyboard.current.digit6Key.wasPressedThisFrame || Keyboard.current.numpad6Key.wasPressedThisFrame,
                    7 => Keyboard.current.digit7Key.wasPressedThisFrame || Keyboard.current.numpad7Key.wasPressedThisFrame,
                    8 => Keyboard.current.digit8Key.wasPressedThisFrame || Keyboard.current.numpad8Key.wasPressedThisFrame,
                    9 => Keyboard.current.digit9Key.wasPressedThisFrame || Keyboard.current.numpad9Key.wasPressedThisFrame,
                    _ => false
                };
            }

            // 0 = spectate yourself (index 0 in list is local player)
            if (IsPressed(0) && vrrigs.Count > 0)
                rig = vrrigs[0];

            // 1-9 = spectate other players (offset by 1 since index 0 is local player)
            if (IsPressed(1) && vrrigs.Count > 1) rig = vrrigs[1];
            if (IsPressed(2) && vrrigs.Count > 2) rig = vrrigs[2];
            if (IsPressed(3) && vrrigs.Count > 3) rig = vrrigs[3];
            if (IsPressed(4) && vrrigs.Count > 4) rig = vrrigs[4];
            if (IsPressed(5) && vrrigs.Count > 5) rig = vrrigs[5];
            if (IsPressed(6) && vrrigs.Count > 6) rig = vrrigs[6];
            if (IsPressed(7) && vrrigs.Count > 7) rig = vrrigs[7];
            if (IsPressed(8) && vrrigs.Count > 8) rig = vrrigs[8];
            if (IsPressed(9) && vrrigs.Count > 9) rig = vrrigs[9];
        }

        static List<VRRig> GetAllVRRigs()
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

        public static void RunCamera()
        {
            if (cam == null) return;

            cam.fieldOfView = FOV;
            cam.nearClipPlane = nearClip;

            if (Dev.Plugin.freecam) return;

            VRRig targetRig = rig;
            if (targetRig == null || targetRig.headMesh == null)
            {
                var fallback = GorillaTagger.Instance?.offlineVRRig;
                if (fallback == null || fallback.headMesh == null) return;
                targetRig = fallback;
            }

            bool isLocal = (targetRig == GorillaTagger.Instance?.offlineVRRig);

            Vector3 targetPosition;
            Quaternion targetRotation;

            if (isLocal)
            {
                // If self-rig smoothing enabled, use virtual clone (original rig untouched)
                if (enableSelfRigSmoothing)
                {
                    Vector3 smoothedBodyPos = smoothHeadPos - new Vector3(0f, 0.15f, 0f);
                    if (FP)
                    {
                        targetPosition = smoothHeadPos + Vector3.up * 0.1f;
                        targetRotation = smoothHeadRot;
                        targetRotation = ApplyRollLock(targetRotation);
                        cam.transform.SetPositionAndRotation(targetPosition, targetRotation);
                        if (hidePlayerCosmetics)
                        {
                            HidePlayerCosmetics(targetRig);
                        }
                        return;
                    }
                    if (TPFront)
                    {
                        float frontZ = Mathf.Abs(z);
                        if (followhead)
                        {
                            targetPosition = smoothHeadPos + smoothHeadRot * Vector3.forward * frontZ * targetRig.scaleFactor
                                             + smoothHeadRot * Vector3.up * y
                                             + smoothHeadRot * Vector3.right * x;
                            targetRotation = Quaternion.LookRotation(smoothHeadPos - targetPosition);
                        }
                        else
                        {
                            targetPosition = smoothedBodyPos + smoothBodyRot * Vector3.forward * frontZ * targetRig.scaleFactor
                                             + smoothBodyRot * Vector3.up * y
                                             + smoothBodyRot * Vector3.right * x;
                            targetRotation = Quaternion.LookRotation(smoothedBodyPos - targetPosition);
                        }
                        ApplySmoothing(targetPosition, targetRotation);
                        return;
                    }
                    // Normal TP
                    if (followhead)
                    {
                        targetPosition = smoothHeadPos + smoothHeadRot * Vector3.forward * z * targetRig.scaleFactor
                                         + smoothHeadRot * Vector3.up * y
                                         + smoothHeadRot * Vector3.right * x;
                        targetRotation = Quaternion.LookRotation(smoothHeadPos - targetPosition);
                    }
                    else
                    {
                        targetPosition = smoothedBodyPos + smoothBodyRot * Vector3.forward * z * targetRig.scaleFactor
                                         + smoothBodyRot * Vector3.up * y
                                         + smoothBodyRot * Vector3.right * x;
                        targetRotation = Quaternion.LookRotation(smoothedBodyPos - targetPosition);
                    }
                    ApplySmoothing(targetPosition, targetRotation);
                    return;
                }
                else
                {
                    // Original local rig handling (no self-rig smoothing)
                    if (FP)
                    {
                        targetPosition = targetRig.headMesh.transform.position + Vector3.up * 0.1f;
                        targetRotation = targetRig.headMesh.transform.rotation;
                        targetRotation = ApplyRollLock(targetRotation);
                        cam.transform.SetPositionAndRotation(targetPosition, targetRotation);
                        if (hidePlayerCosmetics)
                        {
                            HidePlayerCosmetics(targetRig);
                        }
                        return;
                    }

                    if (TPFront)
                    {
                        float frontZ = Mathf.Abs(z);
                        if (followhead)
                        {
                            targetPosition = targetRig.headMesh.transform.position
                                             + targetRig.headMesh.transform.forward * frontZ * targetRig.scaleFactor
                                             + targetRig.headMesh.transform.up * y
                                             + targetRig.headMesh.transform.right * x;
                            targetRotation = Quaternion.LookRotation(targetRig.headMesh.transform.position - targetPosition);
                        }
                        else
                        {
                            targetPosition = targetRig.transform.position
                                             + targetRig.transform.forward * frontZ * targetRig.scaleFactor
                                             + targetRig.transform.up * y
                                             + targetRig.transform.right * x;
                            targetRotation = Quaternion.LookRotation(targetRig.transform.position - targetPosition);
                        }
                        ApplySmoothing(targetPosition, targetRotation);
                        return;
                    }

                    if (followhead)
                    {
                        targetPosition = targetRig.headMesh.transform.position
                                         + targetRig.headMesh.transform.forward * z * targetRig.scaleFactor
                                         + targetRig.headMesh.transform.up * y
                                         + targetRig.headMesh.transform.right * x;
                        targetRotation = Quaternion.LookRotation(targetRig.headMesh.transform.position - targetPosition);
                    }
                    else
                    {
                        targetPosition = targetRig.transform.position
                                         + targetRig.transform.forward * z * targetRig.scaleFactor
                                         + targetRig.transform.up * y
                                         + targetRig.transform.right * x;
                        targetRotation = Quaternion.LookRotation(targetRig.transform.position - targetPosition);
                    }
                    ApplySmoothing(targetPosition, targetRotation);
                    return;
                }
            }

            if (Time.time >= nextUpdateTime)
                nextUpdateTime = Time.time + updateRate;

            if (followhead)
            {
                targetPosition = targetRig.headMesh.transform.position
                                 + targetRig.headMesh.transform.forward * z * targetRig.scaleFactor
                                 + targetRig.headMesh.transform.up * y
                                 + targetRig.headMesh.transform.right * x;
                targetRotation = Quaternion.LookRotation(targetRig.headMesh.transform.position - targetPosition);
            }
            else
            {
                targetPosition = targetRig.transform.position
                                 + targetRig.transform.forward * z * targetRig.scaleFactor
                                 + targetRig.transform.up * y
                                 + targetRig.transform.right * x;
                targetRotation = Quaternion.LookRotation(targetRig.transform.position - targetPosition);
            }
            ApplySmoothing(targetPosition, targetRotation);
        }

        private static void ApplySmoothing(Vector3 targetPosition, Quaternion targetRotation)
        {
            // Apply roll lock if enabled
            if (rollLock)
            {
                Vector3 e = targetRotation.eulerAngles;
                e.z = 0f;
                targetRotation = Quaternion.Euler(e);
            }

            if (enableSmoothing)
            {
                switch (smoothingType)
                {
                    case 1:
                        cam.transform.position = Vector3.Slerp(cam.transform.position, targetPosition, Time.deltaTime / (positionSmoothing * 0.75f));
                        cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, targetRotation, Time.deltaTime / (rotationSmoothing * 0.75f));
                        break;
                    case 2:
                        Vector3 lerpPos = Vector3.Lerp(cam.transform.position, targetPosition, 1f - positionSmoothing);
                        cam.transform.position = Vector3.Lerp(cam.transform.position, lerpPos, 1f - positionSmoothing);
                        cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, targetRotation, 1f - rotationSmoothing);
                        break;
                    case 3:
                        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, targetPosition, ref velocity, positionSmoothing);
                        cam.transform.rotation = SmoothDampRotation(cam.transform.rotation, targetRotation, ref rotationVelocity, rotationSmoothing);
                        break;
                }
            }
            else
            {
                cam.transform.SetPositionAndRotation(targetPosition, targetRotation);
            }
        }

        static void FreecamControls()
        {
            if (Keyboard.current == null || Mouse.current == null) return;

            Vector3 move = Vector3.zero;
            if (Keyboard.current.wKey.isPressed) move += cam.transform.forward;
            if (Keyboard.current.sKey.isPressed) move -= cam.transform.forward;
            if (Keyboard.current.aKey.isPressed) move -= cam.transform.right;
            if (Keyboard.current.dKey.isPressed) move += cam.transform.right;
            if (Keyboard.current.spaceKey.isPressed) move += cam.transform.up;
            if (Keyboard.current.leftCtrlKey.isPressed) move -= cam.transform.up;

            // Apply movement with lerp-based smoothing for a nicer feel
            Vector3 targetPos = move * Time.deltaTime * moveSpeed;
            if (positionSmoothing > 0.01f && enableSmoothing)
            {
                // Use smooth lerp factor - higher smoothing = more gradual response
                float lerpFactor = 1f - positionSmoothing;
                freecamVelocity = Vector3.Lerp(freecamVelocity, targetPos, lerpFactor * Time.deltaTime * 10f);
                cam.transform.position += freecamVelocity;
            }
            else
            {
                // Direct movement when smoothing is disabled
                cam.transform.position += targetPos;
            }

            // Mouse look - always active in freecam
            if (Mouse.current.rightButton.isPressed || Mouse.current.leftButton.isPressed)
            {
                yaw += Mouse.current.delta.x.ReadValue() * lookSensitivity;
                pitch -= Mouse.current.delta.y.ReadValue() * lookSensitivity;
                pitch = Mathf.Clamp(pitch, -89f, 89f);
                cam.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            }
        }

        static void HandleNestInputs()
        {
            // Nests feature has been removed
        }

        public static void ToggleNestVisibility()
        {
            // Nests feature has been removed
        }

        public static void SaveNest()
        {
            // Nests feature has been removed
        }

        public static void CycleNest()
        {
            // Nests feature has been removed
        }

        public static void DeleteLatestNest()
        {
            // Nests feature has been removed
        }

        public static void ClearAllNests()
        {
            // Nests feature has been removed
        }

        private static Quaternion SmoothDampRotation(Quaternion current, Quaternion target, ref Vector4 velocity, float smoothTime)
        {
            float dot = Quaternion.Dot(current, target);
            float sign = (dot > 0f) ? 1f : -1f;
            target = new Quaternion(target.x * sign, target.y * sign, target.z * sign, target.w * sign);

            Vector4 result = new Vector4(
                Mathf.SmoothDamp(current.x, target.x, ref velocity.x, smoothTime),
                Mathf.SmoothDamp(current.y, target.y, ref velocity.y, smoothTime),
                Mathf.SmoothDamp(current.z, target.z, ref velocity.z, smoothTime),
                Mathf.SmoothDamp(current.w, target.w, ref velocity.w, smoothTime)
            );

            return Quaternion.Normalize(new Quaternion(result.x, result.y, result.z, result.w));
        }

        private static void HidePlayerCosmetics(VRRig targetRig)
        {
            if (targetRig == null) return;
            
            try
            {
                foreach (MeshRenderer mr in targetRig.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (mr.gameObject.name.ToLower().Contains("cosmetic") || mr.gameObject.name.ToLower().Contains("monke") || mr.gameObject.name.ToLower().Contains("trail") || mr.gameObject.name.ToLower().Contains("tag") || mr.gameObject.name.ToLower().Contains("infector"))
                    {
                        mr.enabled = false;
                    }
                }
                
                foreach (SkinnedMeshRenderer smr in targetRig.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    if (smr.gameObject.name.ToLower().Contains("cosmetic") || smr.gameObject.name.ToLower().Contains("monke") || smr.gameObject.name.ToLower().Contains("trail") || smr.gameObject.name.ToLower().Contains("tag") || smr.gameObject.name.ToLower().Contains("infector"))
                    {
                        smr.enabled = false;
                    }
                }
            }
            catch { }
        }
    }
}