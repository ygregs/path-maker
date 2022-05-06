using UnityEngine;

namespace PathMaker.UI
{
    public class ChooseTeamUI : MonoBehaviour
    {

        private TeamState m_team = TeamState.AsianTeam;
        public void ToggleTeam()
        {
            if (m_team == TeamState.AsianTeam)
            {
                Locator.Get.Messenger.OnReceiveMessage(MessageType.LobbyUserTeam, TeamState.GreekTeam);
                m_team = TeamState.GreekTeam;
            }
            else
            {
                Locator.Get.Messenger.OnReceiveMessage(MessageType.LobbyUserTeam, TeamState.AsianTeam);
                m_team = TeamState.AsianTeam;
            }
        }
    }
}
