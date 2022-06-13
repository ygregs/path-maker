using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
// [RequireComponent(typeof(Unity.Netcode.Samples.ClientNetworkTransform))]
public class PlayerControlAuthorative : NetworkBehaviour
{
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed;
    [SerializeField] private float crouchSpeed;

    // [SerializeField] private float runSpeedOffset = 2.0f;

    [SerializeField] private float turnSmoothTime = 0.1f;

    // private float crouchSpeedOffset = 0.5f;
    private float turnSmoothVelocity;

    [SerializeField]
    private Vector2 defaultInitialPositionOnPlane = new Vector2(-4, 4);

    [SerializeField]
    private NetworkVariable<PlayerState> networkPlayerState = new NetworkVariable<PlayerState>();

    private CharacterController characterController;

    private Transform characterCamera;

    private Animator animator;

    // client caches animation states
    private PlayerState oldPlayerState = PlayerState.Idle;

    private bool IsInCrouch = false;

    private bool _rotateOnMove = true;
    // private float speed = 5; // units per second
    // private float turnSpeed = 90; // degrees per second
    // private float jumpSpeed = 8;
    // private float gravity = 9.8f;

    [SerializeField] private float Sensitivity = 1f;
    // [SerializeField] private float Gravity = 10;
    // [SerializeField] private bool m_isGrounded;
    // [SerializeField] private float JumpHeight = 1;


    public override void OnNetworkSpawn()
    {
        characterCamera = GameObject.Find("Main Camera").transform;
    }


    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // if (IsClient && IsOwner)
        // {
        //     transform.position = new Vector3(Random.Range(defaultInitialPositionOnPlane.x, defaultInitialPositionOnPlane.y), 0,
        //            Random.Range(defaultInitialPositionOnPlane.x, defaultInitialPositionOnPlane.y));
        // }
        var playerObject = NetworkManager.Singleton?.SpawnManager.GetLocalPlayerObject();
        var player = playerObject?.GetComponent<PathMaker.PlayerHud>();
        player?.SetName();
        // var ingamerunner = GameObject.Find("InGameRunner").GetComponent<PathMaker.ngo.InGameRunner>();
        // ingamerunner.localPlayer = gameObject;
        // ingamerunner.SpawnLocalPlayer();
        // ingamerunner.SpawnClientRpc();
        // ingamerunner.SpawnServerRpc();
    }

    void Update()
    {
        if (Input.GetButtonDown("Crouch"))
        {
            IsInCrouch = !IsInCrouch;
        }
        if (IsClient && IsOwner)
        {
            ClientInput();
        }

        ClientVisuals();
    }


    private void ClientVisuals()
    {
        if (oldPlayerState != networkPlayerState.Value)
        {
            oldPlayerState = networkPlayerState.Value;
            animator.SetTrigger($"{networkPlayerState.Value}");
            // Debug.Log("triggering: " + networkPlayerState.Value.ToString());
        }
    }

    private void ClientInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            //     if (Input.GetButtonDown("Jump") && m_isGrounded)
            //     {
            //         var _velocity = transform.position;
            //         _velocity.y += Mathf.Sqrt(JumpHeight * -2f * Gravity);
            //     }
            // if (characterController.isGrounded)
            // {

            //     vSpeed = 0; // grounded character has vSpeed = 0...
            //     if (Input.GetKeyDown("space"))
            //     { // unless it jumps:
            //         vSpeed = jumpSpeed;
            //     }
            // }
            UpdatePlayerStateServerRpc(PlayerState.Jump);
        }
        else
        {


            // y axis client rotation
            // Vector3 inputRotation = new Vector3(0, Input.GetAxis("Horizontal"), 0);

            // // forward & backward direction
            // Vector3 direction = transform.TransformDirection(Vector3.forward);
            // float forwardInput = Input.GetAxis("Vertical");
            // Vector3 inputPosition = direction * forwardInput;

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;
            // Vector3 moveDir = Vector3.up;

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + characterCamera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;


            // change animation states
            if (vertical == 0 && horizontal == 0)
                if (IsInCrouch)
                {
                    UpdatePlayerStateServerRpc(PlayerState.IdleCrouch);
                }
                else
                {
                    UpdatePlayerStateServerRpc(PlayerState.Idle);
                }
            // else if (!ActiveRunningActionKey() && vertical > 0 && vertical <= 1)
            //     UpdatePlayerStateServerRpc(PlayerState.Walk);
            // else if (ActiveRunningActionKey() && vertical > 0 && vertical <= 1)
            // {
            //     moveDir = moveDir * runSpeedOffset;
            //     UpdatePlayerStateServerRpc(PlayerState.Run);
            // }
            else if (!ActiveRunningActionKey())
                if (IsInCrouch)
                {
                    walkSpeed = crouchSpeed;
                    UpdatePlayerStateServerRpc(PlayerState.WalkCrouch);
                }
                else
                {
                    walkSpeed = 5.0f;
                    UpdatePlayerStateServerRpc(PlayerState.Walk);
                }
            else if (ActiveRunningActionKey())
            {
                walkSpeed = runSpeed;
                if (IsInCrouch)
                {
                    walkSpeed = crouchSpeed;
                    UpdatePlayerStateServerRpc(PlayerState.RunCrouch);
                }
                else
                {
                    walkSpeed = 5.0f;
                    UpdatePlayerStateServerRpc(PlayerState.Run);
                }
            }
            // else if (!ActiveRunningActionKey() && vertical < 0)
            //     // UpdatePlayerStateServerRpc(PlayerState.ReverseWalk);
            //     UpdatePlayerStateServerRpc(PlayerState.Walk);
            // else if (ActiveRunningActionKey() && vertical < 0)
            // {
            //     moveDir = moveDir * runSpeedOffset;
            //     UpdatePlayerStateServerRpc(PlayerState.Run);
            // }

            if (direction.magnitude >= 0.1f)
            {
                // characterController.SimpleMove(moveDir * speed);
                // var _velocity = transform.position;
                // moveDir.y += Gravity * Time.deltaTime;
                // characterController.Move(_velocity * Time.deltaTime);
                characterController.Move(Physics.gravity * Time.deltaTime);

                characterController.Move(moveDir.normalized * walkSpeed * Time.deltaTime);
                if (_rotateOnMove)
                {
                    transform.rotation = Quaternion.Euler(0f, angle, 0f);
                }
            }
            // transform.Rotate(inputRotation * rotationSpeed, Space.World);
            // client is responsible for moving itself
            // characterController.SimpleMove(inputPosition * walkSpeed);
            // transform.Rotate(inputRotation * rotationSpeed, Space.World);
        }
    }

    private static bool ActiveRunningActionKey()
    {
        // return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        return Input.GetButton("Run");
    }

    [ServerRpc]
    public void UpdatePlayerStateServerRpc(PlayerState state)
    {
        networkPlayerState.Value = state;
    }

    // public void SetSensitivity(float newSensitivity)
    // {
    //     Sensitivity = newSensitivity;
    // }

    public void SetRotateOnMove(bool newRotateOnMove)
    {
        _rotateOnMove = newRotateOnMove;
    }
}
