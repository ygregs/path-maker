using UnityEngine;
using Unity.Netcode;
using Cinemachine;


public class SetCinemachinePriority : NetworkBehaviour
{

    public CinemachineFreeLook cinemachinefreelook;
    public CinemachineFreeLook aimFreeLook;
    // public GameObject canvas;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            cinemachinefreelook.Priority = 10;
            aimFreeLook.Priority = 11;
            // canvas.SetActive(true);
        }
        else
        {
            cinemachinefreelook.Priority = 0;
            aimFreeLook.Priority = 0;
            // canvas.SetActive(false);
        }
    }

}