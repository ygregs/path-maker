using UnityEngine;
using Unity.Netcode;

namespace PathMaker.Test
{
    public class DamageZone : NetworkBehaviour
    {
        [SerializeField] private float Damage = 30f;
        [SerializeField] private float ShootDelay = 0.5f;
        [SerializeField] private float LastShootTime;

        void OnTriggerEnter(Collider other)
        {
            ulong clientId = other.GetComponentInParent<NetworkObject>().OwnerClientId;
            FindObjectOfType<PlayerHealthTest>().TakeDamage_ClientRpc(clientId, 0, Damage);
        }
    }
}