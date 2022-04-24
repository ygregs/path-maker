using TMPro;
using UnityEngine;

namespace PathMaker.UI
{
    /// Displays the name of the lobby.
    public class LobbyNameUI : ObserverPanel<LocalLobby>
    {
        [SerializeField]
        TMP_Text m_lobbyNameText;

        public override void ObservedUpdated(LocalLobby observed)
        {
            m_lobbyNameText.SetText(observed.LobbyName);
        }
    }
}
