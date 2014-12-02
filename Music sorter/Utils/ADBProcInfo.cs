using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
namespace Music_sorter.Utils
{
    static class ADBProcInfo
    {
        public static readonly ProcessStartInfo ADB_START_SERVER = new ProcessStartInfo
        {
            FileName = Application.StartupPath + @"\utils\adb.exe",
            Arguments = "start-server",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        public static readonly ProcessStartInfo ADB_KILL_SERVER = new ProcessStartInfo
        {
            FileName = Application.StartupPath + @"\utils\adb.exe",
            Arguments = "kill-server",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
    }
}
