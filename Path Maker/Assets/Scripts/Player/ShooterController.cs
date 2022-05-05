using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.Animations.Rigging;

public class ShooterController : NetworkBehaviour
{
    private PlayerInputs _input;
    private PlayerInput _playerInput;

    [SerializeField] private Rig aimRig;
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField] private float normalSensibility;
    [SerializeField] private float aimSensibility;
    [SerializeField] private Transform debugTransform;
    [SerializeField] private GameObject BulletPrefab;
    [SerializeField] private Transform BulletSpawnPoint;
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
    [SerializeField] private float ShootDelay = 0.5f;
    [SerializeField] private float LastShootTime;
    [SerializeField] private Queue<BulletBehaviour> LastBulletQueue = new Queue<BulletBehaviour>();
    [SerializeField] private BulletBehaviour lastBullet;
    private TPSController _playerController;
    private Animator _animator;
    private float aimRigWeight;

    void Start()
    {
        _input = GetComponent<PlayerInputs>();
        _playerInput = GetComponent<PlayerInput>();
        _playerController = GetComponent<TPSController>();
        _animator = GetComponent<Animator>();
    }
    void Update()
    {
        if (IsLocalPlayer)
        {
            aimRig.weight = Mathf.Lerp(aimRig.weight, aimRigWeight, Time.deltaTime * 20f);
            Vector3 mouseWorldPosition = Vector3.zero;

            Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            Transform hitTransform = null;
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
            {
                debugTransform.position = raycastHit.point;
                mouseWorldPosition = raycastHit.point;
                hitTransform = raycastHit.transform;
            }

            if (_input.aim)
            {
                aimVirtualCamera.gameObject.SetActive(true);
                _playerController.SetSensibility(aimSensibility);
                _playerController.SetRotateOnMove(false);
                aimRigWeight = 1f;
                _animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));
            }
            else
            {
                _input.shoot = false;
                aimVirtualCamera.gameObject.SetActive(false);
                _playerController.SetSensibility(normalSensibility);
                _playerController.SetRotateOnMove(true);
                aimRigWeight = 0f;
                _animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
            }

            if (_input.shoot && _input.aim)
            {
                _input.shoot = false;
                if (LastShootTime + ShootDelay < Time.time)
                {
                    if (IsServer)
                    {
                        Vector3 aimDir = (mouseWorldPosition - BulletSpawnPoint.position).normalized;
                        // var bullet = Instantiate(BulletPrefab, BulletSpawnPoint.position, gameObject.transform.rotation);
                        var bullet = Instantiate(BulletPrefab, BulletSpawnPoint.position, Quaternion.LookRotation(aimDir, Vector3.up));
                        bullet.GetComponent<NetworkObject>().Spawn();
                        LastBulletQueue.Enqueue(bullet.GetComponent<BulletBehaviour>());
                    }
                    else
                    {
                        Shoot_ServerRpc(NetworkManager.Singleton.LocalClientId);
                    }
                    LastShootTime = Time.time;
                }
            }

            if (_input.accelerate)
            {
                _input.accelerate = false;
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
    }

    [ServerRpc(RequireOwnership = false)]
    void Shoot_ServerRpc(ulong clientId)
    {
        Vector3 mouseWorldPosition = Vector3.zero;

        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        Transform hitTransform = null;
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            debugTransform.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
            hitTransform = raycastHit.transform;
        }
        Vector3 aimDir = (mouseWorldPosition - BulletSpawnPoint.position).normalized;
        // var bullet = Instantiate(BulletPrefab, BulletSpawnPoint.position, gameObject.transform.rotation);
        var bullet = Instantiate(BulletPrefab, BulletSpawnPoint.position, Quaternion.LookRotation(aimDir, Vector3.up));
        bullet.GetComponent<NetworkObject>().SpawnWithOwnership(clientId, true);
        AddLastBullet_ClientRpc(clientId, bullet.GetComponent<NetworkBehaviour>().NetworkObjectId);
    }

    [ClientRpc]
    void AddLastBullet_ClientRpc(ulong clientId, ulong bulletNetObjId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            LastBulletQueue.Enqueue(NetworkManager.Singleton.SpawnManager.SpawnedObjects[bulletNetObjId].gameObject.GetComponent<BulletBehaviour>());
        }
    }
}