using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnLocation : MonoBehaviour
{
    public int nbSpawnpoints = 2;
    public GameObject SpawnPointOne;
    public GameObject SpawnPointTwo;

    // Start is called before the first frame update
    public void respawn (GameObject player)
    {
        int SpawnPoint = Random.Range(0, nbSpawnpoints);
        switch (SpawnPoint)
        {
            case 0:
                player.transform.position = SpawnPointOne.transform.position;
                player.transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case 1:
                player.transform.position = SpawnPointTwo.transform.position;
                player.transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            
        }

    }
}
