namespace trackID3TagSwitcher
{
    partial class MessageDialog
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.anserYes = new System.Windows.Forms.Button();
            this.answerNo = new System.Windows.Forms.Button();
            this.answerOK = new System.Windows.Forms.Button();
            this.lblMessage = new System.Windows.Forms.Label();
            this.picture = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picture)).BeginInit();
            this.SuspendLayout();
            // 
            // anserYes
            // 
            this.anserYes.Location = new System.Drawing.Point(97, 215);
            this.anserYes.Name = "anserYes";
            this.anserYes.Size = new System.Drawing.Size(75, 23);
            this.anserYes.TabIndex = 0;
            this.anserYes.Text = "はい";
            this.anserYes.UseVisualStyleBackColor = true;
            this.anserYes.Visible = false;
            this.anserYes.Click += new System.EventHandler(this.anserYes_Click);
            // 
            // answerNo
            // 
            this.answerNo.Location = new System.Drawing.Point(303, 215);
            this.answerNo.Name = "answerNo";
            this.answerNo.Size = new System.Drawing.Size(75, 23);
            this.answerNo.TabIndex = 1;
            this.answerNo.Text = "いいえ";
            this.answerNo.UseVisualStyleBackColor = true;
            this.answerNo.Visible = false;
            this.answerNo.Click += new System.EventHandler(this.answerNo_Click);
            // 
            // answerOK
            // 
            this.answerOK.Location = new System.Drawing.Point(204, 215);
            this.answerOK.Name = "answerOK";
            this.answerOK.Size = new System.Drawing.Size(75, 23);
            this.answerOK.TabIndex = 2;
            this.answerOK.Text = "OK";
            this.answerOK.UseVisualStyleBackColor = true;
            this.answerOK.Visible = false;
            this.answerOK.Click += new System.EventHandler(this.answerOK_Click);
            // 
            // lblMessage
            // 
            this.lblMessage.AutoSize = true;
            this.lblMessage.Location = new System.Drawing.Point(95, 151);
            this.lblMessage.Name = "lblMessage";
            this.lblMessage.Size = new System.Drawing.Size(50, 12);
            this.lblMessage.TabIndex = 3;
            this.lblMessage.Text = "message";
            // 
            // picture
            // 
            this.picture.Location = new System.Drawing.Point(97, 12);
            this.picture.Name = "picture";
            this.picture.Size = new System.Drawing.Size(281, 127);
            this.picture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picture.TabIndex = 4;
            this.picture.TabStop = false;
            this.picture.Visible = false;
            // 
            // MessageDialog
            // 
            this.ClientSize = new System.Drawing.Size(486, 269);
            this.Controls.Add(this.picture);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.answerOK);
            this.Controls.Add(this.answerNo);
            this.Controls.Add(this.anserYes);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MessageDialog";
            this.Text = "確認ダイアログ";
            this.Load += new System.EventHandler(this.Form1_Load_1);
            ((System.ComponentModel.ISupportInitialize)(this.picture)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox boxAlbumPath;
        private System.Windows.Forms.Button btnLoadAlbum;
        private System.Windows.Forms.Button btnSwitcher;
        private System.Windows.Forms.Panel pnlAppHeader;
        private System.Windows.Forms.Panel pnlAppHeaderLine;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Label lblAppTitle;
        private System.Windows.Forms.Panel pnlAppFooter;
        private System.Windows.Forms.Label lblAppLogTitle;
        private System.Windows.Forms.Panel pnlAppFooterLine;
        private System.Windows.Forms.Label lblAppLogText;
        private System.Windows.Forms.Label lblAlbumTitle;
        private System.Windows.Forms.Label lblCurrentTitle;
        private System.Windows.Forms.Label lblNextTitle;
        private System.Windows.Forms.PictureBox imgCurrentMode;
        private System.Windows.Forms.PictureBox imgNextMode;
        private System.Windows.Forms.Label lblCurrentText;
        private System.Windows.Forms.Label lblNextText;
        private System.Windows.Forms.Panel pnlAppLine1;
        private System.Windows.Forms.Panel pnlAppLine2;
        private System.Windows.Forms.PictureBox imgCurrentAlbumArtwork;
        private System.Windows.Forms.Label lblID3TagTitle;
        private System.Windows.Forms.Label lblID3TagNumber;
        private System.Windows.Forms.Label lblArrow2;
        private System.Windows.Forms.Button btnClearCache;
        private System.Windows.Forms.Label lblArtworkTitle;
        private System.Windows.Forms.TextBox boxArtworkName;
        private System.Windows.Forms.Label lblTrackFolder;
        private System.Windows.Forms.TextBox boxTrackFolder;
        private System.Windows.Forms.Label lblID3TagLabel;
        private System.Windows.Forms.CheckBox chkIsDot;
        private System.Windows.Forms.Button btnOpenTrackInfoPage;
        private System.Windows.Forms.Panel pnlPage1;
        private System.Windows.Forms.Panel pnlTrackInfo;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnSaveTrackinfo;
        private System.Windows.Forms.TextBox boxEx1;
        private System.Windows.Forms.Label lblARNName;
        private System.Windows.Forms.TextBox boxEx7;
        private System.Windows.Forms.Label lblSource;
        private System.Windows.Forms.TextBox boxEx8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox boxEx6;
        private System.Windows.Forms.Label lblAirtist;
        private System.Windows.Forms.TextBox boxEx5;
        private System.Windows.Forms.Label lblBPM;
        private System.Windows.Forms.TextBox boxEx4;
        private System.Windows.Forms.Label lblGenre;
        private System.Windows.Forms.TextBox boxEx3;
        private System.Windows.Forms.Label lblCreator;
        private System.Windows.Forms.TextBox boxEx2;
        private System.Windows.Forms.Label lblOriginName;
        private System.Windows.Forms.Label lblEx;
        private System.Windows.Forms.Button btnCloseTrackInfo;
        private System.Windows.Forms.TextBox boxLabelName;
        private System.Windows.Forms.Label lblLabelName;
        private System.Windows.Forms.TextBox boxAlbumNumber;
        private System.Windows.Forms.Label lblAlbumNumberText;
        private System.Windows.Forms.Label lblAlbumTitleText;
        private System.Windows.Forms.TextBox boxAlbumName;
        private System.Windows.Forms.TextBox boxLastWord;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkIsXXs;
        private System.Windows.Forms.ToolTip tipIsDot;
        private System.Windows.Forms.Label lblCustom;
        private System.Windows.Forms.TextBox boxEx9;
        private System.Windows.Forms.Button btnShowCurrentTrackName;
        private System.Windows.Forms.CheckBox autoSearchFile;
        private System.Windows.Forms.Label lblTrackCount;
        public System.Windows.Forms.Button anserYes;
        public System.Windows.Forms.Button answerNo;
        public System.Windows.Forms.Button answerOK;
        public System.Windows.Forms.Label lblMessage;
        public System.Windows.Forms.PictureBox picture;
    }
}

