using UnityEngine;
using Unity.Netcode;

namespace PathMaker.shooting
{
    public class BulletBrain : NetworkBehaviour
    {
        [SerializeField]
        private float m_initialSpeed = 0.01f;
        [SerializeField]
        private float m_acceleratedSpeed = 40f;
        [SerializeField]
        private Rigidbody m_bulletRigidbody;

        [SerializeField] private LayerMask colliderLM = new LayerMask();
        [SerializeField] private GameObject vfxHit;
        [SerializeField] private GameObject vfxAccelerate;
        public GameObject m_shooter;

        public ulong m_shooterId;

        public TeamState m_shooterTeam;

        public void Start()
        {

            // Debug.Log("Bullet brain: a bullet just spawned!");
            // transform.position = transform.position + transform.TransformDirection(Vector3.forward) * m_initialSpeed;
            m_bulletRigidbody.velocity = transform.forward * m_initialSpeed;

        }

        public void Accelerate()
        {
            m_bulletRigidbody.velocity = transform.forward * m_acceleratedSpeed;
            // Bit shift the index of the layer (8) to get a bit mask
            // int layerMask = 1 << 8;

            // This would cast rays only against colliders in layer 8.
            // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
            // layerMask = ~layerMask;

            // Instantiate(vfxAccelerate, transform.position, Quaternion.identity);

            // RaycastHit hit;
            // // Does the ray intersect any objects excluding the player layer
            // if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, colliderLM))
            // {
            //     Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            //     Debug.Log("Did Hit");
            //     Instantiate(vfxHit, transform.TransformDirection(Vector3.forward) * hit.distance, Quaternion.identity);
            //     // Destroy(this);
            // }
            // else
            // {
            //     // Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            //     Debug.Log("Did not Hit");
            //     Instantiate(vfxHit, transform.TransformDirection(Vector3.forward) * hit.distance, Quaternion.identity);
            //     // Destroy(this);
            // }

        }
        private void OnTriggerEnter(Collider other)
        {
            // if (other.gameObject.tag != "Bullet")
            // {
            //     if (other.gameObject.tag == "Player")
            //     {
            //         if (other.gameObject.GetComponent<ThirdPersonShooterController>().m_localId == m_shooterId)
            //         {
            //             // Debug.Log("player hit himself: noting to do with that");
            //         }
            //         else
            //         {
            //             // Debug.Log("Player hit an other player");
            //             Instantiate(vfxHit, transform.position, Quaternion.identity);
            //             Destroy(gameObject);
            //         }
            //     }
            //     else
            //     {
            //         // Debug.Log("Bullet hit something");
            //         Instantiate(vfxHit, transform.position, Quaternion.identity);
            //         // gameObject.GetComponent<NetworkObject>().Despawn();
            //         Destroy(gameObject);
            //     }
            // }
            DebugServerRpc();
        }

        [ServerRpc]
        void DebugServerRpc()
        {
            Debug.Log("bullet collide");
        }
    }
}