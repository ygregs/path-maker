using TMPro;
using UnityEngine;

namespace PathMaker.UI
{
    /// <summary>
    /// Displays the player's name.
    /// </summary>
    public class UserNameUI : ObserverPanel<LobbyUser>
    {
        [SerializeField]
        TMP_Text m_TextField;

        public override void ObservedUpdated(LobbyUser observed)
        {
            m_TextField.SetText("Connected as: " + observed.DisplayName);
        }
    }
}
