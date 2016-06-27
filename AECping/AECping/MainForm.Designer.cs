namespace AECping
{
    partial class MainForm
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione Windows Form

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.btn_pingstop = new System.Windows.Forms.Button();
            this.btn_pingstart = new System.Windows.Forms.Button();
            this.Host = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IP = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ResponseTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Host,
            this.IP,
            this.ResponseTime,
            this.Status});
            this.dataGridView1.Location = new System.Drawing.Point(12, 12);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.Size = new System.Drawing.Size(437, 288);
            this.dataGridView1.TabIndex = 0;
            // 
            // btn_pingstop
            // 
            this.btn_pingstop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_pingstop.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btn_pingstop.Location = new System.Drawing.Point(455, 279);
            this.btn_pingstop.Name = "btn_pingstop";
            this.btn_pingstop.Size = new System.Drawing.Size(90, 23);
            this.btn_pingstop.TabIndex = 1;
            this.btn_pingstop.Text = "Stop Ping";
            this.btn_pingstop.UseVisualStyleBackColor = true;
            this.btn_pingstop.Click += new System.EventHandler(this.btn_pingstop_Click);
            // 
            // btn_pingstart
            // 
            this.btn_pingstart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btn_pingstart.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btn_pingstart.Location = new System.Drawing.Point(455, 250);
            this.btn_pingstart.Name = "btn_pingstart";
            this.btn_pingstart.Size = new System.Drawing.Size(90, 23);
            this.btn_pingstart.TabIndex = 1;
            this.btn_pingstart.Text = "Start Ping";
            this.btn_pingstart.UseVisualStyleBackColor = true;
            this.btn_pingstart.Click += new System.EventHandler(this.btn_pingstart_Click);
            // 
            // Host
            // 
            this.Host.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Host.HeaderText = "Panel";
            this.Host.Name = "Host";
            // 
            // IP
            // 
            this.IP.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.IP.HeaderText = "IP Address";
            this.IP.Name = "IP";
            // 
            // ResponseTime
            // 
            this.ResponseTime.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ResponseTime.HeaderText = "Response";
            this.ResponseTime.Name = "ResponseTime";
            // 
            // Status
            // 
            this.Status.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Status.HeaderText = "Status";
            this.Status.Name = "Status";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(557, 312);
            this.Controls.Add(this.btn_pingstart);
            this.Controls.Add(this.btn_pingstop);
            this.Controls.Add(this.dataGridView1);
            this.Name = "MainForm";
            this.Text = "AEC City SmartWay KeepAlive";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button btn_pingstop;
        private System.Windows.Forms.Button btn_pingstart;
        private System.Windows.Forms.DataGridViewTextBoxColumn Host;
        private System.Windows.Forms.DataGridViewTextBoxColumn IP;
        private System.Windows.Forms.DataGridViewTextBoxColumn ResponseTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn Status;
    }
}

