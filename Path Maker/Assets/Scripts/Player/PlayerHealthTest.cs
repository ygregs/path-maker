using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;

public class PlayerHealthTest : NetworkBehaviour
{
    [SerializeField] private float health = 100f;
    // [SerializeField] private TMP_Text healthText;
    [SerializeField] private Slider healthBar;

    [ClientRpc]
    public void TakeDamage_ClientRpc(ulong clientId, ulong shooterClientId, float damage)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            health -= damage;
            healthBar.value = health;
            if (health <= 0)
            {
                DeathProcedure(clientId);
                // health = 100f;
                // healthBar.value = health;
                // if (IsServer)
                // {
                //     FindObjectOfType<ScoreManager>().IncrementPersonalScore_ClientRpc(shooterClientId);
                // }
                // else
                // {
                //     FindObjectOfType<ScoreManager>().IncrementPersonalScore_ServerRpc();
                // }
            }
            // healthText.text = "Health: " + health.ToString();
        }
    }

    [ClientRpc]
    public void ResetHealthBar_ClientRpc(ulong clientId) {
        if (clientId == NetworkManager.Singleton.LocalClientId) {
            health = 100f;
            healthBar.value = health; 
        }
    }

    void DeathProcedure(ulong clientId)
    {
        foreach (var player in FindObjectsOfType<TPSController>())
        {
            if (player.gameObject.GetComponent<NetworkBehaviour>().OwnerClientId == clientId)
            {
                player.Dying();
            }
        };
    }

    [ServerRpc(RequireOwnership = false)]
    public void DespawnPlayer_ServerRpc(ulong clientId)
    {
        RespawnPlayer_ClientRpc(clientId);
    }

    [ClientRpc]
    void RespawnPlayer_ClientRpc(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            FindObjectOfType<SpawnerManager>().ResetPlayerPosition(clientId);
        }
    }
}