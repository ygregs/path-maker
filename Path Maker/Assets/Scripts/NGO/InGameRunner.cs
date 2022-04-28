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
    public class InGameRunner : NetworkBehaviour, IInGameInputHandler, IReceiveMessages
    {
        private Action m_onConnectionVerified, m_onGameEnd;
        private int m_expectedPlayerCount; // Used by the host, but we can't call the RPC until the network connection completes.
        private bool m_hasConnected = false;

        [SerializeField]
        private GameObject m_playerPrefab;
        private int asianScore = 0;
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
        private PlayerHealth m_health = default;
        [SerializeField]
        private NetworkedDataStore m_dataStore = default;

        private PlayerData m_localUserData; // This has an ID that's not necessarily the OwnerClientId, since all clients will see all spawned objects regardless of ownership.

        private ulong clientDataId;

        // private GameObject[] doorsToClose;

        [SerializeField] private GameObject m_doorPrefab;
        [SerializeField] private GameObject m_manivellePrefab;
        private GameObject doorGo;
        private GameObject manivelleGo;

        [SerializeField] private GameObject greekFPrefab;
        [SerializeField] private GameObject asianFPrefab;
        private GameObject greekF;
        private GameObject asianF;

        void Awake()
        {
            Locator.Get.Messenger.Subscribe(this);
        }

        public void OnReceiveMessage(MessageType type, object msg)
        {
            if (type == MessageType.CompleteTimer && IsServer)
            {
                // CloseDoors();
                WaitForEndingSequence_ClientRpc();
            }

        }

        public void CloseDoors()
        {
            // foreach (var door in doorsToClose)
            // {
            //     var dbcpt = door.GetComponent<DoorBehaviour>();
            //     dbcpt.DoCloseDoor(() => Debug.Log("closing door..."));
            if (NetworkManager.Singleton.IsServer)
            {
                doorGo.GetComponent<DoorBehaviour>().DoCloseDoor(() => Debug.Log("closing door..."));
                doorGo.GetComponent<DoorBehaviour>().SetIsOpen(false);

                // Reset manivelles 
                manivelleGo.GetComponent<ManivelleBehaviour>().DoCloseDoor(() => Debug.Log("reseting manivelles..."));
                manivelleGo.GetComponent<ManivelleBehaviour>().SetIsOpen(false);
                manivelleGo.GetComponent<ManivelleBehaviour>().SetCanOpen(false);

                greekF.GetComponent<NetworkObject>().Despawn();
                asianF.GetComponent<NetworkObject>().Despawn();

                greekF = Instantiate(greekFPrefab, new Vector3(-4.5f, 0, 6.5f), Quaternion.identity);
                greekF.GetComponent<NetworkObject>().Spawn();
                asianF = Instantiate(asianFPrefab, new Vector3(-9.5f, 0, 7f), Quaternion.identity);
                asianF.GetComponent<NetworkObject>().Spawn();
            }
            //     else
            //     {
            //         dbcpt.SetIsOpenServerRpc(false);
            //     }
            // }
        }
        public void Initialize(Action onConnectionVerified, int expectedPlayerCount, Action onGameEnd, LobbyUser localUser)
        {
            m_onConnectionVerified = onConnectionVerified;
            m_expectedPlayerCount = expectedPlayerCount;
            m_onGameEnd = onGameEnd;
            m_localUserData = new PlayerData(localUser.DisplayName, 0, 0, localUser.TeamState, 100.0f);
            Locator.Get.Provide(this); // Simplifies access since some networked objects can't easily communicate locally (e.g. the host might call a ClientRpc without that client knowing where the call originated).
            // doorsToClose = GameObject.FindGameObjectsWithTag("Door");
        }

        public override void OnNetworkSpawn()
        {
            // if (IsHost)
            //     FinishInitialize();
            m_localUserData = new PlayerData(m_localUserData.name, NetworkManager.Singleton.LocalClientId, 0, m_localUserData.teamState, 100.0f);
            VerifyConnection_ServerRpc(m_localUserData.id);
            // asianTeamScore.OnValueChanged += OnAsianScoreChanged;
        }

        public void OnAsianScoreChanged(int prev, int cur)
        {
            asianScoreText.text = (cur).ToString();
        }

        public override void OnNetworkDespawn()
        {
            m_onGameEnd(); // As a backup to ensure in-game objects get cleaned up, if this is disconnected unexpectedly.
            // asianTeamScore.OnValueChanged -= OnAsianScoreChanged;
        }

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

            clientDataId = clientData.id;
            m_dataStore.AddPlayer(clientData.id, clientData.name, clientData.teamState);
            bool areAllPlayersConnected = NetworkManager.ConnectedClients.Count >= m_expectedPlayerCount; // The game will begin at this point, or else there's a timeout for booting any unconnected players.
            VerifyConnectionConfirm_ClientRpc(clientData.id, areAllPlayersConnected);
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
                BeginGame();
            }
        }

        /// <summary>
        /// The game will begin either when all players have connected successfully or after a timeout.
        /// </summary>
        private void BeginGame()
        {
            if (IsServer)
            {
                // doorGo = Instantiate(m_doorPrefab, Vector3.zero, Quaternion.identity);
                doorGo = Instantiate(m_doorPrefab, new Vector3(15, 0, 10), Quaternion.identity);
                doorGo.GetComponent<NetworkObject>().Spawn();
                manivelleGo = Instantiate(m_manivellePrefab, new Vector3(2, 1.20f, 18), Quaternion.identity);
                manivelleGo.GetComponent<NetworkObject>().Spawn();

                greekF = Instantiate(greekFPrefab, new Vector3(-4.5f, 0, 6.5f), Quaternion.identity);
                greekF.GetComponent<NetworkObject>().Spawn();
                asianF = Instantiate(asianFPrefab, new Vector3(-9.5f, 0, 7f), Quaternion.identity);
                asianF.GetComponent<NetworkObject>().Spawn();
            }
            // CloseDoors();
            Locator.Get.Messenger.OnReceiveMessage(MessageType.MinigameBeginning, null);
            Locator.Get.Messenger.OnReceiveMessage(MessageType.StartTimer, null);
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
        public void OnPlayerInput(ulong playerId, TeamState team, ScoreType scoreType, ulong shooterId)
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
            // if (scoreType == ScoreType.Kill)
            // {
            //     // m_health.TakeDamage(playerId, 40.0f);
            //     m_scorer.ScoreSuccess(shooterId, points);
            //     // TakeDamage_ClientRpc(playerId);
            // }
            // else
            // {
            m_scorer.ScoreSuccess(playerId, points);
            // }
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
        // [ClientRpc]
        // void TakeDamage_ClientRpc(ulong id)
        // {
        //     if (m_localUserData.id == id)
        //     {
        //         m_health.TakeDamage(id, 40.0f);
        //     }
        // }

        // [ClientRpc]
        // public void SetAsianScore_ClientRpc(int newScore)
        // {
        //     if (IsServer)
        //         return;
        //     asianScore = newScore;
        // }

        public void OnFlagReturned()
        {
            // Debug.Log("the state is " + roundState.Value.ToString());
            if (roundState.Value == RoundState.Round1)
            {
                SetLastRound();
                Locator.Get.Messenger.OnReceiveMessage(MessageType.SpawnPlayer, null);
                ShowLastRoundAnimationClientRpc();
                CloseDoors();

                // Debug.Log("first flag has returned");
            }
            else
            {
                // Debug.Log("seconde flag has returned");
                Locator.Get.Messenger.OnReceiveMessage(MessageType.ResetTimer, null);
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
        public override void OnDestroy()
        {
            Locator.Get.Messenger.Unsubscribe(this);
        }
    }
}
