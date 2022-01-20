using System;

namespace PathMaker
{

    [Flags]
    public enum GameState
    {
        Menu = 1,
        Settings = 2,
        Matchmaking = 4,
    }

    [System.Serializable]
    public class LocalGameState : Observed<LocalGameState>
    {
        private GameState m_State = GameState.Menu;

        public GameState State
        {
            get => m_State;
            set
            {
                if (m_State != value)
                {
                    m_State = value;
                    OnChanged(this);
                }
            }
        }
    }
}