using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Mail;

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
        int token_update;

        FileIniDataParser parser;
        string PanelININame;
        string INIConfigFile;
        IniData data ;

        BackgroundWorker bw_resumer = new BackgroundWorker();
        BackgroundWorker bw_tokenupdater = new BackgroundWorker();
        BackgroundWorker bw_btnupdater = new BackgroundWorker();

        List<BackgroundWorker> bw_list = new List<BackgroundWorker>();

        List<bool> Link_exceptionraised_list = new List<bool>(); //se c'è un errore link di rete
        List<int> Link_exceptionfailures_list = new List<int>(); //num. errori link di rete
        List<bool> panel_nullresponsealarm_list = new List<bool>(); //se la risposta del pannello è null
        List<int> panel_alarmcount_list = new List<int>();//num risposte "pannello in allarme"
        List<bool> panel_alarm_list = new List<bool>();//se il pannello è in allarme

        csw_connector csw_connector = new csw_connector();
        string token="";


        //variabili globali di segnalazione
        bool LINK_ERROR = false; //errore di rete o id pannello
        bool ALARM = false; //almeno un pannello in allarme
        

        //variabili email
        string emailto;
        string emailsubject;
        int email_period;
        int email_retry;
        int num_sent_alarms = 0; //num allarmi inviati


        delegate void UpdatelabelCallback(bool allarme);

       

        public MainForm()
        {
            InitializeComponent();

            LoadINIConfig();

            try
            {
                bw_resumer.WorkerSupportsCancellation = true;
                bw_resumer.DoWork += new DoWorkEventHandler(resume_thread);

                bw_btnupdater.WorkerSupportsCancellation = true;
                bw_btnupdater.DoWork += new DoWorkEventHandler(updateled_thread);

                bw_tokenupdater.WorkerSupportsCancellation = true;
                bw_tokenupdater.DoWork += new DoWorkEventHandler(updatetoken_thread);

            }
            catch
            {
                throw;
            }

            InitializeThreads();
           
        }

        private void InitializeThreads()
        {
            try
            {

                for (int i = 0; i < numPanels; i++)
                {

                    BackgroundWorker bw = new BackgroundWorker();
                    bw.WorkerSupportsCancellation = true;
                    bw.WorkerReportsProgress = true;
                    bw.DoWork += new DoWorkEventHandler(queryproc_thread);
                    bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
                    bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

                    bw_list.Add(bw);

                    Link_exceptionraised_list.Add(false);
                    Link_exceptionfailures_list.Add(0);
                    panel_alarmcount_list.Add(0);
                    panel_alarm_list.Add(false);
                    panel_nullresponsealarm_list.Add(false);

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void LoadINIConfig()
        {
            try
            {
                parser = new FileIniDataParser();
                INIConfigFile = ".\\Config\\AECSentinelConfig.ini";
                data = parser.ReadFile(INIConfigFile);
                numPanels = Convert.ToInt16(data["Config"]["Panels"]);
                query_period = Convert.ToInt16(data["Webservice"]["Query_period"]);
                threadresume_period = Convert.ToInt16(data["Config"]["Resume_period"]);
                alarm_threshold = Convert.ToInt16(data["Config"]["Alarm_threshold"]);
                token_update = Convert.ToInt16(data["Webservice"]["Token_update"]);

                emailto = data["Email"]["Email_to"];
                emailsubject = data["Email"]["Email_subject"];
                email_retry = Convert.ToInt16(data["Email"]["Email_retry"]);
                email_period = Convert.ToInt16(data["Email"]["Email_period"]);
            }
            catch (IniParser.Exceptions.ParsingException)
            {
                MessageBox.Show("Invalid INI File");
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

            PrepareDataGrid();

            UpdateStatusLabel();

        }

        private void PrepareDataGrid()
        {
            for (int i = 1; i <= numPanels; i++)
            {
                PanelININame = "Panel" + i;
                string s = data["Panels"][PanelININame];
                string[] line = s.Split(',');
                dataGridView1.Rows.Add(line[0]);
                dataGridView1.Rows[dataGridView1.RowCount - 2].Cells[1].Value = line[1];

            }

            btn_startquery.Enabled = true;
            btn_stopquery.Enabled = false;
            //btn_reloadINI.Enabled = true;
            btn_openINI.Enabled = true;

            toolStripStatusLabel1.Image = global::AECSentinel.Properties.Resources.BLUEBTN;
            toolStripStatusLabel2.Image = global::AECSentinel.Properties.Resources.BLUEBTN;
            toolStripStatusLabel1.AccessibleName = "";
            toolStripStatusLabel2.AccessibleName = "";

        }

        private void UpdateStatusLabel()
        {
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
                    foreach (bool s in new System.Collections.ArrayList(Link_exceptionraised_list))
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
            int panelID;
            bool panel_error;
            int num_try = 0;

            try
            {
                BackgroundWorker worker = sender as BackgroundWorker;
                Link_exceptionfailures_list[num_panel] = 0;
                Link_exceptionraised_list[num_panel] = false;

                Debug.WriteLine("bw"+ num_panel+"_ID:"+Thread.CurrentThread.ManagedThreadId);

                // Initialization 

                List<SwitchboardStatus> swbStatus;
                panelID =Convert.ToInt16(dataGridView1.Rows[num_panel].Cells[1].Value);
                
                dataGridView1.Rows[num_panel].Cells[2].Style.BackColor = Color.Silver;

                

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

            

                        //// process
                        if (token != "")

                        {

                            swbStatus = csw_connector.csw_switchboardStatus(token, panelID);

                            panel_error = false;

                            if (swbStatus == null)
                            {
                                panel_nullresponsealarm_list[num_panel]=true;
                                panel_error = true;

                            }
                            else
                            {
                                panel_nullresponsealarm_list[num_panel]=false;

                                if (swbStatus[0].swb_status > 0)
                                    panel_error = true;
                                
                            }

                            if (panel_error)
                            {
                                num_try = 0;
                                panel_alarmcount_list[num_panel]++;
                                Link_exceptionfailures_list[num_panel] = 0;
                                
                                if (panel_nullresponsealarm_list[num_panel])
                                    dataGridView1.Rows[num_panel].Cells[2].Value = "NULL RESPONSE (" + panel_alarmcount_list[num_panel] + ")";
                                else
                                    dataGridView1.Rows[num_panel].Cells[2].Value = "ALARM (" + panel_alarmcount_list[num_panel] + ")";

                            }

                            else

                            {
                                num_try++;
                                panel_alarmcount_list[num_panel] = 0;
                                Link_exceptionfailures_list[num_panel] = 0;
                                
                                dataGridView1.Rows[num_panel].Cells[2].Value = "OK (" + num_try + ")";
                                
                            }

                            //aggiorna i colori delle caselle di stato
                            if (panel_alarmcount_list[num_panel] == 0)
                            {
                                dataGridView1.Rows[num_panel].Cells[2].Style.BackColor = Color.LightGreen;
                                panel_alarm_list[num_panel] = false;
                            }
                            else if (panel_alarmcount_list[num_panel] < alarm_threshold)
                            {
                                dataGridView1.Rows[num_panel].Cells[2].Style.BackColor = Color.Yellow;
                            }
                            else
                            {
                                dataGridView1.Rows[num_panel].Cells[2].Style.BackColor = Color.Red;
                                panel_alarm_list[num_panel] = true;

                                if (num_sent_alarms < email_retry)
                                {
                                    send_email(num_panel);
                                    num_sent_alarms++;
                                }
                            }
                            

                        }

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

            catch (PingException) //rimpiazzare con link exception
            {
                Link_exceptionraised_list[num_panel] = true;
                Link_exceptionfailures_list[num_panel]++;
                //num_try = 0;

                Debug.Write("bw" + num_panel + ":Link Exception");
                dataGridView1.Rows[num_panel].Cells[2].Style.BackColor = Color.Orange;
                dataGridView1.Rows[num_panel].Cells[2].Value = "LINK ERR("+Link_exceptionfailures_list[num_panel]+")";
                LINK_ERROR = true;
            }

        }

        /// <summary>
        /// Update the token string
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updatetoken_thread(object sender, DoWorkEventArgs e)
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

                        //Aggiorna il token
                        token = csw_connector.csw_login(Properties.Settings.Default.ws_USERID, Properties.Settings.Default.ws_PASSWORD, Properties.Settings.Default.ws_URL);
                       

                    }

                    Thread.Sleep(token_update*1000);
                }
            }
            catch
            {
                throw;
            }

        }
        
        /// <summary>Updates the led images in the form.
        /// </summary>
        private void updateled_thread(object sender, DoWorkEventArgs e)
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
                                toolStripStatusLabel2.Image = global::AECSentinel.Properties.Resources.REDBTN;
                                toolStripStatusLabel2.AccessibleName = "RED";
                            }
                        }
                        else
                        {
                            if (!(toolStripStatusLabel2.AccessibleName.Equals("GREEN")))
                            {
                                toolStripStatusLabel2.Image = global::AECSentinel.Properties.Resources.GREENBTN;
                                toolStripStatusLabel2.AccessibleName = "GREEN";
                            }
                        }

                        foreach (bool s in panel_alarm_list)
                        {
                            ALARM = ALARM | s;
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

        /// <summary>Updates the label visibility ALARM images in the form. Invoked by updateled_thread
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

        private void btn_startquery_Click(object sender, EventArgs e)
        {

            Debug.WriteLine("Enabling threads");

            //start token updater thread
            if (bw_tokenupdater.IsBusy != true)
            {
                bw_tokenupdater.RunWorkerAsync();
            }
            
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
            //btn_reloadINI.Enabled = false;

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
            
            //stop token updater thread
            if (bw_tokenupdater.WorkerSupportsCancellation == true)
            {
                bw_tokenupdater.CancelAsync();
            }

            //update form
            btn_startquery.Enabled = true;
            btn_stopquery.Enabled = false;
            //btn_reloadINI.Enabled = true;

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

        private void send_email(int panel_id)
        {
            string smtpAddress = Properties.Settings.Default.SMTP_HOST;
            int portNumber = Properties.Settings.Default.SMTP_PORT;
            bool enableSSL = Properties.Settings.Default.SMTP_SSL;
            string user = Properties.Settings.Default.SMTP_USER;
            string password = Properties.Settings.Default.SMTP_PASSWORD;

            string e_From = Properties.Settings.Default.SMTP_FROM;
            string e_To = emailto;
            string e_subject = emailsubject;

            string timestamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

            string e_body = "ALARM:" + dataGridView1.Rows[panel_id].Cells[1].Value + ";" + dataGridView1.Rows[panel_id].Cells[2].Value + ";" + timestamp;
            try
            {
                using (MailMessage mail = new MailMessage())
                {

                    mail.From = new MailAddress(e_From);
                    mail.To.Add(e_To);
                    mail.Subject = e_subject;
                    mail.Body = e_body;
                    mail.IsBodyHtml = false;
                    // Can set to false, if you are sending pure text.

                    //mail.Attachments.Add(new Attachment("C:\\SomeFile.txt"));
                    //mail.Attachments.Add(new Attachment("C:\\SomeZip.zip"));

                    using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
                    {
                        smtp.Credentials = new NetworkCredential(user, password);
                        smtp.EnableSsl = enableSSL;
                        smtp.Send(mail);
                        //MessageBox.Show("Messaggio inviato");
                    }
                }

            }
            catch (Exception e)

            {
                MessageBox.Show(e.Message + " - " + e.InnerException.Message + " - " + e.InnerException.InnerException.Message);
                //throw;
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start(INIConfigFile);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            foreach (BackgroundWorker bw in bw_list)
            {
                bw.Dispose();
                
            }
            bw_list.Clear();

            Link_exceptionraised_list.Clear();
            Link_exceptionfailures_list.Clear();
            panel_alarmcount_list.Clear();
            panel_alarm_list.Clear();
            panel_nullresponsealarm_list.Clear();


            InitializeThreads();
            LoadINIConfig();

            PrepareDataGrid();
            UpdateStatusLabel();
        }
    }
}
