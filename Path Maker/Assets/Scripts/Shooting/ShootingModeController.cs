using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

namespace PathMaker.shooting
{
    public class ShootingModeController : NetworkBehaviour
    {
        private PlayerInputs _input;
        public ShootingMode _shootingMode = ShootingMode.Blue;
        private ShootingModePanel _bluePanel;
        private ShootingModePanel _greenPanel;
        private ShootingModePanel _redPanel;

        void Awake()
        {
            _input = GetComponent<PlayerInputs>();
            _bluePanel = GameObject.Find("BlueModePanel").GetComponent<ShootingModePanel>();
            _greenPanel = GameObject.Find("GreenModePanel").GetComponent<ShootingModePanel>();
            _redPanel = GameObject.Find("RedModePanel").GetComponent<ShootingModePanel>();
            // print(_bluePanel);
        }

        void Update()
        {
            if (_input.changeShootingModeUp)
            {
                _input.changeShootingModeUp = false;
                switch (_shootingMode)
                {
                    case ShootingMode.Blue:
                        _shootingMode = ShootingMode.Red;
                        _bluePanel.Disable();
                        _redPanel.Enable();
                        break;
                    case ShootingMode.Green:
                        _shootingMode = ShootingMode.Blue;
                        _greenPanel.Disable();
                        _bluePanel.Enable();
                        break;
                    case ShootingMode.Red:
                        _shootingMode = ShootingMode.Green;
                        _redPanel.Disable();
                        _greenPanel.Enable();
                        break;
                    default:
                        break;
                }
            }

            if (_input.changeShootingModeDown)
            {
                _input.changeShootingModeDown = false;
                switch (_shootingMode)
                {
                    case ShootingMode.Blue:
                        _shootingMode = ShootingMode.Green;
                        _bluePanel.Disable();
                        _greenPanel.Enable();
                        break;
                    case ShootingMode.Green:
                        _shootingMode = ShootingMode.Red;
                        _greenPanel.Disable();
                        _redPanel.Enable();
                        break;
                    case ShootingMode.Red:
                        _shootingMode = ShootingMode.Blue;
                        _redPanel.Disable();
                        _bluePanel.Enable();
                        break;
                    default:
                        break;
                }
            }
        }

        public void SetText(int num)
        {
            switch (_shootingMode)
            {
                case ShootingMode.Blue:
                    _bluePanel.SetText(num);
                    break;
                case ShootingMode.Green:
                    _greenPanel.SetText(num);
                    break;
                case ShootingMode.Red:
                    _redPanel.SetText(num);
                    break;
                default:
                    break;
            }
        }
    }
}