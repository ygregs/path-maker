using UnityEngine;

namespace PathMaker
{

    public class InitGameScene : MonoBehaviour
    {
        public void Start()
        {
            Debug.Log("testGameScene loaded as a " + SetupGameScene.ToGameSceneInformation);
        }
    }
}