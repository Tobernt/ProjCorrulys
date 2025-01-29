using CustomNamespace;
using Mirror;
using UnityEngine;

public class MountCarByDistanceWithSeats : NetworkBehaviour
{
    public float interactionRange = 3f;
    private bool isMounted = false;
    private MountableCar currentCar;

    private void Update()
    {
        if (!isLocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.E) && !isMounted)
        {
            GameObject carObject = FindClosestCar();
            if (carObject != null)
            {
                currentCar = carObject.GetComponent<MountableCar>();
                if (currentCar != null)
                {
                    CmdRequestSeat(carObject);
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.E) && isMounted)
        {
            CmdLeaveCar();
        }
    }

    private GameObject FindClosestCar()
    {
        GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");
        GameObject closestCar = null;
        float minDistance = interactionRange;

        foreach (GameObject car in cars)
        {
            float distance = Vector3.Distance(transform.position, car.transform.position);
            if (distance < minDistance)
            {
                closestCar = car;
                minDistance = distance;
            }
        }

        return closestCar;
    }

    [Command]
    private void CmdRequestSeat(GameObject carObject)
    {
        MountableCar car = carObject.GetComponent<MountableCar>();
        if (car != null)
        {
            CustomPlayerController player = GetComponent<CustomPlayerController>();
            int seatIndex = car.AssignSeat(player);
            if (seatIndex >= 0)
            {
                RpcMountCar(carObject, seatIndex);
            }
        }
    }

    [ClientRpc]
    private void RpcMountCar(GameObject carObject, int seatIndex)
    {
        isMounted = true;
        currentCar = carObject.GetComponent<MountableCar>();
        Transform seat = currentCar.seats[seatIndex]; // Resolve seat locally
        transform.SetParent(seat);
        transform.localPosition = Vector3.zero;

        // Reset the player's rotation to (0, 0, 0) when mounting
        transform.localEulerAngles = Vector3.zero;

        CustomPlayerController player = GetComponent<CustomPlayerController>();
        if (player != null)
        {
            player.isMounted = true; // Ensure isMounted is synced across clients
        }

        Debug.Log($"Mounted car: {carObject.name} at seat {seat.name}");
    }

    [Command]
    private void CmdLeaveCar()
    {
        if (currentCar != null)
        {
            CustomPlayerController player = GetComponent<CustomPlayerController>();
            currentCar.FreeSeat(player.currentSeatIndex, player); // Pass the player to FreeSeat
        }
        RpcLeaveCar();
    }

    [ClientRpc]
    private void RpcLeaveCar()
    {
        isMounted = false;
        transform.SetParent(null);

        // Reset the player's rotation to (0, 0, 0) and scale to (1, 1, 1) when exiting
        transform.localEulerAngles = Vector3.zero;
        transform.localScale = Vector3.one;

        CustomPlayerController player = GetComponent<CustomPlayerController>();
        if (player != null)
        {
            player.isMounted = false;
        }

        Debug.Log("Left the car.");
    }
}
