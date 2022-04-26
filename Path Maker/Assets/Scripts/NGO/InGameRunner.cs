using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Networking;
using UnityEngine;
using TMPro;

namespace PathMaker.ngo
{
    /// <summary>
    /// Once the NetworkManager has been spawned, we need something to manage the game state and setup other in-game objects
    /// that is itself a networked object, to track things like network connect events.
    /// </summary>

    public enum RoundState
    {
        None,
        Round1,
        Round2,
    }
    public class InGameRunner : NetworkBehaviour, IInGameInputHandler
    {
        private Action m_onConnectionVerified, m_onGameEnd;
        private int m_expectedPlayerCount; // Used by the host, but we can't call the RPC until the network connection completes.
        // private bool? m_canSpawnInGameObjects;
        // private Queue<Vector2> m_pendingSymbolPositions = new Queue<Vector2>();
        // private float m_symbolSpawnTimer = 0.5f; // Initial time buffer to ensure connectivity before loading objects.
        private int m_remainingFlagCount = 2; // Only used by the host.
        // private float m_timeout = 10;
        private bool m_hasConnected = false;

        private bool m_hasSpawned = false;

        [SerializeField]
        private GameObject m_playerPrefab;

        private NetworkVariable<int> asianTeamScore = new NetworkVariable<int>(0);
        [SerializeField]
        private TMP_Text asianScoreText;
        private NetworkVariable<int> greekTeamScore = new NetworkVariable<int>(0);
        [SerializeField]
        private TMP_Text greekScoreText;
        public RoundState oldRoundState = RoundState.None;
        public NetworkVariable<RoundState> roundState = new NetworkVariable<RoundState>(RoundState.Round1);
        [SerializeField]
        private TMP_Text roundStateText;

        [SerializeField]
        private IntroOutroRunner m_introOutroRunner = default;

        [SerializeField]
        private Scorer m_scorer = default;
        [SerializeField]
        private NetworkedDataStore m_dataStore = default;

        private PlayerData m_localUserData; // This has an ID that's not necessarily the OwnerClientId, since all clients will see all spawned objects regardless of ownership.

        private ulong clientDataId;

        public void Initialize(Action onConnectionVerified, int expectedPlayerCount, Action onGameEnd, LobbyUser localUser)
        {
            m_onConnectionVerified = onConnectionVerified;
            m_expectedPlayerCount = expectedPlayerCount;
            m_onGameEnd = onGameEnd;
            // m_canSpawnInGameObjects = null;
            m_localUserData = new PlayerData(localUser.DisplayName, 0, 0, localUser.TeamState);
            Locator.Get.Provide(this); // Simplifies access since some networked objects can't easily communicate locally (e.g. the host might call a ClientRpc without that client knowing where the call originated).
        }

        public override void OnNetworkSpawn()
        {
            // if (IsHost)
            //     FinishInitialize();
            m_localUserData = new PlayerData(m_localUserData.name, NetworkManager.Singleton.LocalClientId, 0, m_localUserData.teamState);
            VerifyConnection_ServerRpc(m_localUserData.id);


            // Vector3 spawnPos = GetSpawningPosition(m_localUserData.teamState);
            // defaultPlayerPosition = spawnPos;
            // defaultPositionText.text = $"x: {defaultPlayerPosition.x}, y: {defaultPlayerPosition.y}, z: {defaultPlayerPosition.z}";
        }

        public override void OnNetworkDespawn()
        {
            m_onGameEnd(); // As a backup to ensure in-game objects get cleaned up, if this is disconnected unexpectedly.
        }

        // private void FinishInitialize()
        // {
        //     // Spawn flags and player ?
        // }

        /// <summary>
        /// To verify the connection, invoke a server RPC call that then invokes a client RPC call. After this, the actual setup occurs.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void VerifyConnection_ServerRpc(ulong clientId)
        {
            VerifyConnection_ClientRpc(clientId);

            // While we could start pooling symbol objects now, incoming clients would be flooded with the Spawn calls.
            // This could lead to dropped packets such that the InGameRunner's Spawn call fails to occur, so we'll wait until all players join.
            // (Besides, we will need to display instructions, which has downtime during which symbol objects can be spawned.)
        }

        [ClientRpc]
        private void VerifyConnection_ClientRpc(ulong clientId)
        {
            if (clientId == m_localUserData.id)
                VerifyConnectionConfirm_ServerRpc(m_localUserData);
        }

        /// <summary>
        /// Once the connection is confirmed, spawn a player cursor and check if all players have connected.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void VerifyConnectionConfirm_ServerRpc(PlayerData clientData)
        {
            // Vector3 spawnPos = GetSpawningPosition(clientData.teamState);
            // defaultPlayerPosition = spawnPos;
            // defaultPositionText.text = $"x: {defaultPlayerPosition.x}, y: {defaultPlayerPosition.y}, z: {defaultPlayerPosition.z}";
            // GameObject player = Instantiate(m_playerPrefab, spawnPos, Quaternion.identity);
            // player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientData.id);
            // localPlayer = player;
            // localPlayer = NetworkManager.Singleton?.SpawnManager.GetLocalPlayerObject();
            // player.GetComponent<NetworkObject>().Spawn();
            clientDataId = clientData.id;
            m_dataStore.AddPlayer(clientData.id, clientData.name, clientData.teamState);
            bool areAllPlayersConnected = NetworkManager.ConnectedClients.Count >= m_expectedPlayerCount; // The game will begin at this point, or else there's a timeout for booting any unconnected players.
            VerifyConnectionConfirm_ClientRpc(clientData.id, areAllPlayersConnected);
        }

