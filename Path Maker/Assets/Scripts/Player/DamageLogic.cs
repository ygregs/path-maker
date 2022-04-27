using UnityEngine;
using Unity.Netcode;

namespace PathMaker
{

    public class DamageLogic : NetworkBehaviour
    {
        private ulong m_localId;

        public override void OnNetworkSpawn()
        {
            m_localId = NetworkManager.Singleton.LocalClientId;
        }

        // void OnTriggerEnter(Collider collider)
        // {
        //     Debug.Log("Collision !");
        //     if (collider.gameObject.tag == "Bullet")
        //     {
        //         shooting.BulletBrain bulletBrain = collider.gameObject.GetComponent<shooting.BulletBrain>();
        //         ulong shooterId = bulletBrain.m_shooterId;
        //         if (shooterId != m_localId)
        //         {
        //             TeamState shooterTeam = bulletBrain.m_shooterTeam;
        //             SendTakeDamage_ServerRpc(m_localId, shooterId, shooterTeam);
        //         }
        //     }
        // }

        [ServerRpc]
        private void SendTakeDamage_ServerRpc(ulong targetId, ulong shooterId, TeamState state)
        {
            Locator.Get.InGameInputHandler.OnPlayerInput(targetId, state, ScoreType.Kill, shooterId); // add 5 when returned flag
        }
    }
}