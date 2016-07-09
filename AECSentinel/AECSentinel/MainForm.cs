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
using System.Data.SqlClient;

using IniParser;
using IniParser.Model;

using csw_lib;

namespace AECSentinel
{
    
    public partial class MainForm : Form
    {
        int query_period;
        int threadresume_period;
        int alarm_threshold;
        int numPanels;

        FileIniDataParser parser;
        string PanelININame;
        IniData data ;

        BackgroundWorker bw_resumer = new BackgroundWorker();
        BackgroundWorker bw_btnupdater = new BackgroundWorker();

        List<BackgroundWorker> bw_list = new List<BackgroundWorker>();

        List<bool> Linkexceptionraised_list = new List<bool>();
        List<int> Linkexceptionfailures_list = new List<int>();
        List<int> failedquery_list = new List<int>();

       
        bool LINK_ERROR = false;
        bool ALARM = false;

        delegate void UpdatelabelCallback(bool allarme);

        public MainForm()
        {
            InitializeComponent();

            try
            {
                parser = new FileIniDataParser();
                data = parser.ReadFile(".\\Config\\AECSentinelConfig.ini");
                numPanels = Convert.ToInt16(data["Config"]["Panels"]);
                query_period = Convert.ToInt16(data["Webservice"]["Query_period"]);
                threadresume_period = Convert.ToInt16(data["Config"]["Resume_period"]);
                alarm_threshold = Convert.ToInt16(data["Config"]["Alarm_threshold"]);

                bw_resumer.WorkerSupportsCancellation = true;
                bw_resumer.DoWork += new DoWorkEventHandler(resume_thread);

                bw_btnupdater.WorkerSupportsCancellation = true;
                bw_btnupdater.DoWork += new DoWorkEventHandler(updatebtn_thread);


                for (int i = 0; i < numPanels; i++)
                {

                    BackgroundWorker bw = new BackgroundWorker();
                    bw.WorkerSupportsCancellation = true;
                    bw.WorkerReportsProgress = true;
                    bw.DoWork += new DoWorkEventHandler(queryproc_thread);
                    bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
                    bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

                    bw_list.Add(bw);

                    Linkexceptionraised_list.Add(false);
                    Linkexceptionfailures_list.Add(0);
                }
            }
            catch (IniParser.Exceptions.ParsingException )
            {
                MessageBox.Show("Invalid INI File");
            }
           
        }


