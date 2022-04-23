using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


public class SpawnIn : NetworkBehaviour
{
    private void OnGUI()
    {
        if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
        if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
    }
}
