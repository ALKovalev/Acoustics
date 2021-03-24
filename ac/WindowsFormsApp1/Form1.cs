using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
//MessageBox.Show(Convert.ToString(constDN));

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        const float R = 20f;
        const float OFFSET = 5f;
        double DNA; //расчёты
        float DNA1; //отрисовка

        public Form1()
        {
            InitializeComponent();
            this.trackBar1.Scroll += new EventHandler(trackBar1_Scroll);
            pictureBox1.Paint += new PaintEventHandler(pictureBox1_Paint);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            trackBar1.Minimum = -90;
            trackBar1.Maximum = 90;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            rtbMain.Clear();
            pictureBox1.Refresh();
            if (tbDNW.Text == String.Empty) { MessageBox.Show("Заполните поле:'Ширина ДН'"); return; }
            if (tbModV.Text == String.Empty) { MessageBox.Show("Заполните поле:'Модуль коэффициента отражения'"); return; }
            if (tbFazaV.Text == String.Empty) { MessageBox.Show("Заполните поле:'Фаза коэффициента отражения'"); return; }
            if (tbZ0.Text == String.Empty) { MessageBox.Show("Заполните поле:'Глубина источника'"); return; }
            if (tbZ.Text == String.Empty) { MessageBox.Show("Заполните поле:'Глубина приёмника'"); return; }
            if (tbR.Text == String.Empty) { MessageBox.Show("Заполните поле:'Расстояние между приёмником и излучателем'"); return; }
            if (tbLamda.Text == String.Empty) { MessageBox.Show("Заполните поле:'Длина волны'"); return; }

            DNA = (double)(trackBar1.Value);
            double ModV = double.Parse(tbModV.Text);
            if (ModV > 1) { MessageBox.Show("Модуль коэффициента отражения не может быть больше единицы"); return; }
            double FazaV = double.Parse(tbFazaV.Text);
            if (FazaV > 180 || FazaV < -180) { MessageBox.Show("Поле:'Фаза коэффициента отражения' некоректно заполнено"); return; }
            FazaV = (double)(FazaV * Math.PI / 180.0);
            double Lamda = double.Parse(tbLamda.Text);
            if (Lamda == 0) { MessageBox.Show("Поле:'Длина волны' некоректно заполнено"); return; }
            double DNW = double.Parse(tbDNW.Text);
            double Z0 = double.Parse(tbZ0.Text);
            double Z = double.Parse(tbZ.Text);
            double R = double.Parse(tbR.Text);
            double b1 = 0;
            double b2 = 0;
            double P = 0;
            
            double R1 = Math.Sqrt(Math.Pow(R, 2) + Math.Pow((Z - Z0), 2));
            double R2 = Math.Sqrt(Math.Pow(R, 2) + Math.Pow((Z + Z0), 2));
            double R0 = Math.Sqrt(Math.Pow(R, 2) + Math.Pow(Z, 2));
            double Teta0 = Math.Acos(Z / R0); //радианы
            double constant = (double)(2 * Math.PI / Lamda);
            double WidthDN = (DNW * Math.PI) / 180;
            if (DNW > 180 || DNW <= 0) { MessageBox.Show("Поле:'Ширина ДН' некоректно заполненно"); return; }

            b1 = DnANGLE1(DNW, WidthDN, Lamda, Z0, Z, R1, b1, FazaV);
            b2 = DnANGLE2(DNW, WidthDN, Lamda, Z0, Z, R2, b2, FazaV);

            P = Math.Sqrt(Math.Pow(b1, 2)/Math.Pow(R1,2) + ModV * Math.Pow(b2, 2) / Math.Pow(R2, 2) + 2*ModV*b1 *b2*Math.Cos(constant * (R1-R2) - FazaV)/(R1*R2));
            rtbMain.AppendText("Давление звука в точке размещения приёмника: " + P + ";\n");
            graphDN_Paint(WidthDN, DNW, Z0, Lamda);
            graph1_Paint(Z, Z0, ModV, FazaV, R1, R2, b1, b2, WidthDN, DNW, constant, Lamda);
            graph2_Paint(Z, Z0, ModV, FazaV, R1, R2, b1, b2, WidthDN, DNW, constant, Lamda);

        }

        public double DnANGLE1(double DNW, double WidthDN, double Lamda, double Z0, double Z, double R1, double b1, double FazaV)
        {
            if (Z > Z0)
            {
                double Teta1 = Math.Acos((Z - Z0) / R1); //радианы
                double Alpha1 = 90 - DNA; //градусы
                double Alpha2 = 90 + DNA;

                if (Alpha1 <= Alpha2)
                {
                    double Alpha1s = Math.Abs(Alpha1 * Math.PI / 180 - Teta1); //радианы

                    if (Alpha1s == 0)
                    {
                        b1 = 1;
                    }
                    else
                    {
                        if (Alpha1s < (WidthDN / 2))
                        {
                            b1 = DnFUNC(DNW, Lamda, Z0, Alpha1s, FazaV);
                        }
                        else
                        {
                            b1 = 0;
                        }
                    }
                }

                if (Alpha1 > Alpha2)
                {
                    double Alpha1s = Alpha1 * Math.PI / 180 - Teta1; //радианы

                    if (Alpha1s == 0)
                    {
                        b1 = 1;
                    }
                    else
                    {
                        if (Alpha1s < (WidthDN / 2))
                        {
                            b1 = DnFUNC(DNW, Lamda, Z0, Alpha1s, FazaV);
                        }
                        else
                        {
                            b1 = 0;
                        }
                    }
                }
            }

            if (Z < Z0)
            {
                double Teta1 = Math.PI - Math.Acos((Z0 - Z) / R1); //радианы
                double Alpha1 = 90 - DNA; //градусы
                double Alpha2 = 90 + DNA;

                if (Alpha1 <= Alpha2)
                {
                    double Alpha1s = Teta1 - Alpha1 * Math.PI / 180; //радианы

                    if (Alpha1s == 0)
                    {
                        b1 = 1;
                    }
                    else
                    {
                        if (Alpha1s < (WidthDN / 2))
                        {
                            b1 = DnFUNC(DNW, Lamda, Z0, Alpha1s, FazaV);
                        }
                        else
                        {
                            b1 = 0;
                        }
                    }
                }

                if (Alpha1 > Alpha2)
                {
                    double Alpha1s = Math.Abs(Alpha1 * Math.PI / 180 - Teta1); //радианы

                    if (Alpha1s == 0)
                    {
                        b1 = 1;
                    }
                    else
                    {
                        if (Alpha1s < (WidthDN / 2))
                        {
                            b1 = DnFUNC(DNW, Lamda, Z0, Alpha1s, FazaV);
                        }
                        else
                        {
                            b1 = 0;
                        }
                    }
                }
            }


            if (Z == Z0)
            {
                double Teta1 = Math.PI / 2; //радианы
                double Alpha1 = 90 - DNA; //градусы
                double Alpha2 = 90 + DNA;

                if (Alpha1 <= Alpha2)
                {
                    double Alpha1s = Teta1 - Alpha1 * Math.PI / 180; //радианы

                    if (Alpha1s == 0)
                    {
                        b1 = 1;
                    }
                    else
                    {
                        if (Alpha1s < (WidthDN / 2))
                        {
                            b1 = DnFUNC(DNW, Lamda, Z0, Alpha1s, FazaV);
                        }
                        else
                        {
                            b1 = 0;
                        }
                    }
                }

                if (Alpha1 > Alpha2)
                {
                    double Alpha1s = Math.Abs(Alpha1 * Math.PI / 180 - Teta1); //радианы

                    if (Alpha1s == 0)
                    {
                        b1 = 1;
                    }
                    else
                    {
                        if (Alpha1s < (WidthDN / 2))
                        {
                            b1 = DnFUNC(DNW, Lamda, Z0, Alpha1s, FazaV);
                        }
                        else
                        {
                            b1 = 0;
                        }
                    }
                }
            }

            return b1;
        }


        public double DnANGLE2(double DNW, double WidthDN, double Lamda, double Z0, double Z, double R2, double b2, double FazaV)
        {

            if (Z > Z0)
            {
                double Teta2 = Math.Acos((Z + Z0) / R2);
                double Alpha1 = 90 - DNA; //градусы
                double Alpha2 = 90 + DNA;

                if (Alpha1 <= Alpha2)
                {
                    double Alpha2s = Alpha2 * Math.PI / 180 - Teta2;

                     if (Alpha2s == 0)
                    {
                        b2 = 1;
                    }
                    else
                    {
                        if (Alpha2s < (WidthDN / 2))
                        {
                            b2 = DnFUNC(DNW, Lamda, Z0, Alpha2s, FazaV);
                        }
                        else
                        {
                            b2 = 0;
                        }
                    }
                }

                if (Alpha1 > Alpha2)
                {
                    double Alpha2s = Math.Abs(Alpha2 * Math.PI / 180 - Teta2);

                    if (Alpha2s == 0)
                    {
                        b2 = 1;
                    }
                    else
                    {
                        if (Alpha2s < (WidthDN / 2))
                        {
                            b2 = DnFUNC(DNW, Lamda, Z0, Alpha2s, FazaV);
                        }
                        else
                        {
                            b2 = 0;
                        }
                    }
                }
            }

            if (Z < Z0)
            {
                double Teta2 = Math.Acos((Z + Z0) / R2);
                double Alpha1 = 90 - DNA; //градусы
                double Alpha2 = 90 + DNA;

                if (Alpha1 <= Alpha2)
                {
                    double Alpha2s = Alpha2 * Math.PI / 180 - Teta2;

                    if (Alpha2s == 0)
                    {
                        b2 = 1;
                    }
                    else
                    {
                        if (Alpha2s < (WidthDN / 2))
                        {
                            b2 = DnFUNC(DNW, Lamda, Z0, Alpha2s, FazaV);
                        }
                        else
                        {
                            b2 = 0;
                        }
                    }
                }

                if (Alpha1 > Alpha2)
                {
                   double Alpha2s = Math.Abs(Alpha2 * Math.PI / 180 - Teta2);

                   if (Alpha2s == 0)
                    {
                        b2 = 1;
                    }
                    else
                    {
                        if (Alpha2s < (WidthDN / 2))
                        {
                            b2 = DnFUNC(DNW, Lamda, Z0, Alpha2s, FazaV);
                        }
                        else
                        {
                            b2 = 0;
                        }
                    }
                }
            }


            if (Z == Z0)
            {
                double Teta2 = Math.Acos((Z + Z0) / R2);
                double Alpha1 = 90 - DNA; //градусы
                double Alpha2 = 90 + DNA;

                if (Alpha1 <= Alpha2)
                {
                   double Alpha2s = Alpha2 * Math.PI / 180 - Teta2;

                    if (Alpha2s == 0)
                    {
                        b2 = 1;
                    }
                    else
                    {
                        if (Alpha2s < (WidthDN / 2) && Alpha2s != 0)
                        {
                            b2 = DnFUNC(DNW, Lamda, Z0, Alpha2s, FazaV);
                        }
                        else
                        {
                            b2 = 0;
                        }
                    }
                }

                if (Alpha1 > Alpha2)
                {
                   double Alpha2s = Math.Abs(Alpha2 * Math.PI / 180 - Teta2);

                    if (Alpha2s == 0)
                    {
                        b2 = 1;
                    }
                    else
                    {
                        if (Alpha2s < (WidthDN / 2))
                        {
                            b2 = DnFUNC(DNW, Lamda, Z0, Alpha2s, FazaV);
                        }
                        else
                        {
                            b2 = 0;
                        }
                    }
                }
            }
            return b2;
        }
        

        public double DnFUNC(double DNW, double Lamda, double Z0, double x, double FazaV)
        {
            double b;
            double otn = Lamda / (2 * Z0);
            if (Lamda / Z0 > 2)
            {
                if (FazaV == 0)
                {
                    b = Math.Abs(Math.Cos((2 * 180 / DNW) * x / 2 + Math.PI / 2));
                }
                else
                {
                    b = Math.Sin((2 * 180 / DNW) * x) / ((2 * 180 / DNW) * x);
                }
                
            }
            else
            {
                if (FazaV == 0)
                {
                    b = Math.Abs(Math.Cos((2 * 180 / DNW) * x / otn));
                }
                else
                {
                    b = Math.Abs(Math.Sin((2 * 180 / DNW) * x / otn));
                }
            }

            return b;
        }





        public void graphDN_Paint(double WidthDN, double DNW, double Z0, double Lamda)
        {
            double Xmin = -WidthDN / 2;
            double Xmax = WidthDN / 2;
            double Step = 0.001;

            int count = (int)Math.Ceiling((Xmax - Xmin) / Step) + 1;// Количество точек графика
            double[] x = new double[count];
            double[] y = new double[count];
            for (int i = 0; i < count; i++)
            {
                x[i] = Xmin + Step * i;
                y[i] = Math.Sin((2 * 180 / DNW) * x[i]) / ((2 * 180 / DNW) * x[i]);

            }
            // Настраиваем оси графика
            chart1.ChartAreas[0].AxisX.Minimum = Xmin;
            chart1.ChartAreas[0].AxisX.Maximum = Xmax;
            chart1.Series[0].BorderWidth = 2;
            // Добавляем вычисленные значения в графики
            chart1.Series[0].Points.DataBindXY(x, y);
        }



        /////P(Z=const,R)
        public void graph1_Paint(double Z, double Z0, double ModV, double FazaV, double R1, double R2, double b1, double b2, double WidthDN, double DNW, double constant, double Lamda)
        {
            double Step = 0.1;
            int count = (int)Math.Ceiling((R + 200) / Step) + 1;// Количество точек графика
            int accuracy = 6;
            double y = 0;
            double[] x1 = new double[count];
            double[] y1 = new double[count];
            for (int i = 0; i < count; i++)
            {
                x1[i] = 0.1 + Step * i;
                R1 = Math.Sqrt(Math.Pow(x1[i], 2) + Math.Pow((Z - Z0), 2));
                R2 = Math.Sqrt(Math.Pow(x1[i], 2) + Math.Pow((Z + Z0), 2));
                b1 = DnANGLE1(DNW, WidthDN, Lamda, Z0, Z, R1, b1, FazaV);
                b2 = DnANGLE2(DNW, WidthDN, Lamda, Z0, Z, R2, b2, FazaV);
                y = Math.Sqrt(Math.Pow(b1, 2) / Math.Pow(R1, 2) + ModV * Math.Pow(b2, 2) / Math.Pow(R2, 2) + 2 * ModV * b1 * b2 * Math.Cos(constant * (R1 - R2) - FazaV) / (R1 * R2));
                y1[i] = Math.Round(y - y % (double)(Math.Pow(0.1, (double)accuracy)), accuracy);


            }
            // Настраиваем оси графика
            chart2.ChartAreas[0].AxisX.Minimum = 0;
            chart2.ChartAreas[0].AxisX.Maximum = R + 200;
            chart2.Series[0].BorderWidth = 2;
            // Добавляем вычисленные значения в графики
            chart2.Series[0].Points.DataBindXY(x1, y1);
        }




        /////P(Z,R=const)
        public void graph2_Paint(double Z, double Z0, double ModV, double FazaV, double R1, double R2, double b1, double b2, double WidthDN, double DNW, double constant, double Lamda)
        {
            double Step = 0.1;
            int count = (int)Math.Ceiling((Z + 200) / Step) + 1;// Количество точек графика
            int accuracy = 6;
            double y = 0;
            double[] x2 = new double[count];
            double[] y2 = new double[count];

            for (int i = 0; i < count; i++)
            {
                x2[i] = Step * i;
                R1 = Math.Sqrt(Math.Pow(R, 2) + Math.Pow((x2[i] - Z0), 2));
                R2 = Math.Sqrt(Math.Pow(R, 2) + Math.Pow((x2[i] + Z0), 2));
                Z = x2[i];
                b1 = DnANGLE1(DNW, WidthDN, Lamda, Z0, Z, R1, b1, ModV);
                b2 = DnANGLE2(DNW, WidthDN, Lamda, Z0, Z, R2, b2, ModV);
                y = Math.Sqrt(Math.Pow(b1, 2) / Math.Pow(R1, 2) + ModV * Math.Pow(b2, 2) / Math.Pow(R2, 2) + 2 * ModV * b1 * b2 * Math.Cos(constant * (R1 - R2) - FazaV) / (R1 * R2));
                y2[i] = Math.Round(y - y % (double)(Math.Pow(0.1, (double)accuracy)), accuracy);
            }
            // Настраиваем оси графика
            chart3.ChartAreas[0].AxisX.Minimum = 0;
            chart3.ChartAreas[0].AxisX.Maximum = Z;
            chart3.Series[0].BorderWidth = 2;
            // Добавляем вычисленные значения в графики
            chart3.Series[0].Points.DataBindXY(x2, y2);
        }




        void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            int Z0;
            int Z;
            int r;

            if (tbZ0.Text == String.Empty)
            { Z0 = 0; }
            else
            { Z0 = (int)Math.Ceiling(double.Parse(tbZ0.Text)); }

            if (tbZ.Text == String.Empty)
            { Z = 0; }
            else
            { Z = (int)Math.Ceiling(double.Parse(tbZ.Text)); }
            
            if (tbR.Text == String.Empty)
            { r = 20; }
            else
            { r = 20 + (int)Math.Ceiling(double.Parse(tbR.Text)); }

            Pen think_pen = new Pen(Brushes.Black, 1);
            SolidBrush circles = new SolidBrush(Color.Black);
            SolidBrush circles1 = new SolidBrush(Color.BlueViolet);
            int bHeight = 200;
            int bWidth = 400;
            think_pen.EndCap = LineCap.ArrowAnchor;
            g.DrawLine(think_pen, 20, (bHeight / 2) + 3, bWidth - 20, (bHeight / 2) + 3); //r
            g.DrawLine(think_pen, 20, 20, 20, bHeight - 20); //z
            Font f = new Font("Arial", 10, FontStyle.Bold);
            StringFormat sfl = new StringFormat(StringFormatFlags.NoClip);
            sfl.Alignment = StringAlignment.Near;
            // Подписи для координатных осей
            g.DrawString("r", f, Brushes.Black, new RectangleF(370, bHeight / 2, 380, bHeight / 2), sfl);
            g.DrawString("z", f, Brushes.Black, new RectangleF(5, bHeight - 30, 5, bHeight - 20), sfl);
            //Рисуем наши окружности
            g.FillEllipse(circles, new Rectangle(new Point(20, (bHeight / 2) + Z0), new Size(7, 7))); //re
            g.FillEllipse(circles, new Rectangle(new Point(20, (bHeight / 2) - Z0), new Size(7, 7))); //ym
            g.FillEllipse(circles1, new Rectangle(new Point(r, (bHeight / 2) + Z), new Size(7, 7))); //приёмник

            var x = R + 50f * (float)Math.Cos(DNA1);
            var y = (float)(bHeight / 2) + 3 + Z0 + 50f * (float)Math.Sin(DNA1);
            g.DrawLine(Pens.Black, R, (float)(bHeight / 2) + 3 + Z0, x, y);

            x = R + 50f * (float)Math.Cos(-DNA1);
            y = (float)(bHeight / 2) + 3 - Z0 + 50f * (float)Math.Sin(-DNA1);
            g.DrawLine(Pens.Black, R, (float)(bHeight / 2) + 3 - Z0, x, y);
        }

        private void tbDNW_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && number != 8 && number != 44) //цифры, клавиша BackSpace и запятая
            {
                e.Handled = true;
            }
        }

        private void tbModV_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && number != 8 && number != 44) //цифры, клавиша BackSpace и запятая
            {
                e.Handled = true;
            }
        }
        
        private void tbFazaV_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && number != 8 && number != 44 && number != 45) //цифры, клавиша BackSpace, запятая и минус
            {
                e.Handled = true;
            }
        }

        private void tbZ0_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && number != 8 && number != 44) //цифры, клавиша BackSpace и запятая
            {
                e.Handled = true;
            }
        }

        private void tbZ_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && number != 8 && number != 44) //цифры, клавиша BackSpace и запятая
            {
                e.Handled = true;
            }
        }

        private void tbR_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && number != 8 && number != 44) //цифры, клавиша BackSpace и запятая
            {
                e.Handled = true;
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(trackBar1, trackBar1.Value.ToString());
            DNA1 = (float)(trackBar1.Value * Math.PI / 180.0);
            pictureBox1.Invalidate();
        }

        private void tbLamda_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if ((e.KeyChar <= 47 || e.KeyChar >= 58) && number != 8 && number != 44) //цифры, клавиша BackSpace и запятая
            {
                e.Handled = true;
            }
        }
    }
}



