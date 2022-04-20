using UnityEngine;
using TMPro;

namespace PathMaker
{
    public class RegisterManager : MonoBehaviour, IReceiveMessages
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
            print($"Receive message of type {type} : \"{msg}\"");
            if (type == MessageType.RegisterResponse)
            {
                if ((string)msg == "Success")
                {
                    Locator.Get.Messenger.OnReceiveMessage(MessageType.ChangeGameState, GameState.LoginMenu);
                    // Locator.Get.Messenger.OnReceiveMessage(MessageType.ChangeAuthState, AState.Login);
                }
                else
                {
                    Locator.Get.Messenger.OnReceiveMessage(MessageType.DisplayErrorPopup, msg);
                }
            }
        }

        public void Register()
        {
            AuthAsyncRequests.Instance.RegisterNewPlayer(m_playernameInput.text, m_passwordInput.text);
        }

    }
}