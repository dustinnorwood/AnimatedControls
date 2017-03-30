using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnimatedControls
{
    public delegate void AnimatedControlUpdateDelegate();
    public enum AnimationStyles { None, Linear, Accelerate, Decelerate, Spring, Bounce };
    public partial class AnimatedControl : Control
    {
        public int RefreshRate
        {
            get { return Animator.Instance.GetInterval(this); }
            set { Animator.Instance.SetInterval(this, value); }
        }

        protected int m_RefreshSpeed = 10; //Number of timer ticks granted to animate the control
        private int m_RefreshProgress;
        public double RefreshProgress { get { if (m_RefreshSpeed == 0) return 1.0; else return (double)m_RefreshProgress / (double)m_RefreshSpeed; } }

        public AnimationStyles AnimationStyle = AnimationStyles.None;

        public AnimatedControl()
        {
            InitializeComponent();

            Animator.Instance.Add(this, 50);
        }

        protected double GetAnimationPoint()
        {
            switch (this.AnimationStyle)
            {
                case AnimationStyles.Linear:
                    return GetLinearPoint(RefreshProgress);
                case AnimationStyles.Accelerate:
                    return GetAcceleratePoint(RefreshProgress);
                case AnimationStyles.Decelerate:
                    return GetDeceleratePoint(RefreshProgress);
                case AnimationStyles.Spring:
                    return GetSpringPoint(RefreshProgress);
                case AnimationStyles.Bounce:
                    return GetBouncePoint(RefreshProgress);
                default:
                    return 1.0;
            }
        }

        protected virtual void OnRefreshed(EventArgs e)
        {
            m_RefreshProgress = 0;
            Animator.Instance.Draw(this);
        }

        private void draw_frame()
        {
            this.Invalidate();
            this.Update();
        }

        public void DrawFrame()
        {
            m_RefreshProgress++;
            if(IsHandleCreated && InvokeRequired)
            {
                this.Invoke(new AnimatedControlUpdateDelegate(draw_frame));
            }
        }
        private double GetLinearPoint(double d)
        {
            return d;
        }

        private double GetAcceleratePoint(double d)
        {
            return 1 - Math.Sqrt(1 - (1 - d) * (1 - d));
        }

        private double GetDeceleratePoint(double d)
        {
            return Math.Sqrt(1 - (d - 1) * (d - 1));
        }

        private double GetSpringPoint(double d)
        {
            if (d == 0)
                return 0;
            else
                return -(Math.Sin(20 * Math.PI * d) / (20 * Math.PI * d)) + 1;
        }

        private double GetBouncePoint(double d)
        {
            if (d == 0)
                return 0;
            else
                return -(Math.Abs(Math.Sin(20 * Math.PI * d)) / (20 * Math.PI * d)) + 1;
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                Animator.Instance.Remove(this);
            }
        }
    }
}
