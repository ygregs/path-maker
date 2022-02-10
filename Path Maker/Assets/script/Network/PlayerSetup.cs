using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField]
    Behaviour[] componentsToDisable;

    Camera mainCamera;

    private void Start()
    {
        if (!IsLocalPlayer)
        {
            // boucle pour suppr les composants des autres joueurs
            for (int i = 0; i < componentsToDisable.Length; i++)
            {
                componentsToDisable[i].enabled = false;
            }
        }
        else
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.gameObject.SetActive(false);
            }
            
        }
    }
    private void OnDisable()
    {
        if(mainCamera != null)
        {
            mainCamera.gameObject.SetActive(true);
        }
    }
}
