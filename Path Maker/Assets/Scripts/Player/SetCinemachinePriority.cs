using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using Unity.Audio;


public class SetCinemachinePriority : NetworkBehaviour
{

    public CinemachineVirtualCamera cinemachineVirtualLook;
    public CinemachineVirtualCamera aimVirtualLook;
    public GameObject playerCanvas;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            cinemachineVirtualLook.gameObject.GetComponent<AudioListener>().enabled = true;
            cinemachineVirtualLook.Priority = 10;
            aimVirtualLook.Priority = 11;
            playerCanvas.SetActive(true);
        }
        else
        {
            cinemachineVirtualLook.gameObject.GetComponent<AudioListener>().enabled = false;
            cinemachineVirtualLook.Priority = 0;
            aimVirtualLook.Priority = 0;
            playerCanvas.SetActive(false);
        }
    }

}