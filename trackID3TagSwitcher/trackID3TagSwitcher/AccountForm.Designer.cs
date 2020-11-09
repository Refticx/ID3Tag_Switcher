namespace trackID3TagSwitcher
{
    partial class AccountForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if ( disposing && ( components != null ) )
            {
                components.Dispose( );
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent( )
        {
            this.box_user = new System.Windows.Forms.TextBox();
            this.lblArtworkTitle = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.box_pass = new System.Windows.Forms.TextBox();
            this.btn_Login = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.pnlAppFooter = new System.Windows.Forms.Panel();
            this.lblAppLogText = new System.Windows.Forms.Label();
            this.lblAppLogTitle = new System.Windows.Forms.Label();
            this.pnlAppFooterLine = new System.Windows.Forms.Panel();
            this.webProgressBar = new System.Windows.Forms.ProgressBar();
            this.lbl_progress = new System.Windows.Forms.Label();
            this.lbl_progressContent = new System.Windows.Forms.Label();
            this.pnlAppFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // box_user
            // 
            this.box_user.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.box_user.Location = new System.Drawing.Point(85, 14);
            this.box_user.Name = "box_user";
            this.box_user.Size = new System.Drawing.Size(210, 21);
            this.box_user.TabIndex = 28;
            // 
            // lblArtworkTitle
            // 
            this.lblArtworkTitle.AutoSize = true;
            this.lblArtworkTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblArtworkTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblArtworkTitle.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.lblArtworkTitle.Location = new System.Drawing.Point(12, 17);
            this.lblArtworkTitle.Name = "lblArtworkTitle";
            this.lblArtworkTitle.Size = new System.Drawing.Size(64, 15);
            this.lblArtworkTitle.TabIndex = 29;
            this.lblArtworkTitle.Text = "ユーザー名";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.label1.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label1.Location = new System.Drawing.Point(12, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 15);
            this.label1.TabIndex = 31;
            this.label1.Text = "パスワード";
            // 
            // box_pass
            // 
            this.box_pass.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.box_pass.Location = new System.Drawing.Point(85, 41);
            this.box_pass.Name = "box_pass";
            this.box_pass.Size = new System.Drawing.Size(210, 21);
            this.box_pass.TabIndex = 30;
            // 
            // btn_Login
            // 
            this.btn_Login.AllowDrop = true;
            this.btn_Login.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btn_Login.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_Login.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.btn_Login.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.btn_Login.Location = new System.Drawing.Point(146, 68);
            this.btn_Login.Name = "btn_Login";
            this.btn_Login.Size = new System.Drawing.Size(149, 25);
            this.btn_Login.TabIndex = 32;
            this.btn_Login.Text = "ログイン（アカウント作成）";
            this.btn_Login.UseVisualStyleBackColor = false;
            this.btn_Login.Click += new System.EventHandler(this.btn_Login_Click);
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExit.ForeColor = System.Drawing.Color.Crimson;
            this.btnExit.Location = new System.Drawing.Point(347, 12);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(25, 25);
            this.btnExit.TabIndex = 33;
            this.btnExit.Text = "X";
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // pnlAppFooter
            // 
            this.pnlAppFooter.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pnlAppFooter.Controls.Add(this.lblAppLogText);
            this.pnlAppFooter.Controls.Add(this.lblAppLogTitle);
            this.pnlAppFooter.Location = new System.Drawing.Point(0, 141);
            this.pnlAppFooter.Name = "pnlAppFooter";
            this.pnlAppFooter.Size = new System.Drawing.Size(2000, 20);
            this.pnlAppFooter.TabIndex = 34;
            // 
            // lblAppLogText
            // 
            this.lblAppLogText.AutoSize = true;
            this.lblAppLogText.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblAppLogText.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.lblAppLogText.Location = new System.Drawing.Point(47, 2);
            this.lblAppLogText.Name = "lblAppLogText";
            this.lblAppLogText.Size = new System.Drawing.Size(14, 18);
            this.lblAppLogText.TabIndex = 10;
            this.lblAppLogText.Text = "-";
            // 
            // lblAppLogTitle
            // 
            this.lblAppLogTitle.AutoSize = true;
            this.lblAppLogTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblAppLogTitle.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.lblAppLogTitle.Location = new System.Drawing.Point(0, 2);
            this.lblAppLogTitle.Name = "lblAppLogTitle";
            this.lblAppLogTitle.Size = new System.Drawing.Size(42, 18);
            this.lblAppLogTitle.TabIndex = 9;
            this.lblAppLogTitle.Text = "ログ：";
            // 
            // pnlAppFooterLine
            // 
            this.pnlAppFooterLine.BackColor = System.Drawing.Color.Gold;
            this.pnlAppFooterLine.Location = new System.Drawing.Point(0, 139);
            this.pnlAppFooterLine.Name = "pnlAppFooterLine";
            this.pnlAppFooterLine.Size = new System.Drawing.Size(2000, 2);
            this.pnlAppFooterLine.TabIndex = 35;
            // 
            // webProgressBar
            // 
            this.webProgressBar.Location = new System.Drawing.Point(15, 119);
            this.webProgressBar.Name = "webProgressBar";
            this.webProgressBar.Size = new System.Drawing.Size(280, 14);
            this.webProgressBar.TabIndex = 36;
            // 
            // lbl_progress
            // 
            this.lbl_progress.AutoSize = true;
            this.lbl_progress.BackColor = System.Drawing.Color.Transparent;
            this.lbl_progress.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lbl_progress.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.lbl_progress.Location = new System.Drawing.Point(12, 101);
            this.lbl_progress.Name = "lbl_progress";
            this.lbl_progress.Size = new System.Drawing.Size(66, 15);
            this.lbl_progress.TabIndex = 37;
            this.lbl_progress.Text = "進捗状況：";
            // 
            // lbl_progressContent
            // 
            this.lbl_progressContent.AutoSize = true;
            this.lbl_progressContent.BackColor = System.Drawing.Color.Transparent;
            this.lbl_progressContent.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lbl_progressContent.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.lbl_progressContent.Location = new System.Drawing.Point(83, 101);
            this.lbl_progressContent.Name = "lbl_progressContent";
            this.lbl_progressContent.Size = new System.Drawing.Size(12, 15);
            this.lbl_progressContent.TabIndex = 38;
            this.lbl_progressContent.Text = "-";
            // 
            // AccountForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.MidnightBlue;
            this.ClientSize = new System.Drawing.Size(384, 161);
            this.ControlBox = false;
            this.Controls.Add(this.lbl_progressContent);
            this.Controls.Add(this.lbl_progress);
            this.Controls.Add(this.webProgressBar);
            this.Controls.Add(this.pnlAppFooterLine);
            this.Controls.Add(this.pnlAppFooter);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btn_Login);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.box_pass);
            this.Controls.Add(this.lblArtworkTitle);
            this.Controls.Add(this.box_user);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "AccountForm";
            this.Text = "アカウント設定";
            this.Load += new System.EventHandler(this.LoginForm_Load);
            this.pnlAppFooter.ResumeLayout(false);
            this.pnlAppFooter.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox box_user;
        private System.Windows.Forms.Label lblArtworkTitle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox box_pass;
        private System.Windows.Forms.Button btn_Login;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Panel pnlAppFooter;
        private System.Windows.Forms.Label lblAppLogText;
        private System.Windows.Forms.Label lblAppLogTitle;
        private System.Windows.Forms.Panel pnlAppFooterLine;
        private System.Windows.Forms.ProgressBar webProgressBar;
        private System.Windows.Forms.Label lbl_progress;
        private System.Windows.Forms.Label lbl_progressContent;
    }
}