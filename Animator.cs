using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnimatedControls
{

    public class Animator
    {
        private static Animator m_Instance;
        public static Animator Instance 
        { 
            get 
            { 
                if(m_Instance == null) 
                { 
                    m_Instance = new Animator(); 
                } 
                
                return m_Instance; 
            } 
        }
        
        private class AnimatorNode
        {
            public AnimatedControl Control;
            public int Interval;
            public int TickCount;

            public AnimatorNode(AnimatedControl control, int interval)
            {
                Control = control;
                Interval = interval;
                TickCount = 0;
            }
        }

        private System.Threading.Timer m_Timer;
        private List<AnimatorNode> m_Controls;
        private bool m_Running;

        private int m_RefreshPeriod = 5;
        public int RefreshPeriod { get { return m_RefreshPeriod; } set { m_RefreshPeriod = value; } }

        private object m_Lock = new object();

        public Animator()
        {
            m_Timer = new System.Threading.Timer(m_Timer_Tick, null, System.Threading.Timeout.Infinite, m_RefreshPeriod);
            m_Controls = new List<AnimatorNode>();
            m_Running = false;
        }

        private void m_Timer_Tick(object state)
        {
            lock (m_Lock)
            {
                bool isControlAnimating = false;

                foreach (AnimatorNode n in m_Controls)
                {
                    if (n.TickCount > 0 && n.Control.RefreshProgress < 1.0)
                    {
                        isControlAnimating = true;
                        if (--n.TickCount == 0)
                        {
                            n.TickCount = n.Interval;
                            n.Control.DrawFrame();
                        }
                    }
                    else if (n.TickCount > 0)
                    {
                        n.TickCount = 0;
                    }
                }

                if (!isControlAnimating)
                    RunTimer(false);
            }
        }

        public void Add(AnimatedControl control, int milliseconds)
        {
                if (m_Controls.Where(n => n.Control.Equals(control)).Count() == 0)
                {
                    m_Controls.Add(new AnimatorNode(control, milliseconds >= m_RefreshPeriod ? milliseconds / m_RefreshPeriod : 1));
                }
        }

        public void Remove(AnimatedControl control)
        {
                try
                {
                    AnimatorNode node = m_Controls.Find(n => n.Control.Equals(control));
                    node.TickCount = 0;
                    m_Controls.Remove(node);
                }
                catch (ArgumentNullException)
                {
                    System.Diagnostics.Debug.WriteLine("Animator.Remove ArgumentNullException");
                }
        }

        public int GetInterval(AnimatedControl control)
        {
            int interval = -1;
            try
            {
                AnimatorNode node = m_Controls.Find(n => n.Control.Equals(control));
                interval = node.Interval * m_RefreshPeriod;
            }
            catch (ArgumentNullException)
            {
                System.Diagnostics.Debug.WriteLine("Animator.GetInterval ArgumentNullException");
            }
            return interval;
        }

        public void SetInterval(AnimatedControl control, int milliseconds)
        {
            try
            {
                AnimatorNode node = m_Controls.Find(n => n.Control.Equals(control));
                node.Interval = milliseconds >= m_RefreshPeriod ? milliseconds / m_RefreshPeriod : 1;
            }
            catch (ArgumentNullException)
            {
                System.Diagnostics.Debug.WriteLine("Animator.SetInterval ArgumentNullException");
            }
        }

        public void Draw(AnimatedControl control)
        {
                try
                {
                    AnimatorNode node = m_Controls.Find(n => n.Control.Equals(control));
                    node.TickCount = 1;
                    RunTimer(true);
                }
                catch (ArgumentNullException)
                {
                    System.Diagnostics.Debug.WriteLine("Animator.Draw ArgumentNullException");
                }
        }

        private void RunTimer(bool run)
        {
            if(run)
            {
                if (!m_Running)
                {
                    m_Running = true;
                    m_Timer.Change(0, m_RefreshPeriod);
                }
            }
            else
            {
                m_Running = false;
                m_Timer.Change(System.Threading.Timeout.Infinite, m_RefreshPeriod);
            }
        }
    }
}
