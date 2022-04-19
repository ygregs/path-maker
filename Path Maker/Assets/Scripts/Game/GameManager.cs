using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;

using UnityEngine;

namespace PathMaker
{
    public class GameManager : MonoBehaviour, IReceiveMessages
    {
        [SerializeField]
        private List<LocalGameStateObserver> m_GameStateObservers = new List<LocalGameStateObserver>();
        [SerializeField]
        private List<LocalLobbyObserver> m_LocalLobbyObservers = new List<LocalLobbyObserver>();
        [SerializeField]
        private List<LobbyUserObserver> m_LocalUserObservers = new List<LobbyUserObserver>();
        [SerializeField]
        private List<LobbyServiceDataObserver> m_LobbyServiceObservers = new List<LobbyServiceDataObserver>();

        private LocalGameState m_localGameState = new LocalGameState();
        private LobbyUser m_localUser;
        private LocalLobby m_localLobby;

        private LobbyServiceData m_lobbyServiceData = new LobbyServiceData();
        private LobbyContentHeartbeat m_lobbyContentHeartbeat = new LobbyContentHeartbeat();
        private relay.RelayUtpSetup m_relaySetup;
        private relay.RelayUtpClient m_relayClient;

        private void Awake()
        {
            // Do some arbitrary operations to instantiate singletons.
#pragma warning disable IDE0059 // Unnecessary assignment of a value
            var unused = Locator.Get;
#pragma warning restore IDE0059

            Locator.Get.Provide(new Auth.Identity(OnAuthSignIn));
            // Init authenticator locator
            Locator.Get.Provide(new Auth.Authenticator());
            Application.wantsToQuit += OnWantToQuit;
        }
        private void Start()
        {
            RetrieveLogInfo();

            m_localLobby = new LocalLobby { State = LobbyState.Lobby };
            m_localUser = new LobbyUser();
            m_localUser.DisplayName = "New Player";
            Locator.Get.Messenger.Subscribe(this);
            BeginObservers();
        }

        private void RetrieveLogInfo()
        {
            // Get session cookie and update log status
            string cookie = PlayerPrefs.GetString("session_cookie");
            Auth.AuthData authData = Locator.Get.Authenticator.GetAuthData();
            authData.SetContent("session_cookie", cookie);
            // Set log status
            if (!string.IsNullOrEmpty(cookie))
            {
                authData.SetContent("log_status", "LOGGED");
            }
            else
            {
                authData.SetContent("log_status", "NOT_LOGGED");
            }
        }

        private void OnAuthSignIn()
        {
            Debug.Log("Signed in.");
            m_localUser.ID = Locator.Get.Identity.GetSubIdentity(Auth.IIdentityType.Auth).GetContent("id");
            // m_localUser.ID = "ceci_est_mon_id";
            // Debug.Log(AuthenticationService.Instance.PlayerId);
            m_localUser.DisplayName = NameGenerator.GetName(m_localUser.ID);
            m_localLobby.AddPlayer(m_localUser); // The local LobbyUser object will be hooked into UI before the LocalLobby is populated during lobby join, so the LocalLobby must know about it already when that happens.
            //StartVivoxLogin();
        }

        private void BeginObservers()
        {
            foreach (var gameStateObs in m_GameStateObservers)
            {
                gameStateObs.BeginObserving(m_localGameState);
            }
            foreach (var serviceObs in m_LobbyServiceObservers)
            {
                serviceObs.BeginObserving(m_lobbyServiceData);
            }
            foreach (var lobbyObs in m_LocalLobbyObservers)
            {
                lobbyObs.BeginObserving(m_localLobby);
            }
            foreach (var userObs in m_LocalUserObservers)
            {
                userObs.BeginObserving(m_localUser);
            }
        }

        public void OnReceiveMessage(MessageType type, object msg)
        {
            print($"Receive message of type {type} : \"{msg}\"");
            if (type == MessageType.CreateLobbyRequest)
            {
                LocalLobby.LobbyData createLobbyData = (LocalLobby.LobbyData)msg;
                LobbyAsyncRequests.Instance.CreateLobbyAsync(createLobbyData.LobbyName, createLobbyData.MaxPlayerCount, createLobbyData.Private, m_localUser, (r) =>
                    {
                        lobby.ToLocalLobby.Convert(r, m_localLobby);
                        OnCreatedLobby();
                    },
                    OnFailedJoin);
            }
            else if (type == MessageType.JoinLobbyRequest)
            {
                LocalLobby.LobbyData lobbyInfo = (LocalLobby.LobbyData)msg;
                LobbyAsyncRequests.Instance.JoinLobbyAsync(lobbyInfo.LobbyID, lobbyInfo.LobbyCode, m_localUser, (r) =>
                    {
                        lobby.ToLocalLobby.Convert(r, m_localLobby);
                        OnJoinedLobby();
                    },
                    OnFailedJoin);
            }
            else if (type == MessageType.QueryLobbies)
            {
                m_lobbyServiceData.State = LobbyQueryState.Fetching;
                LobbyAsyncRequests.Instance.RetrieveLobbyListAsync(
                    qr =>
                    {
                        if (qr != null)
                            OnLobbiesQueried(lobby.ToLocalLobby.Convert(qr));
                    },
                    er =>
                    {
                        OnLobbyQueryFailed();
                    });
            }
            else if (type == MessageType.ClientUserApproved)
            {
                ConfirmApproval();
            }
            else if (type == MessageType.LobbyUserStatus)
            {
                m_localUser.UserStatus = (UserStatus)msg;
            }
            else if (type == MessageType.ChangeGameState)
            {
                SetGameState((GameState)msg);
            }
        }

