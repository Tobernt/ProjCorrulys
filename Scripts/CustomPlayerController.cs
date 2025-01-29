using Mirror;
using UnityEngine;

namespace CustomNamespace
{
    [AddComponentMenu("Custom/Networked Player Controller")]
    [RequireComponent(typeof(CharacterController))]
    public class CustomPlayerController : NetworkBehaviour
    {
        public int currentSeatIndex = -1; // Default value for no seat assigned
        [SyncVar] public bool isMounted = false; // Sync isMounted across all clients

        [Header("Player Settings")]
        public float moveSpeed = 5f;
        public float jumpForce = 7f;
        public float gravity = -9.81f;

        private CharacterController characterController;
        private CapsuleCollider capsuleCollider;
        private Vector3 velocity;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            capsuleCollider = GetComponent<CapsuleCollider>();

            if (characterController == null)
            {
                Debug.LogError("CharacterController is missing on the player.");
            }

            if (capsuleCollider == null)
            {
                Debug.LogError("CapsuleCollider is missing on the player.");
            }
        }

        public void TogglePhysics(bool enable)
        {
            if (characterController != null)
            {
                characterController.enabled = enable;
            }

            if (capsuleCollider != null)
            {
                capsuleCollider.enabled = enable;
            }

            Debug.Log($"{gameObject.name}: Physics toggled to {(enable ? "enabled" : "disabled")}");
        }

        [ClientRpc]
        public void RpcSetPhysics(bool enable)
        {
            if (characterController != null)
            {
                characterController.enabled = enable;
            }

            if (capsuleCollider != null)
            {
                capsuleCollider.enabled = enable;
            }

            Debug.Log($"{gameObject.name}: Physics set to {(enable ? "enabled" : "disabled")} on client.");
        }


        private void Update()
        {
            if (!isLocalPlayer) return;

            if (isMounted)
            {
                // Disable local movement when mounted
                return;
            }

            HandleMovement();
        }

        private void HandleMovement()
        {
            if (!characterController.enabled) return;

            bool isGrounded = characterController.isGrounded;
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 move = transform.right * horizontal + transform.forward * vertical;

            characterController.Move(move * moveSpeed * Time.deltaTime);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            }

            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }
    }
}
