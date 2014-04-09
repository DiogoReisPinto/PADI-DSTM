namespace MasterServer
{
    partial class Form1
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
            this.Log = new System.Windows.Forms.TextBox();
            this.LogLabel = new System.Windows.Forms.Label();
            this.ServerLabel = new System.Windows.Forms.Label();
            this.PadIntLocationLabel = new System.Windows.Forms.Label();
            this.ServerLocationLoad = new System.Windows.Forms.ListView();
            this.ServerLocation = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ServerLoad = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.PadIntLocation = new System.Windows.Forms.ListView();
            this.PadIntID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.LocationPadInt = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RecoverBtn = new System.Windows.Forms.Button();
            this.FreezeBtn = new System.Windows.Forms.Button();
            this.FailBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Log
            // 
            this.Log.Location = new System.Drawing.Point(12, 42);
            this.Log.Multiline = true;
            this.Log.Name = "Log";
            this.Log.ReadOnly = true;
            this.Log.Size = new System.Drawing.Size(442, 480);
            this.Log.TabIndex = 0;
            // 
            // LogLabel
            // 
            this.LogLabel.AutoSize = true;
            this.LogLabel.Location = new System.Drawing.Point(13, 23);
            this.LogLabel.Name = "LogLabel";
            this.LogLabel.Size = new System.Drawing.Size(28, 13);
            this.LogLabel.TabIndex = 3;
            this.LogLabel.Text = "Log:";
            // 
            // ServerLabel
            // 
            this.ServerLabel.AutoSize = true;
            this.ServerLabel.Location = new System.Drawing.Point(490, 23);
            this.ServerLabel.Name = "ServerLabel";
            this.ServerLabel.Size = new System.Drawing.Size(95, 13);
            this.ServerLabel.TabIndex = 4;
            this.ServerLabel.Text = "Servers Locations:";
            // 
            // PadIntLocationLabel
            // 
            this.PadIntLocationLabel.AutoSize = true;
            this.PadIntLocationLabel.Location = new System.Drawing.Point(490, 250);
            this.PadIntLocationLabel.Name = "PadIntLocationLabel";
            this.PadIntLocationLabel.Size = new System.Drawing.Size(90, 13);
            this.PadIntLocationLabel.TabIndex = 5;
            this.PadIntLocationLabel.Text = "PadInt Locations:";
            // 
            // ServerLocationLoad
            // 
            this.ServerLocationLoad.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.ServerLocation,
            this.ServerLoad});
            this.ServerLocationLoad.Location = new System.Drawing.Point(490, 42);
            this.ServerLocationLoad.Name = "ServerLocationLoad";
            this.ServerLocationLoad.Size = new System.Drawing.Size(344, 136);
            this.ServerLocationLoad.TabIndex = 6;
            this.ServerLocationLoad.UseCompatibleStateImageBehavior = false;
            this.ServerLocationLoad.View = System.Windows.Forms.View.Details;
            this.ServerLocationLoad.SelectedIndexChanged += new System.EventHandler(this.ServerLocationLoad_SelectedIndexChanged);
            // 
            // ServerLocation
            // 
            this.ServerLocation.Text = "Server Location";
            this.ServerLocation.Width = 242;
            // 
            // ServerLoad
            // 
            this.ServerLoad.Text = "Server Load";
            this.ServerLoad.Width = 97;
            // 
            // PadIntLocation
            // 
            this.PadIntLocation.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.PadIntID,
            this.LocationPadInt});
            this.PadIntLocation.Location = new System.Drawing.Point(490, 266);
            this.PadIntLocation.Name = "PadIntLocation";
            this.PadIntLocation.Size = new System.Drawing.Size(344, 256);
            this.PadIntLocation.TabIndex = 7;
            this.PadIntLocation.UseCompatibleStateImageBehavior = false;
            this.PadIntLocation.View = System.Windows.Forms.View.Details;
            // 
            // PadIntID
            // 
            this.PadIntID.Text = "PadInt ID";
            this.PadIntID.Width = 70;
            // 
            // LocationPadInt
            // 
            this.LocationPadInt.Text = "Location";
            this.LocationPadInt.Width = 268;
            // 
            // RecoverBtn
            // 
            this.RecoverBtn.Enabled = false;
            this.RecoverBtn.Location = new System.Drawing.Point(536, 206);
            this.RecoverBtn.Name = "RecoverBtn";
            this.RecoverBtn.Size = new System.Drawing.Size(75, 23);
            this.RecoverBtn.TabIndex = 8;
            this.RecoverBtn.Text = "Recover";
            this.RecoverBtn.UseVisualStyleBackColor = true;
            this.RecoverBtn.Click += new System.EventHandler(this.RecoverBtn_Click);
            // 
            // FreezeBtn
            // 
            this.FreezeBtn.Enabled = false;
            this.FreezeBtn.Location = new System.Drawing.Point(617, 206);
            this.FreezeBtn.Name = "FreezeBtn";
            this.FreezeBtn.Size = new System.Drawing.Size(75, 23);
            this.FreezeBtn.TabIndex = 9;
            this.FreezeBtn.Text = "Freeze";
            this.FreezeBtn.UseVisualStyleBackColor = true;
            this.FreezeBtn.Click += new System.EventHandler(this.FreezeBtn_Click);
            // 
            // FailBtn
            // 
            this.FailBtn.Enabled = false;
            this.FailBtn.Location = new System.Drawing.Point(698, 206);
            this.FailBtn.Name = "FailBtn";
            this.FailBtn.Size = new System.Drawing.Size(75, 23);
            this.FailBtn.TabIndex = 10;
            this.FailBtn.Text = "Fail";
            this.FailBtn.UseVisualStyleBackColor = true;
            this.FailBtn.Click += new System.EventHandler(this.FailBtn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(846, 532);
            this.Controls.Add(this.FailBtn);
            this.Controls.Add(this.FreezeBtn);
            this.Controls.Add(this.RecoverBtn);
            this.Controls.Add(this.PadIntLocation);
            this.Controls.Add(this.ServerLocationLoad);
            this.Controls.Add(this.PadIntLocationLabel);
            this.Controls.Add(this.ServerLabel);
            this.Controls.Add(this.LogLabel);
            this.Controls.Add(this.Log);
            this.Name = "Form1";
            this.Text = "Master Server";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Log;
        private System.Windows.Forms.Label LogLabel;
        private System.Windows.Forms.Label ServerLabel;
        private System.Windows.Forms.Label PadIntLocationLabel;
        private System.Windows.Forms.ListView ServerLocationLoad;
        private System.Windows.Forms.ColumnHeader ServerLocation;
        private System.Windows.Forms.ColumnHeader ServerLoad;
        private System.Windows.Forms.ListView PadIntLocation;
        private System.Windows.Forms.ColumnHeader PadIntID;
        private System.Windows.Forms.ColumnHeader LocationPadInt;
        private System.Windows.Forms.Button RecoverBtn;
        private System.Windows.Forms.Button FreezeBtn;
        private System.Windows.Forms.Button FailBtn;
    }
}

