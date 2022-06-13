using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Transform _mainCamera;

    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").transform;
        }
    }
    void LateUpdate()
    {
        transform.LookAt(transform.position + _mainCamera.forward);
    }
}