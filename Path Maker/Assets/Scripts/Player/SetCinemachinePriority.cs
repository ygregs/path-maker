using UnityEngine;
using Unity.Netcode;
using Cinemachine;


public class SetCinemachinePriority : NetworkBehaviour
{

    public CinemachineFreeLook cinemachinefreelook;
    public CinemachineFreeLook aimFreeLook;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            cinemachinefreelook.Priority = 10;
            aimFreeLook.Priority = 11;
        }
        else
        {
            cinemachinefreelook.Priority = 0;
            aimFreeLook.Priority = 0;
        }
    }

}