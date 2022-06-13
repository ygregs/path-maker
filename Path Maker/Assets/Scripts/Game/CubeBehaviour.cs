using UnityEngine;
using Unity.Netcode;

public class CubeBehaviour : NetworkBehaviour
{
    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
            scoreManager.IncrementScore();
            scoreManager.IncrementPersonalScore_ClientRpc(collider.GetComponent<NetworkObject>().OwnerClientId);
            DespawnCube_ServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void DespawnCube_ServerRpc()
    {
        gameObject.GetComponent<NetworkObject>().Despawn();
    }
}