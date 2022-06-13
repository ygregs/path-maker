using UnityEngine;
using Unity.Netcode;

public class SoloModeGameManager : MonoBehaviour {
    public SpawnerManager spawnerManager;

    void Start() {
        NetworkManager.Singleton.StartHost();
        spawnerManager.SpawnPlayer();
    }

    void OnDestroy() {
        NetworkManager.Singleton.Shutdown();
    }
}