using UnityEngine;
using UnityEngine.InputSystem;

namespace Sehili.Basic
{
    /// <summary>
    /// Drone-style controller for the predator.
    /// WASD moves in the horizontal plane, Q/E controls height, Mouse rotates.
    /// </summary>
    public class PredatorController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 10f;
        [SerializeField] private float _lookSensitivity = 2f;

        // Reference to the transform for movement
        private Transform _cachedTransform;
        private float _rotationX = 0f;
        private float _rotationY = 0f;
        private bool _isFocused = true;

        public Vector3 Position => _cachedTransform.position;

        private void Awake()
        {
            _cachedTransform = transform;
            // Lock cursor for better "First/Third Person" feel during demo
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            HandleFocusInput();

            // Only allow control if the application/simulation is currently focused
            if (!_isFocused) return;
            HandleMovement();
            HandleRotation();
        }

        private void HandleMovement()
        {
            Vector3 input = Vector3.zero;

            // WASD Movement
            if (Keyboard.current.wKey.isPressed) input.z += 1f;
            if (Keyboard.current.sKey.isPressed) input.z -= 1f;
            if (Keyboard.current.aKey.isPressed) input.x -= 1f;
            if (Keyboard.current.dKey.isPressed) input.x += 1f;

            // Q/E Height
            if (Keyboard.current.eKey.isPressed) input.y += 1f;
            if (Keyboard.current.qKey.isPressed) input.y -= 1f;

            // Apply movement local to rotation
            Vector3 movement = _cachedTransform.TransformDirection(input).normalized * _moveSpeed * Time.deltaTime;
            _cachedTransform.position += movement;
        }

        private void HandleRotation()
        {
            // Mouse look
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            _rotationY += mouseDelta.x * _lookSensitivity;
            _rotationX -= mouseDelta.y * _lookSensitivity;
            _rotationX = Mathf.Clamp(_rotationX, -80f, 80f);

            _cachedTransform.rotation = Quaternion.Euler(_rotationX, _rotationY, 0f);
        }

        /// <summary>
        /// Built-in Unity callback that triggers when the user clicks into or out of the application window.
        /// </summary>
        /// <param name="hasFocus">True if the window gained focus, false if it lost focus.</param>
        private void OnApplicationFocus(bool hasFocus)
        {
            // If we click back into the window, automatically re-engage the drone controls
            if (hasFocus)
            {
                SetCursorState(true);
            }
        }

        private void OnDisable()
        {
            // Ensure the cursor is safely returned to normal when the script or play mode stops
            SetCursorState(false);
        }

        // --- Private Methods ---

        /// <summary>
        /// Checks for the Escape key to unlock the cursor and pause drone inputs.
        /// </summary>
        private void HandleFocusInput()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                SetCursorState(false);
            }
        }

        /// <summary>
        /// Helper method to centralize cursor locking and visibility states.
        /// </summary>
        /// <param name="lockCursor">True to lock and hide, false to unlock and show.</param>
        private void SetCursorState(bool lockCursor)
        {
            _isFocused = lockCursor;
            Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !lockCursor;
        }
    }
}