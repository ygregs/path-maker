using System;
using UnityEngine;

namespace PathMaker.player
{
    public class PlayerCanvas : MonoBehaviour
    {

        [SerializeField] private Animator _canvasAnimator;

        public void DoBlackScreen()
        {
            _canvasAnimator.SetTrigger("BlackScreen");
        }
    }
}