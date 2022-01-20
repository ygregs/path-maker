using UnityEngine;

namespace PathMaker.UI
{
    public class StartMatchmakingButtonUI : MonoBehaviour
    {
        public void ToMatchmakingMenu()
        {
            Locator.Get.Messenger.OnReceiveMessage(MessageType.ChangeGameState, GameState.Matchmaking);
        }
    }
}