/*
       
    
    public void graphDN_Paint(double WidthDN, double DNW, double Z0, double Lamda, double FazaV)
        {
            double Xmin = -WidthDN / 2;
            double Xmax = WidthDN / 2;
            double Step = 0.001;


            int count = (int)Math.Ceiling((Xmax - Xmin) / Step) + 1;// Количество точек графика
            double[] x = new double[count];
            double[] y = new double[count];
            for (int i = 0; i < count; i++)
            {
                x[i] = Xmin + Step * i;
                y[i] = DnFUNC(DNW, Lamda, Z0, x[i], FazaV);

            }
            // Настраиваем оси графика
            chart1.ChartAreas[0].AxisX.Minimum = Xmin;
            chart1.ChartAreas[0].AxisX.Maximum = Xmax;
            chart1.Series[0].BorderWidth = 2;
            // Добавляем вычисленные значения в графики
            chart1.Series[0].Points.DataBindXY(x, y);
        }

    

    
    

    public double DnFUNC(double DNW, double Lamda, double Z0, double x)
        {
            double b;
            double otn =  Lamda / (2*Z0);
            if (Lamda / Z0 >= 2)
            {
                b = Math.Sin((2 * 180 / DNW) * x) / ((2 * 180 / DNW) * x);
            }
            else
            {
                b = Math.Abs(Math.Sin((2 * 180 / DNW) * x / otn) / ((2 * 180 / DNW) * x / otn));
            }

            return b;
        }
    


     


        /////P(Z=const,R)
        public void graph1_Paint(double Z, double Z0, double ModV, double FazaV, double R1, double R2, double b1, double b2, double WidthDN, double DNW, double constant, double Lamda)
        {
            double Step = 0.1;
            int count = (int)Math.Ceiling((R + 200) / Step) + 1;// Количество точек графика
            
            if (Z == 0 && ModV == 1 && Math.Abs(FazaV) == Math.PI)
            {
                double[] x1 = new double[count];
                double[] y1 = new double[count];
                for (int i = 0; i < count; i++)
                {
                    x1[i] = 0.1 + Step * i;
                    y1[i] = 0;
                }
                // Настраиваем оси графика
                chart2.ChartAreas[0].AxisX.Minimum = 0;
                chart2.ChartAreas[0].AxisX.Maximum = R + 200;
                chart2.Series[0].BorderWidth = 2;
                // Добавляем вычисленные значения в графики
                chart2.Series[0].Points.DataBindXY(x1, y1);
            }
            else
            {
                double[] x1 = new double[count];
                double[] y1 = new double[count];
                for (int i = 0; i < count; i++)
                {
                    x1[i] = 0.1 + Step * i;
                    R1 = Math.Sqrt(Math.Pow(x1[i], 2) + Math.Pow((Z - Z0), 2));
                    R2 = Math.Sqrt(Math.Pow(x1[i], 2) + Math.Pow((Z + Z0), 2));
                    b1 = DnANGLE1(DNW, WidthDN, Lamda, Z0, Z, R1, b1);
                    b2 = DnANGLE2(DNW, WidthDN, Lamda, Z0, Z, R2, b2);
                    y1[i] = Math.Sqrt(Math.Pow(b1, 2) / Math.Pow(R1, 2) + ModV * Math.Pow(b2, 2) / Math.Pow(R2, 2) + 2 * ModV * b1 * b2 * Math.Cos(constant * (R1 - R2) - FazaV) / (R1 * R2));

                }
                // Настраиваем оси графика
                chart2.ChartAreas[0].AxisX.Minimum = 0;
                chart2.ChartAreas[0].AxisX.Maximum = R + 200;
                chart2.Series[0].BorderWidth = 2;
                // Добавляем вычисленные значения в графики
                chart2.Series[0].Points.DataBindXY(x1, y1);
            }
        }
    
     */
