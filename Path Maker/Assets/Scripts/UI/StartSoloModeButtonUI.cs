using UnityEngine;

namespace PathMaker.UI
{
    public class StartSoloModeButtonUI : MonoBehaviour
    {

        public void StartSolo()
        {
            // print("ehllo");
            Locator.Get.Messenger.OnReceiveMessage(MessageType.ChangeGameState, GameState.SoloMode);
        }
    }
}