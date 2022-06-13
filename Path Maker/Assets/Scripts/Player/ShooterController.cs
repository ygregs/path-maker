using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Audio;

public class ShooterController : NetworkBehaviour
{
    private PlayerInputs _input;
    private PlayerInput _playerInput;

    [SerializeField] private Rig aimRig;
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField] private float normalSensibility;
    [SerializeField] private float aimSensibility;
    [SerializeField] private Transform debugTransform;
    [SerializeField] private GameObject _blueBulletPrefab;
    [SerializeField] private GameObject _greenBulletPrefab;
    [SerializeField] private GameObject _redBulletPrefab;
    [SerializeField] private Transform BulletSpawnPoint;
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
    [SerializeField] private float ShootDelay = 0.5f;
    [SerializeField] private float LastShootTime;
    [SerializeField] private Queue<BulletBehaviour> _blueLastBulletQueue = new Queue<BulletBehaviour>();
    [SerializeField] private Queue<BulletBehaviour> _greenLastBulletQueue = new Queue<BulletBehaviour>();
    [SerializeField] private Queue<BulletBehaviour> _redLastBulletQueue = new Queue<BulletBehaviour>();
    [SerializeField] private BulletBehaviour lastBullet;
    private TPSController _playerController;
    private Animator _animator;
    private float aimRigWeight;
    private PathMaker.shooting.ShootingModeController _shootingModeController;

    //sound 
    [SerializeField] private AudioSource fireSound;

    // Synchronisation of animatior layers weight
    public enum LayerState
    {
        None,
        Base,
        Aim,
        Dying,
    }

    [SerializeField] private NetworkVariable<LayerState> LayerStateVariable = new NetworkVariable<LayerState>();
    private LayerState oldLayerState = LayerState.None;

    public enum RigState
    {
        None,
        Base,
        Aim,
    }

    [SerializeField] private NetworkVariable<RigState> RigStateVariable = new NetworkVariable<RigState>();

    private RigState oldRigState = RigState.None;

    public bool IsDead = false;

    [ServerRpc]
    public void UpdateLayerStateServerRpc(LayerState state)
    {
        LayerStateVariable.Value = state;
    }

    [ServerRpc]
    public void UpdateRigStateServerRpc(RigState state)
    {
        RigStateVariable.Value = state;
    }

    private void ClientVisuals()
    {
        if (oldLayerState != LayerStateVariable.Value)
        {
            oldLayerState = LayerStateVariable.Value;
            switch (LayerStateVariable.Value)
            {
                case LayerState.Aim:
                    _animator.SetLayerWeight(1, 1f);
                    // _animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));
                    break;
                case LayerState.Base:
                    _animator.SetLayerWeight(1, 0f);
                    _animator.SetLayerWeight(2, 0f);
                    // _animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
                    break;
                case LayerState.Dying:
                    _animator.SetLayerWeight(2, 1f);
                    break;
                default:
                    break;
            }
        }

        if (oldRigState != RigStateVariable.Value)
        {
            switch (RigStateVariable.Value)
            {
                case RigState.Base:
                    aimRig.weight = 0f;
                    break;
                case RigState.Aim:
                    aimRig.weight = 1f;
                    break;
                default:
                    break;
            }
        }
    }

    void Start()
    {
        _input = GetComponent<PlayerInputs>();
        _playerInput = GetComponent<PlayerInput>();
        _playerController = GetComponent<TPSController>();
        _animator = GetComponent<Animator>();
        _shootingModeController = GetComponent<PathMaker.shooting.ShootingModeController>();
    }

    void Update()
    {
        if (IsClient && IsOwner && !IsDead)
        {
            // aimRig.weight = Mathf.Lerp(aimRig.weight, aimRigWeight, Time.deltaTime * 20f);
            Vector3 mouseWorldPosition = Vector3.zero;

            if (_input.aim)
            {
                // Place Aim Target when aiming : pas forcement le mieux (temps de retour du rig de la visee ==> a voir si on change / laisse)
                if (IsServer)
                {
                    Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
                    Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
                    Transform hitTransform = null;
                    if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
                    {
                        debugTransform.position = raycastHit.point;
                        mouseWorldPosition = raycastHit.point;
                        hitTransform = raycastHit.transform;
                    }
                }
                else
                {
                    Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
                    Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
                    PlaceAimTarget_ServerRpc(ray);
                }

                aimVirtualCamera.gameObject.SetActive(true);
                _playerController.SetSensibility(aimSensibility);
                _playerController.SetRotateOnMove(false);
                // aimRigWeight = 1f;
                // _animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));
                UpdateRigStateServerRpc(RigState.Aim);
                UpdateLayerStateServerRpc(LayerState.Aim);
            }
            else
            {
                _input.shoot = false;
                aimVirtualCamera.gameObject.SetActive(false);
                _playerController.SetSensibility(normalSensibility);
                _playerController.SetRotateOnMove(true);
                // aimRigWeight = 0f;
                // _animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
                UpdateRigStateServerRpc(RigState.Base);
                UpdateLayerStateServerRpc(LayerState.Base);
                _playerController.RightClamp = float.MinValue;
                _playerController.LeftClamp = float.MaxValue;
            }

            if (_input.shoot && _input.aim && CanShoot())
            {
                fireSound.Play();
                _input.shoot = false;
                if (LastShootTime + ShootDelay < Time.time)
                {
                    if (IsServer)
                    {
                        Vector3 aimDir = (mouseWorldPosition - BulletSpawnPoint.position).normalized;
                        GameObject bullet = null;
                        switch (_shootingModeController._shootingMode)
                        {
                            case PathMaker.shooting.ShootingMode.Blue:
                                _shootingModeController.SetText(_blueLastBulletQueue.Count + 1);
                                bullet = Instantiate(_blueBulletPrefab, BulletSpawnPoint.position, Quaternion.LookRotation(aimDir, Vector3.up));
                                bullet.GetComponent<NetworkObject>().Spawn();
                                _blueLastBulletQueue.Enqueue(bullet.GetComponent<BulletBehaviour>());
                                break;
                            case PathMaker.shooting.ShootingMode.Green:
                                _shootingModeController.SetText(_greenLastBulletQueue.Count + 1);
                                bullet = Instantiate(_greenBulletPrefab, BulletSpawnPoint.position, Quaternion.LookRotation(aimDir, Vector3.up));
                                bullet.GetComponent<NetworkObject>().Spawn();
                                _greenLastBulletQueue.Enqueue(bullet.GetComponent<BulletBehaviour>());
                                break;
                            case PathMaker.shooting.ShootingMode.Red:
                                _shootingModeController.SetText(_redLastBulletQueue.Count + 1);
                                bullet = Instantiate(_redBulletPrefab, BulletSpawnPoint.position, Quaternion.LookRotation(aimDir, Vector3.up));
                                bullet.GetComponent<NetworkObject>().Spawn();
                                _redLastBulletQueue.Enqueue(bullet.GetComponent<BulletBehaviour>());
                                break;
                            default:
                                break;
                        }
                        // var bullet = Instantiate(BulletPrefab, BulletSpawnPoint.position, Quaternion.LookRotation(aimDir, Vector3.up));
                        // LastBulletQueue.Enqueue(bullet.GetComponent<BulletBehaviour>());
                    }
                    else
                    {
                        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
                        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
                        Shoot_ServerRpc(NetworkManager.Singleton.LocalClientId, _shootingModeController._shootingMode, ray);
                    }
                    LastShootTime = Time.time;
                }
            }

            if (_input.accelerate && CanAccelerate())
            {
                print($"player {NetworkManager.Singleton.LocalClientId} accelerate a bullet");
                _input.accelerate = false;

                Queue<BulletBehaviour> LastBulletQueue = _blueLastBulletQueue;
                switch (_shootingModeController._shootingMode)
                {
                    case PathMaker.shooting.ShootingMode.Blue:
                        LastBulletQueue = _blueLastBulletQueue;
                        _shootingModeController.SetText(_blueLastBulletQueue.Count - 1);
                        break;
                    case PathMaker.shooting.ShootingMode.Green:
                        LastBulletQueue = _greenLastBulletQueue;
                        _shootingModeController.SetText(_greenLastBulletQueue.Count - 1);
                        break;
                    case PathMaker.shooting.ShootingMode.Red:
                        LastBulletQueue = _redLastBulletQueue;
                        _shootingModeController.SetText(_redLastBulletQueue.Count - 1);
                        break;
                    default:
                        break;
                }
                print(LastBulletQueue.Count);
                if (LastBulletQueue.Count > 0)
                {
                    BulletBehaviour lastBullet = LastBulletQueue.Dequeue();

                    while (LastBulletQueue.Count > 0 && lastBullet == null)
                    {
                        lastBullet = LastBulletQueue.Dequeue();
                    }
                    if (lastBullet != null)
                    {
                        if (IsServer)
                        {
                            lastBullet.ShootRay();
                        }
                        else
                        {
                            lastBullet.ShootRay_ServerRpc(NetworkManager.Singleton.LocalClientId);
                        }
                    }
                }
            }
        }

        if (IsDead)
        {
            UpdateLayerStateServerRpc(LayerState.Dying);
        }

        ClientVisuals();
    }

    public bool CanShoot()
    {
        switch (_shootingModeController._shootingMode)
        {
            case PathMaker.shooting.ShootingMode.Blue:
                return _blueLastBulletQueue.Count < 2;
            case PathMaker.shooting.ShootingMode.Green:
                return _greenLastBulletQueue.Count < 2;
            case PathMaker.shooting.ShootingMode.Red:
                return _redLastBulletQueue.Count < 2;
            default:
                return false;
        }
    }

    public bool CanAccelerate()
    {
        switch (_shootingModeController._shootingMode)
        {
            case PathMaker.shooting.ShootingMode.Blue:
                return _blueLastBulletQueue.Count > 0 && _blueLastBulletQueue.Count < 3;
            case PathMaker.shooting.ShootingMode.Green:
                return _greenLastBulletQueue.Count > 0 && _greenLastBulletQueue.Count < 3;
            case PathMaker.shooting.ShootingMode.Red:
                return _redLastBulletQueue.Count > 0 && _redLastBulletQueue.Count < 3;
            default:
                return false;
        }
    }

    public void ResetThirdCamera()
    {
        if (IsClient && IsOwner)
        {
            _input.shoot = false;
            aimVirtualCamera.gameObject.SetActive(false);
            _playerController.SetSensibility(normalSensibility);
            _playerController.SetRotateOnMove(true);
            aimRigWeight = 0f;
            // _animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
            UpdateLayerStateServerRpc(LayerState.Aim);
            _playerController.RightClamp = float.MinValue;
            _playerController.LeftClamp = float.MaxValue;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void PlaceAimTarget_ServerRpc(Ray ray)
    {
        PlaceAimTarget_ClientRpc(ray);
    }

    [ClientRpc]
    void PlaceAimTarget_ClientRpc(Ray ray)
    {
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            debugTransform.position = raycastHit.point;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void Shoot_ServerRpc(ulong clientId, PathMaker.shooting.ShootingMode shootingMode, Ray ray)
    {
        Vector3 mouseWorldPosition = Vector3.zero;

        Transform hitTransform = null;
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            debugTransform.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
            hitTransform = raycastHit.transform;
        }
        Vector3 aimDir = (mouseWorldPosition - BulletSpawnPoint.position).normalized;
        GameObject bullet = null;
        switch (shootingMode)
        {
            case PathMaker.shooting.ShootingMode.Blue:
                _shootingModeController.SetText(_blueLastBulletQueue.Count + 1);
                bullet = Instantiate(_blueBulletPrefab, BulletSpawnPoint.position, Quaternion.LookRotation(aimDir, Vector3.up));
                break;
            case PathMaker.shooting.ShootingMode.Green:
                _shootingModeController.SetText(_greenLastBulletQueue.Count + 1);
                bullet = Instantiate(_greenBulletPrefab, BulletSpawnPoint.position, Quaternion.LookRotation(aimDir, Vector3.up));
                break;
            case PathMaker.shooting.ShootingMode.Red:
                _shootingModeController.SetText(_redLastBulletQueue.Count + 1);
                bullet = Instantiate(_redBulletPrefab, BulletSpawnPoint.position, Quaternion.LookRotation(aimDir, Vector3.up));
                break;
            default:
                break;
        }
        // var bullet = Instantiate(BulletPrefab, BulletSpawnPoint.position, Quaternion.LookRotation(aimDir, Vector3.up));
        bullet.GetComponent<NetworkObject>().SpawnWithOwnership(clientId, true);
        AddLastBullet_ClientRpc(clientId, _shootingModeController._shootingMode, bullet.GetComponent<NetworkBehaviour>().NetworkObjectId);
    }

    [ClientRpc]
    void AddLastBullet_ClientRpc(ulong clientId, PathMaker.shooting.ShootingMode shootingMode, ulong bulletNetObjId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            switch (shootingMode)
            {
                case PathMaker.shooting.ShootingMode.Blue:
                    _blueLastBulletQueue.Enqueue(NetworkManager.Singleton.SpawnManager.SpawnedObjects[bulletNetObjId].gameObject.GetComponent<BulletBehaviour>());
                    break;
                case PathMaker.shooting.ShootingMode.Green:
                    _greenLastBulletQueue.Enqueue(NetworkManager.Singleton.SpawnManager.SpawnedObjects[bulletNetObjId].gameObject.GetComponent<BulletBehaviour>());
                    break;
                case PathMaker.shooting.ShootingMode.Red:
                    _redLastBulletQueue.Enqueue(NetworkManager.Singleton.SpawnManager.SpawnedObjects[bulletNetObjId].gameObject.GetComponent<BulletBehaviour>());
                    break;
                default:
                    break;
            }
            // LastBulletQueue.Enqueue(NetworkManager.Singleton.SpawnManager.SpawnedObjects[bulletNetObjId].gameObject.GetComponent<BulletBehaviour>());
        }
    }
}