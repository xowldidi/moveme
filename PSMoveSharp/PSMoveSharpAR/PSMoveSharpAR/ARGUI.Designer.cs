namespace PSMoveSharpAR
{
    partial class ARGUI
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ARGUI));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.imageSliceBox = new System.Windows.Forms.ComboBox();
            this.ImagePausedToggleButton = new System.Windows.Forms.Button();
            this.buttonPause = new System.Windows.Forms.Button();
            this.textBoxServerPort = new System.Windows.Forms.TextBox();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.textBoxServerAddress = new System.Windows.Forms.TextBox();
            this.glControl = new OpenTK.GLControl();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.imageSliceBox);
            this.splitContainer1.Panel1.Controls.Add(this.ImagePausedToggleButton);
            this.splitContainer1.Panel1.Controls.Add(this.buttonPause);
            this.splitContainer1.Panel1.Controls.Add(this.textBoxServerPort);
            this.splitContainer1.Panel1.Controls.Add(this.buttonConnect);
            this.splitContainer1.Panel1.Controls.Add(this.textBoxServerAddress);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.glControl);
            // 
            // imageSliceBox
            // 
            this.imageSliceBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.imageSliceBox.FormattingEnabled = true;
            this.imageSliceBox.Items.AddRange(new object[] {
            resources.GetString("imageSliceBox.Items"),
            resources.GetString("imageSliceBox.Items1"),
            resources.GetString("imageSliceBox.Items2"),
            resources.GetString("imageSliceBox.Items3"),
            resources.GetString("imageSliceBox.Items4"),
            resources.GetString("imageSliceBox.Items5"),
            resources.GetString("imageSliceBox.Items6"),
            resources.GetString("imageSliceBox.Items7")});
            resources.ApplyResources(this.imageSliceBox, "imageSliceBox");
            this.imageSliceBox.Name = "imageSliceBox";
            this.imageSliceBox.SelectedIndexChanged += new System.EventHandler(this.imageSliceBox_SelectedIndexChanged);
            // 
            // ImagePausedToggleButton
            // 
            resources.ApplyResources(this.ImagePausedToggleButton, "ImagePausedToggleButton");
            this.ImagePausedToggleButton.Name = "ImagePausedToggleButton";
            this.ImagePausedToggleButton.UseVisualStyleBackColor = true;
            this.ImagePausedToggleButton.Click += new System.EventHandler(this.ImagePausedToggleButton_Click);
            // 
            // buttonPause
            // 
            resources.ApplyResources(this.buttonPause, "buttonPause");
            this.buttonPause.Name = "buttonPause";
            this.buttonPause.UseVisualStyleBackColor = true;
            this.buttonPause.Click += new System.EventHandler(this.pauseButton_Click);
            // 
            // textBoxServerPort
            // 
            resources.ApplyResources(this.textBoxServerPort, "textBoxServerPort");
            this.textBoxServerPort.Name = "textBoxServerPort";
            this.textBoxServerPort.TextChanged += new System.EventHandler(this.textBoxServerPort_TextChanged);
            // 
            // buttonConnect
            // 
            resources.ApplyResources(this.buttonConnect, "buttonConnect");
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // textBoxServerAddress
            // 
            resources.ApplyResources(this.textBoxServerAddress, "textBoxServerAddress");
            this.textBoxServerAddress.Name = "textBoxServerAddress";
            this.textBoxServerAddress.TextChanged += new System.EventHandler(this.textBoxServerAddress_TextChanged);
            // 
            // glControl
            // 
            this.glControl.BackColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.glControl, "glControl");
            this.glControl.Name = "glControl";
            this.glControl.VSync = false;
            this.glControl.Load += new System.EventHandler(this.glControl_Load);
            this.glControl.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl_Paint);
            // 
            // ARGUI
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "ARGUI";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.TextBox textBoxServerAddress;
        private System.Windows.Forms.Button buttonPause;
        private System.Windows.Forms.TextBox textBoxServerPort;
        private OpenTK.GLControl glControl;
        private System.Windows.Forms.Button ImagePausedToggleButton;
        private System.Windows.Forms.ComboBox imageSliceBox;

    }
}

