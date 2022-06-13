using UnityEngine;
using Unity.Netcode;

namespace PathMaker.shooting
{
    public class PlayerShootingController : MonoBehaviour
    {

        [SerializeField]
        private bool m_hasAlreadyShot;
        [SerializeField]
        private bool m_canAccelerate;
        [SerializeField]
        private bool m_isReloading;

        [SerializeField]
        private GameObject m_bulletPrefab;

        [SerializeField]
        private Transform m_bulletSpawnPoint;

        [SerializeField]
        private GameObject m_currentBullet;

        void Update()
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
                m_canAccelerate = true;
                m_hasAlreadyShot = true;
            }
        }
        public void Shoot()
        {
            m_currentBullet = GameObject.Instantiate(m_bulletPrefab, m_bulletSpawnPoint.position, Quaternion.identity);
            m_currentBullet.GetComponent<NetworkObject>().Spawn();
        }
    }
}