using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Music_sorter.Utils;
namespace Music_sorter
{
    public partial class Form1 : Form
    {
        bool t1 = false;
        bool t2 = false;

        string inputDir;
        string outputDir;
        string nl = Environment.NewLine;
        List<string> musicFiles = new List<string>();
        List<string> musicFilesNew = new List<string>();

        public Form1()
        {
            InitializeComponent();
        }

        private void txtInput1_TextChanged(object sender, EventArgs e)
        {
            if (txtInput1.Text.Equals("")) { t1 = false; }
            if (!txtInput1.Text.Equals("")) { t1 = true; }
            TxtCheck();
        }

        private void txtInput2_TextChanged(object sender, EventArgs e)
        {
            if (txtInput2.Text.Equals("")) { t2 = false; }
            if (!txtInput2.Text.Equals("")) { t2 = true; }
            TxtCheck();
        }

        private void btnBrowse1_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fd = new FolderBrowserDialog())
            {
                fd.Description = "Select folder with your organized music";
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    txtInput1.Text = fd.SelectedPath;
                    inputDir = fd.SelectedPath;
                }
            }
        }

        private void btnBrowse2_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fd = new FolderBrowserDialog())
            {
                fd.Description = "Select folder where you want your music to go";
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    txtInput2.Text = fd.SelectedPath;
                    outputDir = fd.SelectedPath;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (!File.Exists(Application.StartupPath + @"\utils\adb.exe") || !File.Exists(Application.StartupPath + @"\utils\AdbWinApi.dll"))
            {
                MessageBox.Show("A required component could not be found. Please reinstall the program.", "Fatal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }

            AddText("Music sorter for Google Play!" + nl);
            AddText("by Alex Restifo" + nl);
            AddText("----------------------------------" + nl + nl);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /* Oh god more threading... */
            Thread procThread = new Thread(new ParameterizedThreadStart(RunProc));
            procThread.Priority = ThreadPriority.Highest;
            procThread.Start(ADBProcInfo.ADB_START_SERVER);
        }

        private void btnSort_Click(object sender, EventArgs e)
        {
            progress.Visible = true;
            btnSort.Enabled = false;
            txtProgress.Text = "Working...";
            AddText("Starting..." + nl);

            Thread workThread = new Thread(new ThreadStart(DoSort));
            workThread.Priority = ThreadPriority.Highest;
            workThread.Start();
        }

        public void DoSort()
        {
            DirectoryInfo parentDir = new DirectoryInfo(inputDir);
            foreach (FileInfo f in parentDir.GetFiles())
            {
                musicFiles.Add(f.FullName);
            }
            DoFindMusic(parentDir);

            UpdateProp(txtDebug, "Text", musicFiles.Count + " songs detected!" + nl + nl);
            UpdateProp(txtDebug, "Text", "----------------------------" + nl + nl);

            UpdatePropStatus(progress, "Maximum", musicFiles.Count);
            DoCopyFiles();

            if (MessageBox.Show("Would you like to take away track numbers?" + nl + "(01. <songname> --> <songname>)", "Info", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) { DoRemoveNumbers(); }

            MessageBox.Show("Done copying " + musicFiles.Count + " files!");
            UpdateProp(btnSort, "Enabled", true);
            UpdatePropStatus(progress, "Visible", false);
            UpdatePropStatus(txtProgress, "Text", "Ready");
        }

        private void DoRemoveNumbers()
        {
            UpdatePropStatus(txtProgress, "Text", "Renaming...");
            for (int i = 0; i < musicFilesNew.Count; i++)
            {
                string oldFile = musicFilesNew[i];
                if (!(oldFile.ToLower().EndsWith(".mp3") || oldFile.ToLower().EndsWith(".wav"))) { continue; }

                string withoutExt = oldFile.Substring(0, oldFile.LastIndexOf("\\") + 1);
                string rawFileName = oldFile.Substring(oldFile.LastIndexOf("\\") + 1);
                string newFile = rawFileName.Substring(rawFileName.IndexOf(".") + 2);
                string final = withoutExt + newFile;
                File.Move(oldFile, final);
            }
        }

        private void DoFindMusic(DirectoryInfo dir)
        {
            foreach (DirectoryInfo subDir in dir.GetDirectories("*", SearchOption.AllDirectories))
            {
                foreach (FileInfo f in subDir.GetFiles())
                {
                    if (f.Extension.ToLower() == ".mp3" || f.Extension.ToLower() == ".wav") // search for WAVs, I guess....
                    {
                        musicFiles.Add(f.FullName);
                    }
                }
                if (subDir.GetDirectories().Length != 0)
                {
                    foreach (DirectoryInfo evenMoreSubDir in subDir.GetDirectories("*", SearchOption.AllDirectories))
                    {
                        DoFindMusic(evenMoreSubDir);
                    }
                }
                else { continue; }
            }
        }

        public void DoCopyFiles()
        {
            int i = 1;
            foreach (string s in musicFiles)
            {
                string name = s.Substring(s.LastIndexOf("\\") + 1);
                musicFilesNew.Add(outputDir + "\\" + name); //This is adding to a list which contains the locations of the NEW music files AFTER they are copied
                File.Copy(s, outputDir + "\\" + name, true);

                UpdateProp(txtDebug, "Text", "Copied file " + i + "!" + nl);
                UpdatePropStatus(txtProgress, "Text", "Copied file " + i + "/" + musicFiles.Count);
                UpdatePropStatus(progress, "Value", i);
                i++;
            }
        }

        private void TxtCheck()
        {
            if (t1 && t2 == true)
            { btnSort.Enabled = true; }
            else
            { btnSort.Enabled = false; }
        }

        private void AddText(string txt)
        {
            txtDebug.Text += txt;
        }
        #region Experimental!
        public void RunProc(object psi)
        {
            var proc = new Process();
            proc.StartInfo = (ProcessStartInfo) psi;
            proc.EnableRaisingEvents = true;
            proc.ErrorDataReceived += proc_DataReceived;
            proc.OutputDataReceived += proc_DataReceived;

            proc.Start();

            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();
        }

        private void proc_DataReceived(object sender, DataReceivedEventArgs e)
        {
            MessageBox.Show("Data received! Data: " + e.Data);
            UpdateProp(txtDebug, "Text", e.Data + nl);
        }
        #endregion
        #region Thread-Safety
        // UpdateProp() and UpdatePropStatus() allows for cross-threaded updates to the GUI, but it's ugly code.
        private delegate void TSDelegate(Control control, string propertyName, object propertyValue);
        private void UpdateProp(Control control, string propertyName, object propertyValue)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new TSDelegate(UpdateProp), new object[] { control, propertyName, propertyValue });
            }
            else
            {
                control.GetType().InvokeMember(propertyName, BindingFlags.SetProperty, null, control, new object[] { propertyValue });
            }
        }

        private delegate void ToolStripThreadSafeDelegate(ToolStripItem toolstripItem, string propertyName, object propertyValue);
        private void UpdatePropStatus(ToolStripItem toolstripItem, string propertyName, object propertyValue)
        {
            if (toolstripItem.Owner.InvokeRequired)
            {
                ToolStripThreadSafeDelegate callback = new ToolStripThreadSafeDelegate(UpdatePropStatus);
                toolstripItem.Owner.Invoke(callback, new object[] { toolstripItem, propertyName, propertyValue });
            }
            else
            {
                Type t = toolstripItem.GetType();
                PropertyInfo p = t.GetProperty(propertyName);
                p.SetValue(toolstripItem, propertyValue, null);
            }
        }
        #endregion
    }
}
