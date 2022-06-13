using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class SpawnerManager : NetworkBehaviour
{
    [SerializeField] private GameObject prefabToSpawn;
    private GameObject spawnedObject;
    [SerializeField] private GameObject playerAPrefab;
    [SerializeField] private GameObject playerBPrefab;
    [SerializeField] private bool readyToSpawn = false;
    [SerializeField] private SpawnPoint spawnPoint;
    public Vector3 spawnPosition = new Vector3(0f, 0f, 0f);

    public List<GameObject> localPlayersList = new List<GameObject>();

    Vector3 GetSpawnPosition()
    {
        SpawnPoint[] possiblesSpaws = FindObjectsOfType<SpawnPoint>();
        int l = possiblesSpaws.Length;
        int i = 0;
        while (!readyToSpawn && i < l)
        {
            // bool isAsian = isSameTeam(possiblesSpaws[i].IsAsian.Value);
            // print(isAsian);
            // print(possiblesSpaws[i].IsAsian.Value);
            // if (PathMaker.Locator.Get.Authenticator.GetAuthData().GetContent("player_team") == null) {
            //     print("solo mode");
            // }
            if (!possiblesSpaws[i].IsOccupied.Value && isSameTeam(possiblesSpaws[i].IsAsian.Value)) {
            // {
                // if (isAsian != null) {
                    // if  (isSameTeam(possiblesSpaws[i].IsAsian.Value)) {
                readyToSpawn = true;
                spawnPoint = possiblesSpaws[i];
                spawnPoint.SetOccupied(true);
                spawnPosition = spawnPoint.gameObject.transform.position;
                return spawnPosition;                    
                // }
                // else {
// readyToSpawn = true;
                // spawnPoint = possiblesSpaws[i];
                // spawnPoint.SetOccupied(true);
                // spawnPosition = spawnPoint.gameObject.transform.position;
                // return spawnPosition;
                // }
        }
        
            i++;
        }
        if (!readyToSpawn)
        {
            print("All spawns points are occupied... spawing player at (0,0,0)");
        }
        return Vector3.zero;
    }

    public bool isSameTeam(bool IsAsian) {
        print(IsAsian);
        if (PathMaker.Locator.Get.Authenticator.GetAuthData() == null) {
            print("hello");
            // print(PathMaker.Locator.Get.Authenticator);
            PathMaker.Locator.Get.Authenticator.GetAuthData().SetContent("player_team", "AsianTeam");
            return true;
        }
        if (IsAsian) {
            if (PathMaker.Locator.Get.Authenticator.GetAuthData().GetContent("player_team") == "AsianTeam") {
                return true;
            }
            else {
                return false;
            }
        }
        else {
            if (PathMaker.Locator.Get.Authenticator.GetAuthData().GetContent("player_team") == "AsianTeam") {
                return false;
            }
            else {
                return true;
            }
        }
    }

    public void ResetPlayerPosition(ulong clientId)
    {
        RespawnServerRpc(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    void RespawnServerRpc(ulong clientId)
    {
        // print($"called on {NetworkManager.Singleton.LocalClientId}");
        var localPlayerObject = localPlayersList[(int)clientId];
        Debug.Log(localPlayerObject);
        if (localPlayerObject != null)
        {
            if (localPlayerObject.GetComponent<NetworkBehaviour>().OwnerClientId == clientId)
            {
                // print($"try to reset transform for player {clientId}");
                var cc = localPlayerObject?.GetComponent<CharacterController>();
                cc.enabled = false;
                var anim = localPlayerObject.GetComponent<Animator>();
                anim.applyRootMotion = false;
                anim.Play("Idle", 2, 0f);
                localPlayerObject.transform.position = spawnPosition;
                localPlayerObject.GetComponent<TPSController>().IsDead = false;
                localPlayerObject.GetComponent<ShooterController>().IsDead = false;
                cc.height = 2f;
                cc.radius = 0.3f;
                cc.skinWidth = 0.08f;
                cc.enabled = true;
                localPlayerObject.GetComponent<SkinRenderer>().SetMeshesActive(true);
                ResetPlayer_ClientRpc(clientId);
                FindObjectOfType<PlayerHealthTest>().ResetHealthBar_ClientRpc(clientId);
            }
        }
    }

    [ClientRpc]
    void ResetPlayer_ClientRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            foreach (var player in FindObjectsOfType<TPSController>())
            {
                if (player.gameObject.GetComponent<NetworkBehaviour>().OwnerClientId == clientId)
                {
                    // print($"try to reset transform for client {clientId}");
                    var cc = player?.gameObject.GetComponent<CharacterController>();
                    cc.enabled = false;
                    var anim = player?.gameObject.GetComponent<Animator>();
                    anim.applyRootMotion = false;
                    anim.Play("Idle", 2, 0f);
                    player.gameObject.transform.position = spawnPosition;
                    player.IsDead = false;
                    player.gameObject.GetComponent<ShooterController>().IsDead = false;
                    cc.height = 2f;
                    cc.radius = 0.3f;
                    cc.skinWidth = 0.08f;
                    cc.enabled = true;
                    player.gameObject.GetComponent<SkinRenderer>().SetMeshesActive(true);
                }
            };
        }
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
                localPlayersList.Add(playerA);
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
        localPlayersList.Add(playerB);
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