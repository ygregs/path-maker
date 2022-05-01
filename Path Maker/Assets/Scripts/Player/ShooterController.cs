using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class ShooterController : NetworkBehaviour
{
    [SerializeField] private GameObject BulletPrefab;
    [SerializeField] private Transform BulletSpawnPoint;
    [SerializeField] private float ShootDelay = 0.5f;
    [SerializeField] private float LastShootTime;

    [SerializeField] private Queue<BulletBehaviour> LastBulletQueue = new Queue<BulletBehaviour>();
    [SerializeField] private BulletBehaviour lastBullet;

    void Update()
    {
        if (IsLocalPlayer)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                if (LastShootTime + ShootDelay < Time.time)
                {
                    if (IsServer)
                    {
                        var bullet = Instantiate(BulletPrefab, BulletSpawnPoint.position, gameObject.transform.rotation);
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

            if (Input.GetButtonDown("Jump"))
            {
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
        var bullet = Instantiate(BulletPrefab, BulletSpawnPoint.position, gameObject.transform.rotation);
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