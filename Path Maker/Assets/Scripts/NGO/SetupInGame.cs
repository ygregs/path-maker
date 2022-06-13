using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace PathMaker.ngo
{
    /// <summary>
    /// Once the local player is in a lobby and that lobby has entered the In-Game state, this will load in whatever is necessary to actually run the game part.
    /// This will exist in the game scene so that it can hold references to scene objects that spawned prefab instances will need.
    /// </summary>
    public class SetupInGame : MonoBehaviour, IReceiveMessages
    {
        [SerializeField] private GameObject m_prefabNetworkManager = default;
        [SerializeField] private GameObject m_prefabInGameLogic = default;
        // [SerializeField] private GameObject m_prefabHelloWorldManager = default;
        [SerializeField] private GameObject[] m_disableWhileInGame = default;


        private GameObject m_inGameManagerObj;
        private GameObject m_inGameLogicObj;
        private NetworkManager m_networkManager;
        private InGameRunner m_inGameRunner;

        [SerializeField] private GameObject m_spawnerManagerPrefab;
        private GameObject spawnerManagerGO;
        private SpawnerManager spawnerManager;

        [SerializeField]
        private GameObject[] goToSetActive;
        [SerializeField] private GameObject[] goToSetNoneActive;

        private bool m_doesNeedCleanup = false;
        private bool m_hasConnectedViaNGO = false;

        private Action<UnityTransport> m_initializeTransport;
        private LocalLobby m_lobby;
        private LobbyUser m_localUser;

        // [SerializeField] private GameObject spawnerPrefab;

         [SerializeField] private GameObject greekSPrefab;
        [SerializeField] private GameObject asianSPrefab;
        private GameObject greekS;
        private GameObject greekS2;
        private GameObject asianS;
        private GameObject asianS2;


        public void Start()
        {
            Locator.Get.Messenger.Subscribe(this);
            // spawnerManager = spawnerManager_go.GetComponent<SpawnerManager>();
        }
        public void OnDestroy()
        {
            Locator.Get.Messenger.Unsubscribe(this);
        }

        private void SetMenuVisibility(bool areVisible)
        {
            foreach (GameObject go in m_disableWhileInGame)
                go.SetActive(areVisible);
        }

        private void SetGOActive()
        {
            foreach (var go in goToSetActive)
            {
                go.SetActive(true);
            }
        }

        private void SetGODisable() {
            foreach(var go in goToSetNoneActive) {
                go.SetActive(false);
            }
        }

        private void SetGOReActive() {
           foreach(var go in goToSetNoneActive) {
                go.SetActive(true);
            } 
        }

        /// <summary>
        /// The prefab with the NetworkManager contains all of the assets and logic needed to set up the NGO minigame.
        /// The UnityTransport needs to also be set up with a new Allocation from Relay.
        /// </summary>
        private void CreateNetworkManager()
        {
             // position in test scene:
                // greekS = Instantiate(greekSPrefab, new Vector3(7.5f, 0f, 15), Quaternion.Euler(0f, 180f, 0f));
                // position in final scene:
                greekS = Instantiate(greekSPrefab, new Vector3(-298f, 80f, -17f), Quaternion.identity);
                // greekS.GetComponent<NetworkObject>().Spawn();

                // postion in test scene:
                // asianS = Instantiate(asianSPrefab, new Vector3(8.4f, 0, -3f), Quaternion.Euler(0f, 0f, 0f));
                // position in final scene:
                asianS = Instantiate(asianSPrefab, new Vector3(70f, 80f, 19), Quaternion.Euler(0f, -90f, 0f));
                // asianS.GetComponent<NetworkObject>().Spawn();
            spawnerManagerGO = GameObject.Instantiate(m_spawnerManagerPrefab);
            spawnerManager = spawnerManagerGO.GetComponent<SpawnerManager>();


            m_inGameManagerObj = GameObject.Instantiate(m_prefabNetworkManager);
            m_inGameLogicObj = GameObject.Instantiate(m_prefabInGameLogic);
            // GameObject.Instantiate(m_prefabHelloWorldManager);
            m_networkManager = m_inGameManagerObj.GetComponent<NetworkManager>();
            m_inGameRunner = m_inGameLogicObj.GetComponentInChildren<InGameRunner>();
            m_inGameRunner.oldRoundState = RoundState.None;
            m_inGameRunner.roundState.Value = RoundState.Round1;

            m_inGameRunner.Initialize(OnConnectionVerified, m_lobby.PlayerCount, OnGameEnd, m_localUser);

            UnityTransport transport = m_inGameManagerObj.GetComponent<UnityTransport>();
            if (m_localUser.IsHost)
            {
                m_inGameManagerObj.AddComponent<RelayUtpNGOSetupHost>().Initialize(this, m_lobby, () =>
                {
                    m_initializeTransport(transport); m_networkManager.StartHost();
                    spawnerManager.SpawnPlayer();

                    // var spawner = GameObject.Instantiate(spawnerPrefab, Vector3.zero, Quaternion.identity);
                    // spawner.GetComponent<NetworkObject>().Spawn();
                });
            }
            else
                m_inGameManagerObj.AddComponent<RelayUtpNGOSetupClient>().Initialize(this, m_lobby, () =>
                {
                    m_initializeTransport(transport); m_networkManager.StartClient();
                    spawnerManager.SpawnPlayer();
                    // SpawnSpawnerServerRpc();
                });


        }

        // [ServerRpc]
        // void SpawnSpawnerServerRpc()
        // {
        //     var spawner = GameObject.Instantiate(spawnerPrefab, Vector3.zero, Quaternion.identity);
        //     spawner.GetComponent<NetworkObject>().Spawn();
        // }

        private void OnConnectionVerified()
        {
            m_hasConnectedViaNGO = true;
        }

        // These are public for use in the Inspector.
        public void OnLobbyChange(LocalLobby lobby)
        {
            m_lobby = lobby; // Most of the time this is redundant, but we need to get multiple members of the lobby to the Relay setup components, so might as well just hold onto the whole thing.
        }
        public void OnLocalUserChange(LobbyUser user)
        {
            m_localUser = user; // Same, regarding redundancy.
        }

        /// <summary>
        /// Once the Relay Allocation is created, this passes its data to the UnityTransport.
        /// </summary>
        public void SetRelayServerData(string address, int port, byte[] allocationBytes, byte[] key, byte[] connectionData, byte[] hostConnectionData, bool isSecure)
        {
            m_initializeTransport = (transport) => { transport.SetRelayServerData(address, (ushort)port, allocationBytes, key, connectionData, hostConnectionData, isSecure); };
        }

        public void OnReceiveMessage(MessageType type, object msg)
        {
            if (type == MessageType.ConfirmInGameState)
            {
                m_doesNeedCleanup = true;
                SetMenuVisibility(false);
                SetGOActive();
                SetGODisable();
                CreateNetworkManager();
            }

            else if (type == MessageType.MinigameBeginning)
            {
                if (!m_hasConnectedViaNGO)
                {
                    // If this player hasn't successfully connected via NGO, forcibly exit the minigame.
                    Locator.Get.Messenger.OnReceiveMessage(MessageType.DisplayErrorPopup, "Failed to join the game.");
                    OnGameEnd();
                }
            }

            else if (type == MessageType.ChangeGameState)
            {
                // Once we're in-game, any state change reflects the player leaving the game, so we should clean up.
                OnGameEnd();
            }
        }

        /// <summary>
        /// Return to the lobby after the game, whether due to the game ending or due to a failed connection.
        /// </summary>
        private void OnGameEnd()
        {
            if (m_doesNeedCleanup)
            {
                GameObject.Destroy(m_inGameManagerObj); // Since this destroys the NetworkManager, that will kick off cleaning up networked objects.
                GameObject.Destroy(m_inGameLogicObj); // Since this destroys the NetworkManager, that will kick off cleaning up networked objects.
                GameObject.Destroy(spawnerManagerGO);
                GameObject.Destroy(greekS);
                GameObject.Destroy(asianS);
                SetGOReActive();
                SetMenuVisibility(true);
                m_lobby.RelayNGOCode = null;
                m_doesNeedCleanup = false;
            }
        }
    }
}
