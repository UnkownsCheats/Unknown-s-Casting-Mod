using GorillaLocomotion;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

namespace UnknownCasting.Core
{
    public class WASD
    {
        public static float accel = 10f;
        public static float maxSpeed = 8f;
        public static float smoothing = 0.5f;
        public static bool noGravity = false;
        public static bool fly = false;
        private static Vector3 velocity;
        public static float sensitivity = 1.5f;
        private static float yaw;
        private static bool isInitialized = false;
        private static Transform cameraTransform;
        private static bool wasdModeActive = false;
        private static bool wasdModeJustDisabled = false;
        private static Vector3 flyVelocity;
        private static float flyYaw;
        private static bool flyInitialized = false;

        public static void Wasd()
        {
            if (GTPlayer.Instance == null || Keyboard.current == null || Mouse.current == null)
                return;

            Transform playerTransform = GTPlayer.Instance.transform;
            Transform headTransform = GTPlayer.Instance.headCollider.transform;
            
            Camera cam = Camera.main;
            if (cam == null) return;
            
            if (!isInitialized)
            {
                yaw = headTransform.eulerAngles.y;
                isInitialized = true;
            }

            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            bool rightClickHeld = Mouse.current.rightButton.isPressed;
            
            if (mouseDelta.x != 0 && rightClickHeld)
            {
                yaw += mouseDelta.x * sensitivity;
                playerTransform.rotation = Quaternion.Euler(0f, yaw, 0f);
            }

            Vector3 moveForward = cam.transform.forward;
            moveForward.y = 0;
            moveForward.Normalize();
            
            Vector3 moveRight = cam.transform.right;
            moveRight.y = 0;
            moveRight.Normalize();

            Vector3 input = Vector3.zero;
            if (Keyboard.current.wKey.isPressed) input += moveForward;
            if (Keyboard.current.sKey.isPressed) input -= moveForward;
            if (Keyboard.current.dKey.isPressed) input += moveRight;
            if (Keyboard.current.aKey.isPressed) input -= moveRight;
            
            if (Keyboard.current.spaceKey.isPressed) input += Vector3.up;
            if (Keyboard.current.leftCtrlKey.isPressed) input -= Vector3.up;

            float speedMultiplier = Keyboard.current.leftShiftKey.isPressed ? 2.5f : 1f;

            Vector3 desiredVelocity = input.normalized * maxSpeed * speedMultiplier;
            velocity = Vector3.Lerp(velocity, desiredVelocity, smoothing * Time.deltaTime * 10f);
            playerTransform.position += velocity * Time.deltaTime;
        }

        public static void Reset()
        {
            isInitialized = false;
            wasdModeActive = false;
            wasdModeJustDisabled = false;
            flyInitialized = false;
            flyVelocity = Vector3.zero;
        }

        public static void Fly()
        {
            if (!fly || GTPlayer.Instance == null || Keyboard.current == null || Mouse.current == null)
                return;

            Transform playerTransform = GTPlayer.Instance.transform;
            Transform headTransform = GTPlayer.Instance.headCollider.transform;
            Rigidbody rb = GTPlayer.Instance.GetComponent<Rigidbody>();
            
            Camera cam = Camera.main;
            if (cam == null) return;

            if (!flyInitialized)
            {
                flyYaw = headTransform.eulerAngles.y;
                flyVelocity = Vector3.zero;
                flyInitialized = true;
            }

            if (rb != null)
                rb.velocity = Vector3.zero;

            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            bool rightClickHeld = Mouse.current.rightButton.isPressed;

            if (mouseDelta.x != 0 && rightClickHeld)
            {
                flyYaw += mouseDelta.x * sensitivity;
                playerTransform.rotation = Quaternion.Euler(0f, flyYaw, 0f);
            }

            Vector3 moveForward = cam.transform.forward;
            moveForward.y = 0;
            moveForward.Normalize();
            
            Vector3 moveRight = cam.transform.right;
            moveRight.y = 0;
            moveRight.Normalize();

            Vector3 input = Vector3.zero;
            if (Keyboard.current.wKey.isPressed) input += moveForward;
            if (Keyboard.current.sKey.isPressed) input -= moveForward;
            if (Keyboard.current.dKey.isPressed) input += moveRight;
            if (Keyboard.current.aKey.isPressed) input -= moveRight;
            if (Keyboard.current.spaceKey.isPressed) input += Vector3.up;
            if (Keyboard.current.leftCtrlKey.isPressed) input -= Vector3.up;

            float speedMultiplier = Keyboard.current.leftShiftKey.isPressed ? 2.5f : 1f;

            Vector3 desiredVelocity = input.normalized * maxSpeed * speedMultiplier;
            flyVelocity = Vector3.Lerp(flyVelocity, desiredVelocity, smoothing * Time.deltaTime * 10f);
            headTransform.position += flyVelocity * Time.deltaTime;
        }
    }
}