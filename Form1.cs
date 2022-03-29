using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.DoWork += backgroundWorker1_DoWork;
            backgroundWorker1.ProgressChanged += backgroundWorker1_ProgressChanged;
            backgroundWorker1.RunWorkerCompleted += backgroundWorker1_RunWorkerCompleted1;
        }
        private static bool stressing;
        private static int threads;
        private static int currentThread;
        private static int loadamount;
        private static readonly List<Thread> threadlist = new List<Thread>();
        private static bool primefound;
        private static readonly BackgroundWorker backgroundWorker1 = new BackgroundWorker();
        private static bool calculating;
        private static long primevalue;
        public static string GetCPU()
        {
            ManagementObjectSearcher mosProcessor = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            string Procname = null;
            foreach (ManagementObject moProcessor in mosProcessor.Get())
            {
                if (moProcessor["name"] != null)
                {
                    Procname = moProcessor["name"].ToString();
                }
            }
            return Procname;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            output.AppendText("Detected CPU: " + GetCPU() + Environment.NewLine);
            output.AppendText("Logical Core Count: " + Environment.ProcessorCount.ToString() + Environment.NewLine);
            output.AppendText("Idle..." + Environment.NewLine);
        }
        private void startStress_Click(object sender, EventArgs e)
        {
            DialogResult confirm = MessageBox.Show("Not Responsible For Any Damage To PC. Higher Thread Count Will Be A More Intensive Test. Are You Sure You Want To Proceed?", "Caution", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm == DialogResult.Yes)
            {
                if (radioButton3.Checked || radioButton2.Checked || radioButton1.Checked)
                {
                    currentThread = 0;
                    radioButton1.Enabled = false;
                    radioButton2.Enabled = false;
                    radioButton3.Enabled = false;
                    StressCPU();
                }
                else
                {
                    MessageBox.Show("Please Select Stress Level.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            else if (confirm == DialogResult.No)
            {
                MessageBox.Show("Test Canceled.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                radioButton1.Enabled = true;
                radioButton2.Enabled = true;
                radioButton3.Enabled = true;
                return;
            }
        }
        private void stop_Click(object sender, EventArgs e)
        {

            stressing = false;
            backgroundWorker1.CancelAsync();
            radioButton1.Enabled = true;
            radioButton2.Enabled = true;
            radioButton3.Enabled = true;
        }
        public static void StressCPU()
        {
            if (!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }
        private static void Running(int load)
        {
            while (stressing)
            {
                if (!stressing)
                {
                    return;
                }
                FindPrime(load);
            }
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            if (bw.CancellationPending)
            {
                Thread.Sleep(1200);
                e.Cancel = true;
            }
            if (calculating)
            {
                return;
            }
            switch (loadamount)
            {
                case 3: threads = 64; break;
                case 2: threads = 32; break;
                case 1: threads = 16; break;
            }
            for (int i = 0; i < threads; i++)
            {
                Random r = new Random();
                int rnd = new int();
                switch (loadamount)
                {
                    case 3: rnd = r.Next(40, 80); break;
                    case 2: rnd = r.Next(20, 40); break;
                    case 1: rnd = r.Next(10, 20); break;
                }
                threadlist.Add(new Thread(ThreadStart => Running(rnd)));
                threadlist[i].Priority = ThreadPriority.Lowest;
                if (bw.CancellationPending)
                {
                    Thread.Sleep(1200);
                    e.Cancel = true;
                    break;
                }
                backgroundWorker1.ReportProgress(i);
                currentThread++;
                Thread.Sleep(1000);
                if (currentThread == threads)
                {
                    calculating = true;
                    stressing = true;
                    currentThread = 0;
                    if (bw.CancellationPending)
                    {
                        Thread.Sleep(1200);
                        e.Cancel = true;
                        break;
                    }
                    for (int x = 0; x < threads; x++)
                    {
                        if (bw.CancellationPending)
                        {
                            Thread.Sleep(1200);
                            e.Cancel = true;
                            break;
                        }
                        threadlist[x].Start();
                        backgroundWorker1.ReportProgress(x);
                        currentThread++;
                        Thread.Sleep(1000);
                    }
                    if (FindPrimeNumber(0) == 0)
                    {
                        Thread.Sleep(1200);
                        e.Cancel = true;
                        break;
                    }
                }
            }
        }
        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            if (currentThread <= threads)
            {
                output.AppendText("Creating Thread: " + currentThread + Environment.NewLine);
            }
            if (currentThread == threads)
            {
                Thread.Sleep(1000);
                output.AppendText("Stressing..." + Environment.NewLine);
            }
        }
        private void backgroundWorker1_RunWorkerCompleted1(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                try
                {
                    if (threadlist.Count != 0)
                    {
                        for (int t = 0; t < threadlist.Count; t++)
                        {
                            threadlist[t].Abort();
                        }
                    }
                    backgroundWorker1.Dispose();
                    calculating = false;
                    primefound = false;
                    stressing = false;
                    currentThread = 0;
                    threadlist.Clear();
                    output.AppendText("Stress Test Stopped..." + Environment.NewLine);
                }
                catch
                {
                    return;
                }
            }
            else
            {
                for (int i = 1; i < threads + 1; i++)
                {
                    if (primefound)
                    {
                        Thread.Sleep(1000);
                        output.AppendText("Prime Value: " + primevalue + " Found On Thread: " + i + Environment.NewLine);
                        Thread.Sleep(1000);
                    }
                }
                calculating = false;
                primefound = false;
                stressing = false;
                currentThread = 0;
                threadlist.Clear();
                StressCPU();
            }
        }

        private static void FindPrime(int val)
        {
            FindPrimeNumber(val); //set higher value for more time
        }
        private static long FindPrimeNumber(long n)
        {
            int count = 0;
            long a = 2;
            while (count < n)
            {
                if (backgroundWorker1.CancellationPending)
                {
                    return 0;
                }
                long b = 2;
                int prime = 1;// to check if found a prime
                while (b * b <= a)
                {
                    if (backgroundWorker1.CancellationPending)
                    {
                        return 0;
                    }
                    if (a % b == 0)
                    {
                        prime = 0;
                        break;
                    }
                    b++;
                }
                if (prime > 0)
                {
                    count++;
                }
                a++;
            }
            primefound = true;
            primevalue = --a;
            return --a;
        }
        private void Stresser_FormClosing(object sender, FormClosingEventArgs e)
        {
            stressing = false;
            backgroundWorker1.CancelAsync();
            Dispose();
        }
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            loadamount = 3;
        }
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            loadamount = 2;
        }
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            loadamount = 1;
        }
    }
}
