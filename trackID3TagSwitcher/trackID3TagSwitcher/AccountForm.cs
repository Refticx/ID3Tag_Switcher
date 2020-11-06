using DeviceId;
using PastebinAPIs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace trackID3TagSwitcher
{
    public partial class AccountForm : Form
    {
        private MessageForm messageForm = new MessageForm( );    /* ダイアログ用フォームを作成しておく */

        /* サーバー設定 */
        private string m_serverUsername = "";
        private string m_serverPassword = "";
        private string m_serverKey = "";
        private string m_serverLoggedUser = "";
        private string m_serverPage = "";
        private string m_machineId = "";

        /* 各関数処理用変数 */
        private const string START_TXT = "<textarea class=\"textarea\">";
        private const string END_TXT = "</textarea>";
        private const string SPACE_PASS = "[+@]";
        private const string SPACE_MCNID = "[+$]";

        private bool m_coroutine_flag = false;
        private PastebinAPI m_serverAPI;
        private PasteLoginRequest m_serverLoginReq;
        private PasteListRequest m_serverListReq;
        private PasteData[] m_serverDatas;
        private string m_serverUri = "";
        private string m_serverContent = "";
        private WebClient m_webClient;
        private int m_startPos = 0;
        private int m_endPos = 0;
        private int m_length = 0;
        private StringReader m_sr;
        private string m_line = "";
        private DialogResult m_dResult;

        public AccountForm( )
        {
            InitializeComponent( );
        }

        private void LoginForm_Load( object sender , EventArgs e )
        {
            GetCurrentMachineID( );
            LoadSetting( );
            CheckNetwork( );
            CheckServiceInfo( );
        }

        private void GetCurrentMachineID( )
        {
            m_machineId = new DeviceIdBuilder( )
                                .AddMachineName( )
                                .AddMacAddress( )
                                .AddProcessorId( )
                                .AddMotherboardSerialNumber( )
                                .ToString( );
            if ( m_machineId.Length == 0 )
            {
                /* 確認ダイアログを表示 */
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Error_Get_MachineID] + MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Tell_Case_To_Developer] , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
            }
        }

        /// <summary>
        /// ネットワークに接続されているかチェックし、接続されていなかったらアプリを閉じる
        /// </summary>
        private void CheckNetwork( )
        {
            if ( !System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable( ) )
            {
                /* 確認ダイアログを表示 */
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Connect_Network] , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                this.Close( );
            }
        }

        /// <summary>
        /// サーバー接続設定が取得できているかチェックし、できていなければ更新ボタンを押すよう促す
        /// </summary>
        private void CheckServiceInfo( )
        {
            if ( ( m_serverUsername.Length == 0 ) ||
                    ( m_serverPassword.Length == 0 ) ||
                    ( m_serverKey.Length == 0 ) ||
                    ( m_serverPage.Length == 0 ) )
            {
                /* 確認ダイアログを表示 */
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Get_ServerInfo] + MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Press_Reflesh] , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
            }
        }

        #region サーバー接続処理

        /// <summary>
        /// アカウント管理サーバーにログインする
        /// </summary>
        private bool LoginToPastebin( )
        {
            m_serverAPI = new PastebinAPI( );
            m_serverAPI.APIKey = m_serverKey;

            if ( m_serverLoginReq != null )
                m_serverLoginReq = null;
            m_serverLoginReq = new PasteLoginRequest( );
            m_serverLoginReq.Name = m_serverUsername;
            m_serverLoginReq.Password = m_serverPassword;

            m_serverLoggedUser = m_serverAPI.Login( m_serverLoginReq );

            /* ユーザー情報を取得できなかった場合 */
            if ( m_serverLoggedUser.Length == 0 )
            {
                /* 確認ダイアログを表示 */
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Connect_Server] + MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Press_Reflesh] , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return false;
            }

            return true;
            /* MessageBox.Show( "UserKey : " + m_loggedUser ); */
        }

        /// <summary>
        /// アカウント管理サーバーからアカウント管理ページのURLを取得する
        /// </summary>
        private bool GetListToPastebin( )
        {
            if ( m_serverListReq != null )
                m_serverListReq = null;
            m_serverListReq = new PasteListRequest( );
            m_serverListReq.UserKey = m_serverLoggedUser;
            m_serverListReq.ResultsLimit = null;

            if ( m_serverDatas != null )
                m_serverDatas = null;
            m_serverDatas = m_serverAPI.ListPastes( m_serverListReq );
            foreach ( var paste in m_serverDatas )
            {
                if ( paste.Title == m_serverPage )
                {
                    m_serverUri = paste.Url;
                    return true;
                }
            }

            /* 確認ダイアログを表示 */
            messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Irregular_Server_Setting] + MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Press_Reflesh] , MessageForm.MODE_OK );
            messageForm.ShowDialog( );
            return false;
        }

        /// <summary>
        /// アカウント管理ページから全アカウント情報を取得する
        /// </summary>
        private bool GetHtml( )
        {
            if ( m_webClient != null )
                m_webClient = null;
            m_webClient = new WebClient( );

            try
            {
                m_serverContent = m_webClient.DownloadString( m_serverUri );
                if ( m_serverContent.Contains( START_TXT ) )
                {
                    /* アカウントリストを取得する範囲設定 */
                    m_startPos = m_serverContent.IndexOf( START_TXT );
                    m_startPos += START_TXT.Length;
                    m_endPos = m_serverContent.IndexOf( END_TXT );
                    m_length = m_endPos - m_startPos;

                    m_serverContent = m_serverContent.Substring( m_startPos , m_length );
                    if ( m_serverContent.Length == 0 )
                    {
                        /* 確認ダイアログを表示 */
                        messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Get_Account_Info] + 
                                                    MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Re_Login_After] +
                                                    MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Send_Case_To_Developer] , 
                                                    MessageForm.MODE_OK );
                        messageForm.ShowDialog( );
                        return false;
                    }

                    return true;
                }
            }
            catch ( WebException exc )
            {
                /* 確認ダイアログを表示 */
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Error_Get_Account_Info] + MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Send_Error_To_Developer] + exc.ToString( ) , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
            }
            return false;
        }

        /// <summary>
        /// 全アカウント情報の中から自分の情報があるか検索する
        /// </summary>
        private bool AnalysisHtml( )
        {
            /* 入力されたユーザー名が存在しないパターン */
            if ( !m_serverContent.Contains( this.box_user.Text + SPACE_PASS ) )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Found_Your_Account_Make_Now] , MessageForm.MODE_YN );
                m_dResult = messageForm.ShowDialog( );
                if ( m_dResult == DialogResult.Yes )
                    return true;
                else
                    return false;
            }

            if ( m_sr != null )
                m_sr = null;
            m_sr = new StringReader( m_serverContent );
            while ( m_sr.Peek( ) > -1 )
            {
                m_line = m_sr.ReadLine( );

                /* その行内にユーザー名がマッチ */
                if ( m_line.Contains( this.box_user.Text + SPACE_PASS ) )
                {
                    /* パスワードも一致 */
                    if ( m_line.Contains( this.box_pass.Text + SPACE_MCNID ) )
                    {
                        /* アカウント作成したPCのIDも一致 */
                        if ( m_line.Contains( SPACE_MCNID + m_machineId ) )
                        {

                            return true;
                        }
                        /* 違うPCからのログインの場合 */
                        else
                        {
                            messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Match_MachineID] , MessageForm.MODE_OK );
                            messageForm.ShowDialog( );
                            return false;
                        }
                    }
                    /* 入力されたユーザー名に対し、パスワードが一致しないパターン */
                    else
                    {
                        messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Match_Password] , MessageForm.MODE_OK );
                        messageForm.ShowDialog( );
                        return false;
                    }
                }
            }

            return false;
        }

        #endregion

        #region ユーザー入力データの記憶、呼び出し

        private void LoadSetting( )
        {
            string path = Application.StartupPath + "\\item\\acc.cbl";
            string text = "";

            if ( File.Exists( path ) )
            {
                StreamReader sr = new StreamReader( path );
                text = sr.ReadToEnd( );
                sr.Close( );

                string line = "";
                string target = "";
                int st;
                int ed;
                StringReader rs = new StringReader( text );
                while ( rs.Peek( ) > -1 )
                {
                    line = rs.ReadLine( );

                    target = "s_user:";
                    if ( line.Contains( target ) )
                    {
                        st = target.Length;
                        ed = line.Length - st;
                        m_serverUsername = line.Substring( st , ed );
                    }
                    target = "s_pass:";
                    if ( line.Contains( target ) )
                    {
                        st = target.Length;
                        ed = line.Length - st;
                        m_serverPassword = line.Substring( st , ed );
                    }
                    target = "s_key:";
                    if ( line.Contains( target ) )
                    {
                        st = target.Length;
                        ed = line.Length - st;
                        m_serverKey = line.Substring( st , ed );
                    }
                    target = "s_title:";
                    if ( line.Contains( target ) )
                    {
                        st = target.Length;
                        ed = line.Length - st;
                        m_serverPage = line.Substring( st , ed );
                    }
                    target = "l_user:";
                    if ( line.Contains( target ) )
                    {
                        st = target.Length;
                        ed = line.Length - st;
                        this.box_user.Text = line.Substring( st , ed );
                    }
                    target = "l_pass:";
                    if ( line.Contains( target ) )
                    {
                        st = target.Length;
                        ed = line.Length - st;
                        this.box_pass.Text = line.Substring( st , ed );
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
            string path = Application.StartupPath + "\\item\\acc.cbl";
            string text = "";

            text = "s_user:" + m_serverUsername + "\r\n";
            text += "s_pass:" + m_serverPassword + "\r\n";
            text += "s_key:" + m_serverKey + "\r\n";
            text += "s_title:" + m_serverPage + "\r\n";
            text += "l_user:" + this.box_user.Text + "\r\n";
            text += "l_pass:" + this.box_pass.Text + "\r\n";
            
            StreamWriter sw = new StreamWriter( path , false );
            sw.Write( text );
            sw.Close( );
        }

        #endregion

        /// <summary>
        /// ログインフォームを閉じる
        /// </summary>
        private void btnExit_Click( object sender , EventArgs e )
        {
            SaveSetting( );
            this.Close( );
        }

        /// <summary>
        /// ログイン（アカウント登録）する
        /// </summary>
        private void btn_Login_Click( object sender , EventArgs e )
        {
            m_coroutine_flag = false;

            m_coroutine_flag = LoginToPastebin( );
            if ( !m_coroutine_flag )
                return;

            m_coroutine_flag = GetListToPastebin( );
            if ( !m_coroutine_flag )
                return;

            m_coroutine_flag = GetHtml( );
            if ( !m_coroutine_flag )
                return;

            m_coroutine_flag = AnalysisHtml( );
            if ( !m_coroutine_flag )
                return;
        }
    }
}
