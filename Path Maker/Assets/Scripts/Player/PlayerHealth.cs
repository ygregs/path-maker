using UnityEngine;
using Unity.Netcode;
using TMPro;

namespace PathMaker.ngo
{
    public class PlayerHealth : NetworkBehaviour
    {
        [SerializeField] private NetworkVariable<float> health = new NetworkVariable<float>(100f);
        // [SerializeField] private NetworkVariable<int> delta = new NetworkVariable<int>(0);

        public float actualHealth = 100f;

        [SerializeField] private TMP_Text healthText;

        public ulong m_localId;

        public override void OnNetworkSpawn()
        {
            m_localId = NetworkManager.Singleton.LocalClientId;
        }

        void Update()
        {
            // actualHealth = health.Value;
            if (actualHealth != health.Value)
            {
                actualHealth = health.Value;
                healthText.text = actualHealth.ToString("0");
            }
        }

        public void TakeDamage(float damages)
        {
            health.Value -= damages;
            // if (delta.Value == 1)
            // {
            //     var teamStateObj = gameObject.GetComponent<TeamLogic>();
            //     var m_teamState = teamStateObj.playerNetworkTeam.Value;
            //     UpdateScore_ServerRpc(m_localId, m_teamState, ScoreType.Kill);
            //     delta.Value = 0;
            // }
        }


        [ServerRpc]
        private void UpdateScore_ServerRpc(ulong id, TeamState state, ScoreType scoreType)
        {

            Locator.Get.InGameInputHandler.OnPlayerInput(id, state, scoreType, (ulong)0); // add 5 when returned flag
            // OnInputVisuals_ClientRpc();
        }
        // [SerializeField] private PathMaker.ngo.NetworkedDataStore m_dataStore = default;
        // [SerializeField] private TMP_Text m_healthText = default;

        // private ulong m_localId;

        // public override void OnNetworkSpawn()
        // {
        //     m_localId = NetworkManager.Singleton.LocalClientId;
        // }

        // // Called on the host.
        // public void TakeDamage(ulong id, float damages)
        // {
        //     float newHealth = m_dataStore.UpdateHealth(id, damages);
        //     UpdateHealthOutput_ClientRpc(id, newHealth);
        // }

        // [ClientRpc]
        // private void UpdateHealthOutput_ClientRpc(ulong id, float health)
        // {
        //     if (m_localId == id)
        //     {
        //         m_healthText.text = health.ToString("0");
        //         Debug.Log("update health");
        //     }
        // }
    }
}