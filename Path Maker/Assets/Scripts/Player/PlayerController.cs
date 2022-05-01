using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private ulong localId;
    private CharacterController characterController;

    public override void OnNetworkSpawn()
    {
        localId = NetworkManager.Singleton.LocalClientId;
        characterController = gameObject.GetComponent<CharacterController>();
    }

    void Update()
    {
        if (IsLocalPlayer)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            MovePlayer_ServerRpc(horizontal, vertical);
        }
    }

    [ServerRpc]
    void MovePlayer_ServerRpc(float horizontal, float vertical)
    {
        transform.Rotate(0, horizontal * rotateSpeed, 0);
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        float curSpeed = speed * vertical;
        Vector3 move = forward * curSpeed;
        if (move.magnitude > 0.1)
        {
            characterController.SimpleMove(move);
        }
    }
}
