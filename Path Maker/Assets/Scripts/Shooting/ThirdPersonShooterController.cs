using UnityEngine;
using Unity.Netcode;
using Cinemachine;
using System.Collections.Generic;

namespace PathMaker.shooting
{
    public class ThirdPersonShooterController : NetworkBehaviour
    {

        // [SerializeField]
        // private bool m_hasAlreadyShot;
        // [SerializeField]
        // private bool m_canAccelerate;
        // [SerializeField]
        // private bool m_isReloading = false;

        // [SerializeField]
        // private GameObject m_bulletPrefab;

        [SerializeField]
        private Transform m_bulletSpawnPoint;

        // private GameObject m_currentBullet;
        // [SerializeField]
        // private List<GameObject> m_bulletsList;

        [SerializeField] private CinemachineFreeLook aimFreeLookCamera;
        [SerializeField] private CinemachineFreeLook tpsFreeLookCamera;
        // [SerializeField] private float normalSensitivity;
        // [SerializeField] private float aimSensitivity;
        [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
        // [SerializeField] private Transform debugTransform;
        [SerializeField] private PlayerControlAuthorative thirdPersonController;
        [SerializeField]
        // private Animator animator;
        public ulong m_localId;
        [SerializeField] private float delta = 5f;
        [SerializeField] private GameObject cursor;

        // private TeamState m_teamState;
        // [SerializeField]
        // private float timeLeft = 5.0f;
        // private bool m_canShoot = true;

        [SerializeField] private TrailRenderer bulletTrail;

        public override void OnNetworkSpawn()
        {
            m_localId = NetworkManager.Singleton.LocalClientId;
            // var teamStateObj = gameObject.GetComponent<TeamLogic>();
            // m_teamState = teamStateObj.playerNetworkTeam.Value;
            // GetTeam_ServerRpc();
        }

        // [ServerRpc]
        // void GetTeam_ServerRpc()
        // {
        //     var teamStateObj = gameObject.GetComponent<TeamLogic>();
        //     m_teamState = teamStateObj.playerNetworkTeam.Value;
        // }
        void Update()
        {
            if (IsClient && IsOwner)
            {
                tpsFreeLookCamera.Priority = 12;
                // timeLeft -= Time.deltaTime;
                // if (timeLeft <= 0)
                // {
                //     m_isReloading = false;
                //     timeLeft = 0.0f;
                // }

                // Vector3 mouseWorldPosition = Vector3.zero;

                // Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
                // Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
                // Transform hitTransform = null;
                // if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
                // {
                //     debugTransform.position = raycastHit.point;
                //     mouseWorldPosition = raycastHit.point;
                //     hitTransform = raycastHit.transform;
                // }

                if (IsAiming())
                {
                    aimFreeLookCamera.Priority = 13;
                    aimFreeLookCamera.gameObject.SetActive(true);
                    cursor.SetActive(true);
                    // thirdPersonController.SetSensitivity(aimSensitivity);
                    thirdPersonController.SetRotateOnMove(false);
                    // animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime * 13f));

                    // Vector3 worldAimTarget = mouseWorldPosition;
                    // worldAimTarget.y = transform.position.y;
                    // Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

                    // transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
                }
                else
                {
                    cursor.SetActive(false);
                    aimFreeLookCamera.gameObject.SetActive(false);
                    // thirdPersonController.SetSensitivity(normalSensitivity);
                    thirdPersonController.SetRotateOnMove(true);
                    // animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 13f));
                }
                // if (Input.GetButtonDown("Fire1") && !m_hasAlreadyShot && !m_isReloading)
                if (Input.GetButtonDown("Fire1"))
                {
                    if (IsAiming())
                    {
                        delta = 10f;
                        // timeLeft = 5.0f;
                        // m_hasAlreadyShot = true;
                        // m_canAccelerate = true;
                        // m_isReloading = true;

                        // screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
                        // ray = Camera.main.ScreenPointToRay(screenCenterPoint);
                        // Ray ray = Physics.Raycast(m_bulletSpawnPoint.position, m_bulletSpawnPoint.forward, 500f);
                        // Vector3 aimDir = (mouseWorldPosition - m_bulletSpawnPoint.position).normalized;
                        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
                        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
                        ray.origin = ray.GetPoint(delta);
                        if (IsLocalPlayer)
                        {
                            ShootServerRpc(ray);
                        }
                    }
                }
                // SpawnBullet_ServerRpc(aimDir, m_localId);
                // m_currentBullet = GameObject.Instantiate(m_bulletPrefab, m_bulletSpawnPoint.position, Quaternion.LookRotation(aimDir, Vector3.up));
                // m_bulletsList.Add(m_currentBullet);
                // BulletBrain bulletBrain = m_currentBullet.GetComponent<BulletBrain>();
                // bulletBrain.m_shooterId = m_localId;
                // bulletBrain.m_shooterTeam = m_teamState;

                // m_currentBullet.GetComponent<NetworkObject>().Spawn();
                // Debug.Log("spawning bullet");
            }

            // if (Input.GetButtonDown("Accelerate") && m_canAccelerate)
            // {
            //     m_hasAlreadyShot = false;
            //     m_canAccelerate = false;
            //     AccelerateBuller_ServerRpc();
            //     if (m_bulletsList.Count >= 1)
            //     {
            //         int l = m_bulletsList.Count - 1;
            //         m_bulletsList[l].GetComponent<BulletBrain>().Accelerate();
            //         m_bulletsList.RemoveAt(l);

            //     }
            //     if (m_currentBullet == null)
            //     {
            //         m_hasAlreadyShot = false;
            //         m_canAccelerate = false;
            //     }
            // }




        }

        [ServerRpc]
        void ShootServerRpc(Ray ray)
        {
            // Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            // Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            // ray.origin = ray.GetPoint(3.0f);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
            {
                //Raycast hit somthing
                var enemyHealth = raycastHit.transform.GetComponent<ngo.PlayerHealth>();
                // Debug.Log("you hit something");
                // Debug.Log(raycastHit.transform);
                // Debug.Log(enemyHealth);
                if (enemyHealth != null)
                {
                    // if (enemyHealth.m_localId != NetworkManager.Singleton.LocalClientId)
                    // {
                    //     Debug.Log("hit player");
                    enemyHealth.TakeDamage(50);
                    // }
                    // else
                    // {
                    //     Debug.Log("you hit yourself");
                    // }
                }
            }
            ShootClientRpc(ray);
        }

        [ClientRpc]
        void ShootClientRpc(Ray ray)
        {
            // var bullet = Instantiate(bulletTrail, m_bulletSpawnPoint.position, Quaternion.LookRotation(aimDir, Vector3.up));
            var bullet = Instantiate(bulletTrail, m_bulletSpawnPoint.position, Quaternion.identity);
            bullet.AddPosition(m_bulletSpawnPoint.position);
            // Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            // Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
            // ray.origin = ray.GetPoint(3.0f);

            // ray.origin = ray.GetPoint(delta);
            // Transform hitTransform = null;
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
            // if (Physics.Raycast(m_bulletSpawnPoint.position, m_bulletSpawnPoint.forward, out RaycastHit raycastHit, 500f))
            {
                // debugTransform.position = raycastHit.point;
                // mouseWorldPosition = raycastHit.point;
                // hitTransform = raycastHit.transform;
                //     }
                // if (Physics.Raycast(m_bulletSpawnPoint.position, m_bulletSpawnPoint.forward, out RaycastHit hit, 200f))
                // {
                bullet.transform.position = raycastHit.point;
                Debug.Log(raycastHit.transform);
            }
            else
            {
                bullet.transform.position = m_bulletSpawnPoint.position + (m_bulletSpawnPoint.position * 200f);
            }
        }

        // [ServerRpc]
        // private void SpawnBullet_ServerRpc(Vector3 aimDir, ulong ownerId)
        // {
        //     m_currentBullet = GameObject.Instantiate(m_bulletPrefab, m_bulletSpawnPoint.position, Quaternion.LookRotation(aimDir, Vector3.up));
        //     m_bulletsList.Add(m_currentBullet);
        //     BulletBrain bulletBrain = m_currentBullet.GetComponent<BulletBrain>();
        //     bulletBrain.m_shooterId = ownerId;
        //     bulletBrain.m_shooterTeam = m_teamState;

        //     m_currentBullet.GetComponent<NetworkObject>().Spawn();
        //     m_currentBullet.GetComponent<NetworkObject>().ChangeOwnership(ownerId);
        //     Debug.Log("Spawning bullet");
        // }

        // [ServerRpc]
        // private void AccelerateBuller_ServerRpc()
        // {
        //     if (m_bulletsList.Count >= 1)
        //     {
        //         int l = m_bulletsList.Count - 1;
        //         m_bulletsList[l].GetComponent<BulletBrain>().Accelerate();
        //         m_bulletsList.RemoveAt(l);
        //     }
        //     Debug.Log("Accelerating bullet");
        // }
        private static bool IsAiming()
        {
            return Input.GetButton("Fire2");
        }

        // public void HitPlayer(GameObject hit)
        // {
        //     var player = hit.GetComponent<player.PlayerHealth>();
        //     Debug.Log("hit player");
        //     if (player != null)
        //     {
        //         player.TakeDamage(50f);
        //     }
        // }
    }
}