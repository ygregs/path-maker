using System;
using UnityEngine;

namespace PathMaker.ngo
{
    /// <summary>
    /// Handles any visual tasks for running the NGO minigame's intro and outro.
    /// </summary>
    public class IntroOutroRunner : MonoBehaviour
    {
        [SerializeField] private Animator m_animator;
        private Action m_onOutroComplete;

        public void DoIntro()
        {
            // Debug.Log("Doing intro");
            m_animator.SetTrigger("DoIntro");
        }

        public void DoLastRound()
        {
            m_animator.SetTrigger("DoLastRound");
        }

        public void DoOutro(Action onOutroComplete)
        {
            m_onOutroComplete = onOutroComplete;
            // Debug.Log("Doing outro");
            m_animator.SetTrigger("DoOutro");
        }

        /// <summary>
        /// Called via an AnimationEvent.
        /// </summary>
        public void OnIntroComplete()
        {
            Locator.Get.Messenger.OnReceiveMessage(MessageType.InstructionsShown, null);
        }

        public void OnLastRoundComplete()
        {

        }
        /// <summary>
        /// Called via an AnimationEvent.
        /// </summary>
        public void OnOutroComplete()
        {
            m_onOutroComplete?.Invoke();
        }
    }
}
