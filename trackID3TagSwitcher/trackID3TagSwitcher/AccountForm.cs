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
        private const string SPACE_MCNID = "[+=]";
        private const string SPACE_UNQID = "[+!]";

        private bool m_coroutine_flag = false;
        private PastebinAPI m_serverAPI;
        private PasteLoginRequest m_serverLoginReq;
        private PasteListRequest m_serverListReq;
        private PasteData[] m_serverDatas;
        private string m_serverUri = "";
        private string m_serverReq = "";
        private string m_serverContent = "";
        private string m_decryptContent = "";
        private string m_encryptContent = "";
        private WebClient m_webClient;
        private int m_startPos = 0;
        private int m_endPos = 0;
        private int m_length = 0;
        private StringReader m_sr;
        private string m_line = "";
        private DialogResult m_dResult;
        private char m_char;
        public bool isUnlockLicence = false;
        private PasteDeleteRequest m_serverDelReq;
        private PasteCreateRequest m_serverCreateReq;

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

        #region フォーム起動時の初回取得、チェック

        /// <summary>
        /// 使用中のPCのマシンIDを取得する
        /// </summary>
        private void GetCurrentMachineID( )
        {
            /*
            m_machineId = new DeviceIdBuilder( )
                                .AddMachineName( )
                                .AddMacAddress( )
                                .AddProcessorId( )
                                .AddMotherboardSerialNumber( )
                                .ToString( );
                                */
            m_machineId = DeviceInfo.getCPUId( ) + SPACE_UNQID + DeviceInfo.getUUID( );
            // MessageBox.Show( m_machineId );
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

        /// <summary>
        /// ユーザー名、パスワードに使用不可能な文字が含まれているかチェックする
        /// </summary>
        private bool CheckUsernameNPassword( )
        {
            if ( this.box_user.Text.Contains( SPACE_PASS ) )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Cannot_Use_Username] , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return false;
            }
            else if ( this.box_user.Text.Contains( SPACE_MCNID ) )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Cannot_Use_Username] , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return false;
            }
            else if ( this.box_user.Text.Contains( SPACE_UNQID ) )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Cannot_Use_Username] , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return false;
            }

            if ( this.box_pass.Text.Contains( SPACE_PASS ) )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Cannot_Use_Password] , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return false;
            }
            else if ( this.box_pass.Text.Contains( SPACE_MCNID ) )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Cannot_Use_Password] , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return false;
            }
            else if ( this.box_pass.Text.Contains( SPACE_UNQID ) )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Cannot_Use_Password] , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return false;
            }

            return true;
        }

        #endregion

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
                    m_serverReq = paste.Key;
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

                    // MessageBox.Show( m_serverContent );
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
        /// 取得した全アカウント情報を復号化する
        /// </summary>
        private bool DecryptHtml( )
        {
            try
            {
                m_decryptContent = "";
                for ( int i = 0 ; i < m_serverContent.Length ; i++ )
                {
                    //m_char = m_serverContent[i];
                    //m_char = (char)( (int)m_char - 2 );
                    m_decryptContent += (char)( (int)m_serverContent[i] - 2 );
                }
                // MessageBox.Show( m_decryptContent );
                return true;
            }
            catch ( Exception ex )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Get_Account_Info] +
                                            MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Send_Error_To_Developer] +
                                            ex.Message ,
                                            MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return false;
            }
        }

        /// <summary>
        /// 全アカウント情報の中から自分の情報があるか検索する
        /// </summary>
        private bool AnalysisHtml( )
        {
            try
            {
                isUnlockLicence = false;

                /* 入力されたユーザー名が存在しないパターン */
                if ( !m_decryptContent.Contains( this.box_user.Text + SPACE_PASS ) )
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
                m_sr = new StringReader( m_decryptContent );
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
                                isUnlockLicence = true;
                                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Success_Login_pt1] +
                                                            this.box_user.Text +
                                                            MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Success_Login_pt2] ,
                                                            MessageForm.MODE_OK );
                                messageForm.ShowDialog( );
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
            }
            catch ( Exception ex )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Get_Account_Info] +
                                            MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Send_Error_To_Developer] +
                                            ex.Message ,
                                            MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return false;
            }

            messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Found_Your_Account_Make_Now] , MessageForm.MODE_YN );
            m_dResult = messageForm.ShowDialog( );
            if ( m_dResult == DialogResult.Yes )
                return true;
            else
                return false;
        }

        /// <summary>
        /// 自分のアカウント情報を付けたし、暗号化する
        /// </summary>
        private bool EncryptHtml( )
        {
            try
            {
                m_decryptContent += "\r\n";
                m_decryptContent += this.box_user.Text + SPACE_PASS;
                m_decryptContent += this.box_pass.Text + SPACE_MCNID;
                m_decryptContent += m_machineId;

                m_encryptContent = "";
                for ( int i = 0 ; i < m_decryptContent.Length ; i++ )
                {
                    m_encryptContent += (char)( (int)m_decryptContent[i] + 2 );
                }
                // MessageBox.Show( m_encryptContent );
                return true;
            }
            catch ( Exception ex )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Get_Account_Info] +
                                            MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Send_Error_To_Developer] +
                                            ex.Message ,
                                            MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return false;
            }
        }

        /// <summary>
        /// 現在のアカウント管理ページを一旦削除する
        /// </summary>
        private bool DeleteHtml( )
        {
            if ( m_serverDelReq != null )
                m_serverDelReq = null;

            m_serverDelReq = new PasteDeleteRequest( );
            m_serverDelReq.UserKey = m_serverLoggedUser;
            m_serverDelReq.PasteKey = m_serverReq;

            if ( m_serverAPI.DeletePaste( m_serverDelReq ) )
            {
                return true;
            }
            else
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Failed_Login] +
                                            MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Re_Login_After] +
                                            MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Send_Case_To_Developer] ,
                                            MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return false;
            }
        }

        /// <summary>
        /// 新しくアカウントページを作り直す
        /// </summary>
        private bool MakeHtml( )
        {
            if ( m_serverCreateReq != null )
                m_serverCreateReq = null;

            m_serverCreateReq = new PasteCreateRequest( );
            m_serverCreateReq.Code = m_encryptContent;
            m_serverCreateReq.Name = m_serverPage;
            m_serverCreateReq.ExpireDate = PasteExpireDate.Never;
            m_serverCreateReq.Private = PastePrivate.Unlisted;
            m_serverCreateReq.Format = "text";
            m_serverCreateReq.UserKey = m_serverLoggedUser; // Set UserKey In Login,  Empty(or null) In Guest

            if( m_serverAPI.CreatePaste( m_serverCreateReq ).Length != 0 )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Success_Login_pt1] +
                                            this.box_user.Text +
                                            MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Success_Register_pt2] ,
                                            MessageForm.MODE_OK );
                messageForm.ShowDialog( );

                isUnlockLicence = true;
                return true;
            }
            else
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Failed_Login] +
                                            MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Re_Login_After] +
                                            MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Send_Case_To_Developer] ,
                                            MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return false;
            }
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
            if ( isUnlockLicence )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Already_Login_pt1] +
                                            this.box_user.Text +
                                            MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Already_Login_pt2] ,
                                            MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return;
            }

            m_coroutine_flag = false;

            m_coroutine_flag = CheckUsernameNPassword( );
            if ( !m_coroutine_flag )
                return;

            m_coroutine_flag = LoginToPastebin( );
            if ( !m_coroutine_flag )
                return;

            m_coroutine_flag = GetListToPastebin( );
            if ( !m_coroutine_flag )
                return;

            m_coroutine_flag = GetHtml( );
            if ( !m_coroutine_flag )
                return;

            m_coroutine_flag = DecryptHtml( );
            if ( !m_coroutine_flag )
                return;

            m_coroutine_flag = AnalysisHtml( );
            if ( !m_coroutine_flag )
                return;

            /* アカウントが無いため作らなければいけない */
            if ( !isUnlockLicence )
            {
                m_coroutine_flag = EncryptHtml( );
                if ( !m_coroutine_flag )
                    return;

                m_coroutine_flag = DeleteHtml( );
                if ( !m_coroutine_flag )
                    return;

                m_coroutine_flag = MakeHtml( );
                if ( !m_coroutine_flag )
                    return;
            }
        }
    }
}
