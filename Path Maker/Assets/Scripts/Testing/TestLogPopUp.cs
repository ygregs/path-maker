using UnityEngine;

namespace PathMaker
{
    public class TestLogPopUp : MonoBehaviour
    {
        public void SendTestMessage()
        {
            Locator.Get.Messenger.OnReceiveMessage(MessageType.DisplayErrorPopup, "Testing log handler popup.");
        }
    }
}