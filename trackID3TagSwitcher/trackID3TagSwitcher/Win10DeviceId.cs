using System;
using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace trackID3TagSwitcher
{
    public static class DeviceInfo
    {
        public static PhysicalAddress GetMacAddress( )
        {
            foreach ( NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces( ) )
            {
                // Only consider Ethernet network interfaces
                if ( nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                    nic.OperationalStatus == OperationalStatus.Up )
                {
                    return nic.GetPhysicalAddress( );
                }
            }
            return null;
        }

        public static string getCPUId( )
        {
            string cpuInfo = string.Empty;
            ManagementClass mc = new ManagementClass( "win32_processor" );
            ManagementObjectCollection moc = mc.GetInstances( );

            foreach ( ManagementObject mo in moc )
            {
                if ( cpuInfo == "" )
                {
                    //Get only the first CPU's ID
                    cpuInfo = mo.Properties["processorID"].Value.ToString( );
                    break;
                }
            }
            return cpuInfo;
        }

        public static string getUUID( )
        {
            Process process = new Process( );
            ProcessStartInfo startInfo = new ProcessStartInfo( );
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "CMD.exe";
            startInfo.Arguments = "/C wmic csproduct get UUID";
            process.StartInfo = startInfo;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start( );
            process.WaitForExit( );
            string output = process.StandardOutput.ReadToEnd( );

            output = output.Replace( "UUID" , "" );
            output = output.Replace( Environment.NewLine , "" );
            output = output.Replace( "                                  " , "" );
            output = output.Replace( "\r" , "" );
            output = output.Replace( " " , "" );

            return output;
        }
    }
}