using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PC_Heal_Service
{
    public partial class PC_Heal : ServiceBase
    {
        Timer timer = null;
        public PC_Heal()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timer = new Timer();
            timer.Interval = 1000;
            timer.Elapsed += timer_Tick;
            timer.Enabled = true;
            Utilities.WriteFile("Service started!");
            Utilities.WriteFile(StaticInformation());
        }

        private void timer_Tick(object sender, ElapsedEventArgs args)
        {
            // Xử lý một vài logic ở đây
            Utilities.WriteFile(StaticInformation() + NonStaticInformation());
        }

        protected override void OnStop()
        {
            // Ghi log lại khi Services đã được stop
            timer.Enabled = true;
            Utilities.WriteFile("Service stopped!");
        }
        string StaticInformation()
        {
            string result = "";
            using (var computer_System = new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem").Get())
            {
                foreach (var item in computer_System)
                {
                    result += "Computer Name: " + item["Name"].ToString() + "\n";
                    result += "NumberOfProcessors: " + item["NumberOfProcessors"].ToString() + "\n";
                    result += "NumberOfLogicalProcessors: " + item["NumberOfLogicalProcessors"].ToString() + "\n";
                }
            }

            using (var processor = new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
            {
                foreach (var item in processor)
                {
                    result += "Processor: " + item["Name"].ToString() + "\n";
                }
            }
            //get IP, MAC Address
            ManagementClass mgmt = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objCol = mgmt.GetInstances();
            string IP = String.Empty;
            string MAC = String.Empty;

            using (var network = new ManagementClass("Win32_NetworkAdapterConfiguration"))
            {


                foreach (var item in network.GetInstances())
                {
                    if (MAC == String.Empty)
                    {
                        if ((bool)item["IPEnabled"] == true)
                        {
                            MAC = item["MACAddress"].ToString();
                            String[] IPs = (String[])item["IPAddress"];
                            IP = IPs[0];
                        }
                    }
                }
            }
            result += "IP Address: " + IP + "\n";
            result += "Mac Address: " + MAC + "\n";

            return result;

        }
        string NonStaticInformation()
        {
            string result = "";
            DriveInfo dDisk = new DriveInfo("D");
            DriveInfo cDisk = new DriveInfo("C");

            result += "C Disk Free: " + ((cDisk.AvailableFreeSpace / (float)cDisk.TotalSize) * 100).ToString("00.00") + "%\n";
            result += "D Disk Free: " + ((dDisk.AvailableFreeSpace / (float)dDisk.TotalSize) * 100).ToString("00.00") + "%\n";

            using (var processor = new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_PerfFormattedData_PerfOS_Processor").Get())
            {
                double total = 0;
                int n = 0;
                foreach (var item in processor)
                {
                    total += Convert.ToDouble(item["PercentProcessorTime"]);
                    n++;
                }

                result += "CPU Usage: " + String.Format("{0:0.0}", 
                    total / n) + "%\n";
            }

            using (var operating = new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_OperatingSystem").Get())
            {
                foreach (var item in operating)
                {
                    result += "RAM Usage: " + ((int.Parse(item["FreePhysicalMemory"].ToString()) / (float)int.Parse(item["TotalVisibleMemorySize"].ToString())) * 100).ToString("00.00") + "%\n";
                }
            }
            return result;
        }
    }
}
