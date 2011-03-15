namespace PSMoveSharpMouser
{
    partial class MouserMainWindow
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
            this.buttonEnable = new System.Windows.Forms.Button();
            this.checkBoxEnabled = new System.Windows.Forms.CheckBox();
            this.checkBoxCalibrated = new System.Windows.Forms.CheckBox();
            this.groupBoxStatus = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxPosition = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.labelMovePosValY = new System.Windows.Forms.Label();
            this.labelMovePosValX = new System.Windows.Forms.Label();
            this.buttonPause = new System.Windows.Forms.Button();
            this.comboBoxDelay = new System.Windows.Forms.ComboBox();
            this.labelDelay = new System.Windows.Forms.Label();
            this.labelServerPort = new System.Windows.Forms.Label();
            this.textBoxServerPort = new System.Windows.Forms.TextBox();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.labelServerAddress = new System.Windows.Forms.Label();
            this.textBoxServerAddress = new System.Windows.Forms.TextBox();
            this.groupBoxStatus.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.groupBoxPosition.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonEnable
            // 
            this.buttonEnable.Font = new System.Drawing.Font("Microsoft Sans Serif", 66F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonEnable.Location = new System.Drawing.Point(12, 102);
            this.buttonEnable.Name = "buttonEnable";
            this.buttonEnable.Size = new System.Drawing.Size(693, 221);
            this.buttonEnable.TabIndex = 15;
            this.buttonEnable.Text = "Enable";
            this.buttonEnable.UseVisualStyleBackColor = true;
            this.buttonEnable.Click += new System.EventHandler(this.buttonEnable_Click);
            // 
            // checkBoxEnabled
            // 
            this.checkBoxEnabled.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.checkBoxEnabled.AutoSize = true;
            this.checkBoxEnabled.Enabled = false;
            this.checkBoxEnabled.Location = new System.Drawing.Point(82, 6);
            this.checkBoxEnabled.Name = "checkBoxEnabled";
            this.checkBoxEnabled.Size = new System.Drawing.Size(65, 17);
            this.checkBoxEnabled.TabIndex = 16;
            this.checkBoxEnabled.Text = "Enabled";
            this.checkBoxEnabled.UseVisualStyleBackColor = true;
            // 
            // checkBoxCalibrated
            // 
            this.checkBoxCalibrated.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.checkBoxCalibrated.AutoSize = true;
            this.checkBoxCalibrated.Enabled = false;
            this.checkBoxCalibrated.Location = new System.Drawing.Point(3, 6);
            this.checkBoxCalibrated.Name = "checkBoxCalibrated";
            this.checkBoxCalibrated.Size = new System.Drawing.Size(73, 17);
            this.checkBoxCalibrated.TabIndex = 17;
            this.checkBoxCalibrated.Text = "Calibrated";
            this.checkBoxCalibrated.UseVisualStyleBackColor = true;
            // 
            // groupBoxStatus
            // 
            this.groupBoxStatus.Controls.Add(this.tableLayoutPanel1);
            this.groupBoxStatus.Location = new System.Drawing.Point(12, 39);
            this.groupBoxStatus.Name = "groupBoxStatus";
            this.groupBoxStatus.Size = new System.Drawing.Size(172, 56);
            this.groupBoxStatus.TabIndex = 18;
            this.groupBoxStatus.TabStop = false;
            this.groupBoxStatus.Text = "Status";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.checkBoxCalibrated, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxEnabled, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 19);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(158, 30);
            this.tableLayoutPanel1.TabIndex = 18;
            // 
            // groupBoxPosition
            // 
            this.groupBoxPosition.Controls.Add(this.tableLayoutPanel2);
            this.groupBoxPosition.Location = new System.Drawing.Point(190, 39);
            this.groupBoxPosition.Name = "groupBoxPosition";
            this.groupBoxPosition.Size = new System.Drawing.Size(123, 57);
            this.groupBoxPosition.TabIndex = 19;
            this.groupBoxPosition.TabStop = false;
            this.groupBoxPosition.Text = "Position";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.labelMovePosValY, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.labelMovePosValX, 0, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(6, 19);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(111, 30);
            this.tableLayoutPanel2.TabIndex = 20;
            // 
            // labelMovePosValY
            // 
            this.labelMovePosValY.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelMovePosValY.AutoSize = true;
            this.labelMovePosValY.Location = new System.Drawing.Point(81, 8);
            this.labelMovePosValY.Name = "labelMovePosValY";
            this.labelMovePosValY.Size = new System.Drawing.Size(27, 13);
            this.labelMovePosValY.TabIndex = 20;
            this.labelMovePosValY.Text = "N/A";
            // 
            // labelMovePosValX
            // 
            this.labelMovePosValX.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelMovePosValX.AutoSize = true;
            this.labelMovePosValX.Location = new System.Drawing.Point(25, 8);
            this.labelMovePosValX.Name = "labelMovePosValX";
            this.labelMovePosValX.Size = new System.Drawing.Size(27, 13);
            this.labelMovePosValX.TabIndex = 19;
            this.labelMovePosValX.Text = "N/A";
            // 
            // buttonPause
            // 
            this.buttonPause.Enabled = false;
            this.buttonPause.Location = new System.Drawing.Point(492, 12);
            this.buttonPause.Name = "buttonPause";
            this.buttonPause.Size = new System.Drawing.Size(75, 23);
            this.buttonPause.TabIndex = 25;
            this.buttonPause.Text = "Pause";
            this.buttonPause.UseVisualStyleBackColor = true;
            this.buttonPause.Click += new System.EventHandler(this.buttonPause_Click);
            // 
            // comboBoxDelay
            // 
            this.comboBoxDelay.FormattingEnabled = true;
            this.comboBoxDelay.Items.AddRange(new object[] {
            "2",
            "4",
            "8",
            "16",
            "32",
            "64"});
            this.comboBoxDelay.Location = new System.Drawing.Point(655, 13);
            this.comboBoxDelay.Name = "comboBoxDelay";
            this.comboBoxDelay.Size = new System.Drawing.Size(50, 21);
            this.comboBoxDelay.TabIndex = 26;
            this.comboBoxDelay.Text = "16";
            // 
            // labelDelay
            // 
            this.labelDelay.AutoSize = true;
            this.labelDelay.Location = new System.Drawing.Point(590, 17);
            this.labelDelay.Name = "labelDelay";
            this.labelDelay.Size = new System.Drawing.Size(59, 13);
            this.labelDelay.TabIndex = 28;
            this.labelDelay.Text = "Delay (ms):";
            // 
            // labelServerPort
            // 
            this.labelServerPort.AutoSize = true;
            this.labelServerPort.Location = new System.Drawing.Point(319, 17);
            this.labelServerPort.Name = "labelServerPort";
            this.labelServerPort.Size = new System.Drawing.Size(29, 13);
            this.labelServerPort.TabIndex = 27;
            this.labelServerPort.Text = "Port:";
            // 
            // textBoxServerPort
            // 
            this.textBoxServerPort.Location = new System.Drawing.Point(354, 14);
            this.textBoxServerPort.MaxLength = 5;
            this.textBoxServerPort.Name = "textBoxServerPort";
            this.textBoxServerPort.Size = new System.Drawing.Size(40, 20);
            this.textBoxServerPort.TabIndex = 22;
            this.textBoxServerPort.Text = "7899";
            // 
            // buttonConnect
            // 
            this.buttonConnect.Location = new System.Drawing.Point(411, 12);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(75, 23);
            this.buttonConnect.TabIndex = 24;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // labelServerAddress
            // 
            this.labelServerAddress.AutoSize = true;
            this.labelServerAddress.Location = new System.Drawing.Point(9, 17);
            this.labelServerAddress.Name = "labelServerAddress";
            this.labelServerAddress.Size = new System.Drawing.Size(48, 13);
            this.labelServerAddress.TabIndex = 23;
            this.labelServerAddress.Text = "Address:";
            // 
            // textBoxServerAddress
            // 
            this.textBoxServerAddress.Location = new System.Drawing.Point(63, 14);
            this.textBoxServerAddress.Name = "textBoxServerAddress";
            this.textBoxServerAddress.Size = new System.Drawing.Size(250, 20);
            this.textBoxServerAddress.TabIndex = 21;
            this.textBoxServerAddress.Text = global::PSMoveSharpMouser.Properties.Settings.Default.most_recent_server;
            // 
            // MouserMainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(717, 333);
            this.Controls.Add(this.buttonPause);
            this.Controls.Add(this.groupBoxPosition);
            this.Controls.Add(this.comboBoxDelay);
            this.Controls.Add(this.groupBoxStatus);
            this.Controls.Add(this.labelDelay);
            this.Controls.Add(this.buttonEnable);
            this.Controls.Add(this.labelServerPort);
            this.Controls.Add(this.buttonConnect);
            this.Controls.Add(this.textBoxServerPort);
            this.Controls.Add(this.textBoxServerAddress);
            this.Controls.Add(this.labelServerAddress);
            this.MinimumSize = new System.Drawing.Size(725, 367);
            this.Name = "MouserMainWindow";
            this.Text = "PSMove Mouser";
            this.groupBoxStatus.ResumeLayout(false);
            this.groupBoxStatus.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.groupBoxPosition.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonEnable;
        private System.Windows.Forms.CheckBox checkBoxEnabled;
        private System.Windows.Forms.CheckBox checkBoxCalibrated;
        private System.Windows.Forms.GroupBox groupBoxStatus;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.GroupBox groupBoxPosition;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label labelMovePosValY;
        private System.Windows.Forms.Label labelMovePosValX;
        private System.Windows.Forms.Button buttonPause;
        private System.Windows.Forms.ComboBox comboBoxDelay;
        private System.Windows.Forms.Label labelDelay;
        private System.Windows.Forms.Label labelServerPort;
        private System.Windows.Forms.TextBox textBoxServerPort;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Label labelServerAddress;
        private System.Windows.Forms.TextBox textBoxServerAddress;
    }
}

