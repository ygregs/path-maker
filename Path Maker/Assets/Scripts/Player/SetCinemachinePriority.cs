using UnityEngine;
using Unity.Netcode;
using Cinemachine;


    public class SetCinemachinePriority : NetworkBehaviour {

        public CinemachineFreeLook cinemachinefreelook;

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
               cinemachinefreelook.Priority = 10; 
            }
            else {
                cinemachinefreelook.Priority = 0; 
            }
    }

}