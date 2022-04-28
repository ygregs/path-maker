using UnityEngine;
using Unity.Netcode;

namespace PathMaker
{

    public class DamageLogic : NetworkBehaviour
    {
        public ulong m_localId;

        public override void OnNetworkSpawn()
        {
            m_localId = NetworkManager.Singleton.LocalClientId;
        }

        void OnTriggerEnter(Collider collider)
        {

            if (collider.gameObject.tag == "Bullet")
            {
                shooting.BulletBrain bulletBrain = collider.gameObject.GetComponent<shooting.BulletBrain>();
                ulong shooterId = bulletBrain.m_shooterId;
                Debug.Log($"Collision between player {shooterId.ToString()} (shooter) and player {m_localId} (target). (this debug is sent by player {m_localId})");
                // if (shooterId != m_localId)
                // {
                //     TeamState shooterTeam = bulletBrain.m_shooterTeam;
                //     SendTakeDamage_ServerRpc(m_localId, shooterId, shooterTeam);
                // }
                if (collider.gameObject.GetComponent<NetworkObject>().OwnerClientId != m_localId)
                {
                    Debug.Log($"Send take damage server rpc (this debug is sent by player {m_localId})");
                    TeamState shooterTeam = bulletBrain.m_shooterTeam;
                    // Destroy(collider.gameObject);
                    SendTakeDamage_ServerRpc(m_localId, shooterId, shooterTeam);
                    Destroy(collider.gameObject);
                }
            }
        }

        // [ServerRpc]
        // void BulletCollision_ServerRpc() {

        // }

        [ServerRpc(RequireOwnership = false)]
        private void SendTakeDamage_ServerRpc(ulong targetId, ulong shooterId, TeamState state)
        {
            // Locator.Get.InGameInputHandler.OnPlayerInput(targetId, state, ScoreType.Kill, shooterId); // add 5 when returned flag
            Locator.Get.InGameInputHandler.OnPlayerInput(targetId, state, ScoreType.Kill, shooterId); // add 5 when returned flag
        }
    }
}