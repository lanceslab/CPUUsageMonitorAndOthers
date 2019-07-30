namespace CPUUsageMonitor
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.cpuUsageChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.label1 = new System.Windows.Forms.Label();
            this.proVal = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.cpuUsageChart)).BeginInit();
            this.SuspendLayout();
            // 
            // cpuUsageChart
            // 
            chartArea2.Name = "ChartArea1";
            this.cpuUsageChart.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            this.cpuUsageChart.Legends.Add(legend2);
            this.cpuUsageChart.Location = new System.Drawing.Point(-1, -1);
            this.cpuUsageChart.Name = "cpuUsageChart";
            series2.ChartArea = "ChartArea1";
            series2.Legend = "Legend1";
            series2.Name = "Series1";
            this.cpuUsageChart.Series.Add(series2);
            this.cpuUsageChart.Size = new System.Drawing.Size(734, 254);
            this.cpuUsageChart.TabIndex = 0;
            this.cpuUsageChart.Text = "cpuChart";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(23, 284);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(212, 26);
            this.label1.TabIndex = 1;
            this.label1.Text = "Current CPU Usage ";
            // 
            // proVal
            // 
            this.proVal.AutoSize = true;
            this.proVal.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.proVal.Location = new System.Drawing.Point(241, 284);
            this.proVal.Name = "proVal";
            this.proVal.Size = new System.Drawing.Size(46, 26);
            this.proVal.TabIndex = 2;
            this.proVal.Text = "0%";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(734, 362);
            this.Controls.Add(this.proVal);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cpuUsageChart);
            this.Name = "Form1";
            this.Text = "CPU usage";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.cpuUsageChart)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataVisualization.Charting.Chart cpuUsageChart;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label proVal;
    }
}

