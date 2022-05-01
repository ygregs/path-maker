using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerHealthTest : NetworkBehaviour
{
    [SerializeField] private float health = 100f;
    [SerializeField] private TMP_Text healthText;

    [ClientRpc]
    public void TakeDamage_ClientRpc(ulong clientId, ulong shooterClientId, float damage)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            health -= damage;
            if (health <= 0)
            {
                health = 100f;
                if (IsServer)
                {
                    FindObjectOfType<ScoreManager>().IncrementPersonalScore_ClientRpc(shooterClientId);
                }
                else
                {
                    FindObjectOfType<ScoreManager>().IncrementPersonalScore_ServerRpc();
                }
                DespawnPlayer_ServerRpc(clientId);
            }
            healthText.text = "Health: " + health.ToString();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void DespawnPlayer_ServerRpc(ulong clientId)
    {
        NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.Despawn();
        RespawnPlayer_ClientRpc(clientId);
    }

    [ClientRpc]
    void RespawnPlayer_ClientRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            FindObjectOfType<SpawnerManager>().SpawnPlayer();
        }
    }
}