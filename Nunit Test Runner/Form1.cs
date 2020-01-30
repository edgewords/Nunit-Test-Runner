using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nunit_Test_Runner
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.dllpath = this.nunitdllpath.Text;
            Properties.Settings.Default.runnerpath = this.textBoxRunnerPath.Text;
            Properties.Settings.Default.Save();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.nunitdllpath.Text = Properties.Settings.Default.dllpath;
            this.textBoxRunnerPath.Text = Properties.Settings.Default.runnerpath;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.nunitdllpath.Text = this.openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.listBoxTestNames.Items.Clear();
            Assembly assembly = Assembly.LoadFrom(this.nunitdllpath.Text);
            foreach (Type type in assembly.GetTypes())
            {
                foreach (MethodInfo methodInfo in type.GetMethods())
                {
                    var attributes = methodInfo.GetCustomAttributes(true);
                    foreach (var attr in attributes)
                    {
                        if (attr.ToString() == "NUnit.Framework.TestAttribute")
                        {
                            var methodName = methodInfo.Name;
                            this.listBoxTestNames.Items.Add(methodName);
                        }
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.textBoxRunnerPath.Text = this.openFileDialog1.FileName;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.textBoxResults.Clear();
            //string selectedTest = this.listBoxTestNames.Items[listBoxTestNames.SelectedIndex].ToString();
            string selectedTest = this.listBoxTestNames.SelectedItem.ToString();
            string DllPath = "\"" + this.nunitdllpath.Text + "\"";
            string runnerPath = "\"" + this.textBoxRunnerPath.Text + "\"";
            string args = "--test=" + selectedTest + " " + DllPath;
            this.textBoxCMD.Text = runnerPath + " " + args;
            ExecuteCommand(runnerPath, args);
        }

        public bool ExecuteCommand(string exeDir, string args)
        {
            try
            {
                ProcessStartInfo procStartInfo = new ProcessStartInfo();

                procStartInfo.FileName = exeDir;
                procStartInfo.Arguments = args;
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;

                using (Process process = new Process())
                {
                    process.StartInfo = procStartInfo;
                    process.Start();

                    process.WaitForExit();

                    string result = process.StandardOutput.ReadToEnd();
                    Console.WriteLine(result);
                    this.textBoxResults.AppendText(result);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("*** Error occured executing the following commands.");
                this.textBoxResults.AppendText("**** Error occured ****");
                Console.WriteLine(exeDir);
                Console.WriteLine(args);
                Console.WriteLine(ex.Message);
                this.textBoxResults.AppendText(ex.Message);
                return false;
            }
        }
    }
}
