using UnityEngine;

namespace PathMaker.UI
{
    public class StartLobbyButtonUI : MonoBehaviour
    {
        public void ToLobbyMenu()
        {
            Locator.Get.Messenger.OnReceiveMessage(MessageType.ChangeGameState, GameState.JoinMenu);
        }
    }
}