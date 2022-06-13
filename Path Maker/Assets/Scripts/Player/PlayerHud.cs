using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

namespace PathMaker
{

    public class PlayerHud : NetworkBehaviour
    {
        [SerializeField] private NetworkVariable<FixedString128Bytes> _playerName = new NetworkVariable<FixedString128Bytes>();
        private bool overlaySet = false;

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                SetName();
            }
        }

        public void SetName()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                _playerName.Value = Locator.Get.Authenticator.GetAuthData().GetContent("playername");
                // _playerName.Value = "Player " + NetworkManager.Singleton.LocalClientId;
            }
            else
            {
                // SubmitNameRequestServerRpc(Locator.Get.Authenticator.GetAuthData().GetContent("playername"));
                SubmitNameRequestServerRpc("playername", NetworkManager.Singleton.LocalClientId);
            }
        }

        [ServerRpc]
        void SubmitNameRequestServerRpc(string playerName, ulong clientId)
        {
            _playerName.Value = playerName;
            // _playerName.Value = "Player " + clientId;
        }


        public void SetOverlay()
        {
            var _playerOverlay = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            var _playerTeam = gameObject.GetComponentInChildren<TeamLogic>();
            _playerOverlay.text = _playerName.Value.ToString();
            // if (locaPlayerTeam.playerNetworkTeam.Value == TeamState.AsianTeam)
            // {
            //     localPlayerOverlay.text = $"[<color=#953A3A>Asian Team</color>] {_playerName.Value}";
            // }
            // else
            // {
            //     localPlayerOverlay.text = $"[<color=#9BCCFF>Greek Team</color>] {_playerName.Value}";
            // }
        }

        public void Update()
        {
            if (!overlaySet && !string.IsNullOrEmpty(_playerName.Value.ToString()))
            {
                SetOverlay();
                overlaySet = true;
            }
        }

    }

}