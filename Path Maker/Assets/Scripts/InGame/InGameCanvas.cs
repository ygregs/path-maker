using System;
using UnityEngine;

namespace PathMaker.ingame
{
    public class InGameCanvas : MonoBehaviour
    {
        [SerializeField] Animator _animator;

        private Action _onSpawnPlayer;
        public void DoBlackScreen(Action onSpawnPlayer)
        {
            _onSpawnPlayer = onSpawnPlayer;
            _animator.SetTrigger("BlackScreen");
        }

        public void OnSpawnPlayer()
        {
            _onSpawnPlayer?.Invoke();
        }
    }
}