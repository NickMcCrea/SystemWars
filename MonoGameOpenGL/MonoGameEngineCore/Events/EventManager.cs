using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Events
{
    public static class DelegateExtensions
    {
        public static object DynamicInvoke(this Delegate dlg, params object[] args)
        {

            return dlg.Method.Invoke(dlg.Target, BindingFlags.Default, null, args, null);

        }
    }

    public class EventManager
    {
        private readonly Queue<object> m_events = new Queue<object>();
        private readonly Dictionary<Type, List<Delegate>> m_listeners = new Dictionary<Type, List<Delegate>>();

        /// <summary>
        /// Setting this to true will safely flush all subscriptions on the next update.
        /// </summary>
        public bool FlushAllSubscriptionsFlag { get; set; }

        public void Subscribe<T>(Action<T> _eventCallback)
        {
            var t = typeof(T);
            List<Delegate> del;
            if (m_listeners.TryGetValue(t, out del))
            {
                var callback = del as Action<T>;

                callback += _eventCallback;
                m_listeners[t].Add(callback);
            }
            else
            {
                List<Delegate> delList = new List<Delegate>();
                delList.Add(_eventCallback);
                m_listeners.Add(t, delList);
            }

        }

        public void Unsubscribe<T>(Action<T> _eventCallback)
        {
            var t = typeof(T);
            List<Delegate> del;
            if (m_listeners.TryGetValue(t, out del))
            {
                var callback = del as Action<T>;
                Debug.Assert(callback != null);
                callback -= _eventCallback;
                if (callback == null)
                {
                    m_listeners.Remove(t);
                }
                else
                {
                    m_listeners[t].Remove(callback);
                }
            }
        }

        public void Send<T>(T _event)
        {
            m_events.Enqueue(_event);
        }

        public void SendImmediate<T>(T _event)
        {
            List<Delegate> listeners;
            if (m_listeners.TryGetValue(_event.GetType(), out listeners))
            {
                foreach (Delegate listener in listeners)
                {
                    DelegateExtensions.DynamicInvoke(listener, _event);
                }

            }
        }

        public void Update(TimeSpan _delta)
        {
            int count = m_events.Count;

            while (count > 0)
            {
                var evnt = m_events.Dequeue();
                List<Delegate> listeners;
                if (m_listeners.TryGetValue(evnt.GetType(), out listeners))
                {
                    foreach (Delegate listener in listeners)
                    {
                        DelegateExtensions.DynamicInvoke(listener, evnt);
                    }

                }
                count -= 1;
            }
        }

        public void UnsubscribeFromAllEvents()
        {
            foreach (Type t in m_listeners.Keys)
            {
                List<Delegate> listeners;
                if (m_listeners.TryGetValue(t, out listeners))
                {

                    foreach (Delegate d in listeners)
                    {
                        Delegate.Remove(d, d.Target as Delegate);
                        Delegate.Remove(d.Target as Delegate, d);
                    }

                }
                m_listeners[t].Clear();
            }
            m_listeners.Clear();
            m_events.Clear();

        }


    }
}
