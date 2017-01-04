using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Specialized;

namespace Traffic_Simulator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private int GenHigh(int max_rate)
        {
            int new_number=0;

            do
            {
                try
                {
                    new_number = randhigh.Next() % max_rate;
                }
                catch { }
            } while (new_number < 1);
            return new_number;
        }

        private int GenLow(int max_rate, int val)
        {
            int new_number = 0;

            do
            {
                try
                {
                    new_number = randlow.Next() % (max_rate + val);
                }
                catch { }
            } while (new_number < 1);
            return new_number;
        }

        private int extractCall(int max)
        {
            int point = 0;
            try
            {
                point = randhigh.Next() % max;
            }
            catch { }
            return point;
        }


        void TCtxt_Leave(object sender, System.EventArgs e)
        {
            if ((TCtxt.Text != "") && (int.Parse(TCtxt.Text) > 30))
                TCtxt.Text = "30";
        }

        void RATEtxt_Leave(object sender, System.EventArgs e)
        {
            if ((RATEtxt.Text != "") && (int.Parse(RATEtxt.Text) < 1))
                RATEtxt.Text = "1";
        } 


        private void button1_Click(object sender, EventArgs e)
        {
            if ((RATEtxt.Text != "") && (int.Parse(RATEtxt.Text) < 1))
                RATEtxt.Text = "1";

            if ((TCtxt.Text != "") && (int.Parse(TCtxt.Text) > 30))
                TCtxt.Text = "30";

            
            outList.Items.Clear();

            ListDictionary HighCell, LowCell;
           

            int callServiceRate = 0, cellCapacity = 0, numberOfSources = 0, 
                transmissionCycles = 0, max_rateh = 0, max_ratel = 0, maxBuffer=0, bufcon=0;

            try
            {
                callServiceRate = int.Parse(RATEtxt.Text);
                cellCapacity = int.Parse(Xtxt.Text);
                numberOfSources = int.Parse(SOURCEStxt.Text);
                transmissionCycles = int.Parse(TCtxt.Text);
                max_rateh = int.Parse(HIGHtxt.Text);
                max_ratel = int.Parse(LOWtxt.Text);
                maxBuffer = int.Parse(maxBuftxt.Text);
            }
            catch { }

            Hpoints = new double[transmissionCycles + 1];
            Lpoints = new double[transmissionCycles + 1];

            int ct=0;

            for (int t = 1; t <= transmissionCycles; t++)
            {
                for (ulong b = 0; b <= 9999999; b++) { } //code to wait some milliseconds

                outList.Items.Add(t.ToString());
                outList.Items[ct].SubItems.Add(cellCapacity.ToString());
                 
                HighCell = new ListDictionary();
                LowCell = new ListDictionary();


                int Ahsum = 0, Alsum = 0, highLimit = 0, lowLimit = 0, condit = 0, sumTotalAccept = 0,
                    Rhsum = 0, Rlsum = 0, sum = 0, extract=0, tmp=0;
                double sumh = 0, suml = 0, HighLossProbability = 0, LowLossProbability = 0;

                randhigh = new Random();
                randlow = new Random();


                for (int i = 0; i <= numberOfSources; i++)
                {
                    HighCell.Add(i, GenHigh(max_rateh));
                    LowCell.Add(i, GenLow(max_ratel, (i-(i+2))));

                    sumh = sumh + (int)HighCell[i];
                    suml = suml + (int)LowCell[i];
                    sum = (int)(sumh + suml);
                }

                outList.Items[ct].SubItems.Add(callServiceRate.ToString());
                outList.Items[ct].SubItems.Add(sumh.ToString());
                outList.Items[ct].SubItems.Add(suml.ToString());


                foreach (DictionaryEntry val in HighCell)
                {
                    Ahsum += int.Parse(val.Value.ToString());

                    if (Ahsum > cellCapacity)
                    {
                        highLimit = int.Parse(val.Key.ToString()); // max. cells that can be accomodated
                        condit = 1; //i.e no. of cells in hand-off > buffer capacity
                        Rlsum = (int)suml;
                        break;
                    }
                }

                sumTotalAccept = Ahsum;

                if (condit != 1)
                {
                    foreach (DictionaryEntry val in LowCell)
                    {
                        Alsum += int.Parse(val.Value.ToString());

                        if ((Ahsum+Alsum) > cellCapacity)
                        {
                            sumTotalAccept += Alsum;
                            lowLimit = int.Parse(val.Key.ToString());
                            condit = 2;
                            break;
                        }
                    }
                }

                if (condit == 1)
                {
                    Rhsum = sumTotalAccept - cellCapacity;

                    Ahsum = Ahsum - Rhsum;
                    sumTotalAccept = Ahsum;

                    foreach (DictionaryEntry val in HighCell)
                    {
                        int c = int.Parse(val.Key.ToString());
                        if (c > highLimit)
                        {
                            Rhsum += int.Parse(val.Value.ToString());
                        }
                    }

                    if (Rhsum <= maxBuffer)
                    {
                        bufcon = Rhsum;
                        Rhsum = 0;
                    }
                    else if (Rhsum > maxBuffer)
                    {
                        Rhsum = Rhsum - maxBuffer;
                        bufcon = maxBuffer;
                    }

                    do{
                    extract = extractCall(highLimit/6);
                    }while (extract < 1);

                    foreach (DictionaryEntry val in HighCell)
                    {
                        int c = int.Parse(val.Key.ToString());
                        if (c < extract)
                        {
                            int z = int.Parse(val.Value.ToString());
                            Rhsum += z;
                            tmp += z;
                            if (c > extract)
                                break;
                        }
                    }

                    Ahsum = Ahsum - tmp;

                    Alsum = Alsum + tmp;                    
                    Rlsum = Rlsum - tmp;

                    if (Alsum >= callServiceRate)
                    {
                        tmp = Alsum % callServiceRate;

                        Ahsum -= tmp;
                        Rhsum += tmp;

                        Alsum += tmp;
                        Rlsum -= tmp;
                    }
                }

                
                if (condit == 2)
                {
                    Rlsum = sumTotalAccept - cellCapacity;

                    Alsum = Alsum - Rlsum;

                    foreach (DictionaryEntry val in LowCell)
                    {
                        int c = int.Parse(val.Key.ToString());
                        if (c > lowLimit)
                        {
                            Rlsum += int.Parse(val.Value.ToString());
                        }
                    }
                }

                sumTotalAccept = Ahsum + Alsum;

                outList.Items[ct].SubItems.Add(sum.ToString());

                outList.Items[ct].SubItems.Add(bufcon.ToString());
                outList.Items[ct].SubItems.Add(sumTotalAccept.ToString());
                outList.Items[ct].SubItems.Add(Rhsum.ToString());
                outList.Items[ct].SubItems.Add(Rlsum.ToString());

                
                //LowLossProbability = Rlsum / (sumh+suml);
                //HighLossProbability = Rhsum / (sumh+suml);

                //LowLossProbability = (Rlsum / suml) - (1 / suml);
                //HighLossProbability = (Rhsum / sumh) - (1 / sumh);

                LowLossProbability = Rlsum / suml;
                HighLossProbability = Rhsum / sumh;


                HighLossProbability = Math.Round(HighLossProbability, 7);
                LowLossProbability = Math.Round(LowLossProbability, 7);

                Hpoints[t] = HighLossProbability;
                Lpoints[t] = LowLossProbability;

                outList.Items[ct].SubItems.Add(HighLossProbability.ToString());
                outList.Items[ct].SubItems.Add(LowLossProbability.ToString());

                ct++;
            }

            start = true;
            this.Refresh();
        }


        //Draw Graph
        void Form1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (start)
            {
                SolidBrush Gbrush = new SolidBrush(Color.Black);
                Pen Gpen = new Pen(Gbrush);
                Gpen.Width = 1;

                SolidBrush Hbrush = new SolidBrush(Color.Red);
                Pen Hpen = new Pen(Hbrush);
                Hpen.Width = 1.1F;

                SolidBrush Lbrush = new SolidBrush(Color.Blue);
                Pen Lpen = new Pen(Lbrush);
                Lpen.Width = 1.1F;

                Font f = new Font("Arial", 4.5F);
                PointF ptf = new PointF();

                Graphics g = e.Graphics;


                Point[] Hpts = new Point[Hpoints.Length];
                Point[] Lpts = new Point[Lpoints.Length];

                for (int i = 0; i <= Hpts.Length - 1; i++)
                {
                    Hpts[i].X = i * 10;
                    Hpts[i].Y = (this.Height - 100) - (int)Math.Round(Hpoints[i] * Hmult, 5);
                }

                for (int i = 0; i <= Lpts.Length - 1; i++)
                {
                    Lpts[i].X = i * 10;
                    Lpts[i].Y = (this.Height - 100) - (int)Math.Round(Lpoints[i] * Lmult, 5);
                }


                g.TranslateTransform(this.Width - 540, this.Height - 1220);

                //draw horizontal graph points
                for (int i = 1; i <= Lpts.Length - 1; i++)
                {
                    g.FillEllipse(Gbrush, Lpts[i].X, this.Height - 100, 2, 2);

                    ptf.X = Lpts[i].X-1;
                    ptf.Y = (this.Height-100) + 5;
                    g.DrawString(i.ToString(), f, Gbrush, ptf);
                }

                //draw vertical graph points
                for (float i = 1; i <= 15; i++)
                {
                    g.FillEllipse(Gbrush, -3, (this.Height-100)-(i*10), 2, 2);

                    ptf.X = -18;
                    ptf.Y = (this.Height - 101) - (i * 10);
                    if (i < 10)
                        g.DrawString("0." + i.ToString(), f, Gbrush, ptf);
                    else if (i == 10)
                        g.DrawString("1.0", f, Gbrush, ptf);
                    else
                        g.DrawString(Math.Round((i/10),2).ToString(), f, Gbrush, ptf);
                }


                //draw loss probability points for High priority
                for (int i = 1; i <= Hpts.Length - 1; i++)
                {
                    g.FillEllipse(Hbrush, Hpts[i].X, Hpts[i].Y, 2, 2);
                }

                //draw loss probability points for Low priority
                for (int i = 1; i <= Lpts.Length - 1; i++)
                {
                    g.FillEllipse(Lbrush, Lpts[i].X, Lpts[i].Y, 2, 2);
                }

                //draw loss probability curve for High priority
                g.DrawCurve(Hpen, Hpts, 0.5F);
                //draw loss probability curve for Low priority
                g.DrawCurve(Lpen, Lpts, 0.5F);

                Gbrush.Dispose();
                Hbrush.Dispose();
                Lbrush.Dispose();
                Gpen.Dispose();
                Hpen.Dispose();
                Lpen.Dispose();
                f.Dispose();
            }
        }
    }
}
