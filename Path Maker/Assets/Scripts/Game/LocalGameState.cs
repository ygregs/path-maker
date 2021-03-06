using System;

namespace PathMaker
{

    [Flags]
    public enum GameState
    {
        Menu = 1,
        Lobby = 2,
        JoinMenu = 4,
        LoginMenu = 8,
        RegisterMenu = 16,
        InGame = 32,
        PauseMenu = 64,
        SoloMode = 128,
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

        public override void CopyObserved(LocalGameState oldObserved)
        {
            if (m_State == oldObserved.State)
            {
                return;
            }
            m_State = oldObserved.State;
            OnChanged(this);
        }
    }
}