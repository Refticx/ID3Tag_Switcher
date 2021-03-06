﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace trackID3TagSwitcher
{
    public partial class MessageForm : Form
    {
        public const int MODE_YN = 0;
        public const int MODE_OK = 1;

        public MessageForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 本アプリ専用の確認ダイアログフォームのテキスト、ボタン、画像設定を行う。
        /// </summary>
        /// <param name="msg">フォームに表示するメッセージ</param>
        /// <param name="mode">フォームのボタン形式。OKかYesNoの2種類を設定可能。</param>
        /// <param name="imgPath">表示する画像のファイルパスを記入することで、フォーム上に表示可能。</param>
        public void SetFormState(string msg, int mode, string imgPath = null)
        {
            this.lblMessage.Text = msg;
            switch (mode)
            {
                case MODE_YN:
                    this.answerYes.Visible = true;
                    this.answerNo.Visible = true;
                    this.answerOK.Visible = false;
                    break;
                case MODE_OK:
                    this.answerYes.Visible = false;
                    this.answerNo.Visible = false;
                    this.answerOK.Visible = true;
                    break;
            }
            if (imgPath != null)
            {
                this.picture.Visible = true;
                this.picture.ImageLocation = imgPath;
            }
            else
            {
                this.picture.Visible = false;
                this.picture.ImageLocation = "";
            }
        }

        private void answerYes_Click(object sender, EventArgs e)
        {
            //OKボタンが押された時はDialogResult.OKを設定する。
            this.DialogResult = System.Windows.Forms.DialogResult.Yes;
            //ShowDialog()で表示されているので閉じないといけない
            this.Close();
        }

        private void answerOK_Click(object sender, EventArgs e)
        {
            //OKボタンが押された時はDialogResult.OKを設定する。
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            //ShowDialog()で表示されているので閉じないといけない
            this.Close();
        }

        private void answerNo_Click(object sender, EventArgs e)
        {
            //OKボタンが押された時はDialogResult.OKを設定する。
            this.DialogResult = System.Windows.Forms.DialogResult.No;
            //ShowDialog()で表示されているので閉じないといけない
            this.Close();
        }

        private void MessageForm_Load( object sender , EventArgs e )
        {

        }
    }
}
