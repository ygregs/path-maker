using UnityEngine;
using Unity.Netcode;
using Cinemachine;


public class SetCinemachinePriority : NetworkBehaviour
{

    public CinemachineVirtualCamera cinemachineVirtualLook;
    public CinemachineVirtualCamera aimVirtualLook;
    public GameObject playerCanvas;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            cinemachineVirtualLook.Priority = 10;
            aimVirtualLook.Priority = 11;
            playerCanvas.SetActive(true);
        }
        else
        {
            cinemachineVirtualLook.Priority = 0;
            aimVirtualLook.Priority = 0;
            playerCanvas.SetActive(false);
        }
    }

}