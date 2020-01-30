using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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

        //open the DLL button
        private void button1_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.nunitdllpath.Text = this.openFileDialog1.FileName;
            }
        }

        //retrieve test case names button
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

        

        //results path
        private void button5_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                this.textBoxResultsFolder.Text = this.folderBrowserDialog1.SelectedPath;
            }

        }

        //console runner selection button
        private void button3_Click(object sender, EventArgs e)
        {
            if (this.openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.textBoxRunnerPath.Text = this.openFileDialog1.FileName;
            }
        }

        //run the tests button
        private void button4_Click(object sender, EventArgs e)
        {
            string selectedTest="";
            this.textBoxResults.Clear();
            // build comma delimited list of selected tests:
            bool firstTest = true;
            foreach (object selectedItem in listBoxTestNames.SelectedItems)
            {
                if (firstTest)
                {
                    selectedTest += selectedItem.ToString();
                    firstTest = false;
                }
                else
                    selectedTest += "," + selectedItem.ToString();
            }
            //string selectedTest = this.listBoxTestNames.SelectedItem.ToString();
            string DllPath = "\"" + this.nunitdllpath.Text + "\"";
            string runnerPath = "\"" + this.textBoxRunnerPath.Text + "\"";
            string resultsPath = "\"" + this.textBoxResultsFolder.Text + "\"";
            string args = "--test=" + selectedTest + " --result=" + resultsPath +"\\TestResult.xml" + " " + DllPath;
            this.textBoxCMD.Text = runnerPath + " " + args;
            bool testsRan = ExecuteCommand(runnerPath, args);
            //now convert results to html
            // This uses a free tool called nure.exe - https://github.com/eger-geger/nunit-html-report
            if (testsRan)
            {
                string path = Directory.GetCurrentDirectory();
                string pathToNure = "\"" + path + "\\Nure.1.2.0\\tools\\nure.exe\"";
                this.textBoxResults.AppendText(pathToNure + " " + resultsPath + "\\TestResult.xml -o \"" + resultsPath + "\" --html");
                ExecuteCommand(pathToNure, resultsPath + "\\TestResult.xml -o \"" + resultsPath + "\" --html");

            }
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

        private void Form1_Load(object sender, EventArgs e)
        {
            this.nunitdllpath.Text = Properties.Settings.Default.dllpath;
            this.textBoxRunnerPath.Text = Properties.Settings.Default.runnerpath;
            this.textBoxResultsFolder.Text = Properties.Settings.Default.respath;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
                Properties.Settings.Default.dllpath = this.nunitdllpath.Text;
                Properties.Settings.Default.runnerpath = this.textBoxRunnerPath.Text;
                Properties.Settings.Default.respath = this.textBoxResultsFolder.Text;
                Properties.Settings.Default.Save();
                //Properties.Settings.Default.Upgrade();
        }
    }
}
