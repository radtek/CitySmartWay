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

namespace AECSentinel
{
    
    public partial class MainForm : Form
    {
        int read_period;
        int threadresume_period;
        int read_timeout;
        //int sqlwrite_period;
        int failed_ping_alarm;
        int numPanels;

        string DataSource;
        string InitialCatalog;
        string UserID;
        string Password;
        string con_str;

        FileIniDataParser parser;
        string PanelININame;
        IniData data ;

        BackgroundWorker bw_resumer = new BackgroundWorker();
        BackgroundWorker bw_sqlwriter = new BackgroundWorker();
        BackgroundWorker bw_btnupdater = new BackgroundWorker();

        List<BackgroundWorker> bw_list = new List<BackgroundWorker>();

        List<bool> pingexceptionraised_list = new List<bool>();
        List<int> pingexceptionfailures_list = new List<int>();
        List<int> failedping_list = new List<int>();

        //bool sqlwrite_exceptionraised = false;

        //bool SQL_ERROR =false;
        bool READ_ERROR = false;
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
                read_period = Convert.ToInt16(data["Config"]["Ping_period"]);
                threadresume_period = Convert.ToInt16(data["Config"]["PingResume_period"]);
                read_timeout = Convert.ToInt16(data["Config"]["Ping_timeout"]);
                failed_ping_alarm = Convert.ToInt16(data["Config"]["Failed_Ping_Alarm"]);

                DataSource = data["SQLConfig"]["DataSource"];
                InitialCatalog = data["SQLConfig"]["InitialCatalog"]; ;
                UserID = data["SQLConfig"]["UserID"]; ;
                Password = data["SQLConfig"]["Password"]; ;
                //sqlwrite_period = Convert.ToInt16(data["SQLConfig"]["SqlWrite_period"]);
                

                con_str = "Data Source=" + DataSource + ";Initial Catalog=" + InitialCatalog + ";User ID=" + UserID + ";Password=" + Password + ";";

                bw_resumer.WorkerSupportsCancellation = true;
                bw_resumer.DoWork += new DoWorkEventHandler(resume_thread);

                //bw_sqlwriter.WorkerSupportsCancellation = true;
                //bw_sqlwriter.DoWork += new DoWorkEventHandler(writesql_thread);

                bw_btnupdater.WorkerSupportsCancellation = true;
                bw_btnupdater.DoWork += new DoWorkEventHandler(updatebtn_thread);


                for (int i = 0; i < numPanels; i++)
                {

                    BackgroundWorker bw = new BackgroundWorker();
                    bw.WorkerSupportsCancellation = true;
                    bw.WorkerReportsProgress = true;
                    bw.DoWork += new DoWorkEventHandler(pingproc_thread);
                    bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
                    bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

                    bw_list.Add(bw);

                    pingexceptionraised_list.Add(false);
                    pingexceptionfailures_list.Add(0);
                    failedping_list.Add(0);
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
                dataGridView1.Rows.Add(line[0], line[1]);
            }

            btn_pingstart.Enabled = true;
            btn_pingstop.Enabled = false;

            //toolStripStatusLabel1.Image = global::AECping.Properties.Resources.BLUEBTN;
            //toolStripStatusLabel2.Image = global::AECping.Properties.Resources.BLUEBTN;
            toolStripStatusLabel1.AccessibleName="";
            toolStripStatusLabel2.AccessibleName = "";

            toolStripStatusLabel3.Text = "Timeout=" + read_timeout + "ms, Period=" + read_period + "ms, SQL=";// + sqlwrite_period+"ms";

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
                    int num_ping_err = 0;
                    int i = 0;
                    foreach (bool s in new System.Collections.ArrayList(pingexceptionraised_list))
                    {

                        if (s)
                        {
                            if (bw_list[i].IsBusy != true)
                            {
                                 bw_list[i].RunWorkerAsync(i);
                            }
                            num_ping_err++;
                        }

                        i++;
                    }
                    if (num_ping_err == 0) READ_ERROR = false;


                    /*if (sqlwrite_exceptionraised)
                    {
                        if (bw_sqlwriter.IsBusy != true)
                        {
                            bw_sqlwriter.RunWorkerAsync();
                        }
                       
                    }
                    else SQL_ERROR = false;*/

