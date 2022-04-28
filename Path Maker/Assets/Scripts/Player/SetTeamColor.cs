using UnityEngine;

namespace PathMaker.player
{
    public class SetTeamColor : MonoBehaviour
    {
        [SerializeField] private SkinnedMeshRenderer m_skinMeshRender;
        [SerializeField] private TeamLogic m_teamLogic;

        public void Update()
        {
            if (m_teamLogic.playerNetworkTeam.Value == TeamState.AsianTeam)
            {
                m_skinMeshRender.materials[2].SetColor("_Color", new Color(255, 78, 78));
            }
            else
            {
                m_skinMeshRender.materials[2].SetColor("_Color", new Color(78, 82, 255));
            }
        }
    }
}