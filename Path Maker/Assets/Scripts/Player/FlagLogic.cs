using UnityEngine;
using Unity.Netcode;

namespace PathMaker
{

    public class FlagLogic : NetworkBehaviour
    {

        [SerializeField]
        private bool HasFlag;
        private ulong m_localId;

        private TeamState m_teamState;

        public override void OnNetworkSpawn()
        {
            m_localId = NetworkManager.Singleton.LocalClientId;
        }

        void OnCollisionEnter(Collision collision)
        {
            var teamStateObj = gameObject.GetComponent<TeamLogic>();
            m_teamState = teamStateObj.playerNetworkTeam.Value;
            // Debug.Log($"player is in {m_teamState}");
            if (collision.gameObject.tag == "AsianFlag" && (m_teamState == TeamState.GreekTeam) && !HasFlag)
            {
                // Debug.Log($"Player {m_localId} of greek team just get the flag of the asian team");
                HasFlag = true;
                // Destroy(collision.gameObject);
                collision.gameObject.SetActive(false);

                // flag.transform.SetParent(attachPoint);
                // flag.GetComponent<Rigidbody>().isKinematic = true;
                // flag.transform.rotation = new Quaternion(0, 0, 0, 0);
                // flag.transform.position = attachPoint.position;
            }
            else if (collision.gameObject.tag == "GreekFlag" && (m_teamState == TeamState.AsianTeam) && !HasFlag)
            {
                // Debug.Log($"Player {m_localId} of asian team just get the flag of the greek team");
                HasFlag = true;
                // Destroy(collision.gameObject);
                collision.gameObject.SetActive(false);
            }
        }
        void OnTriggerEnter(Collider collider)
        {
            var teamStateObj = gameObject.GetComponent<TeamLogic>();
            m_teamState = teamStateObj.playerNetworkTeam.Value;
            if (collider.gameObject.tag == "AsianBase" && (m_teamState == TeamState.AsianTeam) && HasFlag)
            {
                HasFlag = false;
                SendFlagReturned_ServerRpc(m_localId, m_teamState);
            }
            else if (collider.gameObject.tag == "GreekBase" && (m_teamState == TeamState.GreekTeam) && HasFlag)
            {
                HasFlag = false;
                SendFlagReturned_ServerRpc(m_localId, m_teamState);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendFlagReturned_ServerRpc(ulong id, TeamState state)
        {

            Locator.Get.InGameInputHandler.OnPlayerInput(id, state, ScoreType.Flag); // add 5 when returned flag
            // OnInputVisuals_ClientRpc();
        }
    }
}