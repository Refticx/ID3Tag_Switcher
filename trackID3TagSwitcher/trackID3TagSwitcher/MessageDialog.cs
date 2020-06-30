using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Threading;
using System.Net;

namespace trackID3TagSwitcher
{
    public partial class MessageDialog : Form
    {
        private const int MODE_YN = 0;
        private const int MODE_OK = 1;

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }

        public void SetFormState( string msg , int mode , string imgPath )
        {
            this.lblMessage.Text = msg;
            switch ( mode )
            {
                case MODE_YN:
                    this.anserYes.Visible = true;
                    this.answerNo.Visible = true;
                    this.answerOK.Visible = false;
                    break;
                case MODE_OK:
                    this.anserYes.Visible = false;
                    this.answerNo.Visible = false;
                    this.answerOK.Visible = true;
                    break;
            }
            if (imgPath != "")
            {
                this.picture.Visible = true;
                this.picture.ImageLocation = imgPath;
            }
        }

        private void anserYes_Click(object sender, EventArgs e)
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

        private void answerOK_Click(object sender, EventArgs e)
        {
            //OKボタンが押された時はDialogResult.OKを設定する。
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            //ShowDialog()で表示されているので閉じないといけない
            this.Close();
        }
    }
}
