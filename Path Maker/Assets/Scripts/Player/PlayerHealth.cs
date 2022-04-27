using UnityEngine;
using Unity.Netcode;
using TMPro;

namespace PathMaker
{
    public class PlayerHealth : NetworkBehaviour
    {
        [SerializeField] private PathMaker.ngo.NetworkedDataStore m_dataStore = default;
        [SerializeField] private TMP_Text m_healthText = default;

        private ulong m_localId;

        public override void OnNetworkSpawn()
        {
            m_localId = NetworkManager.Singleton.LocalClientId;
        }

        // Called on the host.
        public void TakeDamage(ulong id, float damages)
        {
            float newHealth = m_dataStore.UpdateHealth(id, damages);
            UpdateHealthOutput_ClientRpc(id, newHealth);
        }

        [ClientRpc]
        private void UpdateHealthOutput_ClientRpc(ulong id, float health)
        {
            if (m_localId == id)
                m_healthText.text = health.ToString("0");
        }
    }
}