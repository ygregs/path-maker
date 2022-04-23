using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class bullet : NetworkBehaviour
{
    [SerializeField] private float speed = 0.1f;
    [SerializeField] private float acc = 500f;

  
    private Transform myTransform;

    // Start is called before the first frame update
    void Start()
    {
        myTransform = GetComponent<Transform>();
        //myTransform.eulerAngles = new Vector3(0, syncYRotation, 0);
    }

    // Update is called once per frame
    void Update()
    {
         myTransform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
        //gameObject.GetComponent<Rigidbody>().velocity = transform.TransformDirection(Vector3.forward) * speed;

        if (IsOwner)
        {
            if (Input.GetMouseButtonDown(1))
            {
                gameObject.GetComponent<Rigidbody>().velocity*= acc;
            }
        }
        
    }
}
