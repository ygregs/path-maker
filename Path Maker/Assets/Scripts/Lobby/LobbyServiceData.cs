using System.Collections.Generic;

namespace PathMaker
{
    // Used when displaying the lobby list, to indicate when we are awaiting an updated lobby query 

    public enum LobbyQueryState
    {
        Empty,
        Fetching,
        Error,
        Fetched
    }

    // Holds data related to the Lobby service itself - The lastest lobby list
    [System.Serializable]
    public class LobbyServiceData : Observed<LobbyServiceData>
    {
        LobbyQueryState m_CurrentState = LobbyQueryState.Empty;

        public LobbyQueryState State
        {
            get
            {
                return m_CurrentState;
            }
            set
            {
                m_CurrentState = value;
                OnChanged(this);
            }
        }

        Dictionary<string, LocalLobby> m_currentLobbies = new Dictionary<string, LocalLobby>();

        public Dictionary<string, LocalLobby> CurrentLobbies
        {
            get { return m_currentLobbies; }
            set
            {
                m_currentLobbies = value;
                OnChanged(this);
            }
        }

        public override void CopyObserved(LobbyServiceData oldObserved)
        {
            m_currentLobbies = oldObserved.CurrentLobbies;
            OnChanged(this);
        }
    }
}