using UnityEngine;
using UnityEngine.InputSystem;
using GPOyun.Core;
using GPOyun.UI;

namespace GPOyun.Player
{
    /// <summary>
    /// A1 Level Player Controller — uses the New Input System package.
    /// WASD / Arrow keys: move (Forward, Backward, Strafe Left/Right). 
    /// Mouse X: rotates the player/screen.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Controls")]
        public float moveSpeed = 5.0f;
        public float rotationSpeed = 120.0f;
        public float mouseSensitivity = 0.15f;
        public float gravity = -9.8f;

        private CharacterController _cc;
        private float _verticalVelocity;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            CreateLabel();
        }

        private void CreateLabel()
        {
            var labelGo = new GameObject("ID_Label");
            labelGo.transform.SetParent(transform);
            labelGo.transform.localPosition = new Vector3(0, 2.2f, 0);
            var text = labelGo.AddComponent<TextMesh>();
            text.text = "PLAYER";
            text.characterSize = 0.2f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.color = Color.white;
            text.fontStyle = FontStyle.Bold;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (keyboard == null) return;

            // ── TAB RELATIONSHIP OVERLAY ──────────────────────────────────
            if (GPOyun.UI.HUDManager.Instance != null)
            {
                GPOyun.UI.HUDManager.Instance.relationshipOverlayActive = keyboard.tabKey.isPressed;
            }
            if (keyboard.tabKey.wasPressedThisFrame && RelationshipMatrix.Instance != null)
            {
                RelationshipMatrix.Instance.PrintGossipReport();
            }

            // ── FSM STATE PAUSED CHECK ────────────────────────────────────
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Paused)
            {
                _verticalVelocity = -1f;
                return;
            }

            // ── MOVEMENT (HAREKET) ─────────────────────────────────────────
            float moveX = 0f;
            float moveZ = 0f;

            // İleri - Geri (W - S / Üst - Alt Ok)
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveZ = 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveZ = -1f;

            // Sağa - Sola Yürüme (A - D / Sol - Sağ Ok)
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveX = -1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveX = 1f;

            // ── ROTATION (DÖNME) ───────────────────────────────────────────
            float rotY = 0f;

            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
            {
                rotY = mouse.delta.ReadValue().x * mouseSensitivity;
            }

            transform.Rotate(Vector3.up, rotY * rotationSpeed * Time.deltaTime);

            // ── GRAVITY (YERÇEKİMİ) ────────────────────────────────────────
            if (_cc.isGrounded)
                _verticalVelocity = -1f;
            else
                _verticalVelocity += gravity * Time.deltaTime;

            Vector3 moveDirection = (transform.forward * moveZ) + (transform.right * moveX);

            if (moveDirection.magnitude > 1f)
            {
                moveDirection.Normalize();
            }

            Vector3 finalMove = moveDirection * moveSpeed;
            finalMove.y = _verticalVelocity;

            _cc.Move(finalMove * Time.deltaTime);

            // ── CURSOR LOCK ────────────────────────────────────────────────
            if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
                Cursor.lockState = CursorLockMode.Locked;
        }
    }
}