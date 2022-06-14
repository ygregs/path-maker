using UnityEngine;
using Unity.Netcode;

public class SpawnPlayerButtonUI : MonoBehaviour {
    public void SpawnPlayer() {
        if (NetworkManager.Singleton.IsListening) {
        var sp = FindObjectOfType<SpawnerManager>();
        sp.SpawnPlayer();
        }
    }
}