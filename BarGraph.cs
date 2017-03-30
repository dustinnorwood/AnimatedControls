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
    public partial class BarGraph : AnimatedControl
    {
        private MemoryGraph m_Graph;
        private List<Tuple<string, double>> m_Pairs;
        private SolidBrush m_Back, m_DataBrush, m_Box;
        private Pen m_DataPen, m_Line;
        private Font m_Font, m_SmallFont;

        public int SpaceBetween = 5;
        public int TheseWickedLines = 10;
        public bool Ascending = false;
        public bool SortEntries = true;

        public int SelectedIndex = -1;

        public int StartIndex
        {
            get
            {
                int s = (m_Y_Offset - SpaceBetween - TopPadding) / (SpaceBetween + TheseWickedLines);
                return s >= 0 ? s : 0;
            }
        }

        public int EndIndex
        {
            get
            {
                int e = (m_Y_Offset + this.ClientSize.Height - SpaceBetween - TopPadding) / (SpaceBetween + TheseWickedLines);
                return e < m_Pairs.Count ? e : m_Pairs.Count;
            }
        }

        public int GetBarLocation(int index)
        {
            return (SpaceBetween + TheseWickedLines) * index + SpaceBetween + TopPadding - m_Y_Offset;
        }

        public int Minimum = 0;
        public int Maximum = 0;

        public int TopPadding = 0;

        private int m_Y_Offset = 0;

        private int m_Y_Offset_Max
        {
            get
            {
                if (m_Pairs != null)
                    return m_Pairs.Count * (SpaceBetween + TheseWickedLines) + SpaceBetween + TopPadding - this.ClientSize.Height;
                else return 0;
            }
        }

        public BarGraph()
        {
            InitializeComponent();
            m_Graph = new MemoryGraph(ClientSize.Width, ClientSize.Height);
            m_Back = new SolidBrush(Color.FromArgb(45, 45, 48));
            m_DataBrush = new SolidBrush(Color.FromArgb(128, 128, 128));
            m_Line = new Pen(Color.FromArgb(128, 153, 217, 234));
            m_Box = new SolidBrush(Color.FromArgb(32, 153, 217, 234));
            m_DataPen = new Pen(Color.FromArgb(192, 192, 192));
            m_Font = new Font("Lucida Console", 10, FontStyle.Regular);
            m_SmallFont = new Font("Lucida Console", 8, FontStyle.Regular);
            m_Pairs = new List<Tuple<string, double>>();
            this.RefreshRate = 20;
            m_RefreshSpeed = 50;
            this.AnimationStyle = AnimationStyles.Decelerate;
        }

        public void Add(string s, double d)
        {
            m_Pairs.Add(new Tuple<string, double>(s, d));
        }

        public void Clear()
        {
            m_Pairs.Clear();
        }

        private double ave = 0;
        private double std = 0;
        public void Feed()
        {
            ave = m_Pairs.Average();
            std = m_Pairs.StandardDeviation();
            if (SortEntries)
            {
                m_Pairs.Sort((pairA, pairB) => pairA.Item2.CompareTo(pairB.Item2));
                if (!Ascending)
                    m_Pairs.Reverse();
            }
            OnRefreshed(EventArgs.Empty);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            m_Y_Offset -= (e.Delta / 120) * 10 * (SpaceBetween + TheseWickedLines);
            if (m_Y_Offset > m_Y_Offset_Max) m_Y_Offset = m_Y_Offset_Max;
            else if (m_Y_Offset < 0) m_Y_Offset = 0;
            this.Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs pe)
        {
            m_Graph.Graphics.FillRectangle(m_Back, this.ClientRectangle);
            if (m_Pairs != null && m_Pairs.Count > 0)
            {
                double maxVal = SortEntries ? (Ascending ? m_Pairs[m_Pairs.Count - 1].Item2 : m_Pairs[0].Item2) : m_Pairs.MaxValue();
                if (maxVal == 0)
                    maxVal = 1;
                double scoot = GetAnimationPoint();
                int a = (int)((m_Graph.Image.Width) * ave / maxVal);
                a = (int)(a * scoot);
                int vu = (int)((m_Graph.Image.Width) * (ave + std) / maxVal);
                vu = (int)(vu * scoot);
                int vd = (int)((m_Graph.Image.Width) * (ave - std) / maxVal);
                vd = (int)(vd * scoot);
                for (int k = StartIndex; k < EndIndex; k++)
                {
                    int x = (int)((m_Graph.Image.Width) * m_Pairs[k].Item2 / maxVal);
                    x = (int)(x * scoot);
                    if (k == SelectedIndex)
                        m_Graph.Graphics.FillRectangle(Brushes.Green, 0, GetBarLocation(k), x, TheseWickedLines);
                    else
                        m_Graph.Graphics.FillRectangle(m_DataBrush, 0, GetBarLocation(k), x, TheseWickedLines);
                    m_Graph.Graphics.DrawString(m_Pairs[k].Item1, m_SmallFont, Brushes.White, 5, GetBarLocation(k) + 2);
                }
                m_Graph.Graphics.FillRectangle(m_Box, vd, 0, vu - vd, this.ClientSize.Height);
                m_Graph.Graphics.DrawLine(m_Line, a, 0, a, this.ClientSize.Height);
            }
            m_Graph.Graphics.DrawString(this.Text, m_Font, Brushes.White, 5, 5);
            pe.Graphics.DrawImage(m_Graph.Image, 0, 0);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            m_Graph.Create(this.ClientSize.Width, this.ClientSize.Height);
        }
    }
}
