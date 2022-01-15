using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace PathMaker
{
    public class GameManager : MonoBehaviour
    {
        private Lobby LocalLobby = null;
        private List<Lobby> AllLobbies = new List<Lobby>();
        private bool hasJoined = false;
        private ConcurrentQueue<string> createdLobbyIds = new ConcurrentQueue<string>();
        private string defaultNewLobbyName = "New lobby";

        async void Awake()
        {
            // Authenticate
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            QueryAllLobbies();
        }

        void OnGUI()
        {
            GUI.BeginGroup(new Rect(100, 100, 1000, 1000));
            if (hasJoined)
            {
                GUI.Label(new Rect(0, 100, 1000, 20), "Lobby name: " + LocalLobby.Name);
                GUI.Label(new Rect(0, 130, 100, 20), LocalLobby.MaxPlayers - LocalLobby.AvailableSlots + " players up to " + LocalLobby.MaxPlayers);
                RefreshInfoButton();
                LeaveButton();
            }
            else
            {

                RefreshButton();
                CreateButton(defaultNewLobbyName);
                defaultNewLobbyName = GUI.TextField(new Rect(0, 60, 100, 20), defaultNewLobbyName, 20);
                GUI.Label(new Rect(0, 100, 100, 20), "Lobbies:");
                GUILayout.BeginArea(new Rect(0, 120, 150, 20));
                foreach (Lobby lobby in AllLobbies)
                {
                    if (GUILayout.Button(lobby.Name + " " + (lobby.MaxPlayers - lobby.AvailableSlots) + "/" + lobby.MaxPlayers)) JoinLobby(lobby.Id);
                }
                GUILayout.EndArea();
            }
            GUI.EndGroup();
        }

        private void RefreshButton()
        {
            if (GUILayout.Button("Refresh")) QueryAllLobbies();
        }

        private void RefreshInfoButton()
        {
            if (GUILayout.Button("Refresh info")) UpdateLocalQuery();
        }

        private async void QueryAllLobbies()
        {
            print("Query all lobbies");
            var response = await Lobbies.Instance.QueryLobbiesAsync();
            AllLobbies = response.Results;
        }

        private async void UpdateLocalQuery()
        {
            string lobbyId = LocalLobby.Id;
            LocalLobby = await Lobbies.Instance.GetLobbyAsync(lobbyId);
        }

        private void CreateButton(string name)
        {
            if (GUILayout.Button("Create")) CreateLobby(name);
        }

        private async void CreateLobby(string input)
        {
            string lobbyName = "New lobby";
            if (input != "")
            {
                lobbyName = input;
            }
            int maxPlayers = 4;
            CreateLobbyOptions options = new CreateLobbyOptions()
            {
                IsPrivate = false,
                Player = new Unity.Services.Lobbies.Models.Player(AuthenticationService.Instance.PlayerId)
            };

            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            // Heartbeat the lobby every 15 seconds.
            createdLobbyIds.Enqueue(lobby.Id);
            print("Populate createLobbyIds queue");
            LocalLobby = lobby;
            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));
            print("Just create new lobby (code): " + lobby.LobbyCode);
            hasJoined = true;
        }

        IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
        {
            var delay = new WaitForSecondsRealtime(waitTimeSeconds);
            while (true)
            {
                Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
                print("Heartbeat from lobby " + lobbyId);
                yield return delay;
            }
        }

        private async void JoinLobby(string lobbyId)
        {
            await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId);
            createdLobbyIds.Enqueue(lobbyId);
            print("Just joined lobby " + lobbyId);
            LocalLobby = await Lobbies.Instance.GetLobbyAsync(lobbyId);
            hasJoined = true;
        }

        private void LeaveButton()
        {
            if (GUILayout.Button("Leave")) LeaveLobby();
        }

        private async void LeaveLobby()
        {
            string playerId = AuthenticationService.Instance.PlayerId;
            while (createdLobbyIds.TryDequeue(out var lobbyId))
            {
                await Lobbies.Instance.RemovePlayerAsync(lobbyId, playerId);
                print("Just leaving lobby (id): " + lobbyId);
            }
            hasJoined = false;
        }

        void OnApplicationQuit()
        {
            while (createdLobbyIds.TryDequeue(out var lobbyId))
            {
                Lobbies.Instance.DeleteLobbyAsync(lobbyId);
                print("Delete lobby " + lobbyId);
            }
        }
    }
}