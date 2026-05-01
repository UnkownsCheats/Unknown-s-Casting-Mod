using UnknownCasting.Core;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace UnknownCasting.Core.Cursor
{
    public class CursorMod : MonoBehaviour
    {
        public static CursorMod Instance { get; private set; }
        public bool cursorEnabled = false;

        private GameObject pointer;
        private GameObject triggerCollider;
        private Vector3 pos;

        void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            if (!cursorEnabled)
            {
                DestroyPointer();
                return;
            }

            if (pointer == null)
            {
                InitializeCursor();
                return;
            }

            UpdateCursorPosition();
            HandleCursorInteraction();
        }

        private void InitializeCursor()
        {
            if (GorillaTagger.Instance == null || GorillaTagger.Instance.rightHandTriggerCollider == null)
                return;

            triggerCollider = GorillaTagger.Instance.rightHandTriggerCollider;
            pointer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointer.transform.localScale = Vector3.one * 0.05f; // Fixed scale
            pointer.layer = LayerMask.NameToLayer("TransparentFX");
            UnityEngine.Object.Destroy(pointer.GetComponent<SphereCollider>());

            // Create a simple material for the cursor
            Renderer renderer = pointer.GetComponent<Renderer>();
            if (renderer != null)
            {
                var shader = Shader.Find("Standard") ?? Shader.Find("Legacy Shaders/Diffuse");
                if (shader != null)
                {
                    Material mat = new Material(shader);
                    mat.color = Color.white;
                    renderer.material = mat;
                }
            }
        }

        private void UpdateCursorPosition()
        {
            if (pointer == null) return;

            if (!XRSettings.isDeviceActive && CameraUpdater.cam != null)
            {
                // Desktop mode - use mouse position
                pointer.GetComponent<Renderer>().enabled = true;
                Vector2 mousePos = Mouse.current.position.ReadValue();
                Ray ray = CameraUpdater.cam.ScreenPointToRay(mousePos);

                // Try multiple layer combinations
                RaycastHit hit;
                int layerMask = LayerMask.GetMask("Default", "Water", "UI", "Gorilla Tag", "World", "Ignore Raycast", "Photon");
                
                if (Physics.Raycast(ray, out hit, 500f, layerMask))
                {
                    pos = hit.point;
                    pointer.transform.position = pos;
                }
                else
                {
                    // If no hit, try one more time with everything
                    if (Physics.Raycast(ray, out hit, 500f, -1))
                    {
                        pos = hit.point;
                        pointer.transform.position = pos;
                    }
                    else
                    {
                        pos = ray.origin + ray.direction * 5f;
                        pointer.transform.position = pos;
                    }
                }
            }
            else
            {
                // VR mode - use controller
                if (GorillaTagger.Instance != null && GorillaTagger.Instance.offlineVRRig != null)
                {
                    pointer.GetComponent<Renderer>().enabled = true;
                    RaycastHit hit;
                    
                    // Get controller transform using reflection
                    Transform controllerTransform = GetRightControllerTransform();
                    
                    int layerMask = LayerMask.GetMask("Default", "Water", "UI", "Gorilla Tag", "World", "Ignore Raycast", "Photon");
                    
                    if (controllerTransform != null && Physics.Raycast(controllerTransform.position,
                               controllerTransform.forward,
                               out hit, 500f, layerMask))
                    {
                        pos = hit.point;
                        pointer.transform.position = pos;
                    }
                    else
                    {
                        // If no hit, try one more time with everything
                        if (controllerTransform != null && Physics.Raycast(controllerTransform.position,
                                   controllerTransform.forward,
                                   out hit, 500f, -1))
                        {
                            pos = hit.point;
                            pointer.transform.position = pos;
                        }
                        else
                        {
                            // If no hit, place cursor at a default distance
                            if (controllerTransform != null)
                            {
                                pos = controllerTransform.position + controllerTransform.forward * 5f;
                            }
                            else
                            {
                                pos = GorillaTagger.Instance.offlineVRRig.headMesh.transform.position + GorillaTagger.Instance.offlineVRRig.headMesh.transform.forward * 5f;
                            }
                            pointer.transform.position = pos;
                        }
                    }
                }
            }
        }

        private Transform GetRightControllerTransform()
        {
            try
            {
                var player = GTPlayer.Instance;
                if (player == null) return null;
                
                // Try to get rightHandAnchor or similar property
                var prop = player.GetType().GetProperty("rightHandAnchor", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop != null)
                {
                    return prop.GetValue(player) as Transform;
                }
                
                // Try field
                var field = player.GetType().GetField("rightControllerTransform", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    return field.GetValue(player) as Transform;
                }
                
                // Fall back to using GorillaTagger's player object
                var playerObj = player.GetType().GetProperty("playerObject", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (playerObj != null)
                {
                    var obj = playerObj.GetValue(player);
                    if (obj is GameObject go)
                    {
                        Transform t = go.transform.Find("RightHandAnchor");
                        if (t != null) return t;
                    }
                }
            }
            catch { }
            return null;
        }

        private void HandleCursorInteraction()
        {
            if (triggerCollider == null) return;

            bool isInteracting = false;

            if (!XRSettings.isDeviceActive)
            {
                // Desktop interaction
                isInteracting = Mouse.current.leftButton.isPressed;
            }
            else
            {
                // VR interaction
                if (GorillaTagger.Instance != null && GorillaTagger.Instance.offlineVRRig != null)
                {
                    isInteracting = GorillaTagger.Instance.offlineVRRig.rightMiddle.gripValue > 0.2f &&
                                  GorillaTagger.Instance.offlineVRRig.rightIndex.triggerValue > 0.3f;
                }
            }

            if (isInteracting)
            {
                TransformFollow tf = triggerCollider.GetComponent<TransformFollow>();
                if (tf != null && tf.enabled)
                {
                    tf.enabled = false;
                    if (pointer != null)
                    {
                        Renderer renderer = pointer.GetComponent<Renderer>();
                        if (renderer != null) renderer.material.color = Color.green;
                    }
                }
                triggerCollider.transform.position = pos;
            }
            else
            {
                TransformFollow tf = triggerCollider.GetComponent<TransformFollow>();
                if (tf != null && !tf.enabled)
                {
                    if (pointer != null)
                    {
                        Renderer renderer = pointer.GetComponent<Renderer>();
                        if (renderer != null) renderer.material.color = Color.white;
                    }
                    tf.enabled = true;
                }
            }
        }

        public void ToggleCursor()
        {
            cursorEnabled = !cursorEnabled;
            if (!cursorEnabled)
            {
                DestroyPointer();
            }
        }

        public void DestroyPointer()
        {
            if (pointer != null)
            {
                if (triggerCollider != null)
                {
                    TransformFollow tf = triggerCollider.GetComponent<TransformFollow>();
                    if (tf != null) tf.enabled = true;
                }
                UnityEngine.Object.Destroy(pointer);
                pointer = null;
            }
        }

        void OnDisable()
        {
            DestroyPointer();
        }
    }
}