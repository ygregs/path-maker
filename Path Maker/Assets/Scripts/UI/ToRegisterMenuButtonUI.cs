using UnityEngine;

namespace PathMaker.UI
{
    public class ToRegisterMenuButtonUI : MonoBehaviour
    {

        public void ToRegisterMenu()
        {
            Locator.Get.Messenger.OnReceiveMessage(MessageType.ChangeGameState, GameState.RegisterMenu);
        }
    }
}