using UnityEngine;
using Unity.Netcode;

namespace PathMaker
{
    public class PlayerSpawner : NetworkBehaviour, IReceiveMessages
    {

        CharacterController cc;
        Vector3 respawnPosition = new Vector3(3, 0, 3);

        public void OnReceiveMessage(MessageType type, object msg)
        {
            if (type == MessageType.SpawnPlayer)
            {
                Respawn();
            }
        }
        void Start()
        {
            Locator.Get.Messenger.Subscribe(this);
            cc = gameObject.GetComponent<CharacterController>();
            respawnPosition = GetSpawningPosition(gameObject.GetComponent<TeamLogic>().playerNetworkTeam.Value);
            Debug.Log($"spawning position is x: {respawnPosition.x}, y: {respawnPosition.y}, z: {respawnPosition.z}");
            RespawnServerRpc();
        }

        private Vector3 GetSpawningPosition(TeamState team)
        {
            SpawnManager spawnManager = GameObject.FindGameObjectWithTag("SpawnManager").GetComponent<SpawnManager>();
            // Debug.Log(spawnManager);
            if (spawnManager.firstAsianSpawn.Value)
            {
                spawnManager.SetSpawnDisponibility(1, team);
                if (team == TeamState.AsianTeam)
                {
                    return spawnManager.AsianSpawnsArray[0].position;
                }
                else
                {
                    return spawnManager.GreekSpawnsArray[0].position;
                    // return spawnManager.GreekSpawnsArray[0].position;
                }
            }
            else
            {
                spawnManager.SetSpawnDisponibility(2, team);
                if (team == TeamState.AsianTeam)
                {
                    return spawnManager.AsianSpawnsArray[1].position;
                }
                else
                {
                    return spawnManager.GreekSpawnsArray[1].position;
                    // return spawnManager.GreekSpawnsArray[1].position;
                }
            }
        }


        public void Respawn()
        {
            RespawnServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        void RespawnServerRpc()
        {
            RespawnClientRpc();
        }

        [ClientRpc]
        void RespawnClientRpc()
        {
            if (IsOwner)
            {
                cc.enabled = false;
                gameObject.transform.position = respawnPosition;
                cc.enabled = true;
            }
        }


        public override void OnDestroy()
        {
            Locator.Get.Messenger.Unsubscribe(this);
        }
    }
}