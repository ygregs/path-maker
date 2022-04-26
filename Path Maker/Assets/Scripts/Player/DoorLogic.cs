using System;
using UnityEngine;
using Unity.Netcode;
using System.Collections;

namespace PathMaker
{

    public class DoorLogic : NetworkBehaviour
    {
        private ulong m_localId;

        private TeamState m_teamState;

        public override void OnNetworkSpawn()
        {
            m_localId = NetworkManager.Singleton.LocalClientId;
        }

        void OnTriggerEnter(Collider collider)
        {
            var teamStateObj = gameObject.GetComponent<TeamLogic>();
            m_teamState = teamStateObj.playerNetworkTeam.Value;
            if (collider.gameObject.tag == "Door" && !collider.gameObject.GetComponent<DoorBehaviour>().IsOpen.Value)
            {
                var doorBehaviour = collider.gameObject.GetComponent<DoorBehaviour>();

                if (doorBehaviour.m_type == DoorType.FirstDoor)
                {
                    doorBehaviour.DoOpenDoor(() => SendFlagReturned_ServerRpc(m_localId, m_teamState, ScoreType.FirstDoor));
                }
                else
                {
                    doorBehaviour.DoOpenDoor(() => SendFlagReturned_ServerRpc(m_localId, m_teamState, ScoreType.LastDoor));
                }
                if (NetworkManager.Singleton.IsServer)
                {
                    doorBehaviour.SetIsOpen(true);
                }
                else
                {
                    doorBehaviour.SetIsOpenServerRpc(true);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendFlagReturned_ServerRpc(ulong id, TeamState state, ScoreType scoreType)
        {

            Locator.Get.InGameInputHandler.OnPlayerInput(id, state, scoreType); // add 5 when returned flag
            // OnInputVisuals_ClientRpc();
        }
    }
}