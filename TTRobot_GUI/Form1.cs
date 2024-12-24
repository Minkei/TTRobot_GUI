using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Ports;
using System.Net;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace TTRobot_GUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            cbCOM.Text = "COM9";
            cbBR.Text = "115200";
            cbsp1.Text = "0";
            cbsp2.Text = "0";
            cbsp3.Text = "0";
        }
        double Xsp, Ysp, Zsp;
        double Xreal, Yreal, Zreal;
        double L2 = 100, L3 = 150, L4 = 60;
        DateTime timenow = DateTime.Now;
        String RXdataFull = "0a0b0c2x2y2z";
        Int32 pulse1 = 0, pulse2 = 0, pulse3 = 0;
        //Int32 offsetpulse1 = 0, offsetpulse2 = 0, offsetpulse3 = 0;
        double offsetangle1 = 0, offsetangle2 = 0, offsetangle3 = 0;
        String TXdataFull = "0a0b0c0s";
        double realtime = 0, step = 0.04;

        double e1 = 0, e2 = 0, e3 = 0;
        double sp1 = 0, sp2 = 0, sp3 = 0;
        double angle1 = 0, angle2 = 0, angle3 = 0;
        double T1IK = 0, T2IK = 0, T3IK = 0;
        double IKX, IKY, IKZ;
        double reloadchart = 1;
        double time_update_chart = 0;
        uint time_tp = 0;
        double erX, erY, erZ;
        //double cbspX = 0, cbspY = 0, cbspZ = 0;
        int home1 = 2, home2 = 2, home3 = 2;
        int dem1 = 0, dem2 = 0, dem3 = 0;
        bool flag_endhome = false;
        bool flag_reset = false;

        double sp1_display = 0, sp2_display = 0, sp3_display = 0;




        private void Form1_Load(object sender, EventArgs e)
        {
            setupchart_angle();
            setupchartErrorOfAngle();
            setupchartPosition();
            setupchartERPosition();
        }
        double QD1_a, QD1_b, QD1_c, QD1_d, QD1_qf, QD1_q0, QD1_tf, QD1_t0;

        private void guna2ToggleSwitch1_CheckedChanged(object sender, EventArgs e)
        {
            if(guna2ToggleSwitch1.Checked == true) time_tp = 1; else time_tp = 0;
            //TXdataFull = Convert.ToString(sp1) + "a" + Convert.ToString(sp2) + "b" + Convert.ToString(sp3) + "c" + Convert.ToString(time_tp) + "s";
            //serialPort1.Write(TXdataFull);
        }

        private void btRun2IK_Click(object sender, EventArgs e)
        {
            cbsp1.Text = Convert.ToString(Math.Round(T1IK,2));
            cbsp2.Text = Convert.ToString(Math.Round(T2IK - 30,2));
            cbsp3.Text = Convert.ToString(Math.Round(T3IK + 75,2));
        }

        private void btCalculateIK_Click(object sender, EventArgs e)
        {
            IKX = Convert.ToDouble(tbIKX.Text);
            IKY = Convert.ToDouble(tbIKY.Text);
            IKZ = Convert.ToDouble(tbIKZ.Text);

            InverseKinematic();

            textBox4.Text = Convert.ToString(T1IK);
            textBox5.Text = Convert.ToString(T2IK);
            textBox6.Text = Convert.ToString(T3IK);
        }

        double QD2_a, QD2_b, QD2_c, QD2_d, QD2_qf, QD2_q0, QD2_tf, QD2_t0;



        //private void btReset_Click(object sender, EventArgs e)
        //{
        //    flag_reset = true;

        //    cvtData();

        //    offsetangle1 = angle1;
        //    offsetangle2 = angle2;
        //    offsetangle3 = angle3;

        //    sp1_display = sp1 - offsetangle1 + 0;
        //    sp2_display = sp2 - offsetangle2 + 30;
        //    sp3_display = sp3 - offsetangle3 - 155;

        //    cbsp1.Text = Convert.ToString(sp1_display);
        //    cbsp2.Text = Convert.ToString(sp2_display);
        //    cbsp3.Text = Convert.ToString(sp3_display);
        //}
        int r11 = 0, r21 = 0, r31 = -1;


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            serialPort1.Close();
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            timer2.Enabled = true;
            flag_reset = false;
        }


        private void timer2_Tick(object sender, EventArgs e)
        {
            cvtData();
            if (home1 == 2) sp1 += 1;
            if (home2 == 2) sp2 -= 1;
            if(home1 == 1 && home2 == 1)
            {
                timer2.Enabled = false;
                timer3.Enabled = true;
            }
            TXdataFull = Convert.ToString(sp1) + "a" + Convert.ToString(sp2) + "b" + Convert.ToString(sp3) + "c" + Convert.ToString(time_tp) + "s";
            serialPort1.Write(TXdataFull);

        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            cvtData();
            dem1 += 1;
            if (dem1 < 80)
            {
                sp1 -= 1;
            }
            else
            {
                dem2 += 1;
                if(dem2<10)
                {
                    sp2 += 1;
                }
                else
                {
                    if (home3 == 2)
                    {
                        sp3 -= 1;
                    }
                    else
                    {
                        timer3.Enabled = false;
                        timer4.Enabled = true;
                        dem1 = 0;
                        dem2 = 0;
                    } 
                }
            }
            TXdataFull = Convert.ToString(sp1) + "a" + Convert.ToString(sp2) + "b" + Convert.ToString(sp3) + "c" + Convert.ToString(time_tp) + "s";
            serialPort1.Write(TXdataFull);
        }
        private void timer4_Tick(object sender, EventArgs e)
        {
            cvtData();
            dem3 += 1;
            if (dem3 < 70)
            {
                sp3 += 1;
            }
            else
            {
                flag_endhome = true;
                dem3 = 0;
                UIupdate();
                timer4.Enabled = false;
            }
            if (flag_endhome == false)
            {
                TXdataFull = Convert.ToString(sp1) + "a" + Convert.ToString(sp2) + "b" + Convert.ToString(sp3) + "c" + Convert.ToString(time_tp) + "s";
                serialPort1.Write(TXdataFull);
            }

        }
        double QD3_a, QD3_b, QD3_c, QD3_d, QD3_qf, QD3_q0, QD3_tf, QD3_t0;

        private void btRun_Click(object sender, EventArgs e)
        {
            cvtData();

            timer1.Enabled = true;

            flag_endhome = false;

            QD1_qf = Convert.ToDouble(cbsp1.Text);
            QD2_qf = Convert.ToDouble(cbsp2.Text);
            QD3_qf = Convert.ToDouble(cbsp3.Text);


            QD1_q0 = sp1;
            QD2_q0 = sp2;
            QD3_q0 = sp3;

            QD1_t0 = realtime;
            QD1_tf = realtime + Convert.ToDouble(tb_time_tp_cubic.Text);

            QD2_t0 = realtime;
            QD2_tf = realtime + Convert.ToDouble(tb_time_tp_cubic.Text);

            QD3_t0 = realtime;
            QD3_tf = realtime + Convert.ToDouble(tb_time_tp_cubic.Text);

            if (checkBoxStep.CheckState == CheckState.Checked) Calculate_Step_Function();
            else
            if(checkBoxCubic.CheckState == CheckState.Checked) Calculate_Cubic_Function();
            else
            if(checkBoxLine.CheckState == CheckState.Checked) Calculate_Line_Funtion();
        }


        private void setupchart_angle()
        {
            chartAngle.Series[0].Points.AddXY(0, 0);
            chartAngle.Series[1].Points.AddXY(0, 0);
            chartAngle.Series[2].Points.AddXY(0, 0);

            chartAngle.ChartAreas[0].AxisX.Minimum = 0;
            chartAngle.ChartAreas[0].AxisX.Maximum = 5;
            chartAngle.ChartAreas[0].AxisX.Interval = 1;
            chartAngle.ChartAreas[0].AxisY.Minimum = -180;
            chartAngle.ChartAreas[0].AxisY.Maximum = 180;
            chartAngle.ChartAreas[0].AxisY.IntervalOffset = 0;
            chartAngle.ChartAreas[0].AxisY.Interval = 45;
        }
        private void setupchartErrorOfAngle()
        {
            chartERAngle.Series[0].Points.AddXY(0, 0);
            chartERAngle.Series[1].Points.AddXY(0, 0);
            chartERAngle.Series[2].Points.AddXY(0, 0);

            chartERAngle.ChartAreas[0].AxisX.Minimum = 0;
            chartERAngle.ChartAreas[0].AxisX.Maximum = 5;
            chartERAngle.ChartAreas[0].AxisX.Interval = 1;
            chartERAngle.ChartAreas[0].AxisY.Minimum = -10;
            chartERAngle.ChartAreas[0].AxisY.Maximum = 10;
            chartERAngle.ChartAreas[0].AxisY.IntervalOffset = 0;
            chartERAngle.ChartAreas[0].AxisY.Interval = 5;
        }
        private void setupchartPosition()
        {
            chartPosition.Series[0].Points.AddXY(0, 0);
            chartPosition.Series[1].Points.AddXY(0, 0);
            chartPosition.Series[2].Points.AddXY(0, 0);

            chartPosition.ChartAreas[0].AxisX.Minimum = 0;
            chartPosition.ChartAreas[0].AxisX.Maximum = 5;
            chartPosition.ChartAreas[0].AxisX.Interval = 1;
            chartPosition.ChartAreas[0].AxisY.Minimum = -2000;
            chartPosition.ChartAreas[0].AxisY.Maximum = 2000;
            chartPosition.ChartAreas[0].AxisY.IntervalOffset = 0;
            chartPosition.ChartAreas[0].AxisY.Interval = 500;
        }
        private void setupchartERPosition()
        {
            chartERPosition.Series[0].Points.AddXY(0, 0);
            chartERPosition.Series[1].Points.AddXY(0, 0);
            chartERPosition.Series[2].Points.AddXY(0, 0);

            chartERPosition.ChartAreas[0].AxisX.Minimum = 0;
            chartERPosition.ChartAreas[0].AxisX.Maximum = 5;
            chartERPosition.ChartAreas[0].AxisX.Interval = 1;
            chartERPosition.ChartAreas[0].AxisY.Minimum = -500;
            chartERPosition.ChartAreas[0].AxisY.Maximum = 500;
            chartERPosition.ChartAreas[0].AxisY.IntervalOffset = 0;
            chartERPosition.ChartAreas[0].AxisY.Interval = 125;
        }
        


        private void cbCOM_SelectedIndexChanged(object sender, EventArgs e)
        {
            serialPort1.PortName = cbCOM.Text;
            serialPort1.BaudRate = Convert.ToInt32(cbBR.Text);
            serialPort1.DataBits = 8;
        }

        private void cbBR_SelectedIndexChanged(object sender, EventArgs e)
        {
            serialPort1.PortName = cbCOM.Text;
            serialPort1.BaudRate = Convert.ToInt32(cbBR.Text);
            serialPort1.DataBits = 8;
        }

        private void checkBoxStep_Click(object sender, EventArgs e)
        {
            checkBoxCubic.CheckState = CheckState.Unchecked;
            checkBoxLine.CheckState = CheckState.Unchecked;
            checkBoxStep.CheckState = CheckState.Checked;
        }

        private void checkBoxLine_Click(object sender, EventArgs e)
        {
            checkBoxCubic.CheckState = CheckState.Unchecked;
            checkBoxLine.CheckState = CheckState.Checked;
            checkBoxStep.CheckState = CheckState.Unchecked;
        }

        private void checkBoxCubic_Click(object sender, EventArgs e)
        {
            checkBoxCubic.CheckState = CheckState.Checked;
            checkBoxLine.CheckState = CheckState.Unchecked;
            checkBoxStep.CheckState = CheckState.Unchecked;
        }


        private void btConnect_Click(object sender, EventArgs e)
        {
            if(cbCOM.Text != "" && cbBR.Text != "")
            {
                if (serialPort1.IsOpen == true)
                {
                    serialPort1.Close();
                    btConnect.Text = "Connect";
                    //timer1.Enabled = false;
                    label_status.Text = "Arduino is disconnected!";
                }
                else
                {
                    serialPort1.Open();
                    btConnect.Text = "Disconnect";
                    //timer1.Enabled = true;
                    label_status.Text = "Arduino is connected!";
                }
            }
            else
            {
                if (cbCOM.Text == "") MessageBox.Show("The serial port was not yet connected!", "Warnning!");
                if (cbBR.Text == "") MessageBox.Show("The baud rate was not yet set up!", "Warnning!");
            }
            
        }

        private void cbCOM_DropDown(object sender, EventArgs e)
        {
            cbCOM.Items.Clear();
            cbCOM.Items.AddRange(SerialPort.GetPortNames());
        }
        
        private void timer1_Tick(object sender, EventArgs e)
        {
            
            UART_Send();
            Invoke(new MethodInvoker(UIupdate));
            realtime += step; 
        }
        
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            RXdataFull = serialPort1.ReadTo("z");
            RXdataFull += "z";
        }

        private void UIupdate()
        {
            //Convert data has received
            cvtData();
            //Chạy updata UI chart và lables song song nhau
            Invoke(new MethodInvoker(ForwardKinematic));
            Invoke(new MethodInvoker(UIupdate_Lables));
            Invoke(new MethodInvoker(UIupdata_Chart));

            //Cập nhật thời gian thực
            label_timer.Text = Convert.ToString(Math.Round(realtime, 2));

        }

        private void cvtData()
        {
            //Xử lý dữ liệu nhận từ UART
            sbyte chara = Convert.ToSByte(RXdataFull.IndexOf("a"));
            sbyte charb = Convert.ToSByte(RXdataFull.IndexOf("b"));
            sbyte charc = Convert.ToSByte(RXdataFull.IndexOf("c"));
            sbyte charx = Convert.ToSByte(RXdataFull.IndexOf("x"));
            sbyte chary = Convert.ToSByte(RXdataFull.IndexOf("y"));
            sbyte charz = Convert.ToSByte(RXdataFull.IndexOf("z"));

            pulse1 = Convert.ToInt32(RXdataFull.Substring(0, chara));
            pulse2 = Convert.ToInt32(RXdataFull.Substring(chara + 1, charb - chara - 1));
            pulse3 = Convert.ToInt32(RXdataFull.Substring(charb + 1, charc - charb - 1));

            home1 = Convert.ToInt32(RXdataFull.Substring(charc + 1, charx - charc - 1));
            home2 = Convert.ToInt32(RXdataFull.Substring(charx + 1, chary - charx - 1));
            home3 = Convert.ToInt32(RXdataFull.Substring(chary + 1, charz - chary - 1));

            //Cập nhật giá trị các biến sai số và góc
            angle1 = pulse1 * 360 / (11 * 270);
            angle2 = pulse2 * 360 / (11 * 168);
            angle3 = pulse3 * 360 / (11 * 270);

            e1 = sp1 - angle1;
            e2 = sp2 - angle2;
            e3 = sp3 - angle3;
        }

        private void UIupdate_Lables()
        {
            lbspT1.Text = Convert.ToString(Math.Round(sp1, 2));
            lbspT2.Text = Convert.ToString(Math.Round(sp2, 2));
            lbspT3.Text = Convert.ToString(Math.Round(sp3, 2));

            lbrspT1.Text = Convert.ToString(Math.Round(angle1, 2));
            lbrspT2.Text = Convert.ToString(Math.Round(angle2, 2));
            lbrspT3.Text = Convert.ToString(Math.Round(angle3, 2));

            lbertheta1.Text = Convert.ToString(Math.Round(e1,2));
            lbertheta2.Text = Convert.ToString(Math.Round(e2, 2));
            lbertheta3.Text = Convert.ToString(Math.Round(e3, 2));

            lbspX.Text = Convert.ToString(Math.Round(Xsp, 2));
            lbspY.Text = Convert.ToString(Math.Round(Ysp, 2));
            lbspZ.Text = Convert.ToString(Math.Round(Zsp, 2));

            lbrspX.Text = Convert.ToString(Math.Round(Xreal, 2));
            lbrspY.Text = Convert.ToString(Math.Round(Yreal, 2));
            lbrspZ.Text = Convert.ToString(Math.Round(Zreal, 2));

            erX = Math.Round(Xsp - Xreal, 2);
            erY = Math.Round(Ysp - Yreal, 2);
            erZ = Math.Round(Zsp - Zreal, 2);

            lberX.Text = Convert.ToString(erX);
            lberY.Text = Convert.ToString(erY);
            lberZ.Text = Convert.ToString(erZ);

            
        }
        private void UIupdata_Chart()
        {
            time_update_chart += step;
            Invoke(new MethodInvoker(delegate ()
            {
                if (time_update_chart > reloadchart*5)
                {
                    chartAngle.ChartAreas[0].AxisX.Minimum = reloadchart * 5;
                    chartERAngle.ChartAreas[0].AxisX.Minimum = reloadchart * 5;
                    chartPosition.ChartAreas[0].AxisX.Minimum = reloadchart * 5;
                    chartERPosition.ChartAreas[0].AxisX.Minimum = reloadchart * 5;
                    reloadchart += 1;
                    chartAngle.ChartAreas[0].AxisX.Maximum = reloadchart * 5;
                    chartERAngle.ChartAreas[0].AxisX.Maximum = reloadchart * 5;
                    chartPosition.ChartAreas[0].AxisX.Maximum = reloadchart * 5;
                    chartERPosition.ChartAreas[0].AxisX.Maximum = reloadchart * 5;
                }
            }));
            Invoke(new MethodInvoker(delegate()
            {
                if(checkBox1_showT1.CheckState == CheckState.Checked)
                {
                    chartAngle.Series["Theta 1"].Points.AddXY(realtime, angle1);
                    chartAngle.Series["spT1"].Points.AddXY(realtime, sp1);
                    chartERAngle.Series["Error of theta 1"].Points.AddXY(realtime, e1);
                }
                else
                {
                    chartAngle.Series["Theta 1"].Points.Clear();
                    chartAngle.Series["spT1"].Points.Clear();
                    chartERAngle.Series["Error of theta 1"].Points.Clear();
                }

                if(checkBox_showT2.CheckState == CheckState.Checked)
                {
                    chartAngle.Series["Theta 2"].Points.AddXY(realtime, angle2);
                    chartAngle.Series["spT2"].Points.AddXY(realtime, sp2);
                    chartERAngle.Series["Error of theta 2"].Points.AddXY(realtime, e2);
                }
                else
                {
                    chartAngle.Series["Theta 2"].Points.Clear();
                    chartAngle.Series["spT2"].Points.Clear();
                    chartERAngle.Series["Error of theta 2"].Points.Clear();
                }
                if(checkBox_showT3.CheckState == CheckState.Checked)
                {
                    chartAngle.Series["Theta 3"].Points.AddXY(realtime, angle3);
                    chartAngle.Series["spT3"].Points.AddXY(realtime, sp3);
                    chartERAngle.Series["Error of theta 3"].Points.AddXY(realtime, e3);
                }
                else
                {
                    chartAngle.Series["Theta 3"].Points.Clear();
                    chartAngle.Series["spT3"].Points.Clear();
                    chartERAngle.Series["Error of theta 3"].Points.Clear();
                }
            }));
            Invoke(new MethodInvoker(delegate ()
            {
                if(checkBox_showX.CheckState == CheckState.Checked)
                {
                    chartPosition.Series["X"].Points.AddXY(realtime, Xreal);
                    chartPosition.Series["spX"].Points.AddXY(realtime, Xsp);
                    chartERPosition.Series["Error of X"].Points.AddXY(realtime, erX);
                }
                else
                {
                    chartPosition.Series["X"].Points.Clear();
                    chartPosition.Series["spX"].Points.Clear();
                    chartERPosition.Series["Error of X"].Points.Clear();
                }
                if(checkBox_showY.CheckState == CheckState.Checked)
                {
                    chartPosition.Series["Y"].Points.AddXY(realtime, Yreal);
                    chartPosition.Series["spY"].Points.AddXY(realtime, Ysp);
                    chartERPosition.Series["Error of Y"].Points.AddXY(realtime, erY);
                }
                else
                {
                    chartPosition.Series["Y"].Points.Clear();
                    chartPosition.Series["spY"].Points.Clear();
                    chartERPosition.Series["Error of Y"].Points.Clear();
                }
                if(checkBox_showZ.CheckState == CheckState.Checked)
                {
                    chartPosition.Series["Z"].Points.AddXY(realtime, Zreal);
                    chartPosition.Series["spZ"].Points.AddXY(realtime, Zsp); 
                    chartERPosition.Series["Error of Z"].Points.AddXY(realtime, erZ);
                }
                else
                {
                    chartPosition.Series["Z"].Points.Clear();
                    chartPosition.Series["spZ"].Points.Clear();
                    chartERPosition.Series["Error of Z"].Points.Clear();
                }
            }));
        }

        
        private void ForwardKinematic()
        {
            double sp1_radian = sp1 * Math.PI / 180;
            double sp2_radian = sp2 * Math.PI / 180;
            double sp3_radian = sp3 * Math.PI / 180;

            Xsp = Math.Round(Math.Cos(sp1_radian) * (L3 * Math.Cos(sp2_radian + sp3_radian) + L2 * Math.Cos(sp2_radian) + L4 * Math.Cos(-Math.PI / 2)), 2);
            Ysp = Math.Round(Math.Sin(sp1_radian) * (L3 * Math.Cos(sp2_radian + sp3_radian) + L2 * Math.Cos(sp2_radian) + L4 * Math.Cos(-Math.PI / 2)), 2);
            Zsp = Math.Round(L3 * Math.Sin(sp2_radian + sp3_radian) + L2 * Math.Sin(sp2_radian) + L4 * Math.Sin(-Math.PI / 2), 2);

            double angle1_radian = angle1 * Math.PI / 180;
            double angle2_radian = angle2 * Math.PI / 180;
            double angle3_radian = angle3 * Math.PI / 180;

            Xreal = Math.Round(Math.Cos(angle1_radian) * (L3 * Math.Cos(angle2_radian + angle3_radian) + L2 * Math.Cos(angle2_radian) + L4 * Math.Cos(-Math.PI / 2)), 2);
            Yreal = Math.Round(Math.Sin(angle1_radian) * (L3 * Math.Cos(angle2_radian + angle3_radian) + L2 * Math.Cos(angle2_radian) + L4 * Math.Cos(-Math.PI / 2)), 2);
            Zreal = Math.Round(L3 * Math.Sin(angle2_radian + angle3_radian) + L2 * Math.Sin(angle2_radian) + L4 * Math.Sin(-Math.PI / 2), 2);
        }
        private void UART_Send()
        {
            if (checkBoxStep.CheckState == CheckState.Checked) 
            {
                TXdataFull = Convert.ToString(sp1) + "a" + Convert.ToString(sp2) + "b" + Convert.ToString(sp3) + "c" + Convert.ToString(time_tp) + "s";
            }
            else
            if (checkBoxLine.CheckState == CheckState.Checked)
            {
                
            }
            else
            if (checkBoxCubic.CheckState == CheckState.Checked)
            {
                if(realtime < QD1_tf)
                {
                    sp1 = QD1_a + QD1_b * (realtime - QD1_t0) + QD1_c * Math.Pow((realtime - QD1_t0), 2) + QD1_d * Math.Pow((realtime - QD1_t0), 3);
                    sp2 = QD2_a + QD2_b * (realtime - QD2_t0) + QD2_c * Math.Pow((realtime - QD2_t0), 2) + QD2_d * Math.Pow((realtime - QD2_t0), 3);
                    sp3 = QD3_a + QD3_b * (realtime - QD3_t0) + QD3_c * Math.Pow((realtime - QD3_t0), 2) + QD3_d * Math.Pow((realtime - QD3_t0), 3);
                }
                TXdataFull = Convert.ToString(sp1) + "a" + Convert.ToString(sp2) + "b" + Convert.ToString(sp3) + "c" + Convert.ToString(time_tp) + "s";
            }
            serialPort1.Write(TXdataFull);
        }
        private void InverseKinematic()
        {
            IKX -= L4 * r11;
            IKY -= L4 * r21;
            IKZ -= L4 * r31;

            T1IK = Math.Atan2(IKY, IKX);

            double k = IKX * Math.Cos(T1IK) + IKY * Math.Sin(T1IK);
            double c3 = ((Math.Pow(k, 2) + Math.Pow(IKZ, 2) - Math.Pow(L2, 2)) - Math.Pow(L3, 2)) / (2 * L2 * L3);
            double s3 = -Math.Sqrt(1 - Math.Pow(c3, 2));

            

            T3IK = Math.Atan2(s3, c3);
            double D = Math.Pow((L2 + L3 * Math.Cos(T3IK)), 2) + Math.Pow(L3 * Math.Sin(T3IK), 2);
            double Dc = k * (L2 + L3 * Math.Cos(T3IK)) + IKZ * (L3 * Math.Sin(T3IK));
            double Ds = IKZ * (L2 + L3 * Math.Cos(T3IK)) - k * L3 * Math.Sin(T3IK);

            double c2 = Dc / D;
            double s2 = Ds / D;

            T2IK = Math.Atan2(s2, c2);

            T1IK = T1IK * 180 / Math.PI;
            T2IK = T2IK * 180 / Math.PI;
            T3IK = T3IK * 180 / Math.PI;

            label25.Text = Convert.ToString(T1IK);
            label21.Text = Convert.ToString(T2IK-30);
            label20.Text = Convert.ToString(T3IK+75);
        }

        private void Calculate_Cubic_Function()
        {
            QD1_a = QD1_q0;
            QD1_b = 0;
            QD1_c = 3 * (QD1_qf - QD1_q0) / Math.Pow((QD1_tf - QD1_t0), 2);
            QD1_d = -2 * (QD1_qf - QD1_q0) / Math.Pow((QD1_tf - QD1_t0), 3);

            QD2_a = QD2_q0;
            QD2_b = 0;
            QD2_c = 3 * (QD2_qf - QD2_q0) / Math.Pow((QD2_tf - QD2_t0), 2);
            QD2_d = -2 * (QD2_qf - QD2_q0) / Math.Pow((QD2_tf - QD2_t0), 3);

            QD3_a = QD3_q0;
            QD3_b = 0;
            QD3_c = 3 * (QD3_qf - QD3_q0) / Math.Pow((QD3_tf - QD3_t0), 2);
            QD3_d = -2 * (QD3_qf - QD3_q0) / Math.Pow((QD3_tf - QD3_t0), 3);
        }
        private void Calculate_Step_Function()
        {
            sp1 = QD1_qf;
            sp2 = QD2_qf;
            sp3 = QD3_qf;
        }
        private void Calculate_Line_Funtion()
        {

        }
    }
}
