using System;
using System.Collections.Generic;
using Stopwatch = System.Diagnostics.Stopwatch;
using UnityEngine;

namespace PathMaker
{
    public class Messenger : IMessenger
    {
        private List<IReceiveMessages> m_receivers = new List<IReceiveMessages>();
        private const float k_durationToleranceMs = 10;

        private Queue<Action> m_pendingReceivers = new Queue<Action>();
        private int m_recurseCount = 0;

        public virtual void Subscribe(IReceiveMessages receiver)
        {
            m_pendingReceivers.Enqueue(() => { DoSubscribe(receiver); });

            void DoSubscribe(IReceiveMessages receiver)
            {
                if (receiver != null && !m_receivers.Contains(receiver))
                {
                    m_receivers.Add(receiver);
                }
            }
        }

        public virtual void Unsubscribe(IReceiveMessages receiver)
        {
            m_pendingReceivers.Enqueue(() => { DoUnsubscribe(receiver); });

            void DoUnsubscribe(IReceiveMessages receiver)
            {
                m_receivers.Remove(receiver);
            }
        }

        public virtual void OnReceiveMessage(MessageType type, object msg)
        {
            if (m_recurseCount > 5)
            {
                Debug.LogError("OnReceiveMessages recursion detected! Is something calling OnReceiveMessage when it receives a message?");
                return;
            }

            if (m_recurseCount == 0)
            {
                while (m_pendingReceivers.Count > 0)
                {
                    m_pendingReceivers.Dequeue()?.Invoke();
                }
            }

            m_recurseCount++;
            Stopwatch stopwatch = new Stopwatch();
            foreach (IReceiveMessages receiver in m_receivers)
            {
                stopwatch.Restart();
                receiver.OnReceiveMessage(type, msg);
                stopwatch.Stop();
                if (stopwatch.ElapsedMilliseconds > k_durationToleranceMs)
                {
                    Debug.LogWarning($"Message recipient \"{receiver}\" took to long to processe message \"{msg}\" of type {type}");
                }
            }
            m_recurseCount--;
        }

        public void OnReProvide(IMessenger previousProvider)
        {
            if (previousProvider is Messenger)
            {
                m_receivers.AddRange((previousProvider as Messenger).m_receivers);
            }
        }

    }

    public enum MessageType
    {
        None = 0,
        ChangeGameState = 100,
    }

    public interface IReceiveMessages
    {
        void OnReceiveMessage(MessageType type, object msg);
    }

    public interface IMessenger : IReceiveMessages, IProvidable<IMessenger>
    {
        void Subscribe(IReceiveMessages receiver);
        void Unsubscribe(IReceiveMessages receiver);
    }
}