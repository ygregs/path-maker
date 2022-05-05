using UnityEngine;
using Unity.Netcode;
using Cinemachine;


public class SetCinemachinePriority : NetworkBehaviour
{

    public CinemachineVirtualCamera cinemachineVirtualLook;
    // public CinemachineFreeLook aimFreeLook;
    // public GameObject canvas;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            cinemachineVirtualLook.Priority = 10;
            // aimFreeLook.Priority = 11;
            // canvas.SetActive(true);
        }
        else
        {
            cinemachineVirtualLook.Priority = 0;
            // aimFreeLook.Priority = 0;
            // canvas.SetActive(false);
        }
    }

}