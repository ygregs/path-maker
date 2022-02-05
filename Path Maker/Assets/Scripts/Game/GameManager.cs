using System.Collections.Generic;
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

        private LocalGameState m_localGameState = new LocalGameState();
        private LobbyUser m_localUser;
        private LocalLobby m_localLobby;

        private void Start()
        {
            m_localLobby = new LocalLobby { State = LobbyState.Lobby };
            m_localUser = new LobbyUser();
            m_localUser.DisplayName = "New Player";
            Locator.Get.Messenger.Subscribe(this);
            BeginObservers();
        }

        private void BeginObservers()
        {
            foreach (var gameStateObs in m_GameStateObservers)
            {
                gameStateObs.BeginObserving(m_localGameState);
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
            if (type == MessageType.ClientUserApproved)
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
            m_localGameState.State = state;
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