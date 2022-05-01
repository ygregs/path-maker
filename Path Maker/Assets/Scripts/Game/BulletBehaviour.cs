using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class BulletBehaviour : NetworkBehaviour
{
    [SerializeField] private Rigidbody rg;
    [SerializeField] private float speed;
    [SerializeField] private float damage;
    [SerializeField] private TrailRenderer BulletTrail;
    [SerializeField] private LayerMask Mask;
    void Start()
    {
        rg.AddForce(gameObject.transform.forward * speed);
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            if (OwnerClientId != collider.gameObject.GetComponentInParent<NetworkObject>().OwnerClientId)
            {
                ulong targetClientId = collider.gameObject.GetComponentInParent<NetworkObject>().OwnerClientId;
                print($"player {OwnerClientId} hit player {targetClientId} with his bullet");
                FindObjectOfType<PlayerHealthTest>().TakeDamage_ClientRpc(targetClientId, OwnerClientId, damage);
                DestroyBulletById_ServerRpc(NetworkObjectId);
            }
        }
        else
        {
            if (collider.gameObject.tag != "Bullet")
                DestroyBulletById_ServerRpc(NetworkObjectId);
        }
    }

    public void ShootRay()
    {
        // Use an object pool instead for these! To keep this tutorial focused, we'll skip implementing one.
        // For more details you can see: https://youtu.be/fsDE_mO4RZM or if using Unity 2021+: https://youtu.be/zyzqA_CPz2E

        // Animator.SetBool("IsShooting", true);
        print($"player {OwnerClientId} accelerate his last bullet");
        Vector3 direction = transform.forward;

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, float.MaxValue, Mask))
        {
            if (hit.collider.gameObject.tag == "Player")
            {
                ulong hitPlayerId = hit.collider.gameObject.GetComponent<NetworkObject>().OwnerClientId;
                print($"player {OwnerClientId} hit player {hitPlayerId}");
                FindObjectOfType<PlayerHealthTest>().TakeDamage_ClientRpc(hitPlayerId, OwnerClientId, damage);
            }
            TrailRenderer trail = Instantiate(BulletTrail, transform.position, Quaternion.identity);
            trail.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId, true);

            StartCoroutine(SpawnTrail(trail, hit));
        }
        SyncWithClientsRay_ClientRpc(transform.position, direction);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShootRay_ServerRpc(ulong clientId)
    {
        print($"player {OwnerClientId} accelerate his last bullet");
        Vector3 direction = transform.forward;

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, float.MaxValue, Mask))
        {
            if (hit.collider.gameObject.tag == "Player")
            {
                ulong hitPlayerId = hit.collider.gameObject.GetComponent<NetworkObject>().OwnerClientId;
                print($"player {NetworkManager.Singleton.LocalClientId} hit player {hitPlayerId}");
                FindObjectOfType<PlayerHealthTest>().TakeDamage_ClientRpc(hitPlayerId, OwnerClientId, damage);
            }
            TrailRenderer trail = Instantiate(BulletTrail, transform.position, Quaternion.identity);
            trail.GetComponent<NetworkObject>().SpawnWithOwnership(clientId, true);

            StartCoroutine(SpawnTrail(trail, hit));
        }
        SyncWithClientsRay_ClientRpc(transform.position, direction);
    }

    [ClientRpc]
    void SyncWithClientsRay_ClientRpc(Vector3 spawnPosition, Vector3 direction)
    {
        if (Physics.Raycast(spawnPosition, direction, out RaycastHit hit, float.MaxValue, Mask))
        {
            TrailRenderer trail = Instantiate(BulletTrail, spawnPosition, Quaternion.identity);
            StartCoroutine(SpawnTrail(trail, hit));
        }
    }

    private IEnumerator SpawnTrail(TrailRenderer Trail, RaycastHit Hit)
    {
        float time = 0;
        Vector3 startPosition = Trail.transform.position;

        while (time < 1)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, Hit.point, time);
            time += Time.deltaTime / Trail.time;

            yield return null;
        }
        Trail.transform.position = Hit.point;
        Destroy(Trail.gameObject, Trail.time);
        DestroyBulletById_ServerRpc(NetworkObjectId);
    }

    [ServerRpc(RequireOwnership = false)]
    void DestroyBulletById_ServerRpc(ulong bulletNetId)
    {
        Destroy(NetworkManager.Singleton.SpawnManager.SpawnedObjects[bulletNetId].gameObject);
    }
}