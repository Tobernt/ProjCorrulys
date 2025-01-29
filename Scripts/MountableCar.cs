using CustomNamespace;
using Mirror;
using UnityEngine;

[AddComponentMenu("Custom/Mountable Car")]
public class MountableCar : NetworkBehaviour
{
    [Header("Car Settings")]
    public Transform[] seats; // Assign these in the Inspector
    private bool[] seatOccupied;

    [Header("Driving Settings")]
    public float driveSpeed = 10f;
    public float turnSpeed = 50f;

    [Header("Ground Detection Settings")]
    public float groundCheckDistance = 1.0f; // Distance to check for the ground
    public LayerMask groundLayer; // Layer for the ground

    private bool isGrounded; // Tracks if the car is grounded

    [Header("Flip Detection Settings")]
    public float flipThreshold = 0.7f; // Threshold for determining if the car is flipped (dot product)
    public float flipTime = 6f; // Time before the car flips itself back
    private float flipTimer = 0f; // Timer to track how long the car has been flipped

    [SyncVar(hook = nameof(OnDriverChanged))] private NetworkIdentity currentDriver;

    private NetworkIdentity carIdentity;

    private void Awake()
    {
        seatOccupied = new bool[seats.Length];
        carIdentity = GetComponent<NetworkIdentity>();
    }

    private void Update()
    {
        CheckFlipStatus();
    }

    private void CheckFlipStatus()
    {
        // Check if the car is flipped based on its upward vector
        if (Vector3.Dot(transform.up, Vector3.up) < flipThreshold)
        {
            // If flipped, increase the flip timer
            flipTimer += Time.deltaTime;

            if (flipTimer >= flipTime)
            {
                CmdFlipCar();
                flipTimer = 0f; // Reset the timer after flipping
            }
        }
        else
        {
            // Reset the timer if the car is not flipped
            flipTimer = 0f;
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdFlipCar()
    {
        RpcFlipCar();
    }

    [ClientRpc]
    private void RpcFlipCar()
    {
        // Reset the car's rotation to upright
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        Debug.Log("Car flipped back to upright position.");
    }

    public int AssignSeat(CustomPlayerController player)
    {
        for (int i = 0; i < seats.Length; i++)
        {
            if (!seatOccupied[i])
            {
                seatOccupied[i] = true;
                player.currentSeatIndex = i;
                player.isMounted = true;

                // Force the client to update physics state
                player.RpcSetPhysics(false);

                if (i == 0) // Seat 1 (index 0) is the driver's seat
                {
                    CmdAssignAuthority(player.netIdentity);
                    currentDriver = player.netIdentity;
                    Debug.Log($"Driver assigned: {player.name}");
                }

                CmdMoveCarUp(); // Apply the upward offset when mounting

                return i; // Return the index of the assigned seat
            }
        }
        Debug.Log("No available seats.");
        return -1; // No seats available
    }

    public void FreeSeat(int seatIndex, CustomPlayerController player)
    {
        if (seatIndex >= 0 && seatIndex < seatOccupied.Length)
        {
            seatOccupied[seatIndex] = false;

            // Force the client to update physics state
            player.RpcSetPhysics(true);

            if (seatIndex == 0) // Driver seat
            {
                CmdRemoveAuthority();
                currentDriver = null;
                Debug.Log($"Driver removed: {player.name}");
            }
        }
    }


    [Command(requiresAuthority = false)]
    private void CmdMoveCarUp()
    {
        transform.position += Vector3.up * 1f; // Move the car up by 1 meter
        RpcMoveCarUp();
    }

    [ClientRpc]
    private void RpcMoveCarUp()
    {
        if (isServer) return; // Server already moved the car
        transform.position += Vector3.up * 1f;
    }

    private void OnDriverChanged(NetworkIdentity oldDriver, NetworkIdentity newDriver)
    {
        Debug.Log($"Driver changed from {oldDriver?.name} to {newDriver?.name}");
    }

    private void FixedUpdate()
    {
        if (carIdentity.isOwned) // Check if the local player owns the car
        {
            HandleDriving();
        }
    }

    private void HandleDriving()
    {
        float move = Input.GetAxis("Vertical") * driveSpeed * Time.fixedDeltaTime;
        float turn = Input.GetAxis("Horizontal") * turnSpeed * Time.fixedDeltaTime;

        transform.Translate(Vector3.forward * move);
        transform.Rotate(Vector3.up * turn);
    }

    [Command(requiresAuthority = false)] // Allow the server to always execute this command
    private void CmdAssignAuthority(NetworkIdentity playerIdentity)
    {
        if (carIdentity.connectionToClient != null)
        {
            carIdentity.RemoveClientAuthority();
        }
        carIdentity.AssignClientAuthority(playerIdentity.connectionToClient);
        Debug.Log($"Authority assigned to: {playerIdentity.connectionToClient.address}");
    }

    [Command(requiresAuthority = false)]
    private void CmdRemoveAuthority()
    {
        if (carIdentity.connectionToClient != null)
        {
            carIdentity.RemoveClientAuthority();
            Debug.Log("Authority removed from car.");
        }
    }
}