        // public void SpawnLocalPlayer()
        // {
        //     var cc = localPlayer?.GetComponent<CharacterController>();
        //     cc.enabled = false;
        //     localPlayer.transform.position = defaultPlayerPosition;
        //     cc.enabled = true;
        // }

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
                }
            }
        }

        [ClientRpc]
        private void VerifyConnectionConfirm_ClientRpc(ulong clientId, bool canBeginGame)
        {
            if (clientId == m_localUserData.id)
            {
                m_onConnectionVerified?.Invoke();
                m_hasConnected = true;
            }

            if (canBeginGame && m_hasConnected)
            {
                // m_timeout = -1;
                BeginGame();
            }
        }

        /// <summary>
        /// The game will begin either when all players have connected successfully or after a timeout.
        /// </summary>
        private void BeginGame()
        {
            // Debug.Log("game begin");
            // m_canSpawnInGameObjects = true;
            Locator.Get.Messenger.OnReceiveMessage(MessageType.MinigameBeginning, null);
            m_introOutroRunner.DoIntro();
        }

        public void Update()
        {
            if (oldRoundState != roundState.Value)
            {
                oldRoundState = roundState.Value;
                if (roundState.Value == RoundState.Round1)
                {
                    roundStateText.text = "ROUND 1";
                }
                else
                {
                    roundStateText.text = "ROUND 2";
                }
            }
            if (asianScoreText.text != asianTeamScore.Value.ToString())
            {
                asianScoreText.text = asianTeamScore.Value.ToString();
            }
            if (greekScoreText.text != greekTeamScore.Value.ToString())
            {
                greekScoreText.text = greekTeamScore.Value.ToString();
            }
        }

        /// <summary>
        /// Called while on the host to determine if incoming input has scored or not.
        /// </summary>
        public void OnPlayerInput(ulong playerId, TeamState team, ScoreType scoreType)
        {
            int points = 0;
            switch (scoreType)
            {
                case ScoreType.Kill:
                    points = 1;
                    break;
                case ScoreType.Flag:
                    points = 3;
                    break;
                case ScoreType.FirstDoor:
                    points = 1;
                    break;
                case ScoreType.LastDoor:
                    points = 2;
                    break;
                default:
                    break;
            }
            m_scorer.ScoreSuccess(playerId, points);
            if (scoreType == ScoreType.Flag)
            {
                OnFlagReturned();
            }
            if (IsServer)
            {
                if (team == TeamState.GreekTeam)
                {
                    greekTeamScore.Value = greekTeamScore.Value + points;
                }
                else
                {
                    asianTeamScore.Value = asianTeamScore.Value + points;
                }
            }
            else
            {
                SubmitScoreChangeServerRpc(points, team);
            }
        }

        [ServerRpc]
        void SubmitScoreChangeServerRpc(int points, TeamState team)
        {
            if (team == TeamState.GreekTeam)
            {
                greekTeamScore.Value = greekTeamScore.Value + points;
            }
            else
            {
                asianTeamScore.Value = asianTeamScore.Value + points;
            }
        }
        public void OnFlagReturned()
        {
            // Debug.Log("the state is " + roundState.Value.ToString());
            if (roundState.Value == RoundState.Round1)
            {
                SetLastRound();
                Locator.Get.Messenger.OnReceiveMessage(MessageType.SpawnPlayer, null);
                ShowLastRoundAnimationClientRpc();
                // Debug.Log("first flag has returned");
            }
            else
            {
                // Debug.Log("seconde flag has returned");
                SpawnManager spawnManager = GameObject.FindGameObjectWithTag("SpawnManager").GetComponent<SpawnManager>();
                spawnManager.ResetSpawns();
                WaitForEndingSequence_ClientRpc();
            }
        }

        [ClientRpc]
        void ShowLastRoundAnimationClientRpc()
        {
            m_introOutroRunner.DoLastRound();
        }

        public void SetLastRound()
        {
            if (IsServer)
            {
                roundState.Value = RoundState.Round2;
            }
            else
            {
                SubmitSetLastRoundRequestServerRpc();
            }
        }

        [ServerRpc]
        void SubmitSetLastRoundRequestServerRpc()
        {
            roundState.Value = RoundState.Round2;
        }

        /// <summary>
        /// The server determines when the game should end. Once it does, it needs to inform the clients to clean up their networked objects first,
        /// since disconnecting before that happens will prevent them from doing so (since they can't receive despawn events from the disconnected server).
        /// </summary>


        [ClientRpc]
        private void WaitForEndingSequence_ClientRpc()
        {
            m_scorer.OnGameEnd();
            m_introOutroRunner.DoOutro(EndGame);
        }

        private void EndGame()
        {
            if (IsHost)
                StartCoroutine(EndGame_ClientsFirst());
        }

        private IEnumerator EndGame_ClientsFirst()
        {
            EndGame_ClientRpc();
            yield return null;
            SendLocalEndGameSignal();
        }

        [ClientRpc]
        private void EndGame_ClientRpc()
        {
            if (IsHost)
                return;
            SendLocalEndGameSignal();
        }

        private void SendLocalEndGameSignal()
        {
            Locator.Get.Messenger.OnReceiveMessage(MessageType.EndGame, null); // We only send this message if the game completes, since the player remains in the lobby in that case. If the player leaves with the back button, that instead sends them to the menu.
            m_onGameEnd();
        }

        public void OnReProvided(IInGameInputHandler previousProvider)
        {
            /*No-op*/
        }
    }
}
