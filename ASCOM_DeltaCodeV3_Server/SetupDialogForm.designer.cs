namespace ASCOM.DeltaCodeV3
{
    partial class SetupDialogForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupDialogForm));
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.picASCOM = new System.Windows.Forms.PictureBox();
            this.chkTrace = new System.Windows.Forms.CheckBox();
            this.boxSerialComPort = new System.Windows.Forms.ComboBox();
            this.radioBaudrate9600 = new System.Windows.Forms.RadioButton();
            this.radioBaudrate19200 = new System.Windows.Forms.RadioButton();
            this.radioBaudrate38400 = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.chkTimeoutHandling = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).BeginInit();
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOK.Location = new System.Drawing.Point(153, 151);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(59, 24);
            this.cmdOK.TabIndex = 0;
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(218, 150);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(59, 25);
            this.cmdCancel.TabIndex = 1;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // picASCOM
            // 
            this.picASCOM.Image = ((System.Drawing.Image)(resources.GetObject("picASCOM.Image")));
            this.picASCOM.Location = new System.Drawing.Point(227, 23);
            this.picASCOM.Name = "picASCOM";
            this.picASCOM.Size = new System.Drawing.Size(50, 59);
            this.picASCOM.TabIndex = 9;
            this.picASCOM.TabStop = false;
            // 
            // chkTrace
            // 
            this.chkTrace.AutoSize = true;
            this.chkTrace.Location = new System.Drawing.Point(16, 133);
            this.chkTrace.Name = "chkTrace";
            this.chkTrace.Size = new System.Drawing.Size(69, 17);
            this.chkTrace.TabIndex = 6;
            this.chkTrace.Text = "Trace on";
            this.chkTrace.UseVisualStyleBackColor = true;
            // 
            // boxSerialComPort
            // 
            this.boxSerialComPort.FormattingEnabled = true;
            this.boxSerialComPort.Location = new System.Drawing.Point(77, 29);
            this.boxSerialComPort.Margin = new System.Windows.Forms.Padding(2);
            this.boxSerialComPort.Name = "boxSerialComPort";
            this.boxSerialComPort.Size = new System.Drawing.Size(92, 21);
            this.boxSerialComPort.TabIndex = 7;
            // 
            // radioBaudrate9600
            // 
            this.radioBaudrate9600.AutoSize = true;
            this.radioBaudrate9600.Location = new System.Drawing.Point(114, 67);
            this.radioBaudrate9600.Name = "radioBaudrate9600";
            this.radioBaudrate9600.Size = new System.Drawing.Size(52, 17);
            this.radioBaudrate9600.TabIndex = 8;
            this.radioBaudrate9600.TabStop = true;
            this.radioBaudrate9600.Text = " 9600";
            this.radioBaudrate9600.UseVisualStyleBackColor = true;
            // 
            // radioBaudrate19200
            // 
            this.radioBaudrate19200.AutoSize = true;
            this.radioBaudrate19200.Location = new System.Drawing.Point(114, 90);
            this.radioBaudrate19200.Name = "radioBaudrate19200";
            this.radioBaudrate19200.Size = new System.Drawing.Size(55, 17);
            this.radioBaudrate19200.TabIndex = 8;
            this.radioBaudrate19200.TabStop = true;
            this.radioBaudrate19200.Text = "19200";
            this.radioBaudrate19200.UseVisualStyleBackColor = true;
            // 
            // radioBaudrate38400
            // 
            this.radioBaudrate38400.AutoSize = true;
            this.radioBaudrate38400.Location = new System.Drawing.Point(114, 113);
            this.radioBaudrate38400.Name = "radioBaudrate38400";
            this.radioBaudrate38400.Size = new System.Drawing.Size(55, 17);
            this.radioBaudrate38400.TabIndex = 8;
            this.radioBaudrate38400.TabStop = true;
            this.radioBaudrate38400.Text = "38400";
            this.radioBaudrate38400.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Com Port";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Baudrate";
            // 
            // chkTimeoutHandling
            // 
            this.chkTimeoutHandling.AutoSize = true;
            this.chkTimeoutHandling.Location = new System.Drawing.Point(16, 156);
            this.chkTimeoutHandling.Name = "chkTimeoutHandling";
            this.chkTimeoutHandling.Size = new System.Drawing.Size(138, 17);
            this.chkTimeoutHandling.TabIndex = 6;
            this.chkTimeoutHandling.Text = "Serial Timeout Handling";
            this.chkTimeoutHandling.UseVisualStyleBackColor = true;
            // 
            // SetupDialogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(289, 187);
            this.Controls.Add(this.radioBaudrate38400);
            this.Controls.Add(this.radioBaudrate19200);
            this.Controls.Add(this.radioBaudrate9600);
            this.Controls.Add(this.boxSerialComPort);
            this.Controls.Add(this.chkTimeoutHandling);
            this.Controls.Add(this.chkTrace);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.picASCOM);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetupDialogForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DeltaCodeV3 Server 2.0.0 pre1 / 2021-12-23";
            this.Load += new System.EventHandler(this.SetupDialogForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.PictureBox picASCOM;
        private System.Windows.Forms.CheckBox chkTrace;
        private System.Windows.Forms.ComboBox boxSerialComPort;
        private System.Windows.Forms.RadioButton radioBaudrate9600;
        private System.Windows.Forms.RadioButton radioBaudrate19200;
        private System.Windows.Forms.RadioButton radioBaudrate38400;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkTimeoutHandling;
    }
}