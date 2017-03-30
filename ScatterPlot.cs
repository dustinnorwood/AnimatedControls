using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MemoryGraphs;
using ProbabilityEngine;

namespace AnimatedControls
{
    public partial class ScatterPlot : AnimatedControl
    {
        class ScatterPoints
        {
            public List<string> NameList = new List<string>();
            public List<double> XCoords = new List<double>();
            public List<double> YCoords = new List<double>();
            public ScatterPoints() { }
            public void Add(string name, double x, double y)
            {
                NameList.Add(name);
                XCoords.Add(x);
                YCoords.Add(y);
            }

            public void Clear()
            {
                NameList.Clear();
                XCoords.Clear();
                YCoords.Clear();
            }

            public int Count { get { return NameList.Count; } }
        }
        private MemoryGraph m_Graph;
        private ScatterPoints m_Pairs;
        private SolidBrush m_Back, m_DataBrush;
        private Font m_Font;

        public int DotRadius = 5;

        public int SelectedIndex = -1, SelectedIndex2 = -1;

        public int TopPadding = 15;

        public ScatterPlot()
        {
            InitializeComponent();
            m_Graph = new MemoryGraph(ClientSize.Width, ClientSize.Height);
            m_Back = new SolidBrush(Color.FromArgb(45, 45, 48));
            m_DataBrush = new SolidBrush(Color.FromArgb(128, 128, 128));
            m_Font = new Font("Lucida Console", 10, FontStyle.Regular);
            m_Pairs = new ScatterPoints();
            this.RefreshRate = 20;
            m_RefreshSpeed = 50;
        }

        public void Add(string s, double d, double e)
        {
            m_Pairs.Add(s, d, e);
        }

        public void Clear()
        {
            m_Pairs.Clear();
        }

        private double xave = 0, yave = 0;
        private double xstd = 0, ystd = 0;
        public void Feed()
        {
            xave = Probability.E(m_Pairs.XCoords);
            yave = Probability.E(m_Pairs.YCoords);

            xstd = m_Pairs.XCoords.StandardDeviation();
            ystd = m_Pairs.YCoords.StandardDeviation();

            OnRefreshed(EventArgs.Empty);
        }


        protected override void OnPaintBackground(PaintEventArgs pe)
        {
            m_Graph.Graphics.FillRectangle(m_Back, this.ClientRectangle);
            if (m_Pairs != null && m_Pairs.Count > 0)
            {
                double xmaxVal = m_Pairs.XCoords.Max();
                double ymaxVal = m_Pairs.YCoords.Max();
                double xminVal = m_Pairs.XCoords.Min();
                double yminVal = m_Pairs.YCoords.Min();
                double scoot = Math.Sqrt(1 - (RefreshProgress - 1) * (RefreshProgress - 1));
                for (int k = 0; k < m_Pairs.Count; k++)
                {
                    int x = (int)(scoot * ((m_Graph.Image.Width) * (m_Pairs.XCoords[k] - xminVal) / (xmaxVal - xminVal + 1)));
                    int y = (int)((m_Graph.Image.Height) * (1 - (m_Pairs.YCoords[k] - yminVal) / (ymaxVal - yminVal + 1)));
                    if (y < 0)
                        y = m_Graph.Image.Height;
                    m_Graph.Graphics.FillEllipse(m_DataBrush, x - DotRadius, y - DotRadius, 2 * DotRadius, 2 * DotRadius);
                }
                if (SelectedIndex >= 0)
                {
                    int x = (int)(scoot * ((m_Graph.Image.Width) * (m_Pairs.XCoords[SelectedIndex] - xminVal) / (xmaxVal - xminVal + 1)));
                    int y = (int)((m_Graph.Image.Height) * (1 - (m_Pairs.YCoords[SelectedIndex] - yminVal) / (ymaxVal - yminVal + 1)));
                    m_Graph.Graphics.FillEllipse(Brushes.Green, x - DotRadius, y - DotRadius, 2 * DotRadius, 2 * DotRadius);
                    //if(y < this.Height / 2)
                    if (x > this.Width / 2)
                        m_Graph.Graphics.DrawString(m_Pairs.NameList[SelectedIndex] + "{" + m_Pairs.XCoords[SelectedIndex].ToString("#.0000") + "," + m_Pairs.YCoords[SelectedIndex].ToString("#.0000") + "}", m_Font, Brushes.White, x - DotRadius - 5 - 7 * m_Pairs.NameList[SelectedIndex].Length, y - DotRadius - 5);
                    else
                        m_Graph.Graphics.DrawString(m_Pairs.NameList[SelectedIndex] + "{" + m_Pairs.XCoords[SelectedIndex].ToString("#.0000") + "," + m_Pairs.YCoords[SelectedIndex].ToString("#.0000") + "}", m_Font, Brushes.White, x + DotRadius + 5, y + DotRadius + 5);
                }
                if (SelectedIndex2 >= 0)
                {
                    int x = (int)(scoot * ((m_Graph.Image.Width) * (m_Pairs.XCoords[SelectedIndex2] - xminVal) / (xmaxVal - xminVal + 1)));
                    int y = (int)((m_Graph.Image.Height) * (1 - (m_Pairs.YCoords[SelectedIndex2] - yminVal) / (ymaxVal - yminVal + 1)));
                    m_Graph.Graphics.FillEllipse(Brushes.Red, x - DotRadius, y - DotRadius, 2 * DotRadius, 2 * DotRadius);
                    if (x > this.Width / 2)
                        m_Graph.Graphics.DrawString(m_Pairs.NameList[SelectedIndex2] + "{" + m_Pairs.XCoords[SelectedIndex2].ToString("#.0000") + "," + m_Pairs.YCoords[SelectedIndex2].ToString("#.0000") + "}", m_Font, Brushes.White, x - DotRadius - 5 - 7 * m_Pairs.NameList[SelectedIndex2].Length, y - DotRadius - 5);
                    else
                        m_Graph.Graphics.DrawString(m_Pairs.NameList[SelectedIndex2] + "{" + m_Pairs.XCoords[SelectedIndex2].ToString("#.0000") + "," + m_Pairs.YCoords[SelectedIndex2].ToString("#.0000") + "}", m_Font, Brushes.White, x + DotRadius + 5, y + DotRadius + 5);
                }
            }
            m_Graph.Graphics.DrawString(this.Text, m_Font, Brushes.White, 5, 5);
            pe.Graphics.DrawImage(m_Graph.Image, 0, 0);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            m_Graph.Create(this.ClientSize.Width, this.ClientSize.Height);
            this.Invalidate();
        }
    }
}
