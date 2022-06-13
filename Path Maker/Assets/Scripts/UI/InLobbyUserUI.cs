using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PathMaker.UI
{
    /// <summary>
    /// When inside a lobby, this will show information about a player, whether local or remote.
    /// </summary>
    [RequireComponent(typeof(LobbyUserObserver))]
    public class InLobbyUserUI : ObserverPanel<LobbyUser>
    {
        [SerializeField]
        TMP_Text m_DisplayNameText;

        [SerializeField]
        TMP_Text m_TeamText;

        [SerializeField]
        TMP_Text m_StatusText;

        [SerializeField]
        Image m_HostIcon;

        // [SerializeField]
        // Image m_EmoteImage;

        // [SerializeField]
        // Sprite[] m_EmoteIcons;

        // [SerializeField]
        // vivox.VivoxUserHandler m_vivoxUserHandler;

        public bool IsAssigned => UserId != null;

        public string UserId { get; private set; }
        private LobbyUserObserver m_observer;

        public void SetUser(LobbyUser myLobbyUser)
        {
            Show();
            if (m_observer == null)
                m_observer = GetComponent<LobbyUserObserver>();
            m_observer.BeginObserving(myLobbyUser);
            UserId = myLobbyUser.ID;
            // m_vivoxUserHandler.SetId(UserId);
        }

        public void OnUserLeft()
        {
            UserId = null;
            Hide();
            m_observer.EndObserving();
        }

        public override void ObservedUpdated(LobbyUser observed)
        {
            m_DisplayNameText.SetText(observed.DisplayName);
            m_StatusText.SetText(SetStatusFancy(observed.UserStatus));
            m_TeamText.SetText(SetTeamFancy(observed.TeamState));
            m_HostIcon.enabled = observed.IsHost;
        }

        string SetTeamFancy(TeamState state)
        {
            switch (state)
            {
                case TeamState.AsianTeam:
                    return "<color=#7E0F0F>Asian</color>"; // Red
                case TeamState.GreekTeam:
                    return "<color=#7CF6EE>Greek</color>"; // Light blue
                default:
                    return "";
            }
        }

        string SetStatusFancy(UserStatus status)
        {
            switch (status)
            {
                case UserStatus.Lobby:
                    return "<color=#56B4E9>In Lobby</color>"; // Light Blue
                case UserStatus.Ready:
                    return "<color=#51FF3D>Ready</color>"; // Light Mint
                case UserStatus.Connecting:
                    return "<color=#F0E442>Connecting...</color>"; // Bright Yellow
                case UserStatus.InGame:
                    return "<color=#005500>In Game</color>"; // Green
                default:
                    return "";
            }
        }
    }
}
