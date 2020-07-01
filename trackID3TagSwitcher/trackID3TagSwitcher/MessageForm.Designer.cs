namespace trackID3TagSwitcher
{
    partial class MessageForm
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
            this.picture = new System.Windows.Forms.PictureBox();
            this.lblMessage = new System.Windows.Forms.Label();
            this.answerOK = new System.Windows.Forms.Button();
            this.answerNo = new System.Windows.Forms.Button();
            this.answerYes = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picture)).BeginInit();
            this.SuspendLayout();
            // 
            // picture
            // 
            this.picture.Location = new System.Drawing.Point(156, 12);
            this.picture.Name = "picture";
            this.picture.Size = new System.Drawing.Size(130, 130);
            this.picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picture.TabIndex = 9;
            this.picture.TabStop = false;
            this.picture.Visible = false;
            // 
            // lblMessage
            // 
            this.lblMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.lblMessage.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.lblMessage.Location = new System.Drawing.Point(12, 151);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(410, 251);
            this.lblMessage.TabIndex = 8;
            this.lblMessage.Text = "message";
            this.lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // answerOK
            // 
            this.answerOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.answerOK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.answerOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.answerOK.ForeColor = System.Drawing.Color.Navy;
            this.answerOK.Location = new System.Drawing.Point(176, 419);
            this.answerOK.Name = "answerOK";
            this.answerOK.Size = new System.Drawing.Size(75, 30);
            this.answerOK.TabIndex = 7;
            this.answerOK.Text = "OK";
            this.answerOK.UseVisualStyleBackColor = false;
            this.answerOK.Visible = false;
            this.answerOK.Click += new System.EventHandler(this.answerOK_Click);
            // 
            // answerNo
            // 
            this.answerNo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.answerNo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.answerNo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.answerNo.ForeColor = System.Drawing.Color.Yellow;
            this.answerNo.Location = new System.Drawing.Point(275, 419);
            this.answerNo.Name = "answerNo";
            this.answerNo.Size = new System.Drawing.Size(75, 30);
            this.answerNo.TabIndex = 6;
            this.answerNo.Text = "いいえ";
            this.answerNo.UseVisualStyleBackColor = false;
            this.answerNo.Visible = false;
            this.answerNo.Click += new System.EventHandler(this.answerNo_Click);
            // 
            // answerYes
            // 
            this.answerYes.BackColor = System.Drawing.Color.Green;
            this.answerYes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.answerYes.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.answerYes.ForeColor = System.Drawing.Color.White;
            this.answerYes.Location = new System.Drawing.Point(69, 419);
            this.answerYes.Name = "answerYes";
            this.answerYes.Size = new System.Drawing.Size(75, 30);
            this.answerYes.TabIndex = 5;
            this.answerYes.Text = "はい";
            this.answerYes.UseVisualStyleBackColor = false;
            this.answerYes.Visible = false;
            this.answerYes.Click += new System.EventHandler(this.answerYes_Click);
            // 
            // MessageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.MidnightBlue;
            this.ClientSize = new System.Drawing.Size(434, 461);
            this.ControlBox = false;
            this.Controls.Add(this.picture);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.answerOK);
            this.Controls.Add(this.answerNo);
            this.Controls.Add(this.answerYes);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MessageForm";
            this.Text = "確認ダイアログ";
            ((System.ComponentModel.ISupportInitialize)(this.picture)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.PictureBox picture;
        public System.Windows.Forms.Label lblMessage;
        public System.Windows.Forms.Button answerOK;
        public System.Windows.Forms.Button answerNo;
        public System.Windows.Forms.Button answerYes;
    }
}