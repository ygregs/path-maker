using UnityEngine;

namespace PathMaker.UI
{
    public class GameStateVisibilityUI : ObserverPanel<LocalGameState>
    {
        [SerializeField]
        private GameState ShowThisWhen;

        public override void ObservedUpdated(LocalGameState observed)
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