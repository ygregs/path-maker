using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class getHitBullet : MonoBehaviour
{
    private void OnTriggerEnter(Collider Col)
    {
        if (Col.gameObject.tag == "Bullet")
        {
            Debug.Log("Player has been hit !");
            //Destroy(gameObject);
            Respawn();
        }
        if (Col.gameObject.tag == "Player")
        {
            Debug.Log("bullet has hit");
            Destroy(gameObject);
            
        }
        
    }
    public static int spawnPoint = 2;
    [SerializeField]
    public GameObject spawnPoint1;

    public GameObject spawnPoint2;

    private Vector3 respawnLocation;


    public void Respawn()
    {

        int spawnPoints = Random.Range(0, spawnPoint);
        switch (spawnPoints)
        {
            case 0:
                transform.position = spawnPoint1.transform.position;
                transform.rotation = Quaternion.Euler(0, 0, 0);
                Debug.Log("Repawn1");
                break;


            case 1:
                transform.position = spawnPoint2.transform.position;
                transform.rotation = Quaternion.Euler(0, 0, 0);
                Debug.Log("Repawn2");
                break;
        }
    }
}
