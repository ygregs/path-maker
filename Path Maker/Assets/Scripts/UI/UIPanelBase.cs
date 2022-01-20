using UnityEngine;

namespace PathMaker.UI
{

    [RequireComponent(typeof(CanvasGroup))]
    public class UIPanelBase : MonoBehaviour
    {
        private bool showing;
        private CanvasGroup m_CanvasGroup;

        protected CanvasGroup MyCanvasGroup
        {
            get
            {
                if (m_CanvasGroup != null)
                {
                    return m_CanvasGroup;
                }
                return m_CanvasGroup = GetComponent<CanvasGroup>();

            }
        }

        public void Toggle()
        {
            if (showing)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        public void Show()
        {
            MyCanvasGroup.alpha = 1;
            MyCanvasGroup.interactable = true;
            MyCanvasGroup.blocksRaycasts = true;
            showing = true;
        }

        public void Hide()
        {
            MyCanvasGroup.alpha = 0;
            MyCanvasGroup.interactable = false;
            MyCanvasGroup.blocksRaycasts = false;
            showing = false;
        }
    }
}