                    Thread.Sleep(threadresume_period);
                };

            }
        }

        private void pingproc_thread(object sender, DoWorkEventArgs e)
        {
            int num_panel = (int)e.Argument;
            //int num_try = 0;

            try
            {
                BackgroundWorker worker = sender as BackgroundWorker;
                pingexceptionfailures_list[num_panel] = 0;
                pingexceptionraised_list[num_panel] = false;

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
                        Thread.Sleep(read_period);

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
                //num_try = 0;

                Debug.Write("bw" + num_panel + ":Ping Exception");
                dataGridView1.Rows[num_panel].Cells[3].Style.BackColor = Color.Orange;
                dataGridView1.Rows[num_panel].Cells[3].Value = "PING ERR("+pingexceptionfailures_list[num_panel]+")";
                READ_ERROR = true;
            }

        }

        private void btn_pingstart_Click(object sender, EventArgs e)
        {

            Debug.WriteLine("Enabling threads");
         

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


            if (bw_sqlwriter.IsBusy != true)
            {
                bw_sqlwriter.RunWorkerAsync();
            }

            //SQL_ERROR = false; 
            READ_ERROR = false;

            if (bw_btnupdater.IsBusy != true)
            {
                bw_btnupdater.RunWorkerAsync();
            }

            btn_pingstart.Enabled = false;
            btn_pingstop.Enabled = true;

        }

        private void btn_pingstop_Click(object sender, EventArgs e)
        {

            Debug.WriteLine("Disabling threads");


            foreach (BackgroundWorker s in bw_list)
            {

                if (s.WorkerSupportsCancellation == true)
                {
                    s.CancelAsync();
                }

            }


            if (bw_resumer.WorkerSupportsCancellation == true)
            {
                bw_resumer.CancelAsync();
            }

            if (bw_sqlwriter.WorkerSupportsCancellation == true)
            {
                bw_sqlwriter.CancelAsync();
            }

            if (bw_btnupdater.WorkerSupportsCancellation == true)
            {
                bw_btnupdater.CancelAsync();
            }

            btn_pingstart.Enabled = true;
            btn_pingstop.Enabled = false;

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

        private void writesql_thread (object sender, DoWorkEventArgs e)
        {
            //string cmd_str;
            BackgroundWorker worker = sender as BackgroundWorker;
            //sqlwrite_exceptionraised=false;
            
            /*try
            {

                using (SqlConnection con = new SqlConnection(con_str))
                {

                    con.Open();

                    //Delete the rows in the sql table
                    cmd_str = "DELETE FROM PING_PANEL_LIST";
                    SqlCommand cmd = new SqlCommand(cmd_str, con);
                    cmd.ExecuteNonQuery();

                    //Initialize
                    for (int i = 0; i < numPanels; i++)
                    {
                        cmd.CommandText =
                            "INSERT INTO PING_PANEL_LIST " +
                            "(ID, NOME, IP_ADDR)" +
                            " Values ('" +
                            i + "','" +
                            dataGridView1.Rows[i].Cells[0].Value.ToString() + "','" +
                            dataGridView1.Rows[i].Cells[1].Value.ToString() + "')";

                        cmd.ExecuteNonQuery();
                    }

                    con.Close();

                }

                while (true)
                {

                    if ((worker.CancellationPending == true))
                    {
                        e.Cancel = true;
                        break;
                    }
                    else
                    {

                        using (SqlConnection con = new SqlConnection(con_str))
                        {

                            con.Open();

                            SqlCommand cmd = new SqlCommand(cmd_str, con);

                            //populate the table
                            for (int i = 0; i < numPanels; i++)
                            {
                                bool status_bool = false;
                                if ( dataGridView1.Rows[i].Cells[3].Value.ToString().Substring(0,7).Equals("Success"))
                                    status_bool = true;

                                cmd.CommandText =
                                    "UPDATE PING_PANEL_LIST " +
                                    "SET " +
                                    "RESPONSE='" +
                                    dataGridView1.Rows[i].Cells[2].Value.ToString() + "'," +
                                    "STATUS='" +
                                    dataGridView1.Rows[i].Cells[3].Value.ToString() + "'," +
                                    "STATUS_BOOL='" +
                                    status_bool + "'" +
                                    " WHERE ID='" + i + "';";

                                cmd.ExecuteNonQuery();
                            }
                            con.Close();
                        }
                    }

                    Thread.Sleep(sqlwrite_period);
                }
            }
            catch
            {
                sqlwrite_exceptionraised = true;
                SQL_ERROR = true;
            }*/
        }

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
                        /*if (SQL_ERROR)
                        {
                            if (!(toolStripStatusLabel1.AccessibleName.Equals("RED")))
                            {
 //                               toolStripStatusLabel1.Image = global::AECping.Properties.Resources.REDBTN;
                                toolStripStatusLabel1.AccessibleName = "RED";
                            }
                        }
                        else
                        {
                            if (!(toolStripStatusLabel1.AccessibleName.Equals("GREEN")))
                            {
                                //toolStripStatusLabel1.Image = global::AECping.Properties.Resources.GREENBTN;
                                toolStripStatusLabel1.AccessibleName = "GREEN";
                            }
                        }
                        */
                        if (READ_ERROR)
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

                        this.updatelabel_thread(ALARM);
                                    

                    }

                    Thread.Sleep(1000);
                }
            }
            catch
            {
                throw;
            }
        }

        private void updatelabel_thread(bool allarme)
        {
            if (this.label1.InvokeRequired)
            {
                UpdatelabelCallback d = new UpdatelabelCallback(updatelabel_thread);
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
