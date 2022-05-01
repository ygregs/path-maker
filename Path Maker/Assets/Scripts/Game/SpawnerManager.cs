using UnityEngine;
using Unity.Netcode;

public class SpawnerManager : NetworkBehaviour
{
    [SerializeField] private GameObject prefabToSpawn;
    private GameObject spawnedObject;
    [SerializeField] private GameObject playerAPrefab;
    [SerializeField] private GameObject playerBPrefab;
    [SerializeField] private bool readyToSpawn = false;
    [SerializeField] private SpawnPoint spawnPoint;
    public Vector3 spawnPosition = new Vector3(0f, 0f, 0f);

    Vector3 GetSpawnPosition()
    {
        SpawnPoint[] possiblesSpaws = FindObjectsOfType<SpawnPoint>();
        int l = possiblesSpaws.Length;
        int i = 0;
        while (!readyToSpawn && i < l)
        {
            if (!possiblesSpaws[i].IsOccupied.Value)
            {
                readyToSpawn = true;
                spawnPoint = possiblesSpaws[i];
                spawnPoint.SetOccupied(true);
                spawnPosition = spawnPoint.gameObject.transform.position;
                return spawnPosition;
            }
            i++;
        }
        if (!readyToSpawn)
        {
            print("All spawns points are occupied... spawing player at (0,0,0)");
        }
        return Vector3.zero;
    }

    public void SpawnPlayer()
    {
        if (NetworkManager.Singleton.IsListening)
        {
            if (spawnPosition == Vector3.zero)
            {
                spawnPosition = GetSpawnPosition();
            }
            if (IsServer)
            {
                var playerA = Instantiate(playerAPrefab, spawnPosition, Quaternion.identity);
                playerA.SetActive(true);
                playerA.GetComponent<NetworkObject>().SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId, true);
            }
            else
            {
                SpawnClientPlayer_ServerRpc(NetworkManager.Singleton.LocalClientId, spawnPosition);
            }
        }
        else
        {
            print("start host / client before spawning player");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnClientPlayer_ServerRpc(ulong clientId, Vector3 pos)
    {
        var playerB = Instantiate(playerBPrefab, pos, Quaternion.identity);
        playerB.SetActive(true);
        playerB.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
    }

    // Spawn cube method
    public void SpawnObject()
    {
        if (NetworkManager.Singleton.IsListening)
        {
            if (IsServer)
            {
                spawnedObject = Instantiate(prefabToSpawn, GetRandomPosition(), Quaternion.identity);
                spawnedObject.GetComponent<NetworkObject>().Spawn();
            }
            else
            {
                SpawnObject_ServerRpc();
            }
        }
        else
        {
            print("start host / client before spawing objects");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnObject_ServerRpc()
    {
        spawnedObject = Instantiate(prefabToSpawn, GetRandomPosition(), Quaternion.identity);
        spawnedObject.GetComponent<NetworkObject>().Spawn();
    }

    private Vector3 GetRandomPosition()
    {
        Vector3 randPosition = new Vector3(0f, 0.5f, 0f);
        randPosition.x = Random.Range(10, -10);
        randPosition.z = Random.Range(0, 10);
        return randPosition;
    }
}