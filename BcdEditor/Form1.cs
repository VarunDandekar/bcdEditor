using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BcdEditor
{
    public partial class BcdEditor : Form
    {
        string path = "";
        string guid = "3ddd0b4b-54c6-4088-805b-3bb618e2597d";
        public BcdEditor()
        {
            InitializeComponent();
        }
        delegate void SetTextCallback(string text);


        private void SetText(string text)
        {

            using (StreamWriter w = File.AppendText("myFile.txt"))
            {
                w.WriteLine(text);
            }
            // invokerequired required compares the thread id of the
            // calling thread to the thread id of the creating thread.
            // if these threads are different, it returns true.
            if (this.label2.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.label2.Text = text;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            path = textBox1.Text;
            if (!File.Exists(path))
            {
                label2.Text = "File not found";
                return;
            }
            if (Path.GetExtension(path) != ".WIM")
            {
                label2.Text = "Invalid File Format(.WIM expected)";
                return;
            }
            AddEntry(path,guid);
            label2.Text = "Entry Added";
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = @"C:\";

            openFileDialog1.Title = "Browse WinPe File";

            openFileDialog1.CheckFileExists = true;

            openFileDialog1.CheckPathExists = true;

            openFileDialog1.DefaultExt = "wim";

            openFileDialog1.Filter = "windows image files (*.wim)|*.wim|All files (*.*)|*.*";

            openFileDialog1.FilterIndex = 2;

            openFileDialog1.RestoreDirectory = true;

            openFileDialog1.ReadOnlyChecked = true;

            openFileDialog1.ShowReadOnly = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
        }

        public void AddEntry(string path, string guid)
        {
            int colon = path.IndexOf(":");
            path = path.Substring(0, colon - 1) + "[" + path.Substring(colon - 1, colon+1) + "]" + path.Substring(colon+1);
            SetText(path);
            try
            {
                ExecuteCommand("%systemroot%\\sysnative\\bcdedit /delete {"+guid+"}");
                ExecuteCommand("%systemroot%\\sysnative\\bcdedit /create {" + guid + "} /d 'Mayfair' /application osloader");
                ExecuteCommand("%systemroot%\\sysnative\\bcdedit /displayorder {" + guid + "} /addlast");
                ExecuteCommand("%systemroot%\\sysnative\\bcdedit /set {" + guid + "} device ramdisk=" + path + ",{ramdiskoptions}");
                ExecuteCommand("%systemroot%\\sysnative\\bcdedit /set {" + guid + "} osdevice ramdisk=" + path + ",{ramdiskoptions}");
                ExecuteCommand("%systemroot%\\sysnative\\bcdedit /set {" + guid + "} path \\Windows\\sysnative\\Boot\\winload.exe");
                ExecuteCommand("%systemroot%\\sysnative\\bcdedit /set {" + guid + "} locale en-Us");
                ExecuteCommand("%systemroot%\\sysnative\\bcdedit /set {" + guid + "} systemroot \\Windows");
                ExecuteCommand("%systemroot%\\sysnative\\bcdedit /set {" + guid + "} detecthal Yes");
                ExecuteCommand("%systemroot%\\sysnative\\bcdedit /set {" + guid + "} winpe Yes");
            }
            catch (Exception e)
            {
                SetText(e.Message);
            }
        }

        public void DeleteEntry(string guid)
        {
            try
            {
                ExecuteCommand("%systemroot%\\sysnative\\bcdedit /delete {" + guid + "}");
            }
            catch (Exception e)
            {
                SetText(e.Message);
            }
        }

        public void ExecuteCommand(string Command)
        {

            ProcessStartInfo startInfo = new ProcessStartInfo("cmd", "/C "+ Command)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                //Verb = "runas",
                CreateNoWindow = true
            };

            Process process = Process.Start(startInfo);
            if ( process != null)
            {
                process.Start();
                SetText("output: " + process.StandardOutput.ReadToEnd());
                SetText("error: " + process.StandardError.ReadToEnd());
                process.WaitForExit();
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            DeleteEntry(guid);
            label2.Text = "Entry Deleted";
        }

    }
}
