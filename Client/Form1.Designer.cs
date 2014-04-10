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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.AbortTxBtn = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Log
            // 
            this.Log.Location = new System.Drawing.Point(27, 163);
            this.Log.Multiline = true;
            this.Log.Name = "Log";
            this.Log.Size = new System.Drawing.Size(304, 160);
            this.Log.TabIndex = 0;
            // 
            // StartEndTransaction
            // 
            this.StartEndTransaction.Location = new System.Drawing.Point(175, 36);
            this.StartEndTransaction.Name = "StartEndTransaction";
            this.StartEndTransaction.Size = new System.Drawing.Size(75, 22);
            this.StartEndTransaction.TabIndex = 1;
            this.StartEndTransaction.Text = "Begin";
            this.StartEndTransaction.UseVisualStyleBackColor = true;
            this.StartEndTransaction.Click += new System.EventHandler(this.StartEndTransaction_Click);
            // 
            // CreatePadInt
            // 
            this.CreatePadInt.Enabled = false;
            this.CreatePadInt.Location = new System.Drawing.Point(256, 79);
            this.CreatePadInt.Name = "CreatePadInt";
            this.CreatePadInt.Size = new System.Drawing.Size(75, 22);
            this.CreatePadInt.TabIndex = 2;
            this.CreatePadInt.Text = "Create";
            this.CreatePadInt.UseVisualStyleBackColor = true;
            this.CreatePadInt.Click += new System.EventHandler(this.CreatePadInt_Click);
            // 
            // AccessPadInt
            // 
            this.AccessPadInt.Enabled = false;
            this.AccessPadInt.Location = new System.Drawing.Point(175, 79);
            this.AccessPadInt.Name = "AccessPadInt";
            this.AccessPadInt.Size = new System.Drawing.Size(75, 22);
            this.AccessPadInt.TabIndex = 3;
            this.AccessPadInt.Text = "Access";
            this.AccessPadInt.UseVisualStyleBackColor = true;
            this.AccessPadInt.Click += new System.EventHandler(this.AccessPadInt_Click);
            // 
            // PadIntRead
            // 
            this.PadIntRead.Enabled = false;
            this.PadIntRead.Location = new System.Drawing.Point(175, 122);
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
            this.PadIntWrite.Location = new System.Drawing.Point(256, 122);
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
            this.WriteValue.Location = new System.Drawing.Point(32, 125);
            this.WriteValue.Name = "WriteValue";
            this.WriteValue.Size = new System.Drawing.Size(100, 20);
            this.WriteValue.TabIndex = 6;
            this.WriteValue.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PadIntID_KeyPress);
            // 
            // PadIntID
            // 
            this.PadIntID.Enabled = false;
            this.PadIntID.Location = new System.Drawing.Point(32, 81);
            this.PadIntID.Name = "PadIntID";
            this.PadIntID.Size = new System.Drawing.Size(100, 20);
            this.PadIntID.TabIndex = 7;
            this.PadIntID.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PadIntID_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 67);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(26, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Uid:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(32, 108);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Value";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Gautami", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(9, 4);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(228, 32);
            this.label3.TabIndex = 10;
            this.label3.Text = "PADI-DSTM Client Application";
            // 
            // AbortTxBtn
            // 
            this.AbortTxBtn.Enabled = false;
            this.AbortTxBtn.Location = new System.Drawing.Point(256, 36);
            this.AbortTxBtn.Name = "AbortTxBtn";
            this.AbortTxBtn.Size = new System.Drawing.Size(75, 22);
            this.AbortTxBtn.TabIndex = 11;
            this.AbortTxBtn.Text = "Abort";
            this.AbortTxBtn.UseVisualStyleBackColor = true;
            this.AbortTxBtn.Click += new System.EventHandler(this.AbortTxBtn_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(54, 41);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(78, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Transaction:";
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(365, 335);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.AbortTxBtn);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
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
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button AbortTxBtn;
        private System.Windows.Forms.Label label4;
    }
}

