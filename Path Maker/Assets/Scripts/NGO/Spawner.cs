using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace PathMaker
{
    public class Spawner : NetworkBehaviour
    {
        [SerializeField] private GameObject hostPrefab;
        [SerializeField] private GameObject clientPrefab;
        public override void OnNetworkSpawn()
        {
            if (IsHost)
            {
                var host = Instantiate(hostPrefab, Vector3.zero, Quaternion.identity);
                host.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId);
            }
            else
            {
                SpawnServerRpc(NetworkManager.Singleton.LocalClientId);
            }
        }

        [ServerRpc]
        public void SpawnServerRpc(ulong clientId)
        {
            var client = Instantiate(clientPrefab, Vector3.zero, Quaternion.identity);
            client.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }


    }
}
