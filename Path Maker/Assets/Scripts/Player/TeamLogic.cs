using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;

namespace PathMaker
{

    public class TeamLogic : NetworkBehaviour
    {
        public NetworkVariable<TeamState> playerNetworkTeam = new NetworkVariable<TeamState>();

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
            }
            else
            {
                state = TeamState.GreekTeam;
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