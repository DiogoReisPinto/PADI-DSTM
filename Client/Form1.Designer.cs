namespace Client
{
    partial class ClientForm
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
            this.StartEndTransaction = new System.Windows.Forms.Button();
            this.CreatePadInt = new System.Windows.Forms.Button();
            this.AccessPadInt = new System.Windows.Forms.Button();
            this.PadIntRead = new System.Windows.Forms.Button();
            this.PadIntWrite = new System.Windows.Forms.Button();
            this.WriteValue = new System.Windows.Forms.TextBox();
            this.PadIntID = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // Log
            // 
            this.Log.Location = new System.Drawing.Point(13, 149);
            this.Log.Multiline = true;
            this.Log.Name = "Log";
            this.Log.Size = new System.Drawing.Size(798, 533);
            this.Log.TabIndex = 0;
            // 
            // StartEndTransaction
            // 
            this.StartEndTransaction.Location = new System.Drawing.Point(641, 97);
            this.StartEndTransaction.Name = "StartEndTransaction";
            this.StartEndTransaction.Size = new System.Drawing.Size(169, 46);
            this.StartEndTransaction.TabIndex = 1;
            this.StartEndTransaction.Text = "StartTransaction";
            this.StartEndTransaction.UseVisualStyleBackColor = true;
            this.StartEndTransaction.Click += new System.EventHandler(this.StartEndTransaction_Click);
            // 
            // CreatePadInt
            // 
            this.CreatePadInt.Enabled = false;
            this.CreatePadInt.Location = new System.Drawing.Point(188, 37);
            this.CreatePadInt.Name = "CreatePadInt";
            this.CreatePadInt.Size = new System.Drawing.Size(135, 39);
            this.CreatePadInt.TabIndex = 2;
            this.CreatePadInt.Text = "Create PadInt";
            this.CreatePadInt.UseVisualStyleBackColor = true;
            this.CreatePadInt.Click += new System.EventHandler(this.CreatePadInt_Click);
            // 
            // AccessPadInt
            // 
            this.AccessPadInt.Enabled = false;
            this.AccessPadInt.Location = new System.Drawing.Point(478, 37);
            this.AccessPadInt.Name = "AccessPadInt";
            this.AccessPadInt.Size = new System.Drawing.Size(135, 39);
            this.AccessPadInt.TabIndex = 3;
            this.AccessPadInt.Text = "Access PadInt";
            this.AccessPadInt.UseVisualStyleBackColor = true;
            this.AccessPadInt.Click += new System.EventHandler(this.AccessPadInt_Click);
            // 
            // PadIntRead
            // 
            this.PadIntRead.Enabled = false;
            this.PadIntRead.Location = new System.Drawing.Point(47, 109);
            this.PadIntRead.Name = "PadIntRead";
            this.PadIntRead.Size = new System.Drawing.Size(75, 23);
            this.PadIntRead.TabIndex = 4;
            this.PadIntRead.Text = "Read";
            this.PadIntRead.UseVisualStyleBackColor = true;
            this.PadIntRead.Click += new System.EventHandler(this.PadIntRead_Click);
            // 
            // PadIntWrite
            // 
            this.PadIntWrite.Enabled = false;
            this.PadIntWrite.Location = new System.Drawing.Point(128, 109);
            this.PadIntWrite.Name = "PadIntWrite";
            this.PadIntWrite.Size = new System.Drawing.Size(75, 23);
            this.PadIntWrite.TabIndex = 5;
            this.PadIntWrite.Text = "Write";
            this.PadIntWrite.UseVisualStyleBackColor = true;
            this.PadIntWrite.Click += new System.EventHandler(this.PadIntWrite_Click);
            // 
            // WriteValue
            // 
            this.WriteValue.Enabled = false;
            this.WriteValue.Location = new System.Drawing.Point(209, 111);
            this.WriteValue.Name = "WriteValue";
            this.WriteValue.Size = new System.Drawing.Size(100, 20);
            this.WriteValue.TabIndex = 6;
            this.WriteValue.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PadIntID_KeyPress);
            // 
            // PadIntID
            // 
            this.PadIntID.Enabled = false;
            this.PadIntID.Location = new System.Drawing.Point(350, 47);
            this.PadIntID.Name = "PadIntID";
            this.PadIntID.Size = new System.Drawing.Size(100, 20);
            this.PadIntID.TabIndex = 7;
            this.PadIntID.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PadIntID_KeyPress);
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(823, 694);
            this.Controls.Add(this.PadIntID);
            this.Controls.Add(this.WriteValue);
            this.Controls.Add(this.PadIntWrite);
            this.Controls.Add(this.PadIntRead);
            this.Controls.Add(this.AccessPadInt);
            this.Controls.Add(this.CreatePadInt);
            this.Controls.Add(this.StartEndTransaction);
            this.Controls.Add(this.Log);
            this.Name = "ClientForm";
            this.Text = "Client";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Log;
        private System.Windows.Forms.Button StartEndTransaction;
        private System.Windows.Forms.Button CreatePadInt;
        private System.Windows.Forms.Button AccessPadInt;
        private System.Windows.Forms.Button PadIntRead;
        private System.Windows.Forms.Button PadIntWrite;
        private System.Windows.Forms.TextBox WriteValue;
        private System.Windows.Forms.TextBox PadIntID;
    }
}

