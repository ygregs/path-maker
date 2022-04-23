using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class respawn : MonoBehaviour
{
    public static int spawnPoint = 2;

    public GameObject spawn1 ;
    public GameObject spawn2;

    private Vector3 respawnLocation;

    public static GameObject spawnPoint1;
    public static GameObject spawnPoint2;



    void Awake()
    {
        spawnPoint1 = spawn1;
        spawnPoint2 = spawn2;
    }


   
    public static void Respawn(GameObject player)
    {
        
        int spawnPoints = Random.Range(0, spawnPoint);
        switch (spawnPoints)
        {
            case 0:
                player.transform.position = spawnPoint1.transform.position;
                player.transform.rotation = Quaternion.Euler(0,0,0);
                Debug.Log("Repawn1");
                break;


            case 1:
                player.transform.position = spawnPoint2.transform.position;
                player.transform.rotation = Quaternion.Euler(0, 0, 0);
                Debug.Log("Repawn2");
                break;
        }
    }
}
