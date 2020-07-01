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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private const string APP_DOWNLOAD_URL = "https://chaoticbootstrage.wixsite.com/scene/id3-tool";     /* アプリ更新があった時に開かせるURL */
        private const string VERSION_INFO_URL = "https://chaoticbootstrage.wixsite.com/scene/idtool-ver";   /* アプリ更新があるかを確認するページ */
        private const string CURRENT_VERSION = "1.12";                                                      /* 現在のアプリのバージョン */

        private string trackcbl = "";                           /* trackinfo.cblから読み込んだ文字列全体を格納する変数 */
        private string[,] TrackID3Tag;                          /* trackinfo.cblから読み込んだ各ID3 Tag情報を記憶する二次元配列 */
        private string currentAlbumReleaseTitle = "";           /* 現在読み込んでいるアルバムのリリースタイトル */
        private string currentAlbumReleaseNumber = "";          /* 現在読み込んでいるアルバムのリリース番号 */
        private string currentAlbumLabelName = "";              /* 現在読み込んでいるアルバムのリリース元レーベル */
        private int currentMaxTrack = 0;                        /* 現在読み込んでいるアルバムの曲数 */
        private bool isTypeFYS = false;                         /* 現在読み込んでいるアルバムの曲形式がARNかFYSかを判断する変数 */
        private bool canStartSwitcher = false;                  /* アルバムを正常に読み込めていて、変換可能かを示す変数 */
        private string new_ver = "";                            /* 取得したアプリ更新ページのバージョンを記憶する変数 */
        private string artworkPath = "";                        /* 取得したアートワークのファイルパスを格納する変数 */
        private string tracksPath = "";                         /* 楽曲が格納されているディレクトリパスを格納する変数 */
        private TagLib.IPicture aawork;                         /* 取得したアートワークのイメージを格納する変数 */
        private MessageForm messageForm = new MessageForm();    /* ダイアログ用フォームを作成しておく */

        /* 配列参照時の要素数 */
        private const int ARN_NAME = 0;
        private const int FYS_NAME = 1;
        private const int SUBTITLE = 2;
        private const int ARTIST = 3;
        private const int COMMENT = 4;
        private const int CREATOR = 5;
        private const int BPM = 6;
        private const int GENRE = 7;
        private const int CUSTOM = 8;

        private const int MAX_TRACK = 20;   /* 一括変更できる最大曲数、ここを変更すると曲情報画面の縦ボックス数を変更できる */
        private const int MAX_TAG = 9;      /* 設定する項目数、ここを変更すると曲情報画面の横ボックス数を変更できる */

        /* デザイナーサイズ */
        private const int EXIT_Y            = 3;
        private const int DESIGN_DEF_X      = 0;
        private const int MAIN_AREA_Y       = 32;
        private const int MAIN_AREA_HIDE_X  = -500;
        private const int SUB_AREA_HIDE_X   = 300;

        private const int APP_WIDTH         = 400;
        private const int APP_HEIGHT        = (APP_WIDTH + 40);
        private const int FOOTER_Y          = (APP_HEIGHT - 20);
        private const int FOOTER_LINE_Y     = (FOOTER_Y - 2);
        private const int EXIT_X            = (APP_WIDTH - 28);

        private const int APP_BIG_WIDTH     = 760;
        private const int APP_BIG_HEIGHT    = (APP_BIG_WIDTH - 60);
        private const int FOOTER_BIG_Y      = (APP_BIG_HEIGHT - 20);
        private const int FOOTER_LINE_BIG_Y = (FOOTER_BIG_Y - 2);
        private const int EXIT_BIG_X        = (APP_BIG_WIDTH - 28);

        /* ダイアログフォームの表示モード */
        private const int MODE_YN = 0;
        private const int MODE_OK = 1;

        /* システムメッセージ */

        enum STRNUM
        {
            CLEAR_LOAD_DATA,

            NOT_FOUND_ID3LIST,
            QST_MAKE_ID3LIST,
            FOUND_ID3LIST,
            NODATA_ID3LIST,
            NEED_MAKE_ID3LIST,

            FOUND_MP3S,
            DIR,

            NOT_FOUND_SONG_DIR,
            PLZ_CHECK_FILE_PATH,
            NOT_FOUND_SONG,

            Irregular_Error,

            Not_Found_Artwork,

            Break_ID3List,
            Plz_Make_ID3List,
        }
        private string[] SYS_MSG_LIST =
        {
            "読み込んだアルバム情報をクリアしました。",

            "楽曲情報を構成する「trackinfo.cbl」が見つかりませんでした。\r\n",
            "ID3リストを作成しますか？",
            "楽曲情報を構成する「trackinfo.cbl」は見つかりました。\r\n",
            "しかしリストデータが空でした。\r\n",
            "ID3リストの作成が必要です。",

            "楽曲を発見しました、ご確認ください。\r\n",
            "格納先：",

            "指定された曲階層が見つかりませんでした。\r\n",
            "再度ファイルパスをご確認ください。",
            "楽曲が見つかりません。",

            "想定外のエラーが発生しました。\r\n今後の本ソフトウェア安定性向上のため、製作者にスクリーンショットを添えてご報告お願いします。\r\n\r\n",

            "設定可能なアートワークを取得できませんでした。",

            "ID3リストファイルが破損しています。",
            "\r\nデータを読み取ることができませんでした。\r\n再度作り直してください。",
        };

        #region 汎用型スクリプト

        /// <summary>
        /// 読み込んだアルバム情報を全て消去する
        /// </summary>
        private void ClearCache( )
        {
            this.boxAlbumPath.Text = "";
            this.lblID3TagNumber.Text = "";
            this.lblID3TagNumber.Visible = false;
            this.lblID3TagTitle.Text = "";
            this.lblID3TagTitle.Visible = false;
            this.lblID3TagLabel.Text = "";
            this.lblID3TagLabel.Visible = false;
            this.imgCurrentAlbumArtwork.ImageLocation = "";
            this.imgCurrentAlbumArtwork.Visible = false;
            this.lblCurrentText.Text = "";
            this.lblCurrentText.Visible = false;
            this.lblNextText.Text = "";
            this.lblNextText.Visible = false;
            this.imgCurrentMode.ImageLocation = "";
            this.imgCurrentMode.Visible = false;
            this.imgNextMode.ImageLocation = "";
            this.imgNextMode.Visible = false;
            this.lblArrow2.Visible = false;
            this.isTypeFYS = false;
            this.canStartSwitcher = false;
            /* 曲数 */
            this.lblTrackCount.Visible = false;
            this.lblTrackCount.Text = "";

            for (int tag = 0; tag < MAX_TAG; tag++)
            {
                for (int track = 0; track < this.currentMaxTrack; track++)
                {
                    this.boxTracks[track, tag].Text = "";
                }
            }
            this.boxAlbumName.Text = "";
            this.boxAlbumNumber.Text = "";
            this.boxLabelName.Text = "";

        }
        /// <summary>
        /// アプリフッターのログ部分にメッセージを表示する
        /// </summary>
        private void SetLog( Color cl , string log )
        {
            this.lblAppLogText.ForeColor = cl;
            this.lblAppLogText.Text = log;
        }

        private void BtnClearCache_Click(object sender, EventArgs e)
        {
            ClearCache();
            SetLog(Color.LimeGreen, this.SYS_MSG_LIST[(int)STRNUM.CLEAR_LOAD_DATA]);
        }
        #endregion

        #region アルバム読み込み

        #region 各読み込みスクリプト

        private bool GetID3List( string path )
        {
            /* ID3リストファイルがあるか検索 */
            bool ret = File.Exists(path);
            if (!ret)
            {
                /* 確認ダイアログを表示 */
                messageForm.SetFormState(this.SYS_MSG_LIST[(int)STRNUM.NOT_FOUND_ID3LIST] + this.SYS_MSG_LIST[(int)STRNUM.QST_MAKE_ID3LIST], MODE_YN);
                DialogResult dr = messageForm.ShowDialog();

                /* YesならID3作成画面へ飛ばす */
                if (dr == DialogResult.Yes)
                    btnOpenTrackInfoPage.PerformClick();

                /* ログメッセージ表示 */
                SetLog(Color.Orange, this.SYS_MSG_LIST[(int)STRNUM.NEED_MAKE_ID3LIST]);
                return false;
            }

            /* あったらリスト情報を読み込む */
            StreamReader sr = new StreamReader(path );
            this.trackcbl = sr.ReadToEnd();
            sr.Close();
            if (this.trackcbl.Length == 0)
            {
                /* 確認ダイアログを表示 */
                messageForm.SetFormState(this.SYS_MSG_LIST[(int)STRNUM.FOUND_ID3LIST] + this.SYS_MSG_LIST[(int)STRNUM.NODATA_ID3LIST] + this.SYS_MSG_LIST[(int)STRNUM.QST_MAKE_ID3LIST], MODE_YN);
                DialogResult dr = messageForm.ShowDialog();

                /* YesならID3作成画面へ飛ばす */
                if (dr == DialogResult.Yes)
                    btnOpenTrackInfoPage.PerformClick();

                /* ログメッセージ表示 */
                SetLog(Color.Orange, this.SYS_MSG_LIST[(int)STRNUM.NEED_MAKE_ID3LIST]);
                return false;
            }

            return true;
        }
        private bool GetAlbumArtwork( string path )
        {
            try
            {
                /* アートワークを見つけたかのフラグ、後半での共通処理で使う */
                bool isFind = false;
                int loopCount = 4;

                /* 自動検索分岐前に、両分岐先で使う変数に値だけ代入しておく */
                this.artworkPath = path + this.boxArtworkName.Text;

                /* アートワーク自動検索の分岐 */
                if (this.autoSearchFile.Checked)
                {
                    /* 指定されたアルバムディレクトリ配下のファイルを全て取得 */
                    string[] files = System.IO.Directory.GetFiles(path, "*", System.IO.SearchOption.AllDirectories);

                    /* ファイルが1つも取得できなかった場合はエラー通知する */
                    if (files.Length == 0)
                        return false;

                    /* アートワークが複数あることを想定してループ処理 */
                    string[] atw = null;
                    int isExt = 0;
                    while (isExt <= loopCount)
                    {
                        switch (isExt)
                        {
                            /* 拡張子を検索 */
                            case 0: atw = files.Where(s => s.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)).ToArray(); break;
                            case 1: atw = files.Where(s => s.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)).ToArray(); break;
                            case 2: atw = files.Where(s => s.EndsWith(".png", StringComparison.OrdinalIgnoreCase)).ToArray(); break;
                            case 3: atw = files.Where(s => s.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)).ToArray(); break;
                        }

                        /* 結果が何か入っていれば */
                        if ((atw != null) && (atw.Length != 0))
                        {
                            /* 検索をする */
                            for (int i = 0; i < atw.Length; i++)
                            {
                                /* 楽曲に設定済みの、システムが自動生成したアートワークファイルは排除する */
                                if (atw[i].Contains("AlbumArtSmall") || atw[i].Contains("Folder"))
                                    continue;
                                /* それ以外の画像は */
                                else
                                {
                                    /* 確認ダイアログを表示 */
                                    messageForm.SetFormState("取得されたアートワークの確認です。\r\nこちらでよろしいでしょうか？\r\n格納先：" + atw[i], MODE_YN, atw[i]);
                                    DialogResult dr = messageForm.ShowDialog();
                                    if (dr == DialogResult.Yes)
                                    {
                                        this.artworkPath = atw[i];
                                        isFind = true;
                                        break;
                                    }
                                }
                            }
                            //MessageBox.Show("取得されたアートワークの格納先\r\n" + this.artworkPath, "取得内容確認", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        /* 拡張子を変更 */
                        isExt++;
                    }
                }
                else
                {
                    /* 手動指定されたファイルパスを検索 */
                    if (File.Exists(this.artworkPath))
                    {
                        /* 確認ダイアログを表示 */
                        messageForm.SetFormState("取得されたアートワークの確認です。\r\nこちらでよろしいでしょうか？\r\n格納先：" + this.artworkPath, MODE_YN, this.artworkPath);
                        DialogResult dr = messageForm.ShowDialog();
                        if (dr == DialogResult.Yes)
                            isFind = true;
                    }
                    else
                        return false;
                }

                /* アートワークが見つかっていたら */
                if (isFind)
                {
                    /* アートワークをアプリ上に表示 */
                    this.imgCurrentAlbumArtwork.ImageLocation = this.artworkPath;
                    this.imgCurrentAlbumArtwork.Visible = true;

                    /* ID3タグを編集する用のアートワーク画像を設定 */
                    this.aawork = new TagLib.Picture(this.artworkPath);

                    /* アートワークが見つかった結果を返す */
                    return true;
                }

                /* 念のための予防措置 */
                return false;
            }
            catch (Exception ex)
            {
                /* 確認ダイアログを表示 */
                messageForm.SetFormState(this.SYS_MSG_LIST[(int)STRNUM.Irregular_Error] + ex.ToString(), MODE_OK);
                messageForm.ShowDialog();
                return false;
            }
        }
        private void GetAlbumInfo( string path )
        {
            this.lblID3TagNumber.Text = "リリースナンバー：" + this.currentAlbumReleaseNumber;
            this.lblID3TagNumber.Visible = true;
            this.lblID3TagTitle.Text = "アルバム名：" + this.currentAlbumReleaseTitle;
            this.lblID3TagTitle.Visible = true;
            this.lblID3TagLabel.Text = "レーベル：" + this.currentAlbumLabelName;
            this.lblID3TagLabel.Visible = true;
            this.lblTrackCount.Text = "曲数：" + this.currentMaxTrack;
            this.lblTrackCount.Visible = true;
        }
        private void GetCurrentType( string path )
        {
            if (!this.isTypeFYS)
            {
                this.lblCurrentText.Text = "A-Remix Nation方式";
                this.lblNextText.Text = "For You Sounds方式";
                this.imgCurrentMode.ImageLocation = Application.StartupPath + "\\item\\arn.jpg";
                this.imgNextMode.ImageLocation = Application.StartupPath + "\\item\\fys.jpg";
            }
            else
            {
                this.lblCurrentText.Text = "For You Sounds方式";
                this.lblNextText.Text = "A-Remix Nation方式";
                this.imgCurrentMode.ImageLocation = Application.StartupPath + "\\item\\fys.jpg";
                this.imgNextMode.ImageLocation = Application.StartupPath + "\\item\\arn.jpg";
            }
            this.lblCurrentText.Visible = true;
            this.lblNextText.Visible = true;
            this.lblArrow2.Visible = true;
            this.imgCurrentMode.Visible = true;
            this.imgNextMode.Visible = true;
        }
        private bool GetMaxTrack( string path )
        {
            try
            {
                /* 楽曲ディレクトリの変数に、仮でテキストボックスの値を入れておく */
                this.tracksPath = path + this.boxTrackFolder.Text;
                /* 楽曲自動検索 */
                if (this.autoSearchFile.Checked)
                {
                    /* 指定ディレクトリ以下のファイルを総取得 */
                    string[] allFiles = System.IO.Directory.GetFiles(path, "*", System.IO.SearchOption.AllDirectories);

                    /* mp3ファイルのみを取得 */
                    string[] songs;
                    songs = allFiles.Where(s => s.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)).ToArray();

                    /* mp3ファイルがあるかどうか */
                    if (songs.Length != 0)
                    {
                        /* 格納先ディレクトリを取得 */
                        string songDir = Path.GetDirectoryName(songs[0]);

                        /* 表示メッセージを作成 */
                        string msg = this.SYS_MSG_LIST[(int)STRNUM.FOUND_MP3S] +
                                        this.SYS_MSG_LIST[(int)STRNUM.DIR] + songDir + "\r\n\r\n";
                        for ( int i = 0; i < songs.Length; i++ )
                        {
                            msg += Path.GetFileName(songs[i]) + "\r\n";
                        }

                        /* 確認ダイアログを表示 */
                        messageForm.SetFormState(msg, MODE_YN);
                        DialogResult dr = messageForm.ShowDialog();

                        /* Yesなら格納先と曲数を記憶する */
                        if (dr == DialogResult.Yes)
                        {
                            this.currentMaxTrack = songs.Length;
                            this.tracksPath = songDir;
                            return true;
                        }
                    }
                }
                else
                {
                    /* 指定ディレクトリがあるか確認 */
                    bool ret = Directory.Exists(this.tracksPath);
                    if (!ret)
                    {
                        /* 確認ダイアログを表示 */
                        messageForm.SetFormState(this.SYS_MSG_LIST[(int)STRNUM.NOT_FOUND_SONG_DIR] +
                                                this.SYS_MSG_LIST[(int)STRNUM.PLZ_CHECK_FILE_PATH], MODE_OK);
                        messageForm.ShowDialog();

                        /* ログメッセージ表示 */
                        SetLog(Color.Orange, this.SYS_MSG_LIST[(int)STRNUM.NOT_FOUND_SONG]);

                        /* 処理を中断 */
                        return false;
                    }

                    DirectoryInfo di = new DirectoryInfo(this.tracksPath);
                    FileInfo[] songs = di.GetFiles("*.mp3", SearchOption.AllDirectories);

                    /* mp3ファイルがあるかどうか */
                    if (songs.Length != 0)
                    {
                        /* 表示メッセージを作成 */
                        string msg = this.SYS_MSG_LIST[(int)STRNUM.FOUND_MP3S] +
                                        this.SYS_MSG_LIST[(int)STRNUM.DIR] + 
                                        songs[0].DirectoryName + "\r\n\r\n";
                        for (int i = 0; i < songs.Length; i++)
                        {
                            msg += songs[i].Name + "\r\n";
                        }

                        /* 確認ダイアログを表示 */
                        messageForm.SetFormState(msg, MODE_YN);
                        DialogResult dr = messageForm.ShowDialog();

                        /* Yesなら格納先と曲数を記憶する */
                        if (dr == DialogResult.Yes)
                        {
                            this.currentMaxTrack = songs.Length;
                            this.tracksPath = songs[0].DirectoryName;
                            return true;
                        }
                    }
                }

                /* ログメッセージ表示 */
                SetLog(Color.Orange, this.SYS_MSG_LIST[(int)STRNUM.NOT_FOUND_SONG]);
                /* 処理を終了 */
                return false;
            }
            catch (Exception ex)
            {
                /* 確認ダイアログを表示 */
                messageForm.SetFormState(this.SYS_MSG_LIST[(int)STRNUM.Irregular_Error] + ex.ToString( ), MODE_OK);
                messageForm.ShowDialog();
                /* ログメッセージ表示 */
                SetLog(Color.Orange, this.SYS_MSG_LIST[(int)STRNUM.NOT_FOUND_SONG]);
                return false;
            }
        }
        private bool AnalysisID3List()
        {
            /* 
             要素数8（ARN名、FYS名、サブタイトル、アーティスト、コメント、作曲者、BPM、ジャンル）
             */
            TrackID3Tag = new string[this.currentMaxTrack, MAX_TAG];
            string line = "";
            string target = "";
            string num = "";
            int st = 0;
            int ed = 0;
            int isBreak = 0;
            bool isFind = false;

            StringReader rs = new StringReader(this.trackcbl);
            while (rs.Peek() > -1)
            {
                line = rs.ReadLine();
                isFind = false;

                target = "Album_Label:";
                if (line.Contains(target))
                {
                    st = target.Length;
                    ed = line.Length - st;
                    this.currentAlbumLabelName = line.Substring(st, ed);
                    /* どれか1つでも対象のワードを発見できたらデータ正常扱いにする */
                    isBreak++;
                    continue;
                }

                target = "Album_Name:";
                if (line.Contains(target))
                {
                    st = target.Length;
                    ed = line.Length - st;
                    this.currentAlbumReleaseTitle = line.Substring(st, ed);
                    /* どれか1つでも対象のワードを発見できたらデータ正常扱いにする */
                    isBreak++;
                    continue;
                }

                target = "Album_Number:";
                if (line.Contains(target))
                {
                    st = target.Length;
                    ed = line.Length - st;
                    this.currentAlbumReleaseNumber = line.Substring(st, ed);
                    /* どれか1つでも対象のワードを発見できたらデータ正常扱いにする */
                    isBreak++;
                    continue;
                }

                target = "Is_Type:";
                if (line.Contains(target))
                {
                    st = target.Length;
                    ed = line.Length - st;
                    string mode = line.Substring(st, ed);
                    if ( mode == "ARN")
                        this.isTypeFYS = false;
                    else if (mode == "FYS")
                        this.isTypeFYS = true;
                    /* どれか1つでも対象のワードを発見できたらデータ正常扱いにする */
                    isBreak++;
                    continue;
                }

                for (int tag = 0; tag < MAX_TAG; tag++)
                {
                    for (int track = 0; track < this.currentMaxTrack; track++)
                    {
                        switch (tag)
                        {
                            case 0: target = "ARN_Name_"; break;
                            case 1: target = "FYS_Name_"; break;
                            case 2: target = "FYS_Subtitle_"; break;
                            case 3: target = "FYS_Artist_"; break;
                            case 4: target = "FYS_Comment_"; break;
                            case 5: target = "Creator_"; break;
                            case 6: target = "BPM_"; break;
                            case 7: target = "Genre_"; break;
                            case 8: target = "FYS_CustomName_"; break;
                        }
                        if (track < 9)
                            num = "0";
                        else
                            num = "";

                        target += num + (track + 1) + ":";
                        st = target.Length;

                        if (line.Contains(target))
                        {
                            ed = line.Length - st;
                            TrackID3Tag[track, tag] = line.Substring(st, ed);
                            isFind = true;
                            break;
                        }
                    }
                    if (isFind)
                        break;
                }
            }
            rs.Close();
            /* 最低限必要な4つのタグが見つけられなかったら破損データ扱いにする */
            if ( isBreak < 4 )
            {
                /* 確認ダイアログを表示 */
                messageForm.SetFormState(this.SYS_MSG_LIST[(int)STRNUM.Break_ID3List] +
                                            this.SYS_MSG_LIST[(int)STRNUM.Plz_Make_ID3List], 
                                            MODE_OK);
                messageForm.ShowDialog();

                /* ログメッセージ表示 */
                SetLog(Color.Orange, this.SYS_MSG_LIST[(int)STRNUM.Break_ID3List]);
                return false;
            }
            return true;
        }
        private void SetID3ListValue()
        {
            int st = 0;
            int ed;
            string target = " (";
            this.boxAlbumName.Text = this.currentAlbumReleaseTitle;
            this.boxAlbumNumber.Text = this.currentAlbumReleaseNumber;
            this.boxLabelName.Text = this.currentAlbumLabelName;
            for (int tag = 0; tag < MAX_TAG; tag++)
            {
                for (int track = 0; track < this.currentMaxTrack; track++)
                {
                    if ( tag != FYS_NAME )
                        this.boxTracks[track, tag].Text = this.TrackID3Tag[track, tag];
                    else
                    {
                        if (this.TrackID3Tag[track, tag].Contains(target))
                        {
                            ed = this.TrackID3Tag[track, tag].IndexOf(target);
                            this.boxTracks[track, tag].Text = this.TrackID3Tag[track, tag].Substring(st, ed);
                        }
                    }
                }
            }
        }
        private void CheckAlbumConfig( string path )
        {
            string file = path + "\\trackinfo.cbl";
            bool ret = true;
            try
            {
                /* 設定されているパスからアートワークを取得する */
                ret = GetAlbumArtwork(path);
                if (!ret)
                {
                    /* 確認ダイアログを表示 */
                    messageForm.SetFormState(this.SYS_MSG_LIST[(int)STRNUM.Not_Found_Artwork], MODE_OK);
                    DialogResult dr = messageForm.ShowDialog();

                    /* アートワークを非表示 */
                    this.imgCurrentAlbumArtwork.ImageLocation = "";
                    this.imgCurrentAlbumArtwork.Visible = false;
                }

                /* 一度曲保存先にあるmp3を全部取得し、何曲あるか確認する */
                ret = GetMaxTrack( path);
                if (!ret)
                {
                    /* try処理終わらせるために例外発生させる */
                    throw new Exception();
                }

                /* ID3 Tagリストを取得 */
                ret = GetID3List(file);
                /* 取得できなかった場合 */
                if (!ret)
                {
                    /* try処理終わらせるために例外発生させる */
                    throw new Exception();
                }

                /* 取得できた曲数分、ID3 Tagの配列を生成し、リストの解析を行う */
                ret = AnalysisID3List( );
                /* データが破損していた場合 */
                if (!ret)
                {
                    /* try処理終わらせるために例外発生させる */
                    throw new Exception();
                }

                /* 現在のアルバム形式がARNかFYSかを表示する */
                GetCurrentType(path);

                /* ディレクトリ名からアルバム名を取得 */
                GetAlbumInfo(path);

                /* ID3リスト作成ページに、解析したタグデータを入力 */
                SetID3ListValue();

                /* ID3リスト作成ページの各ボックスを初期化 */
                DeleteAnotherBoxes();

                SetLog(Color.LimeGreen, "アルバムを読み込みました。");
                this.boxAlbumPath.Text = path;
                this.canStartSwitcher = true;
            }
            catch (Exception ex)
            {
            }
        }

        #endregion

        #region ドラッグアンドドロップでも読み込み

        private void btnLoadAlbum_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void btnLoadAlbum_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void btnLoadAlbum_DragDrop(object sender, DragEventArgs e)
        {
            // 実際にデータを取り出す
            var data = e.Data.GetData(DataFormats.FileDrop, true) as string[];

            // データが取得できたか判定する
            if (data != null)
            {
                foreach (var filePath in data)
                {
                    CheckAlbumConfig(filePath);
                }
            }
        }

        #endregion

        private void BtnLoadAlbum_Click(object sender, EventArgs e)
        {
            var dialog = new CommonOpenFileDialog("アルバムを選択してください。");
            // フォルダ選択モード
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                CheckAlbumConfig(dialog.FileName);
            }
            else
            {
                SetLog( Color.Orange, "読み込みを中止しました。" );
            }

            // オブジェクトを破棄する
            dialog.Dispose();
        }
        
        #region 自動検索

        private void autoSearchFile_CheckedChanged(object sender, EventArgs e)
        {
            if ( this.autoSearchFile.Checked )
            {
                this.boxArtworkName.Enabled = false;
                this.boxTrackFolder.Enabled = false;
            }
            else
            {
                this.boxArtworkName.Enabled = true;
                this.boxTrackFolder.Enabled = true;
            }
        }

        #endregion

        #endregion

        #region ID3 Tag 変更

        private void StartTypeSwitch( )
        {
            /* 曲名のタイプを読み分ける */
            int nameNum = 0;
            if ( !this.isTypeFYS )
                nameNum = FYS_NAME;
            else
                nameNum = ARN_NAME;

            /* ループ処理用の変数を初期化 */
            string num = "";
            int track = 0;

            /* エラーなどでファイル変更が止まった不具合の可能性も考慮し、差し替え前はもう一度ファイル検索する */
            //DirectoryInfo di = new DirectoryInfo(this.tracksPath);
            string[] filepathes = Directory.GetFiles( this.tracksPath , "*mp3" );
            MessageBox.Show(filepathes.ToString());
            //FileInfo[] files = di.GetFiles("*.mp3", SearchOption.AllDirectories);
            /*foreach (FileInfo f in files)
            {
                TagLib.File file = TagLib.File.Create( f.FullName );
                file.Tag.Title = TrackID3Tag[track, nameNum];
                file.Tag.Track = Convert.ToUInt16((track + 1));
                file.Tag.AlbumArtists = new string[] { this.currentAlbumLabelName };
                file.Tag.Album = this.currentAlbumReleaseTitle;
                file.Tag.Composers = new string[] { TrackID3Tag[track, CREATOR] };
                file.Tag.Genres = new string[] { TrackID3Tag[track, GENRE] };
                file.Tag.BeatsPerMinute = Convert.ToUInt16( TrackID3Tag[track, BPM] );
                file.Tag.Pictures = new TagLib.IPicture[1] { this.aawork };
                if (!this.isTypeFYS)
                {
                    file.Tag.Grouping = TrackID3Tag[track, SUBTITLE];
                    file.Tag.Performers = new string[] { TrackID3Tag[track, ARTIST] , TrackID3Tag[track, CREATOR] };
                    file.Tag.Comment = TrackID3Tag[track, COMMENT];
                }
                else
                {
                    file.Tag.Grouping = "";
                    file.Tag.Performers = new string[] { TrackID3Tag[track, CREATOR] };
                    file.Tag.Comment = "";
                }
                file.Save();

                if ( track < 9 )
                    num = "0" + (track + 1);
                else
                    num = "" + (track + 1);

                if ( this.chkIsDot.Checked )
                    num += ". ";
                else
                    num += " ";

                File.Move(f.FullName , f.DirectoryName + "\\" + num + TrackID3Tag[track, nameNum] + ".mp3" );
                file.Dispose( );
                track++;
            }*/
            
            if ( !this.isTypeFYS )
            {
                this.lblCurrentText.Text = "For You Sounds方式";
                this.lblNextText.Text = "A-Remix Nation方式";
                this.imgCurrentMode.ImageLocation = Application.StartupPath + "\\item\\fys.jpg";
                this.imgNextMode.ImageLocation = Application.StartupPath + "\\item\\arn.jpg";
                this.lblCurrentText.Visible = true;
                this.lblNextText.Visible = true;
                this.lblArrow2.Visible = true;
                this.isTypeFYS = true;
            }
            else
            {
                this.lblCurrentText.Text = "A-Remix Nation方式";
                this.lblNextText.Text = "For You Sounds方式";
                this.imgCurrentMode.ImageLocation = Application.StartupPath + "\\item\\arn.jpg";
                this.imgNextMode.ImageLocation = Application.StartupPath + "\\item\\fys.jpg";
                this.lblCurrentText.Visible = true;
                this.lblNextText.Visible = true;
                this.lblArrow2.Visible = true;
                this.isTypeFYS = false;
            }

        }

        private void RewriteCBL( )
        {
            string filePath = this.boxAlbumPath.Text + "\\trackinfo.cbl";
            string text = "";

            string line = "";
            string target = "";

            StringReader rs = new StringReader(this.trackcbl);
            while (rs.Peek() > -1)
            {
                line = rs.ReadLine();

                target = "Is_Type:";
                if (line.Contains(target))
                {
                    if (this.isTypeFYS)
                        text += "Is_Type:FYS\r\n";
                    else
                        text += "Is_Type:ARN\r\n";
                }
                else
                {
                    text += line + "\r\n";
                }
            }

            StreamWriter sw = new StreamWriter(filePath, false);
            sw.Write(text);
            sw.Close();
        }

        private void BtnSwitcher_Click(object sender, EventArgs e)
        {
            if ( !canStartSwitcher )
            {
                SetLog(Color.Orange, "アルバムを読み込んでいないため、開始できません。");
            }
            else
            {
                try
                {
                    StartTypeSwitch( );
                    RewriteCBL( );
                    SetLog(Color.LimeGreen, "曲方式の変換に成功しました。");
                }
                catch (Exception ex)
                {
                    MessageBox.Show( ex.ToString(), "変換エラー" );
                    SetLog(Color.Orange, "曲方式の変換に失敗しました。");
                }
            }
        }

        #endregion

        #region Windows一般操作
        
        #region バージョンチェック

        private void GetNewVersion( )
        {
            DialogResult result = MessageBox.Show("新しいバージョンがリリースされています。\r\n確認しますか？", "【新バージョン確認】",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Exclamation,
                                                    MessageBoxDefaultButton.Button2);
            if (result == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start(APP_DOWNLOAD_URL);
                this.Close();
            }
        }

        private bool CheckNewVersion()
        {
            WebClient wc = new WebClient();
            try
            {
                string html = wc.DownloadString(VERSION_INFO_URL);
                string target = "track_id3_ver:";
                if (html.Contains(target))
                {
                    int st = html.IndexOf(target);
                    int len = target.Length;
                    int ed = 4;
                    this.new_ver = html.Substring(st + len, ed);
                    if (this.new_ver != CURRENT_VERSION)
                        return true;
                }
            }
            catch (WebException exc)
            {
                MessageBox.Show(exc.ToString(), "バージョン取得エラー");
            }

            return false;
        }

        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            bool ret = false;
            ret = CheckNewVersion( );
            if ( ret )
                GetNewVersion();

            LoadSetting();

            /* 座標初期化 */
            this.pnlPage1.Location          = new Point(DESIGN_DEF_X, MAIN_AREA_Y);
            this.ClientSize                 = new Size(APP_WIDTH, APP_HEIGHT);
            this.pnlAppFooterLine.Location  = new Point(DESIGN_DEF_X, FOOTER_LINE_Y);
            this.pnlAppFooter.Location      = new Point(DESIGN_DEF_X, FOOTER_Y);
            this.pnlTrackInfo.Location      = new Point(SUB_AREA_HIDE_X, MAIN_AREA_Y);
            SetupTrackInfoPanel();
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            SaveSetting();
            this.Close();
        }

        #region ユーザー入力データの記憶、呼び出し

        private void LoadSetting()
        {
            string path = Application.StartupPath + "\\item\\config.cbl";
            string text = "";

            if (File.Exists(path))
            {
                StreamReader sr = new StreamReader(path);
                text = sr.ReadToEnd();
                sr.Close();

                string line = "";
                string target = "";
                int st;
                int ed;
                StringReader rs = new StringReader(text);
                while (rs.Peek() > -1)
                {
                    line = rs.ReadLine();

                    target = "Jacket_Name:";
                    if (line.Contains(target))
                    {
                        st = target.Length;
                        ed = line.Length - st;
                        this.boxArtworkName.Text = line.Substring(st, ed);
                    }
                    target = "Track_Folder:";
                    if (line.Contains(target))
                    {
                        st = target.Length;
                        ed = line.Length - st;
                        this.boxTrackFolder.Text = line.Substring(st, ed);
                    }
                    target = "Is_Dot:";
                    if (line.Contains(target))
                    {
                        if (line.Contains("1"))
                            this.chkIsDot.Checked = true;
                        else if (line.Contains("0"))
                            this.chkIsDot.Checked = false;
                    }
                    target = "Is_AutoSearch:";
                    if (line.Contains(target))
                    {
                        if (line.Contains("1"))
                        {
                            this.autoSearchFile.Checked = true;
                            this.boxArtworkName.Enabled = false;
                            this.boxTrackFolder.Enabled = false;
                        }
                        else if (line.Contains("0"))
                        {
                            this.autoSearchFile.Checked = false;
                            this.boxArtworkName.Enabled = true;
                            this.boxTrackFolder.Enabled = true;
                        }
                    }
                }
            }
            else
            {
                SaveSetting( );
            }
        }

        private void SaveSetting( )
        {
            string path = Application.StartupPath + "\\item\\config.cbl";
            string text = "";

            text = "Jacket_Name:" + this.boxArtworkName.Text + "\r\n";
            text += "Track_Folder:" + this.boxTrackFolder.Text + "\r\n";
            if ( this.chkIsDot.Checked)
                text += "Is_Dot:" + "1\r\n";
            else
                text += "Is_Dot:" + "0\r\n";
            if (this.autoSearchFile.Checked)
                text += "Is_AutoSearch:" + "1\r\n";
            else
                text += "Is_AutoSearch:" + "0\r\n";
            
            StreamWriter sw = new StreamWriter(path, false);
            sw.Write(text);
            sw.Close();
        }

        #endregion

        #region ボーダーレスフォームでウィンドウ移動

        //マウスのクリック位置を記憶
        private Point mousePoint;

        private void getMousePos( MouseEventArgs e )
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                //位置を記憶する
                mousePoint = new Point(e.X, e.Y);
            }
        }

        private void moveWindow( MouseEventArgs e )
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                this.Left += e.X - mousePoint.X;
                this.Top += e.Y - mousePoint.Y;
            }
        }
        
        #region クリック時

        private void lblID3TagLabel_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void lblArtworkTitle_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void lblTrackFolder_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void pnlPage1_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void LblAlbumTitle_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void PnlAppHeader_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void LblAppTitle_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void PnlAppHeaderLine_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void PnlAppFooterLine_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void PnlAppFooter_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void LblAppLogTitle_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void LblAppLogText_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void PnlAppLine1_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void PnlAppLine2_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void ImgCurrentAlbumArtwork_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void LblID3TagTitle_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void LblID3TagNumber_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }
        private void LblCurrentTitle_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void LblCurrentText_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void LblArrow_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void LblNextText_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void LblNextTitle_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void ImgCurrentMode_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void LblArrow2_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        private void ImgNextMode_MouseDown(object sender, MouseEventArgs e)
        {
            getMousePos(e);
        }

        #endregion

        #region ドラッグ時
        
        private void lblID3TagLabel_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void lblTrackFolder_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void lblArtworkTitle_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void pnlPage1_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void LblAlbumTitle_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void PnlAppHeader_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void LblAppTitle_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void PnlAppHeaderLine_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void PnlAppFooterLine_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void PnlAppFooter_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void LblAppLogTitle_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void LblAppLogText_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void PnlAppLine1_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void PnlAppLine2_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void ImgCurrentAlbumArtwork_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void LblID3TagTitle_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void LblID3TagNumber_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }
        private void LblCurrentTitle_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void LblCurrentText_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void LblArrow_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void LblNextText_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void LblNextTitle_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void ImgCurrentMode_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void LblArrow2_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }

        private void ImgNextMode_MouseMove(object sender, MouseEventArgs e)
        {
            moveWindow(e);
        }




        #endregion

        #endregion

        #endregion

        #region ページ切り替え

        private async void btnOpenTrackInfoPage_Click(object sender, EventArgs e)
        {
            /* 移動先、計算量定義 */
            int st          = DESIGN_DEF_X;
            int ed          = MAIN_AREA_HIDE_X;
            int size        = APP_WIDTH;
            int footerLineY = FOOTER_LINE_Y;
            int footerY     = FOOTER_Y;
            int exitX       = EXIT_X;
            int vol         = 12;
            int page2X      = SUB_AREA_HIDE_X;
            int plusX       = 60;

            /* アニメーションでレイアウトが重くならないように */
            this.pnlPage1.SuspendLayout();
            this.btnExit.SuspendLayout();
            this.pnlAppFooter.SuspendLayout();
            this.pnlAppFooterLine.SuspendLayout();

            /* テキストボックスは非描画 */
            VisibleBoxes( false );

            /* アニメーション開始 */
            for ( int i = st; i > ed; i -=20 )
            {
                this.pnlPage1.Location = new Point( i, this.pnlPage1.Location.Y);

                size += vol;
                this.ClientSize = new System.Drawing.Size((size + plusX), size);

                exitX += vol;
                this.btnExit.Location = new Point( (exitX + plusX), this.btnExit.Location.Y);
                
                footerY += vol;
                footerLineY += vol;
                this.pnlAppFooter.Location = new Point(this.pnlAppFooter.Location.X, footerY);
                this.pnlAppFooterLine.Location = new Point(this.pnlAppFooterLine.Location.X, footerLineY);

                page2X -= vol;
                this.pnlTrackInfo.Location = new Point(page2X, this.pnlTrackInfo.Location.Y);

                await Task.Run(() => Thread.Sleep(10));
            }

            /* テキストボックス表示させる */
            VisibleBoxes(true);

            /* 固定位置に最終フレームで配置 */
            this.pnlPage1.Location          = new Point(MAIN_AREA_HIDE_X    , MAIN_AREA_Y);
            this.ClientSize                 = new Size( APP_BIG_WIDTH       , APP_BIG_HEIGHT);
            this.btnExit.Location           = new Point(EXIT_BIG_X          , EXIT_Y);
            this.pnlAppFooter.Location      = new Point(DESIGN_DEF_X        , FOOTER_BIG_Y);
            this.pnlAppFooterLine.Location  = new Point(DESIGN_DEF_X        , FOOTER_LINE_BIG_Y);
            this.pnlTrackInfo.Location      = new Point(DESIGN_DEF_X        , MAIN_AREA_Y);
            await Task.Run(() => Thread.Sleep(10));

            /* レイアウトのロック解除 */
            this.pnlPage1.ResumeLayout();
            this.btnExit.ResumeLayout();
            this.pnlAppFooter.ResumeLayout();
            this.pnlAppFooterLine.ResumeLayout();
            //this.lblAppTitle.Text = "(" + this.ClientSize.Width + " , " + this.ClientSize.Height + ")";
        }
        
        private async void btnCloseTrackInfo_Click(object sender, EventArgs e)
        {
            int st          = -500;
            int ed          = DESIGN_DEF_X;
            int size        = APP_BIG_WIDTH;
            int footerLineY = FOOTER_LINE_BIG_Y;
            int footerY     = FOOTER_BIG_Y;
            int exitX       = EXIT_BIG_X;
            int vol         = 12;
            int page2X      = DESIGN_DEF_X;
            int plusX       = 60;

            this.pnlPage1.SuspendLayout();
            this.btnExit.SuspendLayout();
            this.pnlAppFooter.SuspendLayout();
            this.pnlAppFooterLine.SuspendLayout();
            for (int i = st; i < ed; i += 20)
            {
                this.pnlPage1.Location = new Point(i, this.pnlPage1.Location.Y);

                size -= vol;
                this.ClientSize = new System.Drawing.Size( (size + plusX), size);

                exitX -= vol;
                this.btnExit.Location = new Point((exitX + plusX), this.btnExit.Location.Y);

                footerY -= vol;
                footerLineY -= vol;
                this.pnlAppFooter.Location = new Point(this.pnlAppFooter.Location.X, footerY);
                this.pnlAppFooterLine.Location = new Point(this.pnlAppFooterLine.Location.X, footerLineY);

                page2X += vol;
                this.pnlTrackInfo.Location = new Point(page2X, this.pnlTrackInfo.Location.Y);

                await Task.Run(() => Thread.Sleep(10));
            }
            
            /* 固定位置に最終フレームで配置 */
            this.pnlPage1.Location          = new Point(DESIGN_DEF_X    , MAIN_AREA_Y);
            this.ClientSize                 = new Size( APP_WIDTH       , APP_HEIGHT);
            this.btnExit.Location           = new Point(EXIT_X          , EXIT_Y);
            this.pnlAppFooter.Location      = new Point(DESIGN_DEF_X    , FOOTER_Y);
            this.pnlAppFooterLine.Location  = new Point(DESIGN_DEF_X    , FOOTER_LINE_Y);
            this.pnlTrackInfo.Location      = new Point(SUB_AREA_HIDE_X , MAIN_AREA_Y);
            await Task.Run(() => Thread.Sleep(10));
            
            this.pnlPage1.ResumeLayout();
            this.btnExit.ResumeLayout();
            this.pnlAppFooter.ResumeLayout();
            this.pnlAppFooterLine.ResumeLayout();
        }

        #endregion

        #region trackinfo.cbl作成画面

        #region 曲情報画面のセットアップ

        Label[] lblTracks;
        TextBox[,] boxTracks;

        private void AddTrackInfoTitle()
        {
            int DEF_Y = 74;
            int VOL_Y = 25;
            this.lblTracks = new Label[MAX_TRACK];

            for (int i = 0; i < MAX_TRACK; i++)
            {
                this.lblTracks[i] = new Label();
                this.lblTracks[i].SuspendLayout();
                this.pnlTrackInfo.Controls.Add(this.lblTracks[i]);
                this.lblTracks[i].AutoSize = true;
                this.lblTracks[i].BackColor = System.Drawing.Color.Transparent;
                this.lblTracks[i].Font = new System.Drawing.Font("HGPｺﾞｼｯｸM", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
                this.lblTracks[i].ForeColor = System.Drawing.SystemColors.ButtonFace;
                this.lblTracks[i].Location = new System.Drawing.Point(5, DEF_Y);
                this.lblTracks[i].Name = "lblTrack" + i;
                this.lblTracks[i].Size = new System.Drawing.Size(25, 12);
                this.lblTracks[i].TabIndex = 54;
                this.lblTracks[i].Text = "" + (i + 1) + "）";
                this.lblTracks[i].ResumeLayout();
                DEF_Y += VOL_Y;
            }
        }

        private void AddTrackInfoBox()
        {
            int DEF_X = 50;
            int DEF_Y = 74;
            int VOL_X = 72;
            int VOL_Y = 25;
            this.boxTracks = new TextBox[MAX_TRACK, MAX_TAG];

            for (int track = 0; track < MAX_TRACK; track++)
            {
                for (int tag = 0; tag < MAX_TAG; tag++)
                {

                    this.boxTracks[track, tag] = new TextBox();
                    this.boxTracks[track, tag].SuspendLayout();
                    this.pnlTrackInfo.Controls.Add(this.boxTracks[track, tag]);
                    this.boxTracks[track, tag].Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
                    this.boxTracks[track, tag].Location = new System.Drawing.Point(DEF_X, DEF_Y);
                    this.boxTracks[track, tag].Name = "boxTrack" + (track + tag);
                    this.boxTracks[track, tag].Size = new System.Drawing.Size(66, 19);
                    this.boxTracks[track, tag].TabIndex = 53;
                    this.boxTracks[track, tag].Text = "";
                    this.boxTracks[track, tag].ResumeLayout();
                    DEF_X += VOL_X;
                }
                DEF_X = 50;
                DEF_Y += VOL_Y;
            }
        }

        private void VisibleBoxes(bool show)
        {
            for (int track = 0; track < MAX_TRACK; track++)
            {
                for (int tag = 0; tag < MAX_TAG; tag++)
                {

                    this.boxTracks[track, tag].Visible = show;

                }
            }
        }

        private void DeleteAnotherBoxes()
        {
            for (int track = 0; track < MAX_TRACK; track++)
            {
                if (this.currentMaxTrack <= track)
                {
                    for (int tag = 0; tag < MAX_TAG; tag++)
                    {
                        this.boxTracks[track, tag].Text = "";
                    }
                }
            }
        }

        private void SetupTrackInfoPanel()
        {
            AddTrackInfoTitle();
            AddTrackInfoBox();
        }

        #endregion

        private void btnSaveTrackinfo_Click(object sender, EventArgs e)
        {
            try
            {
                WriteTrackInfo( );

                string path = this.boxAlbumPath.Text;
                string file = path + "\\trackinfo.cbl";
                bool ret = File.Exists(file );
                if ( ret )
                {
                    GetID3List(file);     /* まずID3 Tagリストを取得 */
                    AnalysisID3List();        /* 取得できた曲数分、ID3 Tagの配列を生成し、リストの解析を行う */
                    SetID3ListValue();
                }

            }
            catch
            {
                SetLog(Color.Orange, "書き出しに失敗しました。");
            }
        }

        private void WriteTrackInfo( )
        {

            string path = Application.StartupPath + "\\trackinfo.cbl";
            string zero = "";
            string text = "";

            text += "Is_Type:ARN\r\n";
            text += "Album_Label:" + this.boxLabelName.Text + "\r\n";
            text += "Album_Name:" + this.boxAlbumName.Text + "\r\n";
            text += "Album_Number:" + this.boxAlbumNumber.Text + "\r\n";
            text += "==============================\r\n";
            for (int j = 0; j < MAX_TAG; j++)
            {
                for (int i = 0; i < MAX_TRACK; i++)
                {
                    if (i < 9)
                        zero = "0";
                    else
                        zero = "";
                    switch ( j )
                    {
                        case ARN_NAME:
                            text += "ARN_Name_" + zero + (i + 1) + ":" + this.boxTracks[i, ARN_NAME].Text + "\r\n";
                            break;
                        case FYS_NAME:
                            /* ナンバリング */
                            text += "FYS_Name_" + zero + (i + 1) + ":";

                            /* 原曲名 ( */
                            text += this.boxTracks[i, FYS_NAME].Text + " (";

                            /* カスタム名分岐 */
                            if ( this.boxTracks[i, CUSTOM].Text != "" )
                            {
                                /* カスタム名ルート */
                                text += this.boxTracks[i, CUSTOM].Text + ")\r\n";
                            }
                            else
                            {
                                /* 命名規則ルート */

                                /* 作者 */
                                text += this.boxTracks[i, CREATOR].Text;

                                /* 's */
                                if (this.chkIsXXs.Checked)
                                    text += "'s ";
                                else
                                    text += " ";

                                /* ジャンル名 */
                                text += this.boxTracks[i, GENRE].Text;

                                /* Bootleg) */
                                text += " " + this.boxLastWord.Text + ")\r\n";
                            }
                            break;
                        case SUBTITLE:
                            text += "FYS_Subtitle_" + zero + (i + 1) + ":" + this.boxTracks[i, SUBTITLE].Text + "\r\n";
                            break;
                        case ARTIST:
                            text += "FYS_Artist_" + zero + (i + 1) + ":" + this.boxTracks[i, ARTIST].Text + "\r\n";
                            break;
                        case COMMENT:
                            text += "FYS_Comment_" + zero + (i + 1) + ":" + this.boxTracks[i, COMMENT].Text + "\r\n";
                            break;
                        case CREATOR:
                            text += "Creator_" + zero + (i + 1) + ":" + this.boxTracks[i, CREATOR].Text + "\r\n";
                            break;
                        case BPM:
                            text += "BPM_" + zero + (i + 1) + ":" + this.boxTracks[i, BPM].Text + "\r\n";
                            break;
                        case GENRE:
                            text += "Genre_" + zero + (i + 1) + ":" + this.boxTracks[i, GENRE].Text + "\r\n";
                            break;
                        case CUSTOM:
                            text += "FYS_CustomName_" + zero + (i + 1) + ":" + this.boxTracks[i, CUSTOM].Text + "\r\n";
                            break;
                    }
                }
                if ( (j == ARN_NAME) || (j == COMMENT))
                    text += "==============================\r\n";
            }

            SaveFileDialog sfd = new SaveFileDialog();

            //はじめのファイル名を指定する
            //はじめに「ファイル名」で表示される文字列を指定する
            sfd.FileName = "trackinfo.cbl";
            //はじめに表示されるフォルダを指定する
            if ( this.canStartSwitcher )
                sfd.InitialDirectory = this.boxAlbumPath.Text;
            else
                sfd.InitialDirectory = Application.StartupPath;
            //[ファイルの種類]に表示される選択肢を指定する
            //指定しない（空の文字列）の時は、現在のディレクトリが表示される
            sfd.Filter = "cblファイル(*.cbl;)|*.cbl|すべてのファイル(*.*)|*.*";
            //[ファイルの種類]ではじめに選択されるものを指定する
            //2番目の「すべてのファイル」が選択されているようにする
            sfd.FilterIndex = 1;
            //タイトルを設定する
            sfd.Title = "保存先のファイルを選択してください";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            sfd.RestoreDirectory = true;
            //既に存在するファイル名を指定したとき警告する
            //デフォルトでTrueなので指定する必要はない
            sfd.OverwritePrompt = true;
            //存在しないパスが指定されたとき警告を表示する
            //デフォルトでTrueなので指定する必要はない
            sfd.CheckPathExists = true;

            //ダイアログを表示する
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                StreamWriter sw = new StreamWriter(sfd.FileName, false);
                sw.Write(text);
                sw.Close();

                SetLog(Color.LimeGreen, "書き出しに成功しました。");
                string msg = "";
                if (this.canStartSwitcher)
                    msg = "trackinfo.cblの書き出しに成功しました。";
                else
                    msg = "trackinfo.cblの書き出しに成功しました。\r\nこの書き出されたcblファイルを変換したいアルバムのルートディレクトリに配置してください。";
                MessageBox.Show(msg, "書き出し成功");
            }
        }

        private void btnShowCurrentTrackName_Click(object sender, EventArgs e)
        {
            string text = "";
            string zero = "";

            text += "【ARN式表示名】\r\n";
            for (int i = 0; i < MAX_TRACK; i++)
            {
                if (i < 9) zero = "0";
                else zero = "";
                text += zero + (i + 1) + ". " + this.boxTracks[i, ARN_NAME].Text + ".mp3\r\n";
            }
            text += "===============\r\n";
            text += "【FYS式表示名】\r\n";
            for (int i = 0; i < MAX_TRACK; i++)
            {
                /* ナンバリング */
                if (i < 9) zero = "0";
                else zero = "";
                text += zero + (i + 1) + ". ";

                /* 原曲名 ( */
                text += this.boxTracks[i, FYS_NAME].Text + " (";

                /* カスタム名分岐 */
                if (this.boxTracks[i, CUSTOM].Text != "")
                {
                    /* カスタム名入っていたので()内に優先入力 */
                    text += this.boxTracks[i, CUSTOM].Text + ").mp3\r\n";
                }
                else
                {
                    /* カスタム名未入力なので、ツール制度に従って標準入力 */

                    /* 作者名 */
                    text += this.boxTracks[i, CREATOR].Text;

                    /* 's */
                    if (this.chkIsXXs.Checked)
                        text += "'s ";
                    else
                        text += " ";

                    /* ジャンル名 */
                    text += this.boxTracks[i, GENRE].Text;

                    /* Bootleg) */
                    text += " " + this.boxLastWord.Text + ").mp3\r\n";
                }
            }
            MessageBox.Show(text, "【表示名の確認】");
        }

        #endregion

    }
}
