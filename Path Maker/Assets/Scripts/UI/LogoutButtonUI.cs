using UnityEngine;

namespace PathMaker.UI
{
    public class LogoutButtonUI : MonoBehaviour, IReceiveMessages
    {
        public void Start()
        {
            Locator.Get.Messenger.Subscribe(this);
        }

        public void OnReceiveMessage(MessageType type, object msg)
        {
            // print($"Receive message of type {type} : \"{msg}\"");
            if (type == MessageType.LogoutResponse)
            {
                if ((string)msg == "LOGOUT")
                {
                    Locator.Get.Messenger.OnReceiveMessage(MessageType.ChangeAuthState, AState.Logout);
                }
                else
                {
                    Locator.Get.Messenger.OnReceiveMessage(MessageType.DisplayErrorPopup, msg);
                }
            }
        }

        public void Logout()
        {
            AuthAsyncRequests.Instance.Logout();

        }
    }
}