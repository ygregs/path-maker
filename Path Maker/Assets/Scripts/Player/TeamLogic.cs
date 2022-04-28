using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;

namespace PathMaker
{

    public class TeamLogic : NetworkBehaviour
    {
        public NetworkVariable<TeamState> playerNetworkTeam = new NetworkVariable<TeamState>();
        [SerializeField] private SkinnedMeshRenderer m_skinMeshRender;

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                SetTeam();
            }
        }

        public void SetTeam()
        {
            TeamState state;
            if (Locator.Get.Authenticator.GetAuthData().GetContent("player_team") == "AsianTeam")
            {
                state = TeamState.AsianTeam;
                m_skinMeshRender.materials[2].SetColor("_BaseColor", new Color(255f / 255f, 78f / 255f, 78f / 255f));
                // m_skinMeshRender.materials[2].color = Color.green;
            }
            else
            {
                state = TeamState.GreekTeam;
                m_skinMeshRender.materials[2].SetColor("_BaseColor", new Color(78f / 255f, 82f / 255f, 255f / 255f));
            }
            if (NetworkManager.Singleton.IsServer)
            {
                playerNetworkTeam.Value = (state);
            }
            else
            {
                SubmitTeamRequestServerRpc(state);
            }
        }

        [ServerRpc]
        void SubmitTeamRequestServerRpc(TeamState state)
        {
            playerNetworkTeam.Value = state;
        }

    }
}