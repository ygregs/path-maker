using System;
using UnityEngine;

namespace PathMaker
{

    [RequireComponent(typeof(UI.TimerUI))]
    public class Timer : MonoBehaviour, IReceiveMessages
    {
        public class Data : Observed<Timer.Data>
        {
            private float m_timeLeft;
            public float TimeLeft
            {
                get => m_timeLeft;
                set
                {
                    m_timeLeft = value;
                    OnChanged(this);
                }
            }
            public override void CopyObserved(Data oldObserved) { /*No-op, since this is unnecessary.*/ }
        }

        private Data m_data = new Data();
        private UI.TimerUI m_ui;
        private const int k_countdownTime = 30;

        public void OnEnable()
        {
            if (m_ui == null)
                m_ui = GetComponent<UI.TimerUI>();
            m_data.TimeLeft = -1;
            Locator.Get.Messenger.Subscribe(this);
            m_ui.BeginObserving(m_data);
        }
        public void OnDisable()
        {
            Locator.Get.Messenger.Unsubscribe(this);
            m_ui.EndObserving();
        }

        public void OnReceiveMessage(MessageType type, object msg)
        {
            if (type == MessageType.StartTimer)
            {
                m_data.TimeLeft = k_countdownTime;
            }
            else if (type == MessageType.ResetTimer)
            {
                m_data.TimeLeft = -1;
            }
        }

        public void Update()
        {
            if (m_data.TimeLeft < 0)
                return;
            m_data.TimeLeft -= Time.deltaTime;
            if (m_data.TimeLeft < 0)
                Locator.Get.Messenger.OnReceiveMessage(MessageType.CompleteTimer, null);
        }
    }
}
