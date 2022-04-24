using Unity.Netcode;
using UnityEngine;

namespace PathMaker
{
    public class HelloWorldManager : MonoBehaviour
    {
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            SubmitNewPosition();
            GUILayout.EndArea();
        }

        void SubmitNewPosition()
        {
            if (NetworkManager.Singleton != null && GUILayout.Button(NetworkManager.Singleton.IsServer ? "Move" : "Request Position Change"))
            {
                var playerObject = NetworkManager.Singleton?.SpawnManager.GetLocalPlayerObject();
                var player = playerObject?.GetComponent<HelloWorldPlayer>();
                player.Move();
            }
        }
    }
}