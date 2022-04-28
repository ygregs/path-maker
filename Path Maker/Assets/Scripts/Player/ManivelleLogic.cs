using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections;

namespace PathMaker
{

    public class ManivelleLogic : NetworkBehaviour
    {
        private ulong m_localId;

        private TeamState m_teamState;

        [SerializeField] private NetworkVariable<bool> canOpen = new NetworkVariable<bool>(false);

        public override void OnNetworkSpawn()
        {
            m_localId = NetworkManager.Singleton.LocalClientId;
        }

        void OnTriggerEnter(Collider collider)
        {
            var teamStateObj = gameObject.GetComponent<TeamLogic>();
            m_teamState = teamStateObj.playerNetworkTeam.Value;
            if (collider.gameObject.tag == "Manivelle" && !collider.gameObject.GetComponent<ManivelleBehaviour>().IsOpen.Value)
            {
                canOpen.Value = true;
                var manivelleBehaviour = collider.gameObject.GetComponent<ManivelleBehaviour>();

                // if (manivelleBehaviour.m_type == DoorType.FirstDoor)
                // {
                //     manivelleBehaviour.DoOpenDoor(() => SendFlagReturned_ServerRpc(m_localId, m_teamState, ScoreType.FirstDoor));
                // }
                // else
                // {
                if (NetworkManager.Singleton.IsServer)
                {
                    manivelleBehaviour.SetCanOpen(true);
                }
                else
                {
                    manivelleBehaviour.SetCanOpenServerRpc(true);
                }
                manivelleBehaviour.StartOpening(() =>
                    {
                        manivelleBehaviour.StopOpening(() => Debug.Log("stop opening animation"));
                        if (manivelleBehaviour.CanOpen.Value)
                        {
                            manivelleBehaviour.DoOpenDoor(() => SendFlagReturned_ServerRpc(m_localId, m_teamState, ScoreType.FirstDoor));
                            if (NetworkManager.Singleton.IsServer)
                            {
                                manivelleBehaviour.SetIsOpen(true);
                            }
                            else
                            {
                                manivelleBehaviour.SetIsOpenServerRpc(true);
                            }
                        }
                    }
                    );
            }
        }

        void OnTriggerExit(Collider collider)
        {

            var manivelleBehaviour = collider.gameObject.GetComponent<ManivelleBehaviour>();
            if (manivelleBehaviour != null && !manivelleBehaviour.IsOpen.Value)
            {
                manivelleBehaviour.StopOpening(() => Debug.Log("stop opening animation"));
                if (NetworkManager.Singleton.IsServer)
                {
                    manivelleBehaviour.SetCanOpen(false);
                }
                else
                {
                    manivelleBehaviour.SetCanOpenServerRpc(false);
                }
            }
        }

        [ServerRpc]
        private void SendFlagReturned_ServerRpc(ulong id, TeamState state, ScoreType scoreType)
        {

            Locator.Get.InGameInputHandler.OnPlayerInput(id, state, scoreType, (ulong)0); // add 5 when returned flag
            // OnInputVisuals_ClientRpc();
        }
    }
}