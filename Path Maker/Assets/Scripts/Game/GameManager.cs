using System.Collections.Generic;
using UnityEngine;

namespace PathMaker
{
    public class GameManager : MonoBehaviour, IReceiveMessages
    {
        [SerializeField]
        private List<LocalGameStateObserver> m_GameStateObservers = new List<LocalGameStateObserver>();

        private LocalGameState m_localGameState = new LocalGameState();

        private void Start()
        {
            Locator.Get.Messenger.Subscribe(this);
            BeginObservers();
        }

        private void BeginObservers()
        {
            foreach (var gameStateObs in m_GameStateObservers)
            {
                gameStateObs.BeginObserving(m_localGameState);
            }
        }

        public void OnReceiveMessage(MessageType type, object msg)
        {
            print($"Receive message of type {type} : \"{msg}\"");
            if (type == MessageType.ChangeGameState)
            {
                SetGameState((GameState)msg);
            }
        }

        private void SetGameState(GameState state)
        {
            m_localGameState.State = state;
        }
    }
}