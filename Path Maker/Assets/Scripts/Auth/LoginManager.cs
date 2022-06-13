using UnityEngine;
using TMPro;

namespace PathMaker
{
    public class LoginManager : MonoBehaviour, IReceiveMessages
    {
        [SerializeField]
        TMP_InputField m_playernameInput;

        [SerializeField]
        TMP_InputField m_passwordInput;

        public void Start()
        {
            Locator.Get.Messenger.Subscribe(this);
        }

        public void OnReceiveMessage(MessageType type, object msg)
        {
            // print($"Receive message of type {type} : \"{msg}\"");
            if (type == MessageType.LoginResponse)
            {
                if ((string)msg == "LOGGED")
                {
                    Locator.Get.Messenger.OnReceiveMessage(MessageType.ChangeGameState, GameState.Menu);
                    Locator.Get.Messenger.OnReceiveMessage(MessageType.ChangeAuthState, AState.Login);
                }
                else
                {
                    Locator.Get.Messenger.OnReceiveMessage(MessageType.DisplayErrorPopup, msg);
                }
            }
        }

        public void Login()
        {
            AuthAsyncRequests.Instance.LoginUser(m_playernameInput.text, m_passwordInput.text);
        }

    }
}