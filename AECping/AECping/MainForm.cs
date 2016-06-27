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
using System.Diagnostics;
using System.Net.NetworkInformation;

using IniParser;
using IniParser.Model;

namespace AECping
{
    
    public partial class MainForm : Form
    {
        int ping_period;
        int failed_ping_alarm;
        int numPanels;

        FileIniDataParser parser;
        string PanelININame;
        IniData data ;

        BackgroundWorker bw_resumer = new BackgroundWorker();

        List<BackgroundWorker> bw_list = new List<BackgroundWorker>();
        List<bool> pingexceptionraised_list = new List<bool>();
        List<int> pingexceptionfailures_list = new List<int>();
        List<int> failedping_list = new List<int>();

        //BackgroundWorker bw = new BackgroundWorker();
        //BackgroundWorker bw2 = new BackgroundWorker();
        //bool pingexceptionraised = false;
        //int pingexpectionfailures = 0;

        public MainForm()
        {
            InitializeComponent();

            parser = new FileIniDataParser();
            data = parser.ReadFile("AECPingConfig.ini");
            numPanels = Convert.ToInt16(data["Config"]["Panels"]);
            ping_period = Convert.ToInt16(data["Config"]["Ping_period"]);
            failed_ping_alarm = Convert.ToInt16(data["Config"]["Failed_Ping_Alarm"]);

            bw_resumer.WorkerSupportsCancellation = true;
            bw_resumer.DoWork += new DoWorkEventHandler(resume_thread);

            //bw.WorkerSupportsCancellation = true;
            //bw.WorkerReportsProgress = true;
            //bw.DoWork += new DoWorkEventHandler(pingproc);
            //bw.ProgressChanged +=new ProgressChangedEventHandler(bw_ProgressChanged);
            //bw.RunWorkerCompleted +=new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

            //bw2.WorkerSupportsCancellation = true;
            //bw2.WorkerReportsProgress = true;
            //bw2.DoWork += new DoWorkEventHandler(pingproc);
            //bw2.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            //bw2.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

            for (int i = 0; i < numPanels; i++)
            {

                BackgroundWorker bw = new BackgroundWorker();
                bw.WorkerSupportsCancellation = true;
                bw.WorkerReportsProgress = true;
                bw.DoWork += new DoWorkEventHandler(pingproc);
                bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
                bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

                bw_list.Add(bw);

                pingexceptionraised_list.Add(false);
                pingexceptionfailures_list.Add(0);
                failedping_list.Add(0);
            }


            
        }


        private void MainForm_Load(object sender, EventArgs e)
        {

            for (int i = 1; i <= numPanels; i++)
            {
                PanelININame = "Panel" + i;
                string s = data["Panels"][PanelININame];
                string[] line = s.Split(',');
                dataGridView1.Rows.Add(line[0], line[1]);
            }

            btn_pingstop.Enabled = false;

        }

        private void resume_thread(object sender, DoWorkEventArgs e)
        {
            
            BackgroundWorker worker = sender as BackgroundWorker;
            
            Debug.WriteLine("bwresumerID:" + Thread.CurrentThread.ManagedThreadId);


            while (true)
            {

                if ((worker.CancellationPending == true))
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    int i = 0;
                    foreach (bool s in pingexceptionraised_list)
                    {

                        if (s)
                        {
                            if (bw_list[i].IsBusy != true)
                            {
                                failedping_list[i]=0;
                                bw_list[i].RunWorkerAsync(i);
                            }
                        }

                        i++;
                    }

                    Thread.Sleep(ping_period);
                };

            }
        }

