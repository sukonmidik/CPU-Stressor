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
        private static int currentThread = 0;
        private static int loadamount;
        private static readonly List<Thread> threadlist = new List<Thread>();
        private static readonly BackgroundWorker backgroundWorker1 = new BackgroundWorker();
        private static readonly List<long> primevalue = new List<long>();
        private static int count;
        private static long a;
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
                if (stressing)
                {
                    return;
                }
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
            switch (loadamount)
            {
                case 3: threads = 32; break;
                case 2: threads = 16; break;
                case 1: threads = 8; break;
            }
            for (int i = 0; i < threads; i++)
            {
                Random r = new Random();
                int rnd = new int();
                switch (loadamount)
                {
                    case 3: rnd = r.Next(10000000, 100000000); break;
                    case 2: rnd = r.Next(1000000, 10000000); break;
                    case 1: rnd = r.Next(100000, 1000000); break;
                }
                threadlist.Add(new Thread(() => Running(rnd)));
                threadlist[i].Priority = ThreadPriority.Lowest;
                currentThread++;
                if (bw.CancellationPending)
                {
                    Thread.Sleep(1200);
                    e.Cancel = true;
                    break;
                }
                output.Invoke((MethodInvoker)delegate
                {
                    output.AppendText("Creating Thread: " + currentThread + Environment.NewLine);
                });
                Thread.Sleep(1000);
                if (currentThread == threads)
                {
                    stressing = true;
                    output.Invoke((MethodInvoker)delegate
                    {
                        output.AppendText("Stressing..." + Environment.NewLine);
                    });
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
                        Thread.Sleep(1000);
                    }
                    while (stressing)
                    {
                        if (bw.CancellationPending)
                        {
                            Thread.Sleep(1200);
                            e.Cancel = true;
                            break;
                        }
                        if (primevalue.Count == threadlist.Count)
                        {
                            if (bw.CancellationPending)
                            {
                                Thread.Sleep(1200);
                                e.Cancel = true;
                                break;
                            }
                            if (threadlist.Count != 0)
                            {
                                for (int t = 0; t < threadlist.Count; t++)
                                {
                                    threadlist[t].Abort();
                                }
                            }
                            for (int v = 1; v < threads + 1; v++)
                            {
                                if (bw.CancellationPending)
                                {
                                    Thread.Sleep(1200);
                                    e.Cancel = true;
                                    break;
                                }                           
                                output.Invoke((MethodInvoker)delegate
                                {
                                    output.AppendText("Prime Value: " + primevalue[v - 1] + " Found On Thread: " + v + Environment.NewLine);
                                });
                                Thread.Sleep(1000);
                            }
                            break;
                        }
                    }
                    break;
                }
            }
        }
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (currentThread <= threads)
            {
                output.AppendText("Creating Thread: " + currentThread + Environment.NewLine);
            }
            if (currentThread == threads)
            {
                Thread.Sleep(1500);
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
                    primevalue.Clear();
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
                primevalue.Clear();
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
        private static void FindPrimeNumber(long n)
        {
            count = 0;
            a = 2;
            while (count < n)
            {
                if (n == 0)
                {
                    backgroundWorker1.CancelAsync();
                }
                long b = 2;
                int prime = 1;// to check if found a prime
                while (b * b <= a)
                {
                    if (n == 0)
                    {
                        backgroundWorker1.CancelAsync();
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
            primevalue.Add(--a);
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