        private void MainForm_Load(object sender, EventArgs e)
        {

            for (int i = 1; i <= numPanels; i++)
            {
                PanelININame = "Panel" + i;
                string s = data["Panels"][PanelININame];
                string[] line = s.Split(',');
                dataGridView1.Rows.Add(line[0]);
            }

            btn_startquery.Enabled = true;
            btn_stopquery.Enabled = false;

            toolStripStatusLabel1.Image = global::AECSentinel.Properties.Resources.BLUEBTN;
            toolStripStatusLabel2.Image = global::AECSentinel.Properties.Resources.BLUEBTN;
            toolStripStatusLabel1.AccessibleName="";
            toolStripStatusLabel2.AccessibleName = "";

            toolStripStatusLabel3.Text = "Query Period=" + query_period + "ms, Alarm Threshold=" + alarm_threshold;

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
                    int num_link_err = 0;
                    int i = 0;
                    foreach (bool s in new System.Collections.ArrayList(Linkexceptionraised_list))
                    {

                        if (s)
                        {
                            if (bw_list[i].IsBusy != true)
                            {
                                 bw_list[i].RunWorkerAsync(i);
                            }
                            num_link_err++;
                        }

                        i++;
                    }
                    if (num_link_err == 0) LINK_ERROR = false;


                    
                    Thread.Sleep(threadresume_period);
                };

            }
        }

        private void queryproc_thread(object sender, DoWorkEventArgs e)
        {
            int num_panel = (int)e.Argument;
            //int num_try = 0;

            try
            {
                BackgroundWorker worker = sender as BackgroundWorker;
                Linkexceptionfailures_list[num_panel] = 0;
                Linkexceptionraised_list[num_panel] = false;

                Debug.WriteLine("bw"+ num_panel+"_ID:"+Thread.CurrentThread.ManagedThreadId);
                
                /*Ping Ping_Sender = new Ping();
                PingReply Ping_Reply;
                PingOptions Ping_Options = new PingOptions();

                string IPaddress = dataGridView1.Rows[num_panel].Cells[1].Value.ToString();
                int timeout = ping_timeout;
                string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                byte[] ping_buffer = Encoding.ASCII.GetBytes(data);

         
                Ping_Options.Ttl = 128;
                Ping_Options.DontFragment = true;*/

                dataGridView1.Rows[num_panel].Cells[3].Style.BackColor = Color.Silver;

                

                Debug.WriteLine("Query thread started");

                while (true)
                {

                    if ((worker.CancellationPending == true))
                    {
                        e.Cancel = true;
                        break;
                    }
                    else
                    {

                        /*Ping_Reply = Ping_Sender.Send(IPaddress, timeout, ping_buffer, Ping_Options);

                        if (Ping_Reply.Status != IPStatus.Success)
                        {
                            num_try = 0;
                            failedping_list[num_panel]++;
                            pingexceptionfailures_list[num_panel] = 0;
                            dataGridView1.Rows[num_panel].Cells[2].Value = "";
                            dataGridView1.Rows[num_panel].Cells[3].Value = Ping_Reply.Status + "(" + failedping_list[num_panel] + ")";

                        }
                        else
                        {
                            num_try++;
                            failedping_list[num_panel] = 0;
                            pingexceptionfailures_list[num_panel] = 0;
                            dataGridView1.Rows[num_panel].Cells[2].Value = Ping_Reply.RoundtripTime + "ms";
                            dataGridView1.Rows[num_panel].Cells[3].Value = Ping_Reply.Status + "(" + num_try + ")";

                        }

                        //Debug.WriteLine("Host:{3}, Status:{0}, RTT:{1}, TTL:{2}", Ping_Reply.Status, Ping_Reply.RoundtripTime, Ping_Reply.Options.Ttl, (int)e.Argument);

                        if (failedping_list[num_panel] == 0)
                            dataGridView1.Rows[num_panel].Cells[3].Style.BackColor = Color.LightGreen;
                        else if (failedping_list[num_panel] < failed_ping_alarm)
                            dataGridView1.Rows[num_panel].Cells[3].Style.BackColor = Color.Yellow;
                        else
                        {
                            dataGridView1.Rows[num_panel].Cells[3].Style.BackColor = Color.Red;
                            ALARM = true;
                        }
                        */
                        Thread.Sleep(query_period);

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
                Linkexceptionraised_list[num_panel] = true;
                Linkexceptionfailures_list[num_panel]++;
                //num_try = 0;

                Debug.Write("bw" + num_panel + ":Link Exception");
                dataGridView1.Rows[num_panel].Cells[3].Style.BackColor = Color.Orange;
                dataGridView1.Rows[num_panel].Cells[3].Value = "LINK ERR("+Linkexceptionfailures_list[num_panel]+")";
                LINK_ERROR = true;
            }

        }

        private void btn_startquery_Click(object sender, EventArgs e)
        {

            Debug.WriteLine("Enabling threads");
         
            //start threads in the bw list
            int i = 0;
            foreach (BackgroundWorker s in bw_list)
            {

                if (s.IsBusy != true)
                {
                    s.RunWorkerAsync(i);
                }

                i++;
            }


            //start resumer thread
            if (bw_resumer.IsBusy != true)
            {
                bw_resumer.RunWorkerAsync();
            }


            LINK_ERROR = false;

            //start btupdater thread
            if (bw_btnupdater.IsBusy != true)
            {
                bw_btnupdater.RunWorkerAsync();
            }

            //update form
            btn_startquery.Enabled = false;
            btn_stopquery.Enabled = true;

        }

        private void btn_stopquery_Click(object sender, EventArgs e)
        {

            Debug.WriteLine("Disabling threads");

            //stop threads in bw list
            foreach (BackgroundWorker s in bw_list)
            {

                if (s.WorkerSupportsCancellation == true)
                {
                    s.CancelAsync();
                }

            }


            //stop resumer thread
            if (bw_resumer.WorkerSupportsCancellation == true)
            {
                bw_resumer.CancelAsync();
            }

            //stop btnupdater thread
            if (bw_btnupdater.WorkerSupportsCancellation == true)
            {
                bw_btnupdater.CancelAsync();
            }

            //update form
            btn_startquery.Enabled = true;
            btn_stopquery.Enabled = false;

            //toolStripStatusLabel1.Image = global::AECping.Properties.Resources.BLUEBTN;
            //toolStripStatusLabel2.Image = global::AECping.Properties.Resources.BLUEBTN;
            toolStripStatusLabel1.AccessibleName = "";
            toolStripStatusLabel2.AccessibleName = "";
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Console.WriteLine("Form Closed");
            
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Console.WriteLine("Progress Changed");
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


        /// <summary>Updates the button images in the form.
        /// </summary>
        private void updatebtn_thread(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            try
            {

                while (true)
                {

                    if ((worker.CancellationPending == true))
                    {
                        e.Cancel = true;
                        break;
                    }
                    else
                    {
                        
                        if (LINK_ERROR)
                        {
                            if (!(toolStripStatusLabel2.AccessibleName.Equals("RED")))
                            {
 //                               toolStripStatusLabel2.Image = global::AECping.Properties.Resources.REDBTN;
                                toolStripStatusLabel2.AccessibleName = "RED";
                            }
                        }
                        else
                        {
                            if (!(toolStripStatusLabel2.AccessibleName.Equals("GREEN")))
                            {
 //                               toolStripStatusLabel2.Image = global::AECping.Properties.Resources.GREENBTN;
                                toolStripStatusLabel2.AccessibleName = "GREEN";
                            }
                        }

                        this.updatelabel_fromthread(ALARM);
                                    

                    }

                    Thread.Sleep(1000);
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>Updates the label visibility ALARM images in the form. Invoked by updatebtn_thread
        /// </summary>
        private void updatelabel_fromthread(bool allarme)
        {
            if (this.label1.InvokeRequired)
            {
                UpdatelabelCallback d = new UpdatelabelCallback(updatelabel_fromthread);
                this.Invoke(d, new object[] {allarme });
            }
            else
            {
                if (allarme)
                    label1.Visible = true;
                else
                    label1.Visible = false;
            }

        }

    }
}
