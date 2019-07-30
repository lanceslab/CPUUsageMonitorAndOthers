using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Management;

namespace CPUUsageMonitor
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
        
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            System.Management.ManagementObjectSearcher s = new System.Management.ManagementObjectSearcher("root/WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");

            foreach (System.Management.ManagementObject q in s.Get())
            {
                //var tmp = (double)q.GetPropertyValue("CurrentTemperature");
                //double tmp;// = Convert.ToDouble(q("CurrentTemperature"));// (double)q.("CurrentTemperature");
                //tmp = (double) q.GetPropertyValue("CurrentTemperature");
                //tmp = (tmp - -2732) / 10.0;
                //textBox1.AppendText(tmp.ToString() + Environment.NewLine);
                //textBox1.AppendText(q.ToString() + Environment.NewLine);
                //textBox1.AppendText(q.GetText(TextFormat.Mof) + Environment.NewLine);
                string curTemp = (q.GetPropertyValue("CurrentTemperature").ToString() + Environment.NewLine);
                double tmp = Convert.ToDouble(curTemp);
                tmp = (tmp - 2732) / 10.0;
                textBox1.Text = tmp.ToString();
                //textBox1.AppendText(q.GetPropertyValue("CurrentTemperature").ToString() + Environment.NewLine);

            }
            //For Each q As ManagementObject In s.Get()
            //    Dim tmp As Double = CDbl(q("CurrentTemperature"))
            //    tmp = (tmp - -2732) / 10.0

            //    RichTextBox1.AppendText(tmp.ToString & vbCrLf)
            //Next


        }
















    }
}