        private void pingproc(object sender, DoWorkEventArgs e)
        {
            int num_panel = (int)e.Argument;

            try
            {
                BackgroundWorker worker = sender as BackgroundWorker;
                
                Debug.WriteLine("bw"+ num_panel+"_ID:"+Thread.CurrentThread.ManagedThreadId);
                
                Ping Ping_Sender = new Ping();
                PingReply Ping_Reply;
                PingOptions Ping_Options = new PingOptions();

                string IPaddress = dataGridView1.Rows[num_panel].Cells[1].Value.ToString();
                int ping_timeout = 120;
                string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                byte[] ping_buffer = Encoding.ASCII.GetBytes(data);

         
                Ping_Options.Ttl = 128;
                Ping_Options.DontFragment = true;

                dataGridView1.Rows[num_panel].Cells[3].Style.BackColor = Color.Silver;

                Debug.WriteLine("Ping thread started");

                while (true)
                {

                    if ((worker.CancellationPending == true))
                    {
                        e.Cancel = true;
                        break;
                    }
                    else
                    {

                        
                        Ping_Reply = Ping_Sender.Send(IPaddress, ping_timeout, ping_buffer, Ping_Options);

                        if (Ping_Reply.Status != IPStatus.Success)
                        {
                            failedping_list[num_panel]++;
                            pingexceptionfailures_list[num_panel] = 0;
                        }
                        else
                        {
                            failedping_list[num_panel] = 0;
                            pingexceptionfailures_list[num_panel] = 0;
                        }

                        dataGridView1.Rows[num_panel].Cells[2].Value = Ping_Reply.RoundtripTime + "ms";
                        dataGridView1.Rows[num_panel].Cells[3].Value = Ping_Reply.Status + "(" + failedping_list[num_panel] + ")";

                        //Debug.WriteLine("Host:{3}, Status:{0}, RTT:{1}, TTL:{2}", Ping_Reply.Status, Ping_Reply.RoundtripTime, Ping_Reply.Options.Ttl, (int)e.Argument);

                        if (failedping_list[num_panel] == 0)
                            dataGridView1.Rows[num_panel].Cells[3].Style.BackColor = Color.LightGreen;
                        else if (failedping_list[num_panel] < failed_ping_alarm)
                            dataGridView1.Rows[num_panel].Cells[3].Style.BackColor = Color.Yellow;
                        else
                            dataGridView1.Rows[num_panel].Cells[3].Style.BackColor = Color.Red;


                        Thread.Sleep(ping_period);

                    };

                    //worker.ReportProgress((i * 10));
                }


            }

            catch (ThreadAbortException)
            {
                Debug.Write("bw" + num_panel+ ":Thread Abort Exception");
            }

            catch (NullReferenceException)
            {
                Debug.Write("bw"+ num_panel + ": NullReference Exception");
            }

            catch (PingException)
            {
                pingexceptionraised_list[num_panel] = true;
                pingexceptionfailures_list[num_panel]++;

                Debug.Write("bw" + num_panel + ":Ping Exception");
                dataGridView1.Rows[num_panel].Cells[3].Style.BackColor = Color.Orange;
                dataGridView1.Rows[num_panel].Cells[3].Value = "PING ERR("+pingexceptionfailures_list[num_panel]+")";

            }

        }


        private void btn_pingstart_Click(object sender, EventArgs e)
        {

            Debug.WriteLine("Enabling Ping thread");

            int i = 0;
            foreach (BackgroundWorker s in bw_list)
            {

                if (s.IsBusy != true)
                {
                    s.RunWorkerAsync(i);
                }

                i++;
            }

            
            
            if (bw_resumer.IsBusy != true)
            {
                bw_resumer.RunWorkerAsync();
            }

            btn_pingstart.Enabled = false;
            btn_pingstop.Enabled = true;

        }

        private void btn_pingstop_Click(object sender, EventArgs e)
        {

            Debug.WriteLine("Disabling Ping thread");


            int i = 0;
            foreach (BackgroundWorker s in bw_list)
            {

                if (s.WorkerSupportsCancellation == true)
                {
                    s.CancelAsync();
                }

                i++;
            }


            if (bw_resumer.WorkerSupportsCancellation == true)
            {
                bw_resumer.CancelAsync();
            }

            btn_pingstart.Enabled = true;
            btn_pingstop.Enabled = false;

        }


        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Console.WriteLine("Thread Stopped ");
            
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //this.tbProgress.Text = (e.ProgressPercentage.ToString() + "%");
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                //this.tbProgress.Text = "Canceled!";
                Debug.WriteLine("Thread Canceled!");
            }

            else if (!(e.Error == null))
            {
                //this.tbProgress.Text = ("Error: " + e.Error.Message);
                Debug.WriteLine("Thread Error: " + e.Error.Message);
            }

            else

            {
                //this.tbProgress.Text = "Done!";
                Debug.WriteLine("Thread Done!");

               
            }

        }

        
    }
}
