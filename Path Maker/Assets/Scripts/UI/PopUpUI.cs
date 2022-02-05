using System.Text;
using TMPro;
using UnityEngine;

namespace PathMaker
{
    public class PopUpUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField m_popupText;
        [SerializeField]
        private CanvasGroup m_buttonVisibility;
        private float m_buttonVisibilityTimeout = -1;
        private StringBuilder m_currentText = new StringBuilder();

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void ShowPopup(string newText)
        {
            if (!gameObject.activeSelf)
            {
                m_currentText.Clear();
                gameObject.SetActive(true);
            }
            m_currentText.AppendLine(newText);
            m_popupText.SetTextWithoutNotify(m_currentText.ToString());
            DisableButton();
        }

        private void DisableButton()
        {
            m_buttonVisibilityTimeout = 0.5f;
            m_buttonVisibility.alpha = 0.5f;
            m_buttonVisibility.interactable = false;
        }

        private void ReenalbleButton()
        {
            m_buttonVisibility.alpha = 1;
            m_buttonVisibility.interactable = true;
        }

        private void Update()
        {
            if (m_buttonVisibilityTimeout >= 0 && m_buttonVisibilityTimeout - Time.deltaTime < 0)
            {
                ReenalbleButton();
            }
            m_buttonVisibilityTimeout -= Time.deltaTime;
        }

        public void ClearPopup()
        {
            gameObject.SetActive(false);
        }
    }
}