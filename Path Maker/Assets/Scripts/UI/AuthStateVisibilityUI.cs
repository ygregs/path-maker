using UnityEngine;

namespace PathMaker.UI
{
    public class AuthStateVisibilityUI : ObserverPanel<AuthState>
    {
        [SerializeField]
        private AState ShowThisWhen;

        public override void ObservedUpdated(AuthState observed)
        {
            if (!ShowThisWhen.HasFlag(observed.State))
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
    }
}