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
    public partial class LineGraph : AnimatedControl
    {
        private MemoryGraph m_Graph;
        private double[] m_Data;
        private SolidBrush m_Back, m_DataBrush, m_Box;
        private Pen m_DataPen, m_Line;
        private Font m_Font, m_SmallFont;

        public int DataPointRadius = 5;
        public int LineThickness = 1;

        public int Minimum = 0;
        public int Maximum = 0;

        public LineGraph()
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
        }

        public void FeedData(double[] data)
        {
            m_Data = data;
            for (int i = 0; i < m_Data.Length; i++)
            {
                if (double.IsNaN(m_Data[i]) || double.IsInfinity(m_Data[i]))
                    m_Data[i] = 0.0;
            }
            OnRefreshed(EventArgs.Empty);
        }

        public void FeedData(int[] data)
        {
            m_Data = new double[data.Length];
            for (int i = 0; i < data.Length; i++)
                m_Data[i] = (double)data[i];
        }

        protected override void OnPaintBackground(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            m_Graph.Graphics.FillRectangle(m_Back, this.ClientRectangle);
            if (m_Data != null && m_Data.Length > 0)
            {
                double minVal, maxVal;
                if (this.Minimum < this.Maximum)
                {
                    minVal = this.Minimum;
                    maxVal = this.Maximum;
                }
                else
                {
                    minVal = m_Data.Min();
                    if (Minimum < minVal)
                        minVal = Minimum;
                    maxVal = m_Data.Max();
                }

                double range = maxVal - minVal + 1;
                if (range == 0) range++;
                double scoot = (1 - Math.Sqrt(RefreshProgress));
                int a = (int)((m_Graph.Image.Height - DataPointRadius) * (1 - (Probability.E(m_Data) - minVal) / range));
                a += (int)((m_Graph.Image.Height - DataPointRadius - a) * scoot);
                int vu = (int)((m_Graph.Image.Height - DataPointRadius) * (1 - (Probability.E(m_Data) + m_Data.StandardDeviation() - minVal) / range));
                vu += (int)((m_Graph.Image.Height - DataPointRadius - vu) * scoot);
                int vd = (int)((m_Graph.Image.Height - DataPointRadius) * (1 - (Probability.E(m_Data) - m_Data.StandardDeviation() - minVal) / range));
                vd += (int)((m_Graph.Image.Height - DataPointRadius - vd) * scoot);
                m_Graph.Graphics.FillRectangle(m_Box, 0, vu, this.ClientSize.Width, vd - vu);
                m_Graph.Graphics.DrawLine(m_Line, 0, a, this.ClientSize.Width, a);
                for (int k = 0; k < m_Data.Length; k++)
                {
                    //int red = (m_Data[k] - minVal) < range / 2 ? 255 : (int)(255 * (1 - (m_Data[k] - minVal - range / 2) * 2 / range));
                    //int green = (m_Data[k] - minVal) > range / 2 ? 255 : (int)(255 * (m_Data[k] - minVal) * 2 / range);
                    //if (red < 0)
                    //    red = 0;
                    //else if (red > 255)
                    //    red = 255;
                    //if (green < 0)
                    //    green = 0;
                    //else if (green > 255)
                    //    green = 255;
                    //using (SolidBrush b = new SolidBrush(Color.FromArgb(red, green, 0)))
                    //{
                    int x = k * (m_Graph.Image.Width - DataPointRadius) / m_Data.Length;
                    int y = (int)((m_Graph.Image.Height - DataPointRadius) * (1 - (m_Data[k] - minVal) / range));
                    y += (int)((m_Graph.Image.Height - DataPointRadius - y) * scoot);
                    m_Graph.Graphics.FillEllipse(m_DataBrush, x, y, DataPointRadius, DataPointRadius);

                    if (k < m_Data.Length - 1)
                    {
                        //using (Pen p = new Pen(Color.FromArgb(red, green, 0), LineThickness))
                        //{
                        int x0 = (k + 1) * (m_Graph.Image.Width - DataPointRadius) / m_Data.Length + DataPointRadius / 2;
                        int y0 = (int)((m_Graph.Image.Height - DataPointRadius) * (1 - (m_Data[k + 1] - minVal) / range) + DataPointRadius / 2);
                        y0 += (int)((m_Graph.Image.Height - DataPointRadius - y0) * scoot);
                        m_Graph.Graphics.DrawLine(m_DataPen, x + DataPointRadius / 2, y + DataPointRadius / 2, x0, y0);
                        //}
                    }
                    //}
                }
                m_Graph.Graphics.DrawString("Range: {" + minVal.ToString() + "," + maxVal.ToString() + "} mu: " + Probability.E(m_Data).ToString() + " sig: " + m_Data.StandardDeviation().ToString(), m_SmallFont, Brushes.White, 5, this.ClientSize.Height - 5 - m_SmallFont.Height);
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
