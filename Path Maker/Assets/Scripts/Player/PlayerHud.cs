using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace PathMaker
{
    public class PlayerHud : NetworkBehaviour
    {
        [SerializeField]
        private NetworkVariable<NetworkString> playerNetworkName = new NetworkVariable<NetworkString>();

        private bool overlaySet = false;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                playerNetworkName.Value = $"Player {Locator.Get.Authenticator.GetAuthData().GetContent("playername")}";
            }
        }

        public void SetOverlay()
        {
            var localPlayerOverlay = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            localPlayerOverlay.text = $"{playerNetworkName.Value}";
        }

        public void Update()
        {
            if (!overlaySet && !string.IsNullOrEmpty(playerNetworkName.Value))
            {
                SetOverlay();
                overlaySet = true;
            }
        }

    }

}