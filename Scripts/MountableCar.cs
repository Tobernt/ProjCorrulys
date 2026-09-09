using CustomNamespace;
using Mirror;
using UnityEngine;

[AddComponentMenu("Custom/Mountable Car")]
public class MountableCar : NetworkBehaviour
{
    [Header("Car Settings")]
    public Transform[] seats;
    private bool[] seatOccupied;
    [Header("Flip Detection Settings")]
    public float flipThreshold = 0.7f;
    public float flipTime = 6f;
    private float flipTimer = 0f;
    [SyncVar] public PhysicsScene physicsScene;
    [SerializeField] private Rigidbody carRigidbody;
    [SerializeField] private float accelerationForce = 500f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float turnTorque = 50f; // Reduced turning force
    [SerializeField] private float minTurnFactor = 0.2f; // Less turn at low speeds
    [SerializeField] private float dragFactor = 0.98f; // Simulates friction/drag
    [Header("Ground Check Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float raycastDistance = 2.0f; // Editable Raycast Distance
    [SerializeField] private float raycastOffsetY = 0.5f;  // Editable Start Offset
    [SyncVar] private bool isGrounded;

    [SyncVar(hook = nameof(OnDriverChanged))] private NetworkIdentity currentDriver;

    private NetworkIdentity carIdentity;

    private void Awake()
    {
        seatOccupied = new bool[seats.Length];
        carIdentity = GetComponent<NetworkIdentity>();
        carRigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!isGrounded)
        {
            CheckFlipStatus(); // Keep checking flip status as usual
        }
    }

    private Vector3 lastPosition; // Track previous position


    private void PerformGroundCheck()
    {
        if (!physicsScene.IsValid())
        {
            Debug.LogError("❌ Invalid physics scene! Ground check skipped.");
            return;
        }

        Vector3 startPosition = transform.position + Vector3.up * raycastOffsetY;
        float sphereRadius = 0.5f; // Adjustable SphereCast radius
        float castDistance = raycastDistance;

        RaycastHit hit;
        bool grounded = physicsScene.SphereCast(startPosition, sphereRadius, Vector3.down, out hit, castDistance, groundLayer);

        isGrounded = grounded;

        Debug.Log(isGrounded
            ? $"✅ Car Ground Detected! Hit: {hit.collider?.name}, Distance: {hit.distance}"
            : "❌ No ground detected!");

        // Draw debug sphere
        Debug.DrawRay(startPosition, Vector3.down * castDistance, isGrounded ? Color.green : Color.red, 0.1f);
    }


    private void CheckFlipStatus()
    {
        if (Vector3.Dot(transform.up, Vector3.up) < flipThreshold)
        {
            flipTimer += Time.deltaTime;
            if (flipTimer >= flipTime)
            {
                RpcFlipCar();
                flipTimer = 0f;
            }
        }
        else
        {
            flipTimer = 0f;
        }
    }

    [ClientRpc]
    private void RpcFlipCar()
    {
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        Debug.Log("Car flipped back to upright position.");
    }

    public int AssignSeat(CustomPlayerController player)
    {
        physicsScene = gameObject.scene.GetPhysicsScene();
        for (int i = 0; i < seats.Length; i++)
        {
            if (seatOccupied[i])
            {
                // Ensure seat is actually occupied (removes disconnected players)
                if (!IsSeatActuallyOccupied(i))
                {
                    seatOccupied[i] = false;
                    Debug.Log($"Seat {i} was occupied by a disconnected player. Resetting...");
                }
                else
                {
                    continue; // Skip if still occupied
                }
            }

            // Assign seat properly
            seatOccupied[i] = true;
            player.currentSeatIndex = i;
            player.isMounted = true;
            player.RpcSetPhysics(false);

            if (i == 0) // First seat (driver)
            {
                currentDriver = player.netIdentity;
                RpcNotifyDriverAssigned(player.netIdentity, this.netIdentity); // Notify client to request authority
                Debug.Log($"Driver assigned: {player.name}");
            }

            RpcMoveCarUp();
            return i;
        }

        Debug.Log("No available seats.");
        return -1;
    }

    [ClientRpc]
    private void RpcNotifyDriverAssigned(NetworkIdentity playerIdentity, NetworkIdentity carIdentity)
    {
        if (playerIdentity.isLocalPlayer)
        {
            Debug.Log($"🔹 Local player {playerIdentity.name} is requesting car authority.");
            playerIdentity.GetComponent<CustomPlayerController>().CmdRequestCarAuthority(carIdentity);
        }
    }

    private bool IsSeatActuallyOccupied(int seatIndex)
    {
        Collider[] colliders = Physics.OverlapSphere(seats[seatIndex].position, 0.5f);
        foreach (var collider in colliders)
        {
            if (collider.GetComponent<MountCarByDistanceWithSeats>() != null)
            {
                return true; // A real player is in the seat
            }
        }
        return false; // Seat is empty and should be freed
    }

    public void FreeSeat(int seatIndex, CustomPlayerController player)
    {
        if (seatIndex >= 0 && seatIndex < seatOccupied.Length)
        {
            Debug.Log($"🪑 FreeSeat called for seat {seatIndex} on {gameObject.name}");

            seatOccupied[seatIndex] = false;

            // Get seat transform
            Transform seatTransform = seats[seatIndex];

            // Determine exit direction based on seat index (Even = Right, Odd = Left)
            Vector3 exitOffset = (seatIndex % 2 == 0) ? seatTransform.right : -seatTransform.right;
            Vector3 exitPosition = seatTransform.position + (exitOffset * -2f) + (Vector3.up * 1.5f);

            Debug.Log($"🚪 Player exiting {((seatIndex % 2 == 0) ? "right" : "left")} from seat {seatIndex} to {exitPosition}");

            // Move player out of car
            player.RpcDismountCar(exitPosition);

            // Reset player's parent
            player.transform.SetParent(null);
            player.isMounted = false;
            player.currentSeatIndex = -1;

            if (seatIndex == 0) // Driver seat
            {
                currentDriver = null;
                RpcNotifyDriverRemoved(player.netIdentity, this.netIdentity);
                Debug.Log($"🚗 Driver removed: {player.name}");
            }

            Debug.Log($"✅ {player.name} successfully left seat {seatIndex} on {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"⚠️ FreeSeat called with invalid seat index: {seatIndex} on {gameObject.name}");
        }
    }

    [ClientRpc]
    private void RpcNotifyDriverRemoved(NetworkIdentity playerIdentity, NetworkIdentity carIdentity)
    {
        if (playerIdentity.isLocalPlayer) // Only the local player should execute this
        {
            Debug.Log($"🔹 Local player removing car authority.");
            playerIdentity.GetComponent<CustomPlayerController>().CmdRemoveCarAuthority(carIdentity);
        }
    }

    [ClientRpc]
    private void RpcMoveCarUp()
    {
        if (!isServer)
        {
            transform.position += Vector3.up * 1f;
        }
    }

    private void OnDriverChanged(NetworkIdentity oldDriver, NetworkIdentity newDriver)
    {
        Debug.Log($"🔄 Driver changed from {oldDriver?.name} to {newDriver?.name}");

        if (newDriver != null && newDriver.isLocalPlayer)
        {
            Debug.Log($"🔹 Local player is the new driver. Requesting authority...");
            newDriver.GetComponent<CustomPlayerController>().CmdRequestCarAuthority(this.netIdentity);
        }
    }

    private void FixedUpdate()
    {
        if (isOwned)
        {
            PerformGroundCheck();
            HandleDriving();
        }
    }

    private void HandleDriving()
    {
        if (!isGrounded) return;
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        //Forward movement with force
        if (moveInput != 0)
        {
            Vector3 force = transform.forward * moveInput * accelerationForce;
            carRigidbody.AddForce(force, ForceMode.Acceleration);
        }

        //Adjust turning based on speed
        float speedFactor = Mathf.Clamp(carRigidbody.velocity.magnitude / maxSpeed, minTurnFactor, 1f);
        float adjustedTurnTorque = turnInput * turnTorque * speedFactor;

        //Apply turning torque only if the car is moving
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            carRigidbody.AddTorque(Vector3.up * adjustedTurnTorque, ForceMode.Acceleration);
        }

        //Apply drag to slow the car down naturally when not accelerating
        carRigidbody.velocity *= dragFactor;
    }
}
