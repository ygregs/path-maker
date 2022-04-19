using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Samples;
//using Respawn;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(ClientNetworkTransform))]
public class PlayerControlAutorativ : NetworkBehaviour
{

  
    public CharacterController characterController;
    public Transform cam;
    public float turnSmoothTime = 0.1f;
  

    [SerializeField]
    private float speed = 3.5f;

    [SerializeField]
    private float rotationSpeed = 1.5f;

    //jump
 
    public float jumpSpeed;

    private float ySpeed;



    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

    }
    void Start()
    {
        // Respawn.respawn();
        PlayerCameraFollow.Instance.FollowPlayer(transform.Find("player"));
    }

    private void Update()
    {
        if(IsClient && IsOwner)
        {

            // déplacement du joueur de maniere hoizontal
            Vector3 inputRotation = new Vector3(0, Input.GetAxis("Horizontal"), 0);

            Vector3 direction = transform.TransformDirection(Vector3.forward);

            float forwardInput = Input.GetAxis("Vertical");
            if (Input.GetKey(KeyCode.LeftShift) && forwardInput > 0) forwardInput = 2;

            //aplication de la gravité 
            ySpeed += Physics.gravity.y * Time.deltaTime;

            if (characterController.isGrounded)
            {
                ySpeed = 0f;
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Debug.Log("jump");
                    
                    ySpeed = jumpSpeed;
                }
            }
            Vector3 inputPosition = direction * forwardInput;

            Vector3 velocity = inputPosition * speed;

            velocity.y = ySpeed;
            
            

            characterController.Move(velocity * Time.deltaTime);

            transform.Rotate(inputRotation * rotationSpeed, Space.World);

            

        }
  
    }
    private void ClientInput()
    {
        Vector3 inputRotation = new Vector3(0, Input.GetAxis("Horizontal"),0);

        Vector3 direction = transform.TransformDirection(Vector3.forward);
        float forwardInput = Input.GetAxis("Vertical");
        if (Input.GetKey(KeyCode.LeftShift) && forwardInput > 0) forwardInput = 2;

        Vector3 inputPosition = direction * forwardInput;

        Vector3 velocity = inputPosition * speed;
        characterController.SimpleMove(velocity);

        transform.Rotate(inputRotation * rotationSpeed, Space.World);

    }

    

}

