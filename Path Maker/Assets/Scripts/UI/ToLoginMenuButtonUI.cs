using UnityEngine;

namespace PathMaker.UI
{
    public class ToLoginMenuButtonUI : MonoBehaviour
    {

        public void ToLoginMenu()
        {
            Locator.Get.Messenger.OnReceiveMessage(MessageType.ChangeGameState, GameState.LoginMenu);
        }
    }
}