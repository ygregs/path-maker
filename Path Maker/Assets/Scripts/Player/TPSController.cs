using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using System.Collections;

public class TPSController : NetworkBehaviour
{
    [Header("Player")]
    public float MoveSpeed = 5.0f;

    public float SprintSpeed = 8.0f;
    public float CrouchSpeed = 2.0f;

    public float AimSpeed = 2.0f;

    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    private float SpeedChangeRate = 10.0f;

    [Space(10)]
    [SerializeField] private float JumpHeight = 0.5f;

    [SerializeField] private float Gravity = -9.81f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;
    private float LookSensibility;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;
    public float RightClamp = float.MinValue;
    public float LeftClamp = float.MaxValue;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // player
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;
    private PlayerInput _playerInput;

    private Animator _animator;
    private NetworkVariable<PlayerState> networkAnimationState = new NetworkVariable<PlayerState>();
    private PlayerState oldPlayerState = PlayerState.Idle;

    public bool IsDead = false;
    private CharacterController _controller;
    private ShooterController _shooterController;
    private PlayerInputs _input;
    private GameObject _mainCamera;
    private bool _rotateOnMove = true;

    private const float _threshold = 0.01f;

    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
            return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
        }
    }
    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    private void Start()
    {
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInputs>();
        _playerInput = GetComponent<PlayerInput>();
        _animator = GetComponent<Animator>();
        _shooterController = GetComponent<ShooterController>();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    private void Update()
    {
        if (IsClient && IsOwner)
        {
            JumpAndGravity();
            GroundedCheck();
            Move();
        }

        ClientVisuals();
    }

    [ServerRpc]
    public void UpdatePlayerStateServerRpc(PlayerState state)
    {
        networkAnimationState.Value = state;
    }

    private void ClientVisuals()
    {
        if (oldPlayerState != networkAnimationState.Value)
        {
            oldPlayerState = networkAnimationState.Value;
            _animator.SetTrigger($"{networkAnimationState.Value}");
        }
    }

    private void LateUpdate()
    {
        // if (IsLocalPlayer)
        if (IsClient && IsOwner)
        {
            CameraRotation();
        }
    }
    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        // _animator.SetBool(_animIDGrounded, Grounded);
    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier * LookSensibility;
            _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier * LookSensibility;
        }

        // clamp our rotations so our values are limited 360 degrees
        // _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, RightClamp, LeftClamp);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    private void Move()
    {
        if (!IsDead)
        {
            // stop sprint if aiming
            if (_input.aim)
            {
                _input.sprint = false;
            }

            // set target speed based on move speed, sprint speed and if sprint is pressed and aim speed if aim is pressed
            float targetSpeed = MoveSpeed;
            if (_input.sprint && !_input.aim)
            {
                targetSpeed = SprintSpeed;
            }
            else if (_input.aim)
            {
                targetSpeed = AimSpeed;
            }

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                              _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                RotationSmoothTime);

            if (_input.move != Vector2.zero)
            {
                // rotate to face input direction relative to camera position
                if (_rotateOnMove)
                {
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                        new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            if (!_input.jump)
            {
                if (_input.move.x == 0 && _input.move.y == 0)
                {
                    if (_input.crouch)
                    {
                        UpdatePlayerStateServerRpc(PlayerState.IdleCrouch);
                    }
                    else
                    {
                        UpdatePlayerStateServerRpc(PlayerState.Idle);
                    }
                }
                else if (!_input.sprint)
                {
                    if (_input.crouch)
                    {
                        UpdatePlayerStateServerRpc(PlayerState.WalkCrouch);
                    }
                    else
                    {
                        if (_input.aim)
                        {
                            if (_input.move.x == 0 && _input.move.y == 0)
                            {
                                UpdatePlayerStateServerRpc(PlayerState.Idle);
                            }
                            else if (_input.move.x >= 0.85f && _input.move.y < 0.4f && _input.move.y > -0.4f)
                            {
                                UpdatePlayerStateServerRpc(PlayerState.RightStrafeWalk);
                            }
                            else if (_input.move.x <= -0.85f && _input.move.y < 0.4f && _input.move.y > -0.4f)
                            {
                                UpdatePlayerStateServerRpc(PlayerState.LeftStrafeWalk);
                            }
                            else if (_input.move.x < 0.4f && _input.move.x > -0.4f && _input.move.y <= -0.85f)
                            {
                                UpdatePlayerStateServerRpc(PlayerState.BackwardsWalk);
                            }
                            else
                            {
                                UpdatePlayerStateServerRpc(PlayerState.Walk);
                            }
                        }
                        else
                        {
                            UpdatePlayerStateServerRpc(PlayerState.Walk);
                        }
                    }
                }
                else if (_input.sprint && !_input.aim)
                {
                    if (_input.crouch)
                    {
                        UpdatePlayerStateServerRpc(PlayerState.RunCrouch);
                    }
                    else
                    {
                        UpdatePlayerStateServerRpc(PlayerState.Run);
                    }
                }

            }
        }
        else
        {
            UpdatePlayerStateServerRpc(PlayerState.WalkingToDying);
        }
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // update animator if using character
            // _animator.SetBool(_animIDJump, false);
            // _animator.SetBool(_animIDFreeFall, false);
            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                // update animator if using character
                // _animator.SetBool(_animIDJump, true);
                // UpdateJumpingStateServerRpc(true);
                UpdatePlayerStateServerRpc(PlayerState.Jump);
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // update animator if using character
                // _animator.SetBool(_animIDFreeFall, true);
            }

            // if we are not grounded, do not jump
            _input.jump = false;

        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    public void SetSensibility(float newSensibility)
    {
        LookSensibility = newSensibility;
    }

    public void SetRotateOnMove(bool newRotateOnMove)
    {
        if (IsClient && IsOwner)
        {
            _rotateOnMove = newRotateOnMove;
        }
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        // if (animationEvent.animatorClipInfo.weight > 0.5f)
        // {
        //     if (FootstepAudioClips.Length > 0)
        //     {
        //         var index = Random.Range(0, FootstepAudioClips.Length);
        //         AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
        //     }
        // }
    }

    public void OnAim()
    {
        if (IsClient && IsOwner && !IsDead)
        {
            var curX = _input.move.x;
            var curY = _input.move.y;
            if (_input.move.x != 0 || _input.move.y != 0)
            {
                _input.move.x = 0;
                _input.move.y = 0;
            }
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                           _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                0.005f);
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            RightClamp = _mainCamera.transform.eulerAngles.y - 15f;
            LeftClamp = _mainCamera.transform.eulerAngles.y + 30f;
            _input.move.x = curX;
            _input.move.y = curY;
        }
    }

    public void Dying()
    {
        if (IsClient && IsOwner)
        {
            _animator.applyRootMotion = true;
            _input.aim = false;
            _input.sprint = false;
            _controller.height = 0;
            _controller.radius = 0;
            _controller.skinWidth = 0;
            _controller.enabled = false;
            IsDead = true;
            _shooterController.ResetThirdCamera();
            _shooterController.IsDead = true;
            StartCoroutine(WaitForRespawn());
        }
    }

    IEnumerator WaitForRespawn()
    {
        if (IsClient && IsOwner)
        {
            yield return new WaitForSeconds(5);
            FindObjectOfType<PathMaker.ingame.InGameCanvas>().DoBlackScreen(DespawnPlayer);
        }
    }

    void DespawnPlayer()
    {
        if (IsClient && IsOwner)
        {
            gameObject.GetComponent<SkinRenderer>().SetMeshesActive(false);
            FindObjectOfType<PlayerHealthTest>().DespawnPlayer_ServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }
}