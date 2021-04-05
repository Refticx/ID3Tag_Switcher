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
using NAudio.Wave;
using NAudio.Flac;
using MediaToolkit.Model;
using MediaToolkit;
using CUETools.Codecs;
using CUETools.Codecs.FLAKE;

namespace trackID3TagSwitcher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /* マルチスレッド */
        private SynchronizationContext _mainContext;            /* asyncのサブスレッドからメインUIスレッドに処理を戻すための変数 */
        public static Defines.ExitStatus isExitStatus = Defines.ExitStatus.Launching;
        private string trackcbl = "";                           /* trackinfo.cblから読み込んだ文字列全体を格納する変数 */
        private string[,] TrackID3Tag;                          /* trackinfo.cblから読み込んだ各ID3 Tag情報を記憶する二次元配列 */
        private string currentAlbumReleaseTitle = "";           /* 現在読み込んでいるアルバムのリリースタイトル */
        private string currentAlbumReleaseNumber = "";          /* 現在読み込んでいるアルバムのリリース番号 */
        private string currentAlbumLabelName = "";              /* 現在読み込んでいるアルバムのリリース元レーベル */
        private int currentMaxTrack = 0;                        /* 現在読み込んでいるアルバムの曲数 */
        private bool isTypeFYS = false;                         /* 現在読み込んでいるアルバムの曲形式がARNかFYSかを判断する変数 */
        private bool isTypeWaveVo = false;                      /* 現在読み込んでいるアルバムの音源がインストかボーカルかを判断する変数 */
        private bool canStartSwitcher = false;                  /* アルバムを正常に読み込めていて、変換可能かを示す変数 */
        private string new_ver = "";                            /* 取得したアプリ更新ページのバージョンを記憶する変数 */
        private string artworkPath = "";                        /* 取得したアートワークのファイルパスを格納する変数 */
        private string tracksPath = "";                         /* 楽曲が格納されているディレクトリパスを格納する変数 */
        private TagLib.IPicture aawork;                         /* 取得したアートワークのイメージを格納する変数 */
        private MessageForm messageForm = new MessageForm();    /* ダイアログ用フォームを作成しておく */
        private AccountForm loginForm = new AccountForm( );     /* ログイン用フォームを作成しておく */
        private char[] invalidChars;                            /* 設定中の文字列内に、使用不可能な文字があるかチェックするための変数 */
        private string invalidReplase;                          /* 設定中の文字列内に、使用不可能な文字があっ他場合に、置き換えするための変数 */
        private Defines.ConvertExt m_convertExt;                /* タグ設定、サウンドレイヤー対象の音源の拡張子 */
        /* 設定パネル */
        private bool m_settings_open = false;
        private bool m_settings_running = false;


        private Task<bool> m_coroutine_task_bool;
        private Task<string> m_coroutine_task_str;
        private bool m_coroutine_flag = false;
        private string m_coroutine_text = String.Empty;
        private WebClient m_webClient;
        private Uri m_uriAsync;
        private string m_serverContent = String.Empty;
        private string m_startText = String.Empty;
        private string m_endText = String.Empty;
        private int m_startPos = 0;
        private int m_endPos = 0;
        private int m_length = 0;
        private Defines.AlbumLicense m_currLicense;

        /* 音源合成 */
        private Thread coroutineVocalPlus;     /* ボーカル合成用スレッド */
        /// <summary>
        /// アカウントリストのページの暗号化の際の移動オフセット量
        /// </summary>
        private const int ENCRYPT_SHIFT_SIZE_LICENSE_PAGE = 4;
        private const int RGB_MAX = 255;
        private const int RGB_MIN = 0;
        private bool isEnterExitBtn = false;
        private Thread coroutineExitButton;




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
            this.lbl_currTagType.Text = "";
            this.lbl_currTagType.Visible = false;
            this.lbl_currWavesType.Text = "";
            this.lbl_currWavesType.Visible = false;
            this.imgCurrentMode.ImageLocation = "";
            this.imgCurrentMode.Visible = false;
            this.lblArrow2.Visible = false;
            this.isTypeFYS = false;
            this.isTypeWaveVo = false;
            this.canStartSwitcher = false;
            this.box_WaveLink.Text = String.Empty;
            /* 曲数 */
            this.lblTrackCount.Visible = false;
            this.lblTrackCount.Text = "";

            for (int tag = 0; tag < Defines.MAX_TAG; tag++)
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

        private async void BtnClearCache_Click(object sender, EventArgs e)
        {
            await SlideOutPanels( );
            ClearCache();
            SetLog(Color.LimeGreen, MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.CLEAR_LOAD_DATA]);
        }

        private async Task SlideOutPanels( )
        {
            bool loop = true;
            int fast = 30;
            int medium = fast - 5;
            int lower = medium - 10;

            while ( loop )
            {
                if ( ( this.pnl_currLoadInfo.Location.X - fast ) > Defines.MAIN_AREA_HIDE_X )
                    this.pnl_currLoadInfo.Location = new Point( this.pnl_currLoadInfo.Location.X - fast , Defines.CURR_INFO_AREA_Y );
                else
                    this.pnl_currLoadInfo.Location = new Point( Defines.MAIN_AREA_HIDE_X , Defines.CURR_INFO_AREA_Y );

                if ( ( this.pnl_currTagType.Location.X - medium ) > Defines.MAIN_AREA_HIDE_X )
                    this.pnl_currTagType.Location = new Point( this.pnl_currTagType.Location.X - medium , Defines.CURR_ID3_AREA_Y );
                else
                    this.pnl_currTagType.Location = new Point( Defines.MAIN_AREA_HIDE_X , Defines.CURR_ID3_AREA_Y );

                if ( ( this.pnl_execConvert.Location.X - lower ) > Defines.MAIN_AREA_HIDE_X )
                    this.pnl_execConvert.Location = new Point( this.pnl_execConvert.Location.X - lower , Defines.EXEC_CONV_AREA_Y );
                else
                    this.pnl_execConvert.Location = new Point( Defines.MAIN_AREA_HIDE_X , Defines.EXEC_CONV_AREA_Y );

                if ( ( this.pnl_currLoadInfo.Location.X == Defines.MAIN_AREA_HIDE_X ) && ( this.pnl_currTagType.Location.X == Defines.MAIN_AREA_HIDE_X ) && ( this.pnl_execConvert.Location.X == Defines.MAIN_AREA_HIDE_X ) )
                    loop = false;

                await Task.Delay( 1 );
            }
        }

        #endregion

        #region 設定パネル

        private async void img_btn_setting_Click( object sender , EventArgs e )
        {
            /* アニメーション実行中は重複処理させない */
            if ( m_settings_running )
                return;

            if ( !m_settings_open )
            {
                this.pnl_albumSearch.Enabled = false;
                this.pnl_currLoadInfo.Enabled = false;
                this.pnl_currTagType.Enabled = false;
                this.pnl_execConvert.Enabled = false;
                await SlideInSettingPanels( );
            }
            else
            {
                await SlideOutSettingPanels( );
                this.pnl_albumSearch.Enabled = true;
                this.pnl_currLoadInfo.Enabled = true;
                this.pnl_currTagType.Enabled = true;
                this.pnl_execConvert.Enabled = true;
            }
        }

        private async Task SlideInSettingPanels( )
        {
            m_settings_running = true;
            int speed = 20;

            while ( m_settings_running )
            {
                if ( ( this.pnl_settings.Location.Y + speed ) < Defines.DESIGN_DEF_Y )
                    this.pnl_settings.Location = new Point( Defines.DESIGN_DEF_X , this.pnl_settings.Location.Y + speed );
                else
                {
                    this.pnl_settings.Location = new Point( Defines.DESIGN_DEF_X , Defines.DESIGN_DEF_Y );
                    m_settings_open = true;
                    m_settings_running = false;
                }

                await Task.Delay( 1 );
            }
        }

        private async Task SlideOutSettingPanels( )
        {
            m_settings_running = true;
            int speed = 20;

            while ( m_settings_running )
            {
                if ( ( this.pnl_settings.Location.Y - speed ) > Defines.SETTING_AREA_HIDE_Y )
                    this.pnl_settings.Location = new Point( Defines.DESIGN_DEF_X , this.pnl_settings.Location.Y - speed );
                else
                {
                    this.pnl_settings.Location = new Point( Defines.DESIGN_DEF_X , Defines.SETTING_AREA_HIDE_Y );
                    m_settings_open = false;
                    m_settings_running = false;
                }

                await Task.Delay( 1 );
            }
        }

        #endregion

        #region アルバム読み込み

        #region 対象拡張子変更

        private void cmbbx_convertExt_SelectedIndexChanged( object sender , EventArgs e )
        {
            m_convertExt = (Defines.ConvertExt)this.cmbbx_convertExt.SelectedIndex;
        }

        #endregion

        #region 各読み込みスクリプト

        private bool GetID3List( string path )
        {
            /* ID3リストファイルがあるか検索 */
            bool ret = File.Exists(path);
            if (!ret)
            {
                /* 確認ダイアログを表示 */
                messageForm.SetFormState(MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.NOT_FOUND_ID3LIST] + MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.QST_MAKE_ID3LIST], MessageForm.MODE_YN);
                DialogResult dr = messageForm.ShowDialog();

                /* YesならID3作成画面へ飛ばす */
                if (dr == DialogResult.Yes)
                    btnOpenTrackInfoPage.PerformClick();

                /* ログメッセージ表示 */
                SetLog(Color.Orange, MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.NEED_MAKE_ID3LIST]);
                return false;
            }

            /* あったらリスト情報を読み込む */
            StreamReader sr = new StreamReader(path );
            this.trackcbl = sr.ReadToEnd();
            sr.Close();
            if (this.trackcbl.Length == 0)
            {
                /* 確認ダイアログを表示 */
                messageForm.SetFormState(MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.FOUND_ID3LIST] + MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.NODATA_ID3LIST] + MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.QST_MAKE_ID3LIST], MessageForm.MODE_YN);
                DialogResult dr = messageForm.ShowDialog();

                /* YesならID3作成画面へ飛ばす */
                if (dr == DialogResult.Yes)
                    btnOpenTrackInfoPage.PerformClick();

                /* ログメッセージ表示 */
                SetLog(Color.Orange, MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.NEED_MAKE_ID3LIST]);
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
                                if (atw[i].Contains("AlbumArtSmall") || atw[i].Contains("Folder") || atw[i].Contains("AlbumArt_{"))
                                    continue;
                                /* それ以外の画像は */
                                else
                                {
                                    /* 確認ダイアログを表示 */
                                    messageForm.SetFormState(MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Check_Artwork] +
                                                                MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Is_This_OK] +
                                                                MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.DIR] +
                                                                atw[i], MessageForm.MODE_YN, atw[i]);
                                    DialogResult dr = messageForm.ShowDialog();
                                    if (dr == DialogResult.Yes)
                                    {
                                        this.artworkPath = atw[i];
                                        isFind = true;
                                        isExt = 100;
                                        break;
                                    }
                                }
                            }
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
                        messageForm.SetFormState(MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Check_Artwork] +
                                                    MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Is_This_OK] +
                                                    MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.DIR] +
                                                    this.artworkPath, MessageForm.MODE_YN, this.artworkPath);
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
                messageForm.SetFormState(MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Irregular_Error] + ex.ToString(), MessageForm.MODE_OK);
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
                this.imgCurrentMode.ImageLocation = Application.StartupPath + "\\item\\arn.jpg";
                this.lbl_currTagType.Text = "ID3タグ：A-Remix Nation方式";
            }
            else
            {
                this.imgCurrentMode.ImageLocation = Application.StartupPath + "\\item\\fys.jpg";
                this.lbl_currTagType.Text = "ID3タグ：For You Sounds方式";
            }

            if ( !this.isTypeWaveVo )
                this.lbl_currWavesType.Text = "波形：ボーカル版";
            else
                this.lbl_currWavesType.Text = "波形：Instrumental版";

            this.imgCurrentMode.Visible = true;
            this.lbl_currTagType.Visible = true;
            this.lbl_currWavesType.Visible = true;
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

                    /* ユーザー設定の拡張子のファイルのみを取得 */
                    string[] songs;
                    songs = allFiles.Where(s => s.EndsWith("." + m_convertExt.ToString() , StringComparison.OrdinalIgnoreCase)).ToArray();

                    /* 音源ファイルがあるかどうか */
                    if (songs.Length != 0)
                    {
                        /* 格納先ディレクトリを取得 */
                        string songDir = Path.GetDirectoryName(songs[0]);

                        /* 表示メッセージを作成 */
                        string msg = MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.FOUND_MP3S] +
                                        MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.DIR] + songDir + "\r\n\r\n";
                        for ( int i = 0; i < songs.Length; i++ )
                        {
                            msg += Path.GetFileName(songs[i]) + "\r\n";
                        }

                        /* 確認ダイアログを表示 */
                        messageForm.SetFormState(msg, MessageForm.MODE_YN);
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
                        messageForm.SetFormState(MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.NOT_FOUND_SONG_DIR] +
                                                MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.PLZ_CHECK_FILE_PATH], MessageForm.MODE_OK);
                        messageForm.ShowDialog();

                        /* ログメッセージ表示 */
                        SetLog(Color.Orange, MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.NOT_FOUND_SONG]);

                        /* 処理を中断 */
                        return false;
                    }

                    DirectoryInfo di = new DirectoryInfo(this.tracksPath);
                    FileInfo[] songs = di.GetFiles( "." + m_convertExt.ToString( ) , SearchOption.AllDirectories);

                    /* mp3ファイルがあるかどうか */
                    if (songs.Length != 0)
                    {
                        /* 表示メッセージを作成 */
                        string msg = MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.FOUND_MP3S] +
                                        MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.DIR] + 
                                        songs[0].DirectoryName + "\r\n\r\n";
                        for (int i = 0; i < songs.Length; i++)
                        {
                            msg += songs[i].Name + "\r\n";
                        }

                        /* 確認ダイアログを表示 */
                        messageForm.SetFormState(msg, MessageForm.MODE_YN);
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
                SetLog(Color.Orange, MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.NOT_FOUND_SONG]);
                /* 処理を終了 */
                return false;
            }
            catch (Exception ex)
            {
                /* 確認ダイアログを表示 */
                messageForm.SetFormState(MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Irregular_Error] + ex.ToString( ), MessageForm.MODE_OK);
                messageForm.ShowDialog();
                /* ログメッセージ表示 */
                SetLog(Color.Orange, MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.NOT_FOUND_SONG]);
                return false;
            }
        }
        private bool AnalysisID3List()
        {
            /* 
             要素数8（ARN名、FYS名、サブタイトル、アーティスト、コメント、作曲者、Defines.BPM、ジャンル）
             */
            TrackID3Tag = new string[this.currentMaxTrack, Defines.MAX_TAG];
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

                target = Defines.TRACKINFO_ALBUM_LABEL + ":";
                if (line.Contains(target))
                {
                    st = target.Length;
                    ed = line.Length - st;
                    this.currentAlbumLabelName = line.Substring(st, ed);
                    /* どれか1つでも対象のワードを発見できたらデータ正常扱いにする */
                    isBreak++;
                    continue;
                }

                target = Defines.TRACKINFO_ALBUM_NAME + ":";
                if (line.Contains(target))
                {
                    st = target.Length;
                    ed = line.Length - st;
                    this.currentAlbumReleaseTitle = line.Substring(st, ed);
                    /* どれか1つでも対象のワードを発見できたらデータ正常扱いにする */
                    isBreak++;
                    continue;
                }

                target = Defines.TRACKINFO_ALBUM_NUMBER + ":";
                if (line.Contains(target))
                {
                    st = target.Length;
                    ed = line.Length - st;
                    this.currentAlbumReleaseNumber = line.Substring(st, ed);
                    /* どれか1つでも対象のワードを発見できたらデータ正常扱いにする */
                    isBreak++;
                    continue;
                }

                target = Defines.TRACKINFO_ID3_TYPE + ":";
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

                target = Defines.TRACKINFO_WAVE_TYPE + ":";
                if ( line.Contains( target ) )
                {
                    st = target.Length;
                    ed = line.Length - st;
                    string mode = line.Substring( st , ed );
                    if ( mode == "INST" )
                        this.isTypeWaveVo = true;
                    else if ( mode == "VO" )
                        this.isTypeWaveVo = false;
                    /* このタグはアップデートで追加した機能であり、見つける必要はない */
                    continue;
                }

                target = Defines.TRACKINFO_WAVE_LINK + ":";
                if ( line.Contains( target ) )
                {
                    st = target.Length;
                    ed = line.Length - st;
                    string link = line.Substring( st , ed );
                    this.box_WaveLink.Text = link;
                    /* このタグはアップデートで追加した機能であり、見つける必要はない */
                    continue;
                }

                for (int tag = 0; tag < Defines.MAX_TAG; tag++)
                {
                    for (int track = 0; track < this.currentMaxTrack; track++)
                    {
                        switch (tag)
                        {
                            case 0: target = "Defines.ARN_NAME_"; break;
                            case 1: target = "Defines.FYS_NAME_"; break;
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
                messageForm.SetFormState(MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Break_ID3List] +
                                            MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Make_ID3List], 
                                            MessageForm.MODE_OK);
                messageForm.ShowDialog();

                /* ログメッセージ表示 */
                SetLog(Color.Orange, MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Break_ID3List]);
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
            for (int tag = 0; tag < Defines.MAX_TAG; tag++)
            {
                for (int track = 0; track < this.currentMaxTrack; track++)
                {
                    if ( tag != Defines.FYS_NAME )
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
        private async Task SlideInPanels( )
        {
            bool loop = true;
            int fast = 30;
            int medium = fast - 5;
            int lower = medium - 10;

            while ( loop )
            {
                if ( ( this.pnl_currLoadInfo.Location.X + fast ) < Defines.DESIGN_DEF_X )
                    this.pnl_currLoadInfo.Location = new Point( this.pnl_currLoadInfo.Location.X + fast , Defines.CURR_INFO_AREA_Y);
                else
                    this.pnl_currLoadInfo.Location = new Point( Defines.DESIGN_DEF_X , Defines.CURR_INFO_AREA_Y );

                if ( ( this.pnl_currTagType.Location.X + medium ) < Defines.DESIGN_DEF_X )
                    this.pnl_currTagType.Location = new Point( this.pnl_currTagType.Location.X + medium , Defines.CURR_ID3_AREA_Y );
                else
                    this.pnl_currTagType.Location = new Point( Defines.DESIGN_DEF_X , Defines.CURR_ID3_AREA_Y );

                if ( ( this.pnl_execConvert.Location.X + lower ) < Defines.DESIGN_DEF_X )
                    this.pnl_execConvert.Location = new Point( this.pnl_execConvert.Location.X + lower , Defines.EXEC_CONV_AREA_Y );
                else
                    this.pnl_execConvert.Location = new Point( Defines.DESIGN_DEF_X , Defines.EXEC_CONV_AREA_Y );

                if ( ( this.pnl_currLoadInfo.Location.X == Defines.DESIGN_DEF_X ) && ( this.pnl_currTagType.Location.X == Defines.DESIGN_DEF_X ) && ( this.pnl_execConvert.Location.X == Defines.DESIGN_DEF_X ) )
                    loop = false;

                await Task.Delay( 1 );
            }
        }
        private async Task CheckAlbumConfig( string path )
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
                    messageForm.SetFormState(MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Found_Artwork], MessageForm.MODE_OK);
                    DialogResult dr = messageForm.ShowDialog();

                    /* アートワークを非表示 */
                    this.imgCurrentAlbumArtwork.ImageLocation = "";
                    this.imgCurrentAlbumArtwork.Visible = false;
                }

                /* 一度曲保存先にある音源を全部取得し、何曲あるか確認する */
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

                /* 各種パネルの表示アニメーション */
                await SlideInPanels( );

                SetLog(Color.LimeGreen, MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Success_Load_Album]);
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

        private async void btnLoadAlbum_DragDrop(object sender, DragEventArgs e)
        {
            // 実際にデータを取り出す
            var data = e.Data.GetData(DataFormats.FileDrop, true) as string[];

            // データが取得できたか判定する
            if (data != null)
            {
                foreach (var filePath in data)
                {
                    await CheckAlbumConfig( filePath );
                    break;
                }
            }
        }

        #endregion

        private async void BtnLoadAlbum_Click(object sender, EventArgs e)
        {
            var dialog = new CommonOpenFileDialog("アルバムを選択してください。");
            // フォルダ選択モード
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                
                await CheckAlbumConfig( dialog.FileName );
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
                nameNum = Defines.FYS_NAME;
            else
                nameNum = Defines.ARN_NAME;

            /* ループ処理用の変数を初期化 */
            string num = "";
            int track = 0;

            /* 楽曲をすべて取得し、名前順にソートする */
            DirectoryInfo di = new DirectoryInfo(this.tracksPath);
            FileInfo[] files = di.GetFiles("*." + m_convertExt.ToString() , SearchOption.AllDirectories);
            Array.Sort(files, (x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name));

            /*
            string msg = "";
            for (int i = 0; i < files.Length; i++)
            {
                msg += files[i].Name + "\r\n";
            }

            messageForm.SetFormState(msg, MessageForm.MODE_OK);
            messageForm.ShowDialog();
            */

            foreach (var f in files)
            {
                TagLib.File file = TagLib.File.Create( f.FullName );
                file.Tag.Title = TrackID3Tag[track, nameNum];
                file.Tag.Track = Convert.ToUInt16((track + 1));
                file.Tag.AlbumArtists = new string[] { this.currentAlbumLabelName };
                file.Tag.Album = this.currentAlbumReleaseTitle;
                file.Tag.Composers = new string[] { TrackID3Tag[track, Defines.CREATOR] };
                file.Tag.Genres = new string[] { TrackID3Tag[track, Defines.GENRE] };
                file.Tag.BeatsPerMinute = Convert.ToUInt16( TrackID3Tag[track, Defines.BPM] );
                file.Tag.Pictures = new TagLib.IPicture[1] { this.aawork };
                file.Tag.Conductor = this.currentAlbumReleaseNumber + "_" + ( track + 1 ).ToString( );
                if (!this.isTypeFYS)
                {
                    file.Tag.Grouping = TrackID3Tag[track, Defines.SUBTITLE];
                    file.Tag.Performers = new string[] { TrackID3Tag[track, Defines.ARTIST] , TrackID3Tag[track, Defines.CREATOR] };
                    file.Tag.Comment = TrackID3Tag[track, Defines.COMMENT];
                }
                else
                {
                    file.Tag.Grouping = "";
                    file.Tag.Performers = new string[] { TrackID3Tag[track, Defines.CREATOR] };
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

                /* システム予約文字が含まれているかの確認 */
                TrackID3Tag[track, nameNum] = CheckRegisterWord( TrackID3Tag[track, nameNum] );
                
                File.Move(f.FullName , f.DirectoryName + "\\" + num + TrackID3Tag[track, nameNum] + "." + m_convertExt.ToString( ) );
                file.Dispose( );
                track++;

                //await Task.Run( ( ) => Thread.Sleep( 1 ) );
            }

            /* UI上のオブジェクトを操作するためメインスレッドで処理させる */
            _mainContext.Post( _ => OnUI_SwitchAlbumType( ) , null );
        }

        /// <summary>
        /// UI上のアルバムタイプの表示を切り替える
        /// </summary>
        private void OnUI_SwitchAlbumType( )
        {
            if ( !this.isTypeFYS )
            {
                this.imgCurrentMode.ImageLocation = Application.StartupPath + "\\item\\fys.jpg";
                this.lbl_currTagType.Text = "ID3タグ：For You Sounds方式";
                this.isTypeFYS = true;
            }
            else
            {
                this.imgCurrentMode.ImageLocation = Application.StartupPath + "\\item\\arn.jpg";
                this.lbl_currTagType.Text = "ID3タグ：A-Remix Nation方式";
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

                target = Defines.TRACKINFO_ID3_TYPE + ":";
                if (line.Contains(target))
                {
                    if (this.isTypeFYS)
                        text += Defines.TRACKINFO_ID3_TYPE + ":" + "FYS\r\n";
                    else
                        text += Defines.TRACKINFO_ID3_TYPE + ":" + "ARN\r\n";
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

        private async void BtnSwitcher_Click(object sender, EventArgs e)
        {
            if ( !canStartSwitcher )
            {
                /* 確認ダイアログを表示 */
                messageForm.SetFormState(MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Loaded_Album] +
                                            MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Loaded_Album_Reason],
                                            MessageForm.MODE_OK);
                messageForm.ShowDialog();

                SetLog(Color.Orange, MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Loaded_Album]);
                
            }
            else
            {
                try
                {
                    /* 連続実行できないようにするため、ボタンロックする */
                    this.btnSwitcher.Enabled = false;
                    this.btnClearCache.Enabled = false;
                    this.btnLoadAlbum.Enabled = false;
                    this.btnOpenTrackInfoPage.Enabled = false;
                    this.btnExit.Enabled = false;

                    await Task.Run( ( ) => StartTypeSwitch( ) );
                    RewriteCBL( );

                    SetLog(Color.LimeGreen, MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Success_Convert_Song_ID3]);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains( "別のプロセスで使用" ))
                    {
                        /* 確認ダイアログを表示 */
                        messageForm.SetFormState(MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Process_Use_Error] + ex.Message, MessageForm.MODE_OK);
                        messageForm.ShowDialog();
                    }
                    else
                    {
                        /* 確認ダイアログを表示 */
                        messageForm.SetFormState(MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Irregular_Error] + ex.ToString(), MessageForm.MODE_OK);
                        messageForm.ShowDialog();
                    }
                    SetLog(Color.Orange, MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Failed_Convert_Song_ID3]);
                }
                finally
                {
                    /* ボタンロックを解除する */
                    this.btnSwitcher.Enabled = true;
                    this.btnClearCache.Enabled = true;
                    this.btnLoadAlbum.Enabled = true;
                    this.btnOpenTrackInfoPage.Enabled = true;
                    this.btnExit.Enabled = true;
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
                System.Diagnostics.Process.Start(Defines.APP_DOWNLOAD_URL);
                this.Close();
            }
        }

        private bool CheckNewVersion()
        {
            WebClient wc = new WebClient();
            try
            {
                string html = wc.DownloadString( Defines.VERSION_INFO_URL );
                string target = "track_id3_ver:";
                if (html.Contains(target))
                {
                    int st = html.IndexOf(target);
                    int len = target.Length;
                    int ed = 4;
                    this.new_ver = html.Substring(st + len, ed);
                    if (this.new_ver != Defines.CURRENT_VERSION )
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

            /* Exit StatusをRunningに変更するためセーブを掛ける */
            SaveSetting( launch: true );

            /* 座標初期化 */
            this.pnlPage1.Location          = new Point(Defines.DESIGN_DEF_X, Defines.DESIGN_DEF_Y);
            this.ClientSize                 = new Size(Defines.APP_WIDTH, Defines.APP_HEIGHT);
            this.pnlAppFooterLine.Location  = new Point(Defines.DESIGN_DEF_X, Defines.FOOTER_LINE_Y);
            this.pnlAppFooter.Location      = new Point(Defines.DESIGN_DEF_X, Defines.FOOTER_Y);
            this.pnlAppLine1.Size           = new Size(Defines.APP_WIDTH, Defines.LINE_HEIGHT);
            this.pnlAppLine2.Size           = new Size(Defines.APP_WIDTH, Defines.LINE_HEIGHT);
            this.pnlTrackInfo.Location      = new Point(Defines.DESIGN_DEF_X, Defines.MAIN_AREA_Y);
            this.pnl_currLoadInfo.Location  = new Point(Defines.MAIN_AREA_HIDE_X, Defines.CURR_INFO_AREA_Y);
            this.pnl_currTagType.Location   = new Point(Defines.MAIN_AREA_HIDE_X, Defines.CURR_ID3_AREA_Y);
            this.pnl_execConvert.Location   = new Point(Defines.MAIN_AREA_HIDE_X , Defines.EXEC_CONV_AREA_Y);
            this.pnl_settings.Location      = new Point(Defines.DESIGN_DEF_X , Defines.SETTING_AREA_HIDE_Y);
            SetupTrackInfoPanel();
            

/* 使用不可能な文字があるかチェックできるようにするための初期化 */
invalidChars = System.IO.Path.GetInvalidFileNameChars();
            invalidReplase = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

            _mainContext = SynchronizationContext.Current;
            /*
            coroutineExitButton = new Thread( new ThreadStart( OnFX_btn_Exit ) );
            coroutineExitButton.Start( );
            */

#if !DEBUG
            this.button2.Visible = false;
#endif
        }

        private async void BtnExit_Click(object sender, EventArgs e)
        {
            this.Hide( );

            if ( AccountForm.isUnlockLicence )
            {
                Task<bool> m_coroutine_task;
                if ( loginForm.IsDisposed )
                {
                    loginForm = new AccountForm( );
                }
                loginForm.Show( );
                m_coroutine_task = loginForm.Logout( );
                await m_coroutine_task;
                loginForm.Close( );
            }

            SaveSetting( launch: false );

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

                    target = Defines.CONFIG_JACKET_NAME + ":";
                    if (line.Contains(target))
                    {
                        st = target.Length;
                        ed = line.Length - st;
                        this.boxArtworkName.Text = line.Substring(st, ed);
                    }
                    target = Defines.CONFIG_TRACK_FOLDER + ":";
                    if (line.Contains(target))
                    {
                        st = target.Length;
                        ed = line.Length - st;
                        this.boxTrackFolder.Text = line.Substring(st, ed);
                    }
                    target = Defines.CONFIG_IS_DOT + ":";
                    if (line.Contains(target))
                    {
                        if (line.Contains("1"))
                            this.chkIsDot.Checked = true;
                        else if (line.Contains("0"))
                            this.chkIsDot.Checked = false;
                    }
                    target = Defines.CONFIG_AUTO_SEARCH + ":";
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
                    target = Defines.CONFIG_REPLACE_REGISTER_WORD + ":";
                    if (line.Contains(target))
                    {
                        if (line.Contains("1"))
                        {
                            this.isReplaceRegisterWord.Checked = true;
                        }
                        else if (line.Contains("0"))
                        {
                            this.isReplaceRegisterWord.Checked = false;
                        }
                    }
                    target = Defines.CONFIG_APP_EXIT + ":";
                    if ( line.Contains( target ) )
                    {
                        if ( line.Contains( Defines.ExitStatus.Launching.ToString( ) ) )
                        {
                            isExitStatus = Defines.ExitStatus.Running;
                        }
                        else if ( line.Contains( Defines.ExitStatus.Closing.ToString( ) ) )
                        {
                            isExitStatus = Defines.ExitStatus.Running;
                        }
                        else if ( line.Contains( Defines.ExitStatus.Running.ToString( ) ) )
                        {
                            MessageBox.Show( "ForceClosed" );
                            isExitStatus = Defines.ExitStatus.ForceClosed;
                        }
                    }
                    target = Defines.CONFIG_CONVERT_EXT + ":";
                    if ( line.Contains( target ) )
                    {
                        if ( line.Contains( Defines.ConvertExt.flac.ToString( ) ) )
                        {
                            m_convertExt = Defines.ConvertExt.flac;
                        }
                        else if ( line.Contains( Defines.ConvertExt.mp3.ToString( ) ) )
                        {
                            m_convertExt = Defines.ConvertExt.mp3;
                        }
                        else if ( line.Contains( Defines.ConvertExt.wav.ToString( ) ) )
                        {
                            m_convertExt = Defines.ConvertExt.wav;
                        }
                        else
                        {
                            m_convertExt = Defines.ConvertExt.flac;
                        }
                        this.cmbbx_convertExt.SelectedIndex = (int)m_convertExt;
                    }
                    target = Defines.CONFIG_ACC_DEC + ":";
                    if ( line.Contains( target ) )
                    {
                        Defines.Encrypt_Shift_Size_Config_File = Convert.ToInt32( line.Substring( target.Length , (line.Length - target.Length) ) );
                    }
                }
            }
            else
            {
                SaveSetting( launch: true );
            }
        }

        private void SaveSetting( bool launch )
        {
            string path = Application.StartupPath + "\\item\\config.cbl";
            string text = "";

            text = Defines.CONFIG_JACKET_NAME + ":" + this.boxArtworkName.Text + "\r\n";
            text += Defines.CONFIG_TRACK_FOLDER + ":" + this.boxTrackFolder.Text + "\r\n";

            if ( this.chkIsDot.Checked)
                text += Defines.CONFIG_IS_DOT + ":" + "1\r\n";
            else
                text += Defines.CONFIG_IS_DOT + ":" + "0\r\n";

            if (this.autoSearchFile.Checked)
                text += Defines.CONFIG_AUTO_SEARCH + ":" + "1\r\n";
            else
                text += Defines.CONFIG_AUTO_SEARCH + ":" + "0\r\n";

            if (this.isReplaceRegisterWord.Checked)
                text += Defines.CONFIG_REPLACE_REGISTER_WORD + ":" + "1\r\n";
            else
                text += Defines.CONFIG_REPLACE_REGISTER_WORD + ":" + "0\r\n";

            text += Defines.CONFIG_APP_EXIT + ":";
            if ( launch )
                text += Defines.ExitStatus.Running.ToString( ) + "\r\n";
            else
                text += Defines.ExitStatus.Closing.ToString( ) + "\r\n";

            text += Defines.CONFIG_CONVERT_EXT + ":";
            text += m_convertExt.ToString( ) + "\r\n";

            text += Defines.CONFIG_ACC_DEC + ":";
            text += Defines.Encrypt_Shift_Size_Config_File.ToString( ) + "\r\n";

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


        private void pnl_settings_MouseDown( object sender , MouseEventArgs e )
        {
            getMousePos( e );
        }
        private void lbl_title_header_settings_MouseDown( object sender , MouseEventArgs e )
        {
            getMousePos( e );
        }
        private void pnl_line_setting_header_MouseDown( object sender , MouseEventArgs e )
        {
            getMousePos( e );
        }
        private void lbl_title_settings_albumSearch_MouseDown( object sender , MouseEventArgs e )
        {
            getMousePos( e );
        }
        private void lbl_opt_settings_albumSearch_ext_MouseDown( object sender , MouseEventArgs e )
        {
            getMousePos( e );
        }
        private void pnl_line_setting_albumSearch_MouseDown( object sender , MouseEventArgs e )
        {
            getMousePos( e );
        }
        private void lbl_title_settings_execConv_MouseDown( object sender , MouseEventArgs e )
        {
            getMousePos( e );
        }

        private void lbl_title_currLoadInfo_MouseDown( object sender , MouseEventArgs e )
        {
            getMousePos( e );
        }

        private void pnl_AppLine3_MouseDown( object sender , MouseEventArgs e )
        {
            getMousePos( e );
        }

        private void lbl_title_currTagType_MouseDown( object sender , MouseEventArgs e )
        {
            getMousePos( e );
        }

        private void lbl_title_execConvert_MouseDown( object sender , MouseEventArgs e )
        {
            getMousePos( e );
        }

        private void pnl_execConvert_MouseDown( object sender , MouseEventArgs e )
        {
            getMousePos( e );
        }

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
        private void pnl_currLoadInfo_MouseDown( object sender , MouseEventArgs e )
        {
            getMousePos( e );
        }

        private void pnl_currTagType_MouseDown( object sender , MouseEventArgs e )
        {
            getMousePos( e );
        }

        private void pnl_albumSearch_MouseDown( object sender , MouseEventArgs e )
        {
            getMousePos( e );
        }

        #endregion

        #region ドラッグ時



        private void pnl_settings_MouseMove( object sender , MouseEventArgs e )
        {
            moveWindow( e );
        }


        private void lbl_title_header_settings_MouseMove( object sender , MouseEventArgs e )
        {
            moveWindow( e );
        }


        private void pnl_line_setting_header_MouseMove( object sender , MouseEventArgs e )
        {
            moveWindow( e );
        }


        private void lbl_title_settings_albumSearch_MouseMove( object sender , MouseEventArgs e )
        {
            moveWindow( e );
        }


        private void lbl_opt_settings_albumSearch_ext_MouseMove( object sender , MouseEventArgs e )
        {
            moveWindow( e );
        }


        private void pnl_line_setting_albumSearch_MouseMove( object sender , MouseEventArgs e )
        {
            moveWindow( e );
        }


        private void lbl_title_settings_execConv_MouseMove( object sender , MouseEventArgs e )
        {
            moveWindow( e );
        }
        private void lbl_title_currLoadInfo_MouseMove( object sender , MouseEventArgs e )
        {
            moveWindow( e );
        }

        private void pnl_AppLine3_MouseMove( object sender , MouseEventArgs e )
        {
            moveWindow( e );
        }

        private void lbl_title_currTagType_MouseMove( object sender , MouseEventArgs e )
        {
            moveWindow( e );
        }

        private void lbl_title_execConvert_MouseMove( object sender , MouseEventArgs e )
        {
            moveWindow( e );
        }

        private void pnl_execConvert_MouseMove( object sender , MouseEventArgs e )
        {
            moveWindow( e );
        }

        private void pnl_currLoadInfo_MouseMove( object sender , MouseEventArgs e )
        {
            moveWindow( e );
        }

        private void pnl_currTagType_MouseMove( object sender , MouseEventArgs e )
        {
            moveWindow( e );
        }

        private void pnl_albumSearch_MouseMove( object sender , MouseEventArgs e )
        {
            moveWindow( e );
        }

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
            int st          = Defines.DESIGN_DEF_X;
            int ed          = Defines.MAIN_AREA_HIDE_X;
            int width       = Defines.APP_WIDTH;
            int height      = Defines.APP_HEIGHT;
            int footerLineY = Defines.FOOTER_LINE_Y;
            int footerY     = Defines.FOOTER_Y;
            int exitX       = Defines.HEADERT_BTN_EXIT_MOVE_X;
            int accountX    = Defines.HEADERT_BTN_ACCOUNT_MOVE_X;
            int settingX    = Defines.HEADERT_BTN_SETTING_MOVE_X;
            int volX        = 7;
            int volY        = 5;
            int page2X      = Defines.SUB_AREA_HIDE_X;
            int plusX       = 60;

            /* アニメーションでレイアウトが重くならないように */
            this.pnlPage1.SuspendLayout();
            this.btnExit.SuspendLayout();
            this.img_btn_account.SuspendLayout();
            this.img_btn_setting.SuspendLayout( );
            this.pnlAppFooter.SuspendLayout();
            this.pnlAppFooterLine.SuspendLayout();

            /* テキストボックスは非描画 */
            VisibleBoxes( false );

            /* アニメーション開始 */
            for ( int i = st; i > ed; i -=20 )
            {
                this.pnlPage1.Location = new Point( i, this.pnlPage1.Location.Y);

                width += volX;
                height += volY;
                this.ClientSize = new System.Drawing.Size( width , height );

                exitX += volX;
                this.btnExit.Location = new Point( (exitX + plusX), this.btnExit.Location.Y);

                accountX += volX;
                this.img_btn_account.Location = new Point( ( accountX + plusX ) , this.img_btn_account.Location.Y );

                settingX += volX;
                this.img_btn_setting.Location = new Point( ( settingX + plusX ) , this.img_btn_setting.Location.Y );

                footerY += volY;
                footerLineY += volY;
                this.pnlAppFooter.Location = new Point(this.pnlAppFooter.Location.X, footerY);
                this.pnlAppFooterLine.Location = new Point(this.pnlAppFooterLine.Location.X, footerLineY);

                //page2X -= volX;
                //this.pnlTrackInfo.Location = new Point(page2X, this.pnlTrackInfo.Location.Y);

                await Task.Run(() => Thread.Sleep(10));
            }

            /* テキストボックス表示させる */
            VisibleBoxes(true);

            /* 固定位置に最終フレームで配置 */
            this.pnlPage1.Location          = new Point(Defines.MAIN_AREA_HIDE_X            , Defines.DESIGN_DEF_Y);
            this.ClientSize                 = new Size(Defines.APP_BIG_WIDTH                , Defines.APP_BIG_HEIGHT);
            this.btnExit.Location           = new Point(Defines.HEADERT_BTN_EXIT_BIG_X      , Defines.EXIT_Y);
            this.img_btn_account.Location   = new Point(Defines.HEADERT_BTN_ACCOUNT_BIG_X   , Defines.EXIT_Y);
            this.img_btn_setting.Location   = new Point(Defines.HEADERT_BTN_SETTING_BIG_X   , Defines.EXIT_Y);
            this.pnlAppFooter.Location      = new Point(Defines.DESIGN_DEF_X                , Defines.FOOTER_BIG_Y);
            this.pnlAppFooterLine.Location  = new Point(Defines.DESIGN_DEF_X                , Defines.FOOTER_LINE_BIG_Y);
            //this.pnlTrackInfo.Location      = new Point(Defines.DESIGN_DEF_X        , Defines.MAIN_AREA_Y);
            await Task.Run(() => Thread.Sleep(10));

            /* レイアウトのロック解除 */
            this.pnlPage1.ResumeLayout();
            this.btnExit.ResumeLayout();
            this.img_btn_account.ResumeLayout();
            this.img_btn_setting.ResumeLayout( );
            this.pnlAppFooter.ResumeLayout();
            this.pnlAppFooterLine.ResumeLayout();
            //this.lblAppTitle.Text = "(" + this.ClientSize.Width + " , " + this.ClientSize.Height + ")";
        }
        
        private async void btnCloseTrackInfo_Click(object sender, EventArgs e)
        {
            int st          = -500;
            int ed          = Defines.DESIGN_DEF_X;
            int width       = Defines.APP_BIG_WIDTH;
            int height      = Defines.APP_BIG_HEIGHT;
            int footerLineY = Defines.FOOTER_LINE_BIG_Y;
            int footerY     = Defines.FOOTER_BIG_Y;
            int exitX       = Defines.HEADERT_BTN_EXIT_MOVE_BIG_X;
            int accountX    = Defines.HEADERT_BTN_ACCOUNT_MOVE_BIG_X;
            int settingX    = Defines.HEADERT_BTN_SETTING_MOVE_BIG_X;
            int volX        = 8;
            int volY        = 6;
            int page2X      = Defines.DESIGN_DEF_X;
            int plusX       = 69;

            this.pnlPage1.SuspendLayout();
            this.btnExit.SuspendLayout();
            this.img_btn_account.SuspendLayout( );
            this.img_btn_setting.SuspendLayout( );
            this.pnlAppFooter.SuspendLayout();
            this.pnlAppFooterLine.SuspendLayout();
            for (int i = st; i < ed; i += 20)
            {
                this.pnlPage1.Location = new Point(i, this.pnlPage1.Location.Y);

                width -= volX;
                height -= volY;
                this.ClientSize = new System.Drawing.Size( width , height );

                exitX -= volX;
                this.btnExit.Location = new Point((exitX + plusX), this.btnExit.Location.Y);

                accountX -= volX;
                this.img_btn_account.Location = new Point( ( accountX + plusX ) , this.img_btn_account.Location.Y );

                settingX -= volX;
                this.img_btn_setting.Location = new Point( ( settingX + plusX ) , this.img_btn_setting.Location.Y );

                footerY -= volY;
                footerLineY -= volY;
                this.pnlAppFooter.Location = new Point(this.pnlAppFooter.Location.X, footerY);
                this.pnlAppFooterLine.Location = new Point(this.pnlAppFooterLine.Location.X, footerLineY);

                //page2X += volX;
                //this.pnlTrackInfo.Location = new Point(page2X, this.pnlTrackInfo.Location.Y);

                await Task.Run(() => Thread.Sleep(10));
            }
            
            /* 固定位置に最終フレームで配置 */
            this.pnlPage1.Location          = new Point(Defines.DESIGN_DEF_X            , Defines.DESIGN_DEF_Y);
            this.ClientSize                 = new Size(Defines.APP_WIDTH                , Defines.APP_HEIGHT);
            this.btnExit.Location           = new Point(Defines.HEADERT_BTN_EXIT_X      , Defines.EXIT_Y);
            this.img_btn_account.Location   = new Point(Defines.HEADERT_BTN_ACCOUNT_X   , Defines.EXIT_Y);
            this.img_btn_setting.Location   = new Point(Defines.HEADERT_BTN_SETTING_X   , Defines.EXIT_Y);
            this.pnlAppFooter.Location      = new Point(Defines.DESIGN_DEF_X            , Defines.FOOTER_Y);
            this.pnlAppFooterLine.Location  = new Point(Defines.DESIGN_DEF_X            , Defines.FOOTER_LINE_Y);
            //this.pnlTrackInfo.Location      = new Point(Defines.DESIGN_DEF_X    , Defines.MAIN_AREA_Y);
            await Task.Run(() => Thread.Sleep(10));
            
            this.pnlPage1.ResumeLayout();
            this.btnExit.ResumeLayout();
            this.img_btn_account.ResumeLayout( );
            this.img_btn_setting.ResumeLayout( );
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
            this.lblTracks = new Label[Defines.MAX_TRACK];

            for (int i = 0; i < Defines.MAX_TRACK; i++)
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
            this.boxTracks = new TextBox[Defines.MAX_TRACK, Defines.MAX_TAG];

            for (int track = 0; track < Defines.MAX_TRACK; track++)
            {
                for (int tag = 0; tag < Defines.MAX_TAG; tag++)
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
            for (int track = 0; track < Defines.MAX_TRACK; track++)
            {
                for (int tag = 0; tag < Defines.MAX_TAG; tag++)
                {

                    this.boxTracks[track, tag].Visible = show;

                }
            }
        }

        private void DeleteAnotherBoxes()
        {
            for (int track = 0; track < Defines.MAX_TRACK; track++)
            {
                if (this.currentMaxTrack <= track)
                {
                    for (int tag = 0; tag < Defines.MAX_TAG; tag++)
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

        private string CheckRegisterWord( string text )
        {
            /* ワード内にシステム予約文字が含まれていないかったら */
            if (text.IndexOfAny(invalidChars) < 0)
            {
                /* そのまま文字を返す */
                return text;
            }
            /* ワード内にシステム予約文字が含まれていたら */
            else
            {
                /* 置き換えを毎回確認する場合 */
                if (this.isReplaceRegisterWord.Checked)
                {
                    /* 確認ダイアログを表示 */
                    messageForm.SetFormState(MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Found_Cannot_Use_Word] +
                                                MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Cannot_Word_Replace_Another_Word] +
                                                MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Select_Replace_Way] +
                                                MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Target_Word] +
                                                text,
                                                MessageForm.MODE_YN);
                    DialogResult dr = messageForm.ShowDialog();

                    /* Yesなら空白に置き換える */
                    if (dr == DialogResult.Yes)
                        text = ReolaceRegisterWordToNull(text);
                    /* Noなら全角文字に置き換える */
                    else
                        text = ReolaceRegisterWordToEM(text);
                }
                /* そうでない場合 */
                else
                {
                    /* 空白に置き換える */
                    text = ReolaceRegisterWordToNull(text);
                }

                /* 変換した文字を返す */
                return text;
            }
        }

        private string ReolaceRegisterWordToEM(string text)
        {
            text = text.Replace("\\", "￥");
            text = text.Replace(":", "：");
            text = text.Replace("*", "＊");
            text = text.Replace("?", "？");
            text = text.Replace("\"", "”");
            text = text.Replace("<", "＜");
            text = text.Replace(">", "＞");
            text = text.Replace("|", "｜");
            text = text.Replace("/", "／");
            return text;
        }

        private string ReolaceRegisterWordToNull(string text)
        {
            /* システム使用不可能な文字を1字ずつチェックする */
            foreach (char c in invalidReplase)
            {
                /* その文字が含まれていたら空白にする */
                text = text.Replace(c.ToString(), "");
            }
            return text;
        }

        private void WriteTrackInfo( )
        {
            string path = Application.StartupPath + "\\trackinfo.cbl";
            string zero = "";
            string text = "";

            text += Defines.TRACKINFO_ID3_TYPE + ":" + "ARN\r\n";
            if ( !this.chkbx_off_vocal.Checked )
                text += Defines.TRACKINFO_WAVE_TYPE + ":" + "VO\r\n";
            else
                text += Defines.TRACKINFO_WAVE_TYPE + ":" + "INST\r\n";
            text += Defines.TRACKINFO_ALBUM_LABEL + ":" + this.boxLabelName.Text + "\r\n";
            text += Defines.TRACKINFO_ALBUM_NAME + ":" + this.boxAlbumName.Text + "\r\n";
            text += Defines.TRACKINFO_ALBUM_NUMBER + ":" + this.boxAlbumNumber.Text + "\r\n";
            text += Defines.TRACKINFO_WAVE_LINK + ":" + this.box_WaveLink.Text + "\r\n";
            text += "==============================\r\n";
            for (int j = 0; j < Defines.MAX_TAG; j++)
            {
                for (int i = 0; i < Defines.MAX_TRACK; i++)
                {
                    if (i < 9)
                        zero = "0";
                    else
                        zero = "";
                    switch ( j )
                    {
                        case Defines.ARN_NAME:
                            text += "Defines.ARN_NAME_" + zero + (i + 1) + ":" + this.boxTracks[i, Defines.ARN_NAME].Text + "\r\n";
                            break;
                        case Defines.FYS_NAME:
                            /* ナンバリング */
                            text += "Defines.FYS_NAME_" + zero + (i + 1) + ":";

                            /* 原曲名 ( */
                            text += this.boxTracks[i, Defines.FYS_NAME].Text + " (";

                            /* カスタム名分岐 */
                            if ( this.boxTracks[i, Defines.CUSTOM].Text != "" )
                            {
                                /* カスタム名ルート */
                                text += this.boxTracks[i, Defines.CUSTOM].Text + ")\r\n";
                            }
                            else
                            {
                                /* 命名規則ルート */

                                /* 作者 */
                                text += this.boxTracks[i, Defines.CREATOR].Text;

                                /* 's */
                                if (this.chkIsXXs.Checked)
                                    text += "'s ";
                                else
                                    text += " ";

                                /* ジャンル名 */
                                text += this.boxTracks[i, Defines.GENRE].Text;

                                /* Bootleg) */
                                text += " " + this.boxLastWord.Text + ")\r\n";
                            }
                            break;
                        case Defines.SUBTITLE:
                            text += "FYS_Subtitle_" + zero + (i + 1) + ":" + this.boxTracks[i, Defines.SUBTITLE].Text + "\r\n";
                            break;
                        case Defines.ARTIST:
                            text += "FYS_Artist_" + zero + (i + 1) + ":" + this.boxTracks[i, Defines.ARTIST].Text + "\r\n";
                            break;
                        case Defines.COMMENT:
                            text += "FYS_Comment_" + zero + (i + 1) + ":" + this.boxTracks[i, Defines.COMMENT].Text + "\r\n";
                            break;
                        case Defines.CREATOR:
                            text += "Creator_" + zero + (i + 1) + ":" + this.boxTracks[i, Defines.CREATOR].Text + "\r\n";
                            break;
                        case Defines.BPM:
                            text += "BPM_" + zero + (i + 1) + ":" + this.boxTracks[i, Defines.BPM].Text + "\r\n";
                            break;
                        case Defines.GENRE:
                            text += "Genre_" + zero + (i + 1) + ":" + this.boxTracks[i, Defines.GENRE].Text + "\r\n";
                            break;
                        case Defines.CUSTOM:
                            text += "FYS_CustomName_" + zero + (i + 1) + ":" + this.boxTracks[i, Defines.CUSTOM].Text + "\r\n";
                            break;
                    }
                }
                if ( (j == Defines.ARN_NAME) || (j == Defines.COMMENT))
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
            for (int i = 0; i < Defines.MAX_TRACK; i++)
            {
                if (i < 9) zero = "0";
                else zero = "";
                text += zero + (i + 1) + ". " + this.boxTracks[i, Defines.ARN_NAME].Text + "." + m_convertExt.ToString( ) + "\r\n";
            }
            text += "===============\r\n";
            text += "【FYS式表示名】\r\n";
            for (int i = 0; i < Defines.MAX_TRACK; i++)
            {
                /* ナンバリング */
                if (i < 9) zero = "0";
                else zero = "";
                text += zero + (i + 1) + ". ";

                /* 原曲名 ( */
                text += this.boxTracks[i, Defines.FYS_NAME].Text + " (";

                /* カスタム名分岐 */
                if (this.boxTracks[i, Defines.CUSTOM].Text != "")
                {
                    /* カスタム名入っていたので()内に優先入力 */
                    text += this.boxTracks[i, Defines.CUSTOM].Text + ")." + m_convertExt.ToString( ) + "\r\n";
                }
                else
                {
                    /* カスタム名未入力なので、ツール制度に従って標準入力 */

                    /* 作者名 */
                    text += this.boxTracks[i, Defines.CREATOR].Text;

                    /* 's */
                    if (this.chkIsXXs.Checked)
                        text += "'s ";
                    else
                        text += " ";

                    /* ジャンル名 */
                    text += this.boxTracks[i, Defines.GENRE].Text;

                    /* Bootleg) */
                    text += " " + this.boxLastWord.Text + ")." + m_convertExt.ToString( ) + "\r\n";
                }
            }
            MessageBox.Show(text, "【表示名の確認】");
        }

        #endregion

        #region 波形合成

        private NAudio.Wave.IWavePlayer waveOutDevice;
        private WaveMixerStream32 mixer;
        private string melodyOnly = "";
        private string vocalOnly = "";
        WaveFileReader[] reader = new WaveFileReader[2];
        WaveOffsetStream[] offsetStream = new WaveOffsetStream[2];
        WaveChannel32[] channelSteam = new WaveChannel32[2];

        private void btn_vocalLayer_Click( object sender , EventArgs e )
        {
            if ( this.box_WaveLink.Text == String.Empty )
            {
                /* 確認ダイアログを表示 */
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Found_Wave_Link] , MessageForm.MODE_OK );
                DialogResult dr = messageForm.ShowDialog( );
            }
            else
            {

            }
        }

        private void StartVocalPlus( )
        {
            /*
             * coroutineExitButton = new Thread( new ThreadStart( OnFX_btn_Exit ) );
            coroutineExitButton.Start( );
            */
        }


        private async void button1_Click_1( object sender , EventArgs e )
        {
            //OpenFileDialogクラスのインスタンスを作成
            OpenFileDialog ofd = new OpenFileDialog( );

            //はじめのファイル名を指定する
            //はじめに「ファイル名」で表示される文字列を指定する
            ofd.FileName = "default.html";
            //はじめに表示されるフォルダを指定する
            //指定しない（空の文字列）の時は、現在のディレクトリが表示される
            // ofd.InitialDirectory = @"C:\";
            //[ファイルの種類]に表示される選択肢を指定する
            //指定しないとすべてのファイルが表示される
            ofd.Filter = "HTMLファイル(*.html;*.htm)|*.html;*.htm|すべてのファイル(*.*)|*.*";
            //[ファイルの種類]ではじめに選択されるものを指定する
            //2番目の「すべてのファイル」が選択されているようにする
            ofd.FilterIndex = 2;
            //タイトルを設定する
            ofd.Title = "開くファイルを選択してください";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            ofd.RestoreDirectory = true;
            //存在しないファイルの名前が指定されたとき警告を表示する
            //デフォルトでTrueなので指定する必要はない
            ofd.CheckFileExists = true;
            //存在しないパスが指定されたとき警告を表示する
            //デフォルトでTrueなので指定する必要はない
            ofd.CheckPathExists = true;

            //ダイアログを表示する
            if ( ofd.ShowDialog( ) == DialogResult.OK )
            {
                //OKボタンがクリックされたとき、選択されたファイル名を表示する
                melodyOnly = ofd.FileName;
                MessageBox.Show( melodyOnly );
            }

            //ダイアログを表示する
            if ( ofd.ShowDialog( ) == DialogResult.OK )
            {
                //OKボタンがクリックされたとき、選択されたファイル名を表示する
                vocalOnly = ofd.FileName;
                MessageBox.Show( vocalOnly );
            }

            if ( (melodyOnly.Length != 0) && ( vocalOnly.Length != 0 ) )
                await Task.Run( ( ) => TestConvertFlac( ) );
        }

        private void TestConvertFlac( )
        {

            string dir = Path.GetDirectoryName( vocalOnly );
            string fileNameNotExt = System.IO.Path.GetFileNameWithoutExtension( vocalOnly );

            string melodyOnlyWav = melodyOnly.Replace( ".flac" , ".wav" );
            string vocalOnlyWav = vocalOnly.Replace( ".flac" , ".wav" );

            MediaToolkit.Options.ConversionOptions opt = new MediaToolkit.Options.ConversionOptions( );
            opt.AudioSampleRate = MediaToolkit.Options.AudioSampleRate.Hz48000;
            if ( melodyOnly.Contains( ".flac" ) )
            {
                var inputFile = new MediaFile { Filename = melodyOnly };
                var outputFile = new MediaFile { Filename = melodyOnlyWav };
                using ( var engine = new Engine( ) )
                {
                    engine.Convert( inputFile , outputFile );
                }
            }

            if ( vocalOnly.Contains( ".flac" ) )
            {
                var inputFile = new MediaFile { Filename = vocalOnly };
                var outputFile = new MediaFile { Filename = vocalOnlyWav };
                using ( var engine = new Engine( ) )
                {
                    engine.Convert( inputFile , outputFile );
                }
            }

            /*
            using ( var rd1 = new FlacReader( melodyOnly ) )
            using ( var rd2 = new FlacReader( vocalOnly ) )
            {
                if ( File.Exists( dir + "/_resample" ) )
                {
                    File.Delete( dir + "/_resample" );
                }
                if ( File.Exists( dir + "/" + fileNameNotExt + "_mixed.flac" ) )
                {
                    File.Delete( dir + "/" + fileNameNotExt + "_mixed.flac" );
                }

                var mixer = new NAudio.Wave.SampleProviders.MixingSampleProvider( new[] { rd1 , rd2 } );
                WaveFileWriter.CreateWaveFile16( dir + "/_resample" , mixer );
            }
            */

            using ( var reader1 = new AudioFileReader( melodyOnlyWav ) )
            using ( var reader2 = new AudioFileReader( vocalOnlyWav ) )
            {
                if ( File.Exists( dir + "/_resample" ) )
                {
                    File.Delete( dir + "/_resample" );
                }
                if ( File.Exists( dir + "/" + fileNameNotExt + "_mixed.flac" ) )
                {
                    File.Delete( dir + "/" + fileNameNotExt + "_mixed.flac" );
                }

                var mixer = new NAudio.Wave.SampleProviders.MixingSampleProvider( new[] { reader1 , reader2 } );
                //WaveFileWriter.CreateWaveFile( dir + "/_resample" , mixer.ToWaveProvider( ) );
                WaveFileWriter.CreateWaveFile16( dir + "/_resample" , mixer );
                //WaveFileReader reader = new WaveFileReader( "myfile.wav" );

                //WaveStream waveStream = WaveFormatConversionStream.CreatePcmStream( new Mp3FileReader( inputStream ) );
                //byte[] bytes = new byte[waveStream.Length];
                // SaveWaveToMp3( dir + "/" , "_resample" , fileNameNotExt + "_mixed.flac" );
                /* */
                //byte[] wav = File.ReadAllBytes( dir + "/_resample" );
                /*byte[] mp3 = WavToMP3( wav );
                System.IO.FileStream fs = new System.IO.FileStream( dir + "/" + fileNameNotExt + "_mixed.mp3" , System.IO.FileMode.Create , System.IO.FileAccess.Write );
                fs.Write( mp3 , 0 , mp3.Length );
                fs.Close( );*/

                //WaveFileReader wavFile = new WaveFileReader( dir + "/_resample" );
            }

            System.IO.FileStream fs = new System.IO.FileStream( dir + "/_resample" , System.IO.FileMode.Open , System.IO.FileAccess.ReadWrite );
            IAudioSource audioSource = new WAVReader( null , fs );
            AudioBuffer buff = new AudioBuffer( audioSource , 0x10000 );
            FlakeWriter fw = new FlakeWriter( dir + "/" + fileNameNotExt + "_mixed.flac" , audioSource.PCM );
            
            fw.CompressionLevel = 8;
            while ( audioSource.Read( buff , -1 ) != 0 )
            {
                fw.Write( buff );
            }
            fw.Close( );
            fw.Dispose( );
            fs.Close( );
            fs.Dispose( );

            File.Delete( dir + "/_resample" );
            /* File.Delete( melodyOnlyWav );
            File.Delete( vocalOnlyWav ); */

            MessageBox.Show( dir + "/" + fileNameNotExt + "_mixed.flac" );
        }

        private void TwoWaveMix( )
        {
            //OpenFileDialogクラスのインスタンスを作成
            OpenFileDialog ofd = new OpenFileDialog( );

            //はじめのファイル名を指定する
            //はじめに「ファイル名」で表示される文字列を指定する
            ofd.FileName = "default.html";
            //はじめに表示されるフォルダを指定する
            //指定しない（空の文字列）の時は、現在のディレクトリが表示される
            ofd.InitialDirectory = @"C:\";
            //[ファイルの種類]に表示される選択肢を指定する
            //指定しないとすべてのファイルが表示される
            ofd.Filter = "HTMLファイル(*.html;*.htm)|*.html;*.htm|すべてのファイル(*.*)|*.*";
            //[ファイルの種類]ではじめに選択されるものを指定する
            //2番目の「すべてのファイル」が選択されているようにする
            ofd.FilterIndex = 2;
            //タイトルを設定する
            ofd.Title = "開くファイルを選択してください";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            ofd.RestoreDirectory = true;
            //存在しないファイルの名前が指定されたとき警告を表示する
            //デフォルトでTrueなので指定する必要はない
            ofd.CheckFileExists = true;
            //存在しないパスが指定されたとき警告を表示する
            //デフォルトでTrueなので指定する必要はない
            ofd.CheckPathExists = true;

            //ダイアログを表示する
            if ( ofd.ShowDialog( ) == DialogResult.OK )
            {
                //OKボタンがクリックされたとき、選択されたファイル名を表示する
                melodyOnly = ofd.FileName;
            }

            //ダイアログを表示する
            if ( ofd.ShowDialog( ) == DialogResult.OK )
            {
                //OKボタンがクリックされたとき、選択されたファイル名を表示する
                vocalOnly = ofd.FileName;
            }

            //Setup the Mixer
            mixer = new WaveMixerStream32( );

            mixer.AutoStop = false;

            if ( waveOutDevice == null )

            {
                waveOutDevice = new AsioOut( );

                waveOutDevice.Init( mixer );
                waveOutDevice.Play( );

            }

            reader[0] = new WaveFileReader( melodyOnly );
            offsetStream[0] = new WaveOffsetStream( reader[0] );
            channelSteam[0] = new WaveChannel32( offsetStream[0] );

            reader[1] = new WaveFileReader( vocalOnly );
            offsetStream[1] = new WaveOffsetStream( reader[1] );
            channelSteam[1] = new WaveChannel32( offsetStream[1] );

            // You only need to do this once per stream

            mixer.AddInputStream( channelSteam[0] );
            mixer.AddInputStream( channelSteam[1] );

            // sampleLoaded[position] = openFileDialog.FileName;
        }

        private void SaveWaveToMp3( string dir , string wav , string mp3 )
        {
            var inputFile = new MediaFile { Filename = dir + wav };
            var outputFile = new MediaFile { Filename = dir + mp3 };
            MediaToolkit.Options.ConversionOptions opt = new MediaToolkit.Options.ConversionOptions( );
            opt.AudioSampleRate = MediaToolkit.Options.AudioSampleRate.Hz48000;

            using ( var engine = new Engine( ) )
            {
                engine.Convert( inputFile , outputFile /* , opt */ );
            }
        }

        public static Byte[] WavToMP3( byte[] wavFile )
        {
            using ( MemoryStream source = new MemoryStream( wavFile ) )
            using ( NAudio.Wave.WaveFileReader rdr = new NAudio.Wave.WaveFileReader( source ) )
            {
                WaveLib.WaveFormat fmt = new WaveLib.WaveFormat( rdr.WaveFormat.SampleRate , rdr.WaveFormat.BitsPerSample , rdr.WaveFormat.Channels );

                // convert to MP3 at 96kbit/sec...
                Yeti.Lame.BE_CONFIG conf = new Yeti.Lame.BE_CONFIG( fmt , /* ((uint)fmt.wBitsPerSample * 10) */ 320 );

                // Allocate a 1-second buffer
                int blen = rdr.WaveFormat.AverageBytesPerSecond;
                byte[] buffer = new byte[blen];

                // Do conversion
                using ( MemoryStream output = new MemoryStream( ) )
                {
                    Yeti.MMedia.Mp3.Mp3Writer mp3 = new Yeti.MMedia.Mp3.Mp3Writer( output , fmt , conf );

                    int readCount;
                    while ( ( readCount = rdr.Read( buffer , 0 , blen ) ) > 0 )
                        mp3.Write( buffer , 0 , readCount );
                    mp3.Close( );

                    return output.ToArray( );
                }
            }
        }

        private async Task<bool> SoundLayerCoroutine( )
        {
            /* 1 */
            if ( !NetworkFunc.CheckNetworkWithMessage( ) )
                return false;

            /* 2 */
            if ( this.currentAlbumReleaseNumber == String.Empty )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Load_Album_Number] , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return false;
            }

            /* 3 */
            m_coroutine_task_bool = GetHtmlAsync( Defines.ALBUM_LICENSE_URL );
            m_coroutine_flag = await m_coroutine_task_bool;
            if ( !m_coroutine_flag )
                return false;

            /* 4 */
            m_coroutine_task_str = NetworkFunc.DecryptHtmlAsyncWithMessage( m_serverContent , ENCRYPT_SHIFT_SIZE_LICENSE_PAGE );
            m_coroutine_text = await m_coroutine_task_str;
            if ( m_coroutine_text == String.Empty )
                return false;
            m_serverContent = m_coroutine_text;

            /* 5 */
            m_coroutine_task_bool = CheckNeedLicense( m_serverContent );
            m_coroutine_flag = await m_coroutine_task_bool;
            if ( !m_coroutine_flag )
                return false;

            if (m_currLicense == Defines.AlbumLicense.Non_License)
            {
                
            }
            else if ( m_currLicense == Defines.AlbumLicense.Need_License )
            {
            }

            return true;
        }

        /// <summary>
        /// 指定されたページのテキストデータを取得する
        /// </summary>
        private async Task<bool> GetHtmlAsync( string uri )
        {
            if ( m_webClient != null )
                m_webClient = null;
            m_webClient = new WebClient( );

            if ( m_uriAsync != null )
                m_uriAsync = null;
            m_uriAsync = new Uri( uri );

            if ( m_serverContent != String.Empty )
                m_serverContent = String.Empty;

            m_startText = "[" + this.currentAlbumReleaseNumber + "]";
            m_endText = "[/" + this.currentAlbumReleaseNumber + "]";

            try
            {
                //SetLog( this.lbl_progressContent , Color.AliceBlue , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Try_Get_Data_List] );
                m_serverContent = await m_webClient.DownloadStringTaskAsync( m_uriAsync );
                if ( m_serverContent.Contains( m_startText ) )
                {
                    m_startPos = m_serverContent.IndexOf( m_startText );
                    m_startPos += m_startText.Length;
                    m_endPos = m_serverContent.IndexOf( m_endText );
                    m_length = m_endPos - m_startPos;

                    m_serverContent = m_serverContent.Substring( m_startPos , m_length );
                    if ( m_serverContent == String.Empty )
                    {
                        messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Get_Account_Info] +
                                                    MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Re_Login_After] +
                                                    MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Send_Case_To_Developer] ,
                                                    MessageForm.MODE_OK );
                        messageForm.ShowDialog( );
                        return false;
                    }

                    // MessageBox.Show( m_serverContent );
                    return true;
                }
                else
                {
                    return true;
                }
            }
            catch ( WebException exc )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Error_Get_Account_Info] + MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Send_Error_To_Developer] + exc.ToString( ) , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                //SetLog( this.lbl_progressContent , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Get_Data_List] );
                return false;
            }
        }

        /// <summary>
        /// 取得したページデータからライセンスが必要なアルバムかどうかを調べる
        /// </summary>
        private async Task<bool> CheckNeedLicense( string str )
        {
            try
            {
                //SetLog( this.lbl_progressContent , Color.AliceBlue , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Try_Get_Data_List] );
                switch ( str )
                {
                    case "Non_License":
                        m_currLicense = Defines.AlbumLicense.Non_License;
                        break;

                    case "Need_License":
                        m_currLicense = Defines.AlbumLicense.Need_License;
                        break;
                }
                await Task.Delay( 1 );
                return true;
            }
            catch ( Exception ex )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Irregular_Error] + ex.ToString( ) , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                //SetLog( this.lbl_progressContent , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Get_Data_List] );
                return false;
            }
        }


        #endregion

        #region アカウント

        private void img_btn_account_Click( object sender , EventArgs e )
        {
            if ( loginForm.IsDisposed )
            {
                loginForm = new AccountForm( );
            }
            loginForm.Show( );
        }

        #endregion

        private void button2_Click( object sender , EventArgs e )
        {
            DebugText debugUI = new DebugText( );
                debugUI.Show( );
        }


        /*
        private void OnFX_btn_Exit( )
        {
            int r = RGB_MAX;
            int g = RGB_MAX;
            int b = RGB_MAX;

            bool r_r = false;
            bool g_r = false;
            bool b_r = false;

            while ( true )
            {
                if ( !this.isEnterExitBtn )
                {
                    if ( (RGB_MIN < r) && (r < RGB_MAX ) )
                    {
                        if ( !r_r )
                            r -= 1;
                        else
                            r += 1;

                        if ( r <= RGB_MIN )
                        {
                            r = RGB_MIN;
                            r_r = true;
                        }
                        else if ( RGB_MAX <= r )
                        {
                            r = RGB_MAX;
                            r_r = true;
                        }
                    }
                }
                Invoke( new labelnaiyo( SetColor_btn_Exit ) , r );
                Thread.Sleep( 1 );
            }
        }

        delegate void labelnaiyo( int naiyo );
        private void SetColor_btn_Exit( int r )
        {
            this.btn_Exit.Image
        }
        */
    }
}
