using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
//using Mirror;


public class tire : NetworkBehaviour
{
    [SerializeField]
    public GameObject projectil;

    private GameObject bullet;

    [SerializeField]
    public float bullet_speed;

    [SerializeField]
    private float initial_speed;

    void Start()
    {
        transform.position = transform.position + transform.TransformDirection(Vector3.forward) * 0.1f;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsHost)
        {
            InputHost();
        }
        else
        {
            InputClientServerRpc();
        }
        

            
    }

    private void InputHost()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //Destroy(bullet);
            bullet = Instantiate(projectil, transform.position, Quaternion.identity) as GameObject;  
            bullet.GetComponent<NetworkObject>().Spawn();
            bullet.GetComponent<Rigidbody>().velocity = transform.TransformDirection(Vector3.forward) * initial_speed;
            //Destroy(bullet, 3);
        }
        //if (Input.GetMouseButtonDown(1))
        //{
           // bullet.GetComponent<Rigidbody>().velocity *= bullet_speed;
            //Destroy(bullet, 2);

        //}
    }

    [ServerRpc]
    private void InputClientServerRpc()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("L click");
            //Destroy(bullet);
            bullet = Instantiate(projectil, transform.position, Quaternion.identity) as GameObject;
            bullet.GetComponent<NetworkObject>().Spawn();
            bullet.GetComponent<Rigidbody>().velocity = transform.TransformDirection(Vector3.forward) * initial_speed;
            //Destroy(bullet, 3);
        }
        if (Input.GetMouseButtonDown(1))
        {
            bullet.GetComponent<Rigidbody>().velocity *= bullet_speed;
            //Destroy(bullet, 2);

        }
    }

}