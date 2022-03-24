using System;
using System.Management;
using System.Threading;
using System.Windows.Forms;
namespace CPU_Stress_Test
{
    public partial class Stresser : Form
    {
        public Stresser()
        {
            InitializeComponent();
        }
        private static bool stressing;
        private static int threads;
        private static int currentThread;
        private void Form1_Load(object sender, EventArgs e)
        {
            output.AppendText("Idle" + Environment.NewLine);
        }
        private void startStress_Click(object sender, EventArgs e)
        {
            DialogResult confirm = MessageBox.Show("Not Responsible For Any Damage To PC. Higher Thread Count Will A More Intensive Test. Are You Sure You Want To Proceed?", "Caution", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm == DialogResult.Yes)
            {
                threads = Convert.ToInt32(threadAmount.Value);
                stressing = true;
                StressCPU(threads);
            }
            else if (confirm == DialogResult.No)
            {
                MessageBox.Show("Test Canceled.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }
        private void stop_Click(object sender, EventArgs e)
        {
            stressing = false;
            backgroundWorker1.CancelAsync();
            output.AppendText(Environment.NewLine + "Idle" + Environment.NewLine);
        }
        private void StressCPU(int Threads)
        {
            for (int i = 1; i < threads + 1; i++)
            {
                output.AppendText("Creating Thread: " + i + Environment.NewLine);
            }
            output.AppendText("Stressing...");
            if (!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }
        private static void Running()
        {
            while (stressing)
            {
                if (!stressing)
                {
                    stressing = false;
                }
            }
        }
        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            ThreadStart burn = new ThreadStart(Running);
            currentThread = 1;
            while (currentThread < threads)
            {
                if (backgroundWorker1.CancellationPending)
                {
                    return;
                }
                Thread NewThread = new Thread(burn);
                NewThread.Start();
                currentThread++;
            }
        }
    }
}
