using System;
using UnityEngine;
using Unity.Netcode;

namespace PathMaker
{
    public enum DoorType
    {
        FirstDoor,
        LastDoor,
    }
    public class DoorBehaviour : NetworkBehaviour
    {

        public NetworkVariable<bool> IsOpen = new NetworkVariable<bool>(false);

        public DoorType m_type = DoorType.FirstDoor;

        [SerializeField]
        private Animator m_animator;

        private Action m_onOpenComplete;
        private Action m_onCloseComplete;

        public void DoOpenDoor(Action onOpenComplete)
        {
            m_onOpenComplete = onOpenComplete;
            m_animator.SetTrigger("Open");
        }

        public void SetIsOpen(bool state)
        {
            IsOpen.Value = state;
        }

        [ServerRpc]
        public void SetIsOpenServerRpc(bool state)
        {
            IsOpen.Value = state;
        }
        public void DoCloseDoor(Action onCloseComplete)
        {
            m_onCloseComplete = onCloseComplete;
            m_animator.SetTrigger("Close");
        }
        public void OnOpenComplete()
        {
            m_onOpenComplete?.Invoke();
        }

        public void OnCloseComplete()
        {
            m_onCloseComplete?.Invoke();
        }
    }
}