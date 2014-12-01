using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace MonoGameEngineCore.Helper
{
    public class XNATimer
    {
        public delegate void XNATimerIntervalCallBack(GameTime gameTime);
        public event XNATimerIntervalCallBack XNATimerEventHandler;
    
        private float m_timer;
        private float m_interval;

        public bool Enabled { get; set; }

        public float Interval
        {
            get { return m_interval; }
            set { m_interval = value; }
        }

        public XNATimer(int _intervalInMilliSeconds, XNATimerIntervalCallBack _callback)
        {
            Enabled = true;
            XNATimerEventHandler += _callback;
            m_interval = _intervalInMilliSeconds;
           
        }

        public void Update(GameTime _gameTime)
        {
            if(!Enabled)
                return;

            m_timer += (float)_gameTime.ElapsedGameTime.TotalMilliseconds;

            if (m_timer > m_interval)
            {
                if (XNATimerEventHandler != null)
                    XNATimerEventHandler(_gameTime);

                m_timer = 0;
            
            }
        }

        public void Reset()
        {
            m_timer = 0;
        }
    }
}