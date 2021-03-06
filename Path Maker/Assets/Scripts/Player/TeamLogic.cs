using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;

namespace PathMaker
{

    public class TeamLogic : NetworkBehaviour
    {
        public NetworkVariable<TeamState> playerNetworkTeam = new NetworkVariable<TeamState>();

        [SerializeField] private SkinnedMeshRenderer[] m_skinMeshRenders;

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
                foreach(var mesh in m_skinMeshRenders) {
                    mesh.materials[0].SetColor("_BaseColor", new Color(255f / 255f, 78f / 255f, 78f / 255f));
                    mesh.materials[0].SetColor("_EmissionColor", new Color(255f / 255f, 78f / 255f, 78f / 255f) * Mathf.LinearToGammaSpace(2f));

                }
                // m_skinMeshRender.materials[2].color = Color.green;
            }
            else
            {
                state = TeamState.GreekTeam;
                foreach(var mesh in m_skinMeshRenders) {
                    mesh.materials[0].SetColor("_BaseColor", new Color(78f / 255f, 82f / 255f, 255f / 255f));
                    mesh.materials[0].SetColor("_EmissionColor", new Color(78f / 255f, 82f / 255f, 255f / 255f) * Mathf.LinearToGammaSpace(2f));
                }
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