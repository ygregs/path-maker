using UnityEngine;

namespace PathMaker.UI
{
    /// For navigating the main menu.
    public class BackButtonUI : MonoBehaviour
    {
        public void ToJoinMenu()
        {
            Locator.Get.Messenger.OnReceiveMessage(MessageType.ChangeGameState, GameState.JoinMenu);
        }

        public void ToMenu()
        {
            Locator.Get.Messenger.OnReceiveMessage(MessageType.ChangeGameState, GameState.Menu);
        }
        public void ToLoginMenu()
        {
            Locator.Get.Messenger.OnReceiveMessage(MessageType.ChangeGameState, GameState.LoginMenu);
        }
    }
}