        private void SetGameState(GameState state)
        {
            bool isLeavingLobby = (state == GameState.Menu || state == GameState.JoinMenu) && m_localGameState.State == GameState.Lobby;
            m_localGameState.State = state;
            if (isLeavingLobby)
            {
                OnLeftLobby();
            }
        }

        private void OnLobbiesQueried(IEnumerable<LocalLobby> lobbies)
        {
            var newLobbyDict = new Dictionary<string, LocalLobby>();
            foreach (var lobby in lobbies)
                newLobbyDict.Add(lobby.LobbyID, lobby);

            m_lobbyServiceData.State = LobbyQueryState.Fetched;
            m_lobbyServiceData.CurrentLobbies = newLobbyDict;
        }

        private void OnLobbyQueryFailed()
        {
            m_lobbyServiceData.State = LobbyQueryState.Error;
        }

        private void OnCreatedLobby()
        {
            m_localUser.IsHost = true;
            OnJoinedLobby();
        }

        private void OnJoinedLobby()
        {
            LobbyAsyncRequests.Instance.BeginTracking(m_localLobby.LobbyID);
            m_lobbyContentHeartbeat.BeginTracking(m_localLobby, m_localUser);
            SetUserLobbyState();

            // The host has the opportunity to reject incoming players, but to do so the player needs to connect to Relay without having game logic available.
            // In particular, we should prevent players from joining voice chat until they are approved.
            OnReceiveMessage(MessageType.LobbyUserStatus, UserStatus.Connecting);
            if (m_localUser.IsHost)
            {
                StartRelayConnection();
                //StartVivoxJoin();
            }
            else
            {
                StartRelayConnection();
            }
        }

        private void OnLeftLobby()
        {
            m_localUser.ResetState();
            LobbyAsyncRequests.Instance.LeaveLobbyAsync(m_localLobby.LobbyID, ResetLocalLobby);
            m_lobbyContentHeartbeat.EndTracking();
            LobbyAsyncRequests.Instance.EndTracking();
            //m_vivoxSetup.LeaveLobbyChannel();

            if (m_relaySetup != null)
            {
                Component.Destroy(m_relaySetup);
                m_relaySetup = null;
            }
            if (m_relayClient != null)
            {
                m_relayClient.Dispose();
                StartCoroutine(FinishCleanup());

                // We need to delay slightly to give the disconnect message sent during Dispose time to reach the host, so that we don't destroy the connection without it being flushed first.
                IEnumerator FinishCleanup()
                {
                    yield return null;
                    Component.Destroy(m_relayClient);
                    m_relayClient = null;
                }
            }
        }

        private void OnFailedJoin()
        {
            SetGameState(GameState.JoinMenu);
        }


        private void StartRelayConnection()
        {
            if (m_localUser.IsHost)
                m_relaySetup = gameObject.AddComponent<relay.RelayUtpSetupHost>();
            else
                m_relaySetup = gameObject.AddComponent<relay.RelayUtpSetupClient>();
            m_relaySetup.BeginRelayJoin(m_localLobby, m_localUser, OnRelayConnected);

            void OnRelayConnected(bool didSucceed, relay.RelayUtpClient client)
            {
                Component.Destroy(m_relaySetup);
                m_relaySetup = null;

                if (!didSucceed)
                {
                    Debug.LogError("Relay connection failed! Retrying in 5s...");
                    StartCoroutine(RetryConnection(StartRelayConnection, m_localLobby.LobbyID));
                    return;
                }

                m_relayClient = client;
                if (m_localUser.IsHost)
                    CompleteRelayConnection();
                else
                    Debug.Log("Client is now waiting for approval...");
            }
        }

        private IEnumerator RetryConnection(Action doConnection, string lobbyId)
        {
            yield return new WaitForSeconds(5);
            if (m_localLobby != null && m_localLobby.LobbyID == lobbyId && !string.IsNullOrEmpty(lobbyId)) // Ensure we didn't leave the lobby during this waiting period.
                doConnection?.Invoke();
        }

        private void ConfirmApproval()
        {
            if (!m_localUser.IsHost && m_localUser.IsApproved)
            {
                CompleteRelayConnection();
                // StartVivoxJoin();
            }
        }

        private void CompleteRelayConnection()
        {
            OnReceiveMessage(MessageType.LobbyUserStatus, UserStatus.Lobby);
        }

        private void SetUserLobbyState()
        {
            SetGameState(GameState.Lobby);
            OnReceiveMessage(MessageType.LobbyUserStatus, UserStatus.Lobby);
        }

        private void ResetLocalLobby()
        {
            m_localLobby.CopyObserved(new LocalLobby.LobbyData(), new Dictionary<string, LobbyUser>());
            m_localLobby.AddPlayer(m_localUser); // As before, the local player will need to be plugged into UI before the lobby join actually happens.
            m_localLobby.RelayServer = null;
        }

        private IEnumerator LeaveBeforeQuit()
        {
            ForceLeaveAttempt();
            yield return null;
            Application.Quit();
        }

        private bool OnWantToQuit()
        {
            bool canQuit = string.IsNullOrEmpty(m_localLobby?.LobbyID);
            StartCoroutine(LeaveBeforeQuit());
            return canQuit;
        }

        private void OnDestroy()
        {
            ForceLeaveAttempt();
        }

        private void ForceLeaveAttempt()
        {
            Locator.Get.Messenger.Unsubscribe(this);
        }
    }
}