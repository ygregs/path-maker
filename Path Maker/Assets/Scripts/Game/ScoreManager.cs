using UnityEngine;
using Unity.Netcode;
using TMPro;

public class ScoreManager : NetworkBehaviour
{
    [SerializeField] private NetworkVariable<int> Score = new NetworkVariable<int>(0);
    [SerializeField] private int personalScore = 0;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text personalText;

    public override void OnNetworkSpawn()
    {
        Score.OnValueChanged += OnScoreChanged;
    }

    public override void OnNetworkDespawn()
    {
        Score.OnValueChanged -= OnScoreChanged;
    }

    public void OnScoreChanged(int prev, int cur)
    {
        scoreText.text = "Score: " + cur.ToString();
    }

    public void IncrementScore()
    {
        if (NetworkManager.Singleton.IsListening)
        {
            if (IsServer)
            {
                Score.Value = Score.Value + 1;
            }
            else
            {
                IncrementScore_ServerRpc();
            }
        }
        else
        {
            print("start host / client to increment score");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void IncrementPersonalScore_ServerRpc()
    {
        personalScore++;
        personalText.text = "Personal score: " + personalScore.ToString();
    }

    [ClientRpc]
    public void IncrementPersonalScore_ClientRpc(ulong clientId)
    {
        print($"incrementing score of player {clientId}");
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            print($"incrementing score of player {NetworkManager.Singleton.LocalClientId}");
            personalScore++;
            personalText.text = "Personal score: " + personalScore.ToString();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void IncrementScore_ServerRpc()
    {
        Score.Value = Score.Value + 1;
    }

}