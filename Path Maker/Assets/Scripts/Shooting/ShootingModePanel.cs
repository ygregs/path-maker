using UnityEngine;
using TMPro;

namespace PathMaker.shooting
{
    public class ShootingModePanel : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        [SerializeField] private TMP_Text _numText;
        [SerializeField] private Color _zeroColor;
        [SerializeField] private Color _firstColor;
        [SerializeField] private Color _secondColor;

        void Awake()
        {
            _canvasGroup = gameObject.GetComponent<CanvasGroup>();
        }
        public void Enable()
        {
            _canvasGroup.alpha = 1f;
        }

        public void Disable()
        {
            _canvasGroup.alpha = 0.4f;
        }

        public void SetText(int num)
        {
            switch (num)
            {
                case 0:
                    _numText.color = _zeroColor;
                    break;
                case 1:
                    _numText.color = _firstColor;
                    break;
                case 2:
                    _numText.color = _secondColor;
                    break;
                default:
                    break;
            }
            _numText.text = num.ToString();
        }
    }
}