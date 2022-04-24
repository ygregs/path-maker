using System;

namespace PathMaker
{

    [Flags]
    public enum AState
    {
        Login = 1,
        Logout = 2,
    }

    [System.Serializable]
    public class AuthState : Observed<AuthState>
    {
        private AState m_State = AState.Logout;

        public AState State
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

        public override void CopyObserved(AuthState oldObserved)
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