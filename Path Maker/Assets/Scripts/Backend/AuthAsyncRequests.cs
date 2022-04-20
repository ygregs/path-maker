using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace PathMaker
{

    public class User
    {
        public string playername;
        public string password;
    }


    public class AuthAsyncRequests : MonoBehaviour
    {
        // Making singleton pattern
        private static AuthAsyncRequests s_instance;

        public static AuthAsyncRequests Instance
        {
            get
            {
                if (s_instance == null)
                    // s_instance = new AuthAsyncRequests();
                    s_instance = (new GameObject("AuthAsyncRequestGO")).AddComponent<AuthAsyncRequests>();
                return s_instance;
            }
        }

        public void LoginUser(string playername, string password)
        {

            if (!string.IsNullOrEmpty(playername) && !string.IsNullOrEmpty(password))
            {
                StartCoroutine(SendLoginData(playername, password));
            }
            else
            {
                Locator.Get.Messenger.OnReceiveMessage(MessageType.LoginResponse, "Playername and password must be not empty.");
            }

        }

        public void Logout()
        {
            StartCoroutine(SendLogoutRequest());
        }

        private IEnumerator SendLoginData(string playername, string password)
        {
            var user = new User
            {
                playername = playername,
                password = password
            };
            // Delete cookie before requesting a new login
            Webservices.CookieString = null;

            var request = Webservices.Post("api/auth/login", JsonUtility.ToJson(user));
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                //Debug.LogError(request.error);
                Locator.Get.Messenger.OnReceiveMessage(MessageType.LoginResponse, request.error);
            }

            Webservices.ResponseData responseData = Webservices.ResponseData.CreateFromJSON(request.downloadHandler.text);
            if (responseData.success == null)
            {
                Debug.Log(responseData.success);
                Locator.Get.Messenger.OnReceiveMessage(MessageType.LoginResponse, request.downloadHandler.text);
            }
            else
            {
                Webservices.CookieString = request.GetResponseHeader("set-cookie");
                Locator.Get.Authenticator.GetAuthData().SetContent("log_status", "LOGGED");
                Locator.Get.Authenticator.GetAuthData().SetContent("session_cookie", Webservices.CookieString);
                Locator.Get.Messenger.OnReceiveMessage(MessageType.LoginResponse, "LOGGED");
                GetPlayerData();
            }

            // Debug.Log(request.downloadHandler.data);
        }

        public IEnumerator SendLogoutRequest()
        {
            var request = Webservices.Get("api/auth/logout");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Locator.Get.Messenger.OnReceiveMessage(MessageType.LogoutResponse, request.error);
            }
            else
            {
                Webservices.CookieString = null;
                Locator.Get.Authenticator.GetAuthData().SetContent("log_status", "NOT_LOGGED");
                Locator.Get.Authenticator.GetAuthData().SetContent("session_cookie", null);
                Locator.Get.Messenger.OnReceiveMessage(MessageType.LogoutResponse, "LOGOUT");
                Locator.Get.Messenger.OnReceiveMessage(MessageType.RenameRequest, "Guest");
            }
        }

        public class ResponsePlayer
        {
            public string message;

            public string playername;

            public string _id;

            public static ResponsePlayer CreateFromJSON(string jsonString)
            {
                return JsonUtility.FromJson<ResponsePlayer>(jsonString);
            }
        }

        public void GetPlayerData()
        {
            StartCoroutine(GetPlayerDataRequest());
        }

        public IEnumerator GetPlayerDataRequest()
        {
            var request = Webservices.Get("api/player/data");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError)
            {

                Locator.Get.Messenger.OnReceiveMessage(MessageType.DisplayErrorPopup, request.error); // Lobby error type, then HTTP error type.
            }
            else
            {
                ResponsePlayer responsePlayer = ResponsePlayer.CreateFromJSON(request.downloadHandler.text);
                Locator.Get.Messenger.OnReceiveMessage(MessageType.RenameRequest, responsePlayer.playername);
            }
        }

    }
}