using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Org.Mentalis.Utilities;
using System.Threading;
using System.Diagnostics;
//using SmsClient;
using System.Net;
using System.Net.Mail;

namespace CPUUsageMonitor
{
    public partial class Form2 : Form
    {

        bool iscontinue = true;
        //private static CpuUsage cpu;
        private Temperature temp;


        public Form2()
        {
            InitializeComponent();

            Shown += new EventHandler(Form2_Shown);

            // To report progress from the background worker we need to set this property
            backgroundWorker1.WorkerReportsProgress = true;
            // This event will be raised on the worker thread when the worker starts
            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            // This event will be raised when we call ReportProgress
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);


        }



        //    
        private void populate_services()
        {
            //Enumerate asynchronously using Object Searcher
            //===============================================
            listBox1.Items.Clear();
            //Instantiate an object searcher with the query
            ManagementObjectSearcher searcher =
               new ManagementObjectSearcher(new SelectQuery("Win32_Service"));

            
            foreach (ManagementObject obj in searcher.Get())
            {
                //listBox1.Items.Add(obj.ToString());
                listBox1.Items.Add(obj.GetPropertyValue("Name").ToString());
            }

        }
        //    
        //private void populate_services()
        //{
        //    //Enumerate asynchronously using Object Searcher
        //    //===============================================
        //    listBox1.Items.Clear();
        //    //Instantiate an object searcher with the query
        //    ManagementObjectSearcher searcher =
        //       new ManagementObjectSearcher(new SelectQuery("Win32_Service"));

        //    // Create a results watcher object, and handler for results and completion
        //    ManagementOperationObserver results = new ManagementOperationObserver();
        //    ObjectHandler objectHandler = new ObjectHandler();

        //    // Attach handler to events for results and completion
        //    results.ObjectReady += new ObjectReadyEventHandler(objectHandler.NewObject);
            
        //    results.Completed += new CompletedEventHandler(objectHandler.Done);

        //    //Call the asynchronous overload of Get() to start the enumeration
        //    searcher.Get(results);

        //    //Do something else while results arrive asynchronously
        //    while (!objectHandler.IsCompleted)
        //    {
        //        listBox1.Items.Add(objectHandler.ToString());
        //        listBox1.Items.Add(results.ToString());

        //        listBox1.Items.Add(searcher.ToString());
        //        // listBox1.Items.Add(objectHandler.ReturnObject.GetText(TextFormat.Mof));
        //        System.Threading.Thread.Sleep(1000);
        //    }

        //    objectHandler.Reset();

        //}

        //Handler for asynchronous results
        public class ObjectHandler
        {
            private bool isCompleted = false;
            public string serviceString = "";
            private ManagementBaseObject returnObject;



            //Property allows accessing the result object in the main function
            public ManagementBaseObject ReturnObject
            {
                get
                {
                    return returnObject;
                }
            }



            public void NewObject(object sender, ObjectReadyEventArgs obj)
            {

                //Console.WriteLine("Service : {0}, State = {1}", obj.NewObject["Name"], obj.NewObject["State"]);
                serviceString = string.Format("Service : {0}, State = {1}", obj.NewObject["Name"], obj.NewObject["State"]);
                returnObject = obj.NewObject;
   
            }

            public bool IsCompleted
            {
                get
                {
                    return isCompleted;
                }
            }

            public void Reset()
            {
                isCompleted = false;
            }

            public void Done(object sender, CompletedEventArgs obj)
            {
                isCompleted = true;
            }
        }

        private void populateEnvironmentVariables()
        {
            listBox1.Items.Clear();
            // Create a query for system environment variables only
            SelectQuery query = new SelectQuery("Win32_Environment", "UserName=\"<SYSTEM>\"");

            // Initialize an object searcher with this query
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

            // Get the resulting collection and loop through it
            foreach (ManagementBaseObject envVar in searcher.Get())
            {
                //Console.WriteLine("System environment variable {0} = {1}", envVar["Name"], envVar["VariableValue"]);

                //string variableYoAddString = string.Format("System environment variable {0} = {1}", envVar["Name"], envVar["VariableValue"]);
                string variableYoAddString = string.Format("Environment variable:    {0} = {1}", envVar["Name"], envVar["VariableValue"]);
                listBox1.Items.Add(variableYoAddString);


            }

        }




        NotifyIcon notifyIcon;
        //void click(object sender, RoutedEventArgs e)
        void tooHotNotification()
        {
            // Configure and show a notification icon in the system tray
            this.notifyIcon = new NotifyIcon();
            this.notifyIcon.BalloonTipText = "WARNING" + Environment.NewLine + "CPU Too Hot !";
            this.notifyIcon.Text = "CPU Too Hot !";
            this.notifyIcon.Icon = new System.Drawing.Icon("NotifyIcon.ico");
            this.notifyIcon.Visible = true;
            this.notifyIcon.ShowBalloonTip(10);
        }

        private void populateCPUInfo()
        {
            try
            {
                // Creates and returns a CpuUsage instance that can be used to query the CPU time on this operating system.
                //cpu = CpuUsage.Create();

                /// Creating a New Thread 
                Thread thread = new Thread(new ThreadStart(delegate()
                {
                    try
                    {
                        while (iscontinue)
                        {

                            var cpuload = new PerformanceCounter("Processor", "% Processor Time", "_Total");

                            // RAM 
                            var ramload = new PerformanceCounter("Memory", "% Committed Bytes In Use", true);
                            float perfCounterValue = cpuload.NextValue();
                            float perfCounterValueRAM = ramload.NextValue();

                            //Thread has to sleep for at least 1 sec for accurate value.
                            System.Threading.Thread.Sleep(1000);

                            perfCounterValue = cpuload.NextValue();
                            perfCounterValueRAM = ramload.NextValue();



                            //populateCPU_TempInfo();

                            //To Update The UI Thread we have to Invoke  it. 
                            this.Invoke(new System.Windows.Forms.MethodInvoker(delegate()
                            {
                                //int process = cpu.Query(); //Determines the current average CPU load.
                                int process = (int)perfCounterValue; // cpu.Query(); //Determines the current average CPU load.
                                int processRAM = (int)perfCounterValueRAM; // cpu.Query(); //Determines the current average CPU load.
                                progressBar1.Value = process;
                                progressBar2.Value = processRAM;
                                this.Text = "       CPU:  " + process.ToString() + "%" + "     RAM:  " + processRAM.ToString() + "%";




                                //System.Management.ManagementObjectSearcher s = new System.Management.ManagementObjectSearcher("root/WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
                               //this.Text += " TEMP: " + s.GetType().ToString();
                                //foreach (System.Management.ManagementObject q in s.Get())
                                //{
                                //    string curTemp = (q.GetPropertyValue("CurrentTemperature").ToString() + Environment.NewLine);
                                //    double tmp = Convert.ToDouble(curTemp);
                                //    tmp = (tmp - 2732) / 10.0;
                                //    label4.Text = tmp.ToString();
                                //    //textBox1.AppendText(q.GetPropertyValue("CurrentTemperature").ToString() + Environment.NewLine);
                                //    //temp = new Temperature();
                                //    ////temp.InstanceName = "POOP";
                                //    //double cpuTemp = temp.CurrentValue;

                                //    //progressBar2.Value = (int) temp;
                                //    //label4.Text = cpuTemp.ToString();

                                //    double maxTemp = 75;
                                //    //double maxTemp = 50;
                                //    if (tmp < maxTemp)
                                //    {
                                //        label4.BackColor = Color.LightGreen;
                                //    }
                                //    else
                                //    {
                                //        label4.BackColor = Color.LightPink;
                                //        // POP UP NOTIFIER
                                //        tooHotNotification();
                                //        // send text
                                //        //string pcName = System.Environment.MachineName;
                                //        string pcName = string.Format( "The processor on: {0} is to hot!  The current temp is {1}  c", System.Environment.MachineName, tmp.ToString());
                                //        //sendText("CPU Temp", "The processor on: " + pcName + "  is to hot! The current temp is " + 
                                //        //    tmp.ToString() + " c");//sendText(subject,  body);
                                //        sendText("Warning CPU Too Hot", pcName);
                                //    }

                                //    //label4.Text = temp.CurrentValue.ToString();

                                //}


                                //System.Management.ManagementObjectSearcher ramQuery = new System.Management.ManagementObjectSearcher("root/WMI", "SELECT * FROM Win32_PhysicalMemory");


                            }));

                            Thread.Sleep(450);//Thread sleep for 450 milliseconds 
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "ERROR");
                    }

                }));

                thread.Priority = ThreadPriority.Highest;
                thread.IsBackground = true;
                thread.Start();//Start the Thread
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "ERROR");
                Console.WriteLine(ex);
            }

        }




        private void populateCPU_TempInfo()
        {


            System.Management.ManagementObjectSearcher s = new System.Management.ManagementObjectSearcher("root/WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");

            foreach (System.Management.ManagementObject q in s.Get())
            {
                string curTemp = (q.GetPropertyValue("CurrentTemperature").ToString() + Environment.NewLine);
                double tmp = Convert.ToDouble(curTemp);
                tmp = (tmp - 2732) / 10.0;
                label4.Text = tmp.ToString();
                //textBox1.AppendText(q.GetPropertyValue("CurrentTemperature").ToString() + Environment.NewLine);

            }

        }




        private void populateRAMInfo()
        {
            try
            {
                // Creates and returns a CpuUsage instance that can be used to query the CPU time on this operating system.
                //cpu = CpuUsage.Create();

                /// Creating a New Thread 
                Thread thread = new Thread(new ThreadStart(delegate()
                {
                    try
                    {
                        while (iscontinue)
                        {

                            var cpuload = new PerformanceCounter("Ram", "% Processor Time", "_Total");
                            float perfCounterValue = cpuload.NextValue();
                            //Thread has to sleep for at least 1 sec for accurate value.
                            System.Threading.Thread.Sleep(1000);

                            perfCounterValue = cpuload.NextValue();


                            //To Update The UI Thread we have to Invoke  it. 
                            this.Invoke(new System.Windows.Forms.MethodInvoker(delegate()
                            {
                                //int process = cpu.Query(); //Determines the current average CPU load.
                                int process = (int)perfCounterValue; // cpu.Query(); //Determines the current average CPU load.
                                progressBar2.Value = process;
                                this.Text = "   RAM   " + process.ToString() + "%";
                                //proVal.Text = process.ToString() + "%";
                                //cpuUsageChart.Series[0].Points.AddY(process);//Add process to chart 

                                //if (cpuUsageChart.Series[0].Points.Count > 40)//clear old data point after Thrad Sleep time * 40
                                //    cpuUsageChart.Series[0].Points.RemoveAt(0);

                            }));

                            Thread.Sleep(450);//Thread sleep for 450 milliseconds 
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "ERROR");
                    }

                }));

                thread.Priority = ThreadPriority.Highest;
                thread.IsBackground = true;
                thread.Start();//Start the Thread
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "ERROR");
                Console.WriteLine(ex);
            }

        }




        private void populateDRIVES()
        {
            listBox1.Items.Clear();

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_LogicalDisk");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    //Console.WriteLine("-----------------------------------");
                    //Console.WriteLine("Win32_LogicalDisk instance");
                    //Console.WriteLine("-----------------------------------");
                    //Console.WriteLine("Access: {0}", queryObj["Access"]);
                    //Console.WriteLine("Availability: {0}", queryObj["Availability"]);
                    //Console.WriteLine("BlockSize: {0}", queryObj["BlockSize"]);
                    //Console.WriteLine("Caption: {0}", queryObj["Caption"]);
                    //Console.WriteLine("Compressed: {0}", queryObj["Compressed"]);
                    //Console.WriteLine("ConfigManagerErrorCode: {0}", queryObj["ConfigManagerErrorCode"]);
                    //Console.WriteLine("ConfigManagerUserConfig: {0}", queryObj["ConfigManagerUserConfig"]);
                    //Console.WriteLine("CreationClassName: {0}", queryObj["CreationClassName"]);
                    //Console.WriteLine("Description: {0}", queryObj["Description"]);
                    //Console.WriteLine("DeviceID: {0}", queryObj["DeviceID"]);
                    //Console.WriteLine("DriveType: {0}", queryObj["DriveType"]);
                    //Console.WriteLine("ErrorCleared: {0}", queryObj["ErrorCleared"]);
                    //Console.WriteLine("ErrorDescription: {0}", queryObj["ErrorDescription"]);
                    //Console.WriteLine("ErrorMethodology: {0}", queryObj["ErrorMethodology"]);
                    //Console.WriteLine("FileSystem: {0}", queryObj["FileSystem"]);
                    //Console.WriteLine("FreeSpace: {0}", queryObj["FreeSpace"]);
                    //Console.WriteLine("InstallDate: {0}", queryObj["InstallDate"]);
                    //Console.WriteLine("LastErrorCode: {0}", queryObj["LastErrorCode"]);
                    //Console.WriteLine("MaximumComponentLength: {0}", queryObj["MaximumComponentLength"]);
                    //Console.WriteLine("MediaType: {0}", queryObj["MediaType"]);
                    //Console.WriteLine("Name: {0}", queryObj["Name"]);
                    //Console.WriteLine("NumberOfBlocks: {0}", queryObj["NumberOfBlocks"]);
                    //Console.WriteLine("PNPDeviceID: {0}", queryObj["PNPDeviceID"]);//     vlistBox1.Items.Add(string.Format

                    listBox1.Items.Add("-----------------------------------");
                    listBox1.Items.Add("Win32_LogicalDisk instance");
                    listBox1.Items.Add("-----------------------------------");
                    //listBox1.Items.Add(string.Format("Access: {0}", queryObj["Access"]));
                    //listBox1.Items.Add(string.Format("Availability: {0}", queryObj["Availability"]));
                    //listBox1.Items.Add(string.Format(string.Format("BlockSize: {0}", queryObj["BlockSize"])));
                    //listBox1.Items.Add(string.Format("Caption: {0}", queryObj["Caption"]));
                    //listBox1.Items.Add(string.Format("Compressed: {0}", queryObj["Compressed"]));
                    //listBox1.Items.Add(string.Format("ConfigManagerErrorCode: {0}", queryObj["ConfigManagerErrorCode"]));
                    //listBox1.Items.Add(string.Format("ConfigManagerUserConfig: {0}", queryObj["ConfigManagerUserConfig"]));
                    //listBox1.Items.Add(string.Format("CreationClassName: {0}", queryObj["CreationClassName"]));
                    //listBox1.Items.Add(string.Format("Description: {0}", queryObj["Description"]));
                    //listBox1.Items.Add(string.Format("DeviceID: {0}", queryObj["DeviceID"]));
                    //listBox1.Items.Add(string.Format("DriveType: {0}", queryObj["DriveType"]));
                    //listBox1.Items.Add(string.Format("ErrorCleared: {0}", queryObj["ErrorCleared"]));
                    //listBox1.Items.Add(string.Format("ErrorDescription: {0}", queryObj["ErrorDescription"]));
                    //listBox1.Items.Add(string.Format("ErrorMethodology: {0}", queryObj["ErrorMethodology"]));
                    //listBox1.Items.Add(string.Format("FileSystem: {0}", queryObj["FileSystem"]));
                    //listBox1.Items.Add(string.Format("FreeSpace: {0}", queryObj["FreeSpace"]));
                    //listBox1.Items.Add(string.Format("InstallDate: {0}", queryObj["InstallDate"]));
                    //listBox1.Items.Add(string.Format("LastErrorCode: {0}", queryObj["LastErrorCode"]));
                    //listBox1.Items.Add(string.Format("MaximumComponentLength: {0}", queryObj["MaximumComponentLength"]));
                    //listBox1.Items.Add(string.Format("MediaType: {0}", queryObj["MediaType"]));
                    //listBox1.Items.Add(string.Format("Name: {0}", queryObj["Name"]));
                    //listBox1.Items.Add(string.Format("NumberOfBlocks: {0}", queryObj["NumberOfBlocks"]));
                    //listBox1.Items.Add(string.Format("PNPDeviceID: {0}", queryObj["PNPDeviceID"]));//     


                    listBox1.Items.Add(string.Format("Name: {0}", queryObj["Name"]));
                    listBox1.Items.Add(string.Format("Caption: {0}", queryObj["Caption"]));
                    listBox1.Items.Add(string.Format("Availability: {0}", queryObj["Availability"]));
                    listBox1.Items.Add(string.Format("MediaType: {0}", queryObj["MediaType"]));
                    listBox1.Items.Add(string.Format("DeviceID: {0}", queryObj["DeviceID"]));
                    listBox1.Items.Add(string.Format("DriveType: {0}", queryObj["DriveType"]));
                    listBox1.Items.Add(string.Format("FreeSpace: {0}", queryObj["FreeSpace"]));
                    listBox1.Items.Add(string.Format("MaximumComponentLength: {0}", queryObj["MaximumComponentLength"]));


                    if (queryObj["PowerManagementCapabilities"] == null)
                        //Console.WriteLine("PowerManagementCapabilities: {0}", queryObj["PowerManagementCapabilities"]);
                        listBox1.Items.Add(string.Format("PowerManagementCapabilities: {0}", queryObj["PowerManagementCapabilities"]));
                    else
                    {
                        UInt16[] arrPowerManagementCapabilities = (UInt16[])(queryObj["PowerManagementCapabilities"]);
                        foreach (UInt16 arrValue in arrPowerManagementCapabilities)
                        {
                            //Console.WriteLine("PowerManagementCapabilities: {0}", arrValue);
                            listBox1.Items.Add(string.Format("PowerManagementCapabilities: {0}", arrValue));
                        }
                    }
                    //Console.WriteLine("PowerManagementSupported: {0}", queryObj["PowerManagementSupported"]);
                    //Console.WriteLine("ProviderName: {0}", queryObj["ProviderName"]);
                    //Console.WriteLine("Purpose: {0}", queryObj["Purpose"]);
                    //Console.WriteLine("QuotasDisabled: {0}", queryObj["QuotasDisabled"]);
                    //Console.WriteLine("QuotasIncomplete: {0}", queryObj["QuotasIncomplete"]);
                    //Console.WriteLine("QuotasRebuilding: {0}", queryObj["QuotasRebuilding"]);
                    //Console.WriteLine("Size: {0}", queryObj["Size"]);
                    //Console.WriteLine("Status: {0}", queryObj["Status"]);
                    //Console.WriteLine("StatusInfo: {0}", queryObj["StatusInfo"]);
                    //Console.WriteLine("SupportsDiskQuotas: {0}", queryObj["SupportsDiskQuotas"]);
                    //Console.WriteLine("SupportsFileBasedCompression: {0}", queryObj["SupportsFileBasedCompression"]);
                    //Console.WriteLine("SystemCreationClassName: {0}", queryObj["SystemCreationClassName"]);
                    //Console.WriteLine("SystemName: {0}", queryObj["SystemName"]);
                    //Console.WriteLine("VolumeDirty: {0}", queryObj["VolumeDirty"]);
                    //Console.WriteLine("VolumeName: {0}", queryObj["VolumeName"]);
                    //Console.WriteLine("VolumeSerialNumber: {0}", queryObj["VolumeSerialNumber"]);

                    //listBox1.Items.Add(string.Format("PowerManagementSupported: {0}", queryObj["PowerManagementSupported"]));
                    //listBox1.Items.Add(string.Format("ProviderName: {0}", queryObj["ProviderName"]));
                    //listBox1.Items.Add(string.Format("Purpose: {0}", queryObj["Purpose"]));
                    //listBox1.Items.Add(string.Format("QuotasDisabled: {0}", queryObj["QuotasDisabled"]));
                    //listBox1.Items.Add(string.Format("QuotasIncomplete: {0}", queryObj["QuotasIncomplete"]));
                    //listBox1.Items.Add(string.Format("QuotasRebuilding: {0}", queryObj["QuotasRebuilding"]));
                    //listBox1.Items.Add(string.Format("Size: {0}", queryObj["Size"]));
                    //listBox1.Items.Add(string.Format("Status: {0}", queryObj["Status"]));
                    //listBox1.Items.Add(string.Format("StatusInfo: {0}", queryObj["StatusInfo"]));
                    //listBox1.Items.Add(string.Format("SupportsDiskQuotas: {0}", queryObj["SupportsDiskQuotas"]));
                    //listBox1.Items.Add(string.Format("SupportsFileBasedCompression: {0}", queryObj["SupportsFileBasedCompression"]));
                    //listBox1.Items.Add(string.Format("SystemCreationClassName: {0}", queryObj["SystemCreationClassName"]));
                    //listBox1.Items.Add(string.Format("SystemName: {0}", queryObj["SystemName"]));
                    //listBox1.Items.Add(string.Format("VolumeDirty: {0}", queryObj["VolumeDirty"]));
                    //listBox1.Items.Add(string.Format("VolumeName: {0}", queryObj["VolumeName"]));
                    //listBox1.Items.Add(string.Format("VolumeSerialNumber: {0}", queryObj["VolumeSerialNumber"]));

                    listBox1.Items.Add(string.Format("SystemName: {0}", queryObj["SystemName"]));
                    listBox1.Items.Add(string.Format("VolumeName: {0}", queryObj["VolumeName"]));
                    listBox1.Items.Add(string.Format("Size: {0}", queryObj["Size"]));
                    listBox1.Items.Add(string.Format("Status: {0}", queryObj["Status"]));
                    listBox1.Items.Add(string.Format("StatusInfo: {0}", queryObj["StatusInfo"]));
                }


            }
            catch (ManagementException e)
            {
                //Console.WriteLine("An error occurred while querying for WMI data: " + e.Message);
                listBox1.Items.Add(string.Format("An error occurred while querying for WMI data: " + e.Message));
            }

            //Console.WriteLine();
            //Console.WriteLine("Press any key to quit");
            //Console.ReadKey();
            //listBox1.SelectedIndex = 0;
        }




        private void populateBIOS()
        {

            listBox1.Items.Clear();

            try 
            { 

                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BIOS");  
 
                foreach (ManagementObject queryObj in searcher.Get()) 
                { 
                    //Console.WriteLine("-----------------------------------"); 
                    //Console.WriteLine("Win32_BIOS instance"); 
                    //Console.WriteLine("-----------------------------------"); 
                    listBox1.Items.Add("-----------------------------------"); 
                    listBox1.Items.Add("Win32_BIOS instance");
                    listBox1.Items.Add("-----------------------------------");

                    if (queryObj["BiosCharacteristics"] == null)
                    {
                        //Console.WriteLine("BiosCharacteristics: {0}", queryObj["BiosCharacteristics"]);
                        listBox1.Items.Add(string.Format("BiosCharacteristics: {0}", queryObj["BiosCharacteristics"]));
                    }
                    else
                    {
                        UInt16[] arrBiosCharacteristics = (UInt16[])(queryObj["BiosCharacteristics"]);
                        foreach (UInt16 arrValue in arrBiosCharacteristics)
                        {
                            //Console.WriteLine("BiosCharacteristics: {0}", arrValue);
                            listBox1.Items.Add(string.Format("BiosCharacteristics: {0}", arrValue));
                        }
                    }

                    if (queryObj["BIOSVersion"] == null)
                    {
                        //Console.WriteLine("BIOSVersion: {0}", queryObj["BIOSVersion"]);
                        listBox1.Items.Add(string.Format("BiosCharacteristics: {0}", queryObj["BIOSVersion"]));
                    }
                    else
                    {
                        String[] arrBIOSVersion = (String[])(queryObj["BIOSVersion"]);
                        foreach (String arrValue in arrBIOSVersion)
                        {
                            //Console.WriteLine("BIOSVersion: {0}", arrValue);
                            listBox1.Items.Add(string.Format("BIOSVersion: {0}", arrValue));
                        }
                    } 
                    //Console.WriteLine("BuildNumber: {0}", queryObj["BuildNumber"]); 
                    //Console.WriteLine("Caption: {0}", queryObj["Caption"]); 
                    //Console.WriteLine("CodeSet: {0}", queryObj["CodeSet"]); 
                    //Console.WriteLine("CurrentLanguage: {0}", queryObj["CurrentLanguage"]); 
                    //Console.WriteLine("Description: {0}", queryObj["Description"]); 
                    //Console.WriteLine("IdentificationCode: {0}", queryObj["IdentificationCode"]); 
                    //Console.WriteLine("InstallableLanguages: {0}", queryObj["InstallableLanguages"]); 
                    //Console.WriteLine("InstallDate: {0}", queryObj["InstallDate"]); 
                    //Console.WriteLine("LanguageEdition: {0}", queryObj["LanguageEdition"]); 
                    listBox1.Items.Add(string.Format("BuildNumber: {0}", queryObj["BuildNumber"]));
                    listBox1.Items.Add(string.Format("Caption: {0}", queryObj["Caption"]));
                    listBox1.Items.Add(string.Format("CodeSet: {0}", queryObj["CodeSet"]));
                    listBox1.Items.Add(string.Format("CurrentLanguage: {0}", queryObj["CurrentLanguage"]));
                    listBox1.Items.Add(string.Format("Description: {0}", queryObj["Description"]));
                    listBox1.Items.Add(string.Format("IdentificationCode: {0}", queryObj["IdentificationCode"]));
                    listBox1.Items.Add(string.Format("InstallableLanguages: {0}", queryObj["InstallableLanguages"]));
                    listBox1.Items.Add(string.Format("InstallDate: {0}", queryObj["InstallDate"]));
                    listBox1.Items.Add(string.Format("LanguageEdition: {0}", queryObj["LanguageEdition"]));

                    if (queryObj["ListOfLanguages"] == null)
                    {
                        //Console.WriteLine("ListOfLanguages: {0}", queryObj["ListOfLanguages"]);
                        listBox1.Items.Add(string.Format("ListOfLanguages: {0}", queryObj["ListOfLanguages"]));
                    }
                    else
                    {
                        String[] arrListOfLanguages = (String[])(queryObj["ListOfLanguages"]);
                        foreach (String arrValue in arrListOfLanguages)
                        {
                            //Console.WriteLine("ListOfLanguages: {0}", arrValue);
                            listBox1.Items.Add(string.Format("ListOfLanguages: {0}", arrValue));
                        }
                    } 
                    //Console.WriteLine("Manufacturer: {0}", queryObj["Manufacturer"]); 
                    //Console.WriteLine("Name: {0}", queryObj["Name"]); 
                    //Console.WriteLine("OtherTargetOS: {0}", queryObj["OtherTargetOS"]); 
                    //Console.WriteLine("PrimaryBIOS: {0}", queryObj["PrimaryBIOS"]); 
                    //Console.WriteLine("ReleaseDate: {0}", queryObj["ReleaseDate"]); 
                    //Console.WriteLine("SerialNumber: {0}", queryObj["SerialNumber"]); 
                    //Console.WriteLine("SMBIOSBIOSVersion: {0}", queryObj["SMBIOSBIOSVersion"]); 
                    //Console.WriteLine("SMBIOSMajorVersion: {0}", queryObj["SMBIOSMajorVersion"]); 
                    //Console.WriteLine("SMBIOSMinorVersion: {0}", queryObj["SMBIOSMinorVersion"]); 
                    //Console.WriteLine("SMBIOSPresent: {0}", queryObj["SMBIOSPresent"]); 
                    //Console.WriteLine("SoftwareElementID: {0}", queryObj["SoftwareElementID"]); 
                    //Console.WriteLine("SoftwareElementState: {0}", queryObj["SoftwareElementState"]); 
                    //Console.WriteLine("Status: {0}", queryObj["Status"]); 
                    //Console.WriteLine("TargetOperatingSystem: {0}", queryObj["TargetOperatingSystem"]); 
                    //Console.WriteLine("Version: {0}", queryObj["Version"]); 
                    listBox1.Items.Add(string.Format("Manufacturer: {0}", queryObj["Manufacturer"]));
                    listBox1.Items.Add(string.Format("Name: {0}", queryObj["Name"]));
                    listBox1.Items.Add(string.Format("OtherTargetOS: {0}", queryObj["OtherTargetOS"]));
                    listBox1.Items.Add(string.Format("PrimaryBIOS: {0}", queryObj["PrimaryBIOS"]));
                    listBox1.Items.Add(string.Format("ReleaseDate: {0}", queryObj["ReleaseDate"]));
                    listBox1.Items.Add(string.Format("SerialNumber: {0}", queryObj["SerialNumber"]));
                    listBox1.Items.Add(string.Format("SMBIOSBIOSVersion: {0}", queryObj["SMBIOSBIOSVersion"]));
                    listBox1.Items.Add(string.Format("SMBIOSMajorVersion: {0}", queryObj["SMBIOSMajorVersion"]));
                    listBox1.Items.Add(string.Format("SMBIOSMinorVersion: {0}", queryObj["SMBIOSMinorVersion"]));
                    listBox1.Items.Add(string.Format("SMBIOSPresent: {0}", queryObj["SMBIOSPresent"]));
                    listBox1.Items.Add(string.Format("SoftwareElementID: {0}", queryObj["SoftwareElementID"]));
                    listBox1.Items.Add(string.Format("SoftwareElementState: {0}", queryObj["SoftwareElementState"]));
                    listBox1.Items.Add(string.Format("Status: {0}", queryObj["Status"]));
                    listBox1.Items.Add(string.Format("TargetOperatingSystem: {0}", queryObj["TargetOperatingSystem"]));
                    listBox1.Items.Add(string.Format("Version: {0}", queryObj["Version"]));
                } 
            } 
            catch (ManagementException e) 
            { 
                MessageBox.Show("An error occurred while querying for WMI data: " + e.Message); 
            } 

        }




        private void Form2_Load(object sender, EventArgs e)
        {

            populateCPUInfo();

            //populateCPU_TempInfo();

            //populateEnvironmentVariables();

            //populate_services();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            iscontinue = false;
        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            // Start the background worker
            //backgroundWorker1.RunWorkerAsync();
        }





        // On worker thread so do our thing!
        void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // Your background task goes here
            for (int i = 0; i <= 100; i++)
            {
                // Report progress to 'UI' thread
                backgroundWorker1.ReportProgress(i);
                // Simulate long task
                System.Threading.Thread.Sleep(100);
            }
        }
        // Back on the 'UI' thread so we can update the progress bar
        void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // The progress percentage is a property of e
            progressBar1.Value = e.ProgressPercentage;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            populate_services();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            populateEnvironmentVariables();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            populateBIOS();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            populateDRIVES();
        }






        private void sendText(string subject, string body)
        {


            // Collect user input from the form and stow content into the objects member variables
            //mTo = Trim(txtPhoneNumber.Text) & Trim(cboCarrier.SelectedItem.ToString());
            //mFrom = Trim(txtSender.Text);
            //mSubject = Trim(txtSubject.Text);
            //mMailServer = Trim(txtMailServer.Text);
            //mMsg = Trim(txtMessage.Text);

            //mTo = textBoxToNumber.Text.Trim() + cboCarrier.SelectedItem.ToString().Trim();
            //mFrom = textBoxFromNumber.Text.Trim() + cboCarrier.SelectedItem.ToString().Trim();
            //mSubject = textBoxSubject.Text.Trim();
            //mMailServer = textBoxFromPassword.Text.Trim();
            //mMsg = textBoxMsgBody.Text.Trim();


            //// Within a try catch, format and send the message to the recipient.  Catch and handle any errors.
            try
            {

                // WORKING WITH GMAIL !!!!!
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential("gsspankster@gmail.com", "00101548");
                MailMessage msg = new MailMessage();
                //msg.To.Add("taylorinsofdaytona@yahoo.com");
                msg.To.Add("lanceandsheri@yahoo.com");
                //msg.To.Add("386-235-2097@mobile.att.net");  // SAID IT WORKED
                //msg.To.Add("386-527-2019@mobile.att.net");
                msg.From = new MailAddress("gsspankster@gmail.com");
                msg.Subject = subject;
                msg.Body = body;
                client.Send(msg);
                MessageBox.Show("Success!");
                // WORKING WITH GMAIL !!!!!


                //// WORKING WITH ROADRUNNER !!!!!
                ////SmtpClient client = new SmtpClient("smtp-server.cfl.rr.com", 25);
                //SmtpClient client = new SmtpClient("smtp-server.cfl.rr.com", 587);
                ////client.DeliveryFormat = SmtpDeliveryFormat.International;
                //client.EnableSsl = true;
                //client.Timeout = 10000;
                //client.DeliveryMethod = SmtpDeliveryMethod.Network;
                //client.UseDefaultCredentials = false;
                //client.Credentials = new NetworkCredential("TaylorInsurance_Lance@cfl.rr.com", "00101548");
                ////client.Credentials = new NetworkCredential("TaylorInsurance_Lance", "00101548");
                //MailMessage msg = new MailMessage();
                ////msg.To.Add("386-235-2097@mobile.att.net");
                //msg.To.Add("lanceandsheri@yahoo.com");
                //msg.From = new MailAddress("TaylorInsurance_Lance@cfl.rr.com");
                //msg.Subject = textBoxSubject.Text;
                //msg.Body = textBoxMsgBody.Text;
                //client.Send(msg);
                //MessageBox.Show("Success!");
                //// WORKING WITH ROADRUNNER !!!!!


                //// WORKING WITH YAHOO !!!!!
                //SmtpClient client = new SmtpClient("smtp.mail.yahoo.com", 465);
                //client.EnableSsl = true;
                //client.Timeout = 10000;
                //client.DeliveryMethod = SmtpDeliveryMethod.Network;
                //client.UseDefaultCredentials = false;
                //client.Credentials = new NetworkCredential("lanceandsheri@yahoo.com", "A261696b");
                //MailMessage msg = new MailMessage();
                //msg.To.Add("386-235-2097@mobile.att.net");
                //msg.From = new MailAddress("gsspankster@gmail.com");
                //msg.Subject = textBoxSubject.Text;
                //msg.Body = textBoxMsgBody.Text;
                //client.Send(msg);
                //MessageBox.Show("Success!");
                //// WORKING WITH YAHOO !!!!!


                ////SmtpClient client = new SmtpClient("lanceandsheri@yahoo.com", 465);
                //SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                //client.EnableSsl = true;
                //client.Timeout = 10000;
                //client.DeliveryMethod = SmtpDeliveryMethod.Network;
                //client.UseDefaultCredentials = false;
                ////client.Credentials = new NetworkCredential("same@cfl.rr.com", "password");// 
                ////client.Credentials = new NetworkCredential("lanceandsheri@yahoo.com", "A261696b");
                //client.Credentials = new NetworkCredential("gsspankster@gmail.com", "00101548");
                //MailMessage msg = new MailMessage();
                //msg.To.Add("taylorinsofdaytona@yahoo.com");
                ////msg.From = new MailAddress("same@cfl.rr.com");
                //msg.From = new MailAddress("gsspankster@gmail.com");//  lanceandsheri@yahoo.com
                //msg.Subject = textBoxSubject.Text;
                //msg.Body = textBoxMsgBody.Text;
                //client.Send(msg);
                //MessageBox.Show("Success!");

                ////cboCarrier.Items.Add("@mobile.att.net");
                //SmtpClient client = new SmtpClient("@mobile.att.net");
                //MailMessage message = new MailMessage();
                //message.From = new MailAddress("lanceandsheri@yahoo.com");
                //message.To.Add("taylorinsofdaytona@yahoo.com");// = new MailAddress("taylorinsofdaytona@yahoo.com");
                //message.Body = ("Test Body");
                //message.Subject = ("Test Subject");
                //client.Credentials = new System.Net.NetworkCredential("lanceandsheri@yahoo.com", "A261696b");
                //client.Port = System.Convert.ToInt32(25);
                //client.Send(message);

                //MailMessage message = new MailMessage(mFrom, mTo, mSubject, mMsg);
                //SmtpClient mySmtpClient = new SmtpClient(mMailServer);
                ////mySmtpClient.UseDefaultCredentials = True;
                //mySmtpClient.UseDefaultCredentials = true;


                //mySmtpClient.Send(message);// ERROR HERE
                //MessageBox.Show("The mail message has been sent to " + message.To.ToString(), "Mail", MessageBoxButtons.OK, MessageBoxIcon.Information);



            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (SmtpException ex)
            {
                MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }








    }
}
