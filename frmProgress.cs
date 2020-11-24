using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackgroundWorkerApp
{
    public partial class frmProgress : Form
    {
        public frmProgress()
        {
            InitializeComponent();
        }

        struct DataProcess {
            public int Process;
            public int Delay;
        };

        private async void btnStart_Click(object sender, EventArgs e)
        {
            DataProcess setting = new DataProcess()
            {
                Delay = 10,
                Process = 100
            };

            if (!bgwProcess.IsBusy)
            {
                bgwProcess.RunWorkerAsync(setting);
            }

            var progress = new Progress<ProgressReport>();
            progress.ProgressChanged += Task_ProgressChanged;

            await ProcessData_Task(setting, progress);
            MessageBox.Show("Task has been completed", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }



        private void btnStop_Click(object sender, EventArgs e)
        {
            if (bgwProcess.IsBusy)
            {
                bgwProcess.CancelAsync();
            }
        }

        private void bgwProcess_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            lblPercent.Text = e.ProgressPercentage + " %";
        }

        private void bgwProcess_DoWork(object sender, DoWorkEventArgs e)
        {
            int process = ((DataProcess)e.Argument).Process;
            int delay = ((DataProcess)e.Argument).Delay;

            try
            {
                for (int i = 0; i < process; i++)
                {
                    if (bgwProcess.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    Thread.Sleep(delay);
                    int percentProcess = (i + 1) * 100 / process;
                    bgwProcess.ReportProgress(percentProcess);
                    Console.WriteLine($"Processing: {percentProcess}%");
                }
            }
            catch (Exception ex)
            {
                bgwProcess.CancelAsync();
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void bgwProcess_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Console.WriteLine("Process has been cancelled");
            }
            else
            {
                MessageBox.Show("Process has been completed", "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private Task ProcessData_Task(DataProcess setting, IProgress<ProgressReport> progress)
        {
            int process = setting.Process;
            int delay = setting.Delay;

            return Task.Run(() =>
            {
                for (int i = 0; i < process; i++)
                {
                    Thread.Sleep(delay * 2);
                    int percentProcess = (i + 1) * 100 / process;
                    //UpdateProgressTask(percentProcess);
                    progress.Report(new ProgressReport() { PercentComplete = percentProcess });

                    Console.WriteLine($"Task: {percentProcess}%");
                }
            });
        }

        private void UpdateProgressTask(int percent)
        {
            this.Invoke((MethodInvoker)delegate
            {
                progressBar1.Value = percent;
                lblPercentTask.Text = percent + " %";
            });
        }

        private void Task_ProgressChanged(object sender, ProgressReport e)
        {
            progressBar1.Value = e.PercentComplete;
            lblPercentTask.Text = e.PercentComplete + " %";
        }

        class ProgressReport
        {
            public int PercentComplete { get; set; }
        }
    }
}
