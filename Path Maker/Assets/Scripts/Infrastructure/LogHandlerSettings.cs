using UnityEngine;

namespace PathMaker
{
    public class LogHandlerSettings : MonoBehaviour, IReceiveMessages
    {
        [SerializeField]
        [Tooltip("Only los of this level or higher will appear in the console.")]
        private LogMode m_editorLogVerbosity = LogMode.Critical;

        [SerializeField]
        private PopUpUI m_popUp;

        private void Awake()
        {
            LogHandler.Get().mode = m_editorLogVerbosity;
            Locator.Get.Messenger.Subscribe(this);
        }

        private void OnDestroy()
        {
            Locator.Get.Messenger.Unsubscribe(this);
        }

        // Update the lo verbosity while in the Editor.
        public void OnValidate()
        {
            LogHandler.Get().mode = m_editorLogVerbosity;
        }

        public void OnReceiveMessage(MessageType type, object msg)
        {
            if (type == MessageType.DisplayErrorPopup && msg != null)

            {
                SpawnErrorPopUp((string)msg);
            }
        }

        private void SpawnErrorPopUp(string errorMessage)
        {
            m_popUp.ShowPopup(errorMessage);
        }

    }
}