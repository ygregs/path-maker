using TMPro;
using UnityEngine;

namespace PathMaker.UI
{
    /// <summary>
    /// After all players ready up for the game, this will show the countdown that occurs.
    /// This countdown is purely visual, to give clients a moment if they need to un-ready before entering the game; 
    /// clients will actually wait for a message from the host confirming that they are in the game, instead of assuming the game is ready to go when the countdown ends.
    /// </summary>
    public class TimerUI : ObserverBehaviour<Timer.Data>
    {
        [SerializeField]
        TMP_Text m_TimerText;

        protected override void UpdateObserver(Timer.Data data)
        {
            base.UpdateObserver(data);
            if (observed.TimeLeft <= 0)
                m_TimerText.SetText("");
            else
                m_TimerText.SetText($"{observed.TimeLeft:0}"); // Note that the ":0" formatting rounds, not truncates.
        }
    }
}