using UnityEngine;
using UnityEngine.SceneManagement;

namespace PathMaker
{

    public class LoadSceneManager : MonoBehaviour
    {

        public void LoadGameScene()
        {
            SceneManager.LoadScene("testGameScene");
        }

        public void LoadGameSceneAsHost()
        {
            SetupGameScene.ToGameSceneInformation = "host";
            LoadGameScene();
        }

        public void LoadGameSceneAsClient()
        {
            SetupGameScene.ToGameSceneInformation = "client";
            LoadGameScene();
        }
    }
}