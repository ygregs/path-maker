using TMPro;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

namespace PathMaker
{

    public class PlayerHud : NetworkBehaviour
    {
        [SerializeField]
        private NetworkVariable<FixedString128Bytes> playerNetworkName = new NetworkVariable<FixedString128Bytes>();

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
                playerNetworkName.Value = Locator.Get.Authenticator.GetAuthData().GetContent("playername");
            }
            else
            {
                SubmitNameRequestServerRpc(Locator.Get.Authenticator.GetAuthData().GetContent("playername"));
            }
        }

        [ServerRpc]
        void SubmitNameRequestServerRpc(string playername)
        {
            playerNetworkName.Value = playername;
        }

        public void SetOverlay()
        {
            var localPlayerOverlay = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            localPlayerOverlay.text = $"{playerNetworkName.Value}";
        }

        public void Update()
        {
            if (!overlaySet && !string.IsNullOrEmpty(playerNetworkName.Value.ToString()))
            {
                SetOverlay();
                overlaySet = true;
            }
        }

    }

}