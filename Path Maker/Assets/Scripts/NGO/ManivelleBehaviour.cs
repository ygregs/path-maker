using System;
using UnityEngine;
using Unity.Netcode;

namespace PathMaker
{
    public enum ManiType
    {
        Easy,
        Hard,
    }
    public class ManivelleBehaviour : NetworkBehaviour
    {

        public NetworkVariable<bool> IsOpen = new NetworkVariable<bool>(false);
        public NetworkVariable<bool> CanOpen = new NetworkVariable<bool>(false);

        public ManiType m_type = ManiType.Easy;

        [SerializeField]
        private Unity.Netcode.Components.NetworkAnimator m_animator;

        private Action m_onOpeningComplete;
        private Action m_onStopOpeningComplete;
        private Action m_onOpenComplete;
        private Action m_onCloseComplete;

        public void StartOpening(Action onOpeningComplete)
        {
            m_onOpeningComplete = onOpeningComplete;
            Debug.Log("start opening animation");
            m_animator.Animator.SetBool("IsActivating", true);
        }

        public void StopOpening(Action onStopOpeningComplete)
        {
            m_onStopOpeningComplete = onStopOpeningComplete;
            Debug.Log("stop opening animation");
            m_animator.Animator.SetBool("IsActivating", false);
        }

        public void DoOpenDoor(Action onOpenComplete)
        {
            m_onOpenComplete = onOpenComplete;
            Debug.Log("do openning animation");
            m_animator.Animator.SetBool("IsOpen", true);
        }

        public void SetIsOpen(bool state)
        {
            IsOpen.Value = state;
        }

        public void SetCanOpen(bool state)
        {
            CanOpen.Value = state;
        }

        [ServerRpc]
        public void SetIsOpenServerRpc(bool state)
        {
            IsOpen.Value = state;
        }

        [ServerRpc]
        public void SetCanOpenServerRpc(bool state)
        {
            CanOpen.Value = state;
        }

        public void DoCloseDoor(Action onCloseComplete)
        {
            m_onCloseComplete = onCloseComplete;
            m_animator.Animator.SetBool("IsOpen", false);
            // m_animator.SetTrigger("Close");
        }
        public void OnOpenComplete()
        {
            m_onOpenComplete?.Invoke();
        }

        public void OnCloseComplete()
        {
            m_onCloseComplete?.Invoke();
        }
        public void OnOpeningComplete()
        {
            m_onOpeningComplete?.Invoke();
        }

        public void OnStopOpeningComplete()
        {
            m_onStopOpeningComplete?.Invoke();
        }
    }
}