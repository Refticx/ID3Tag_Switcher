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
using PastebinAPI_nikibobi;
using System.Threading;

namespace trackID3TagSwitcher
{
    public partial class AccountForm : Form
    {
        private MessageForm messageForm = new MessageForm( );    /* ダイアログ用フォームを作成しておく */

        /* プログレスバー設定 */
        /// <summary>
        /// プログレスバーに割り当てる、async処理内のファンクション数
        /// </summary>
        private const int SERVER_FUNC_NUM = 20;

        /* ==================== サーバー設定 ==================== */

        /// <summary>
        /// サーバーから取得するページの最大件数
        /// </summary>
        private const int SERVER_MAX_PAGE = 3;

        /// <summary>
        /// アカウントリストのページの暗号化の際の移動オフセット量
        /// </summary>
        private const int ENCRYPT_SHIFT_SIZE_ACC_PAGE = 2;

        /// <summary>
        /// サインインリストのページの暗号化の際の移動オフセット量
        /// </summary>
        private const int ENCRYPT_SHIFT_SIZE_SIGN_PAGE = 3;

        /// <summary>
        /// アカウントコンフィグファイルの暗号化の際の移動オフセット量
        /// </summary>
        private const int ENCRYPT_SHIFT_SIZE_CONFIG_FILE = 5;

        /// <summary>
        /// サーバーアカウントデータの暗号化の際の移動オフセット量
        /// </summary>
        private const int ENCRYPT_SHIFT_SIZE_SERVER_ACC = 6;

        public static string m_serverUsername = "";
        public static string m_serverPassword = "";
        public static string m_serverKey = "";
        public static string m_serverAccPage = "";
        public static string m_serverSignPage = "";
        public static string m_serverSettingInfo = "";
        private string m_machineId = "";

        /* 各関数処理用変数 */
        private const string START_TXT = "<textarea class=\"textarea\">";
        private const string END_TXT = "</textarea>";
        private const string SPACE_PASS = "[+@]";
        private const string SPACE_MCNID = "[+=]";
        private const string SPACE_UNQID = "[+!]";
        private const string SPACE_SIGN = "[+*]";
        private const string STATUS_SIGNIN = "1";
        private const string STATUS_SIGNOUT = "0";
        private const string START_TXT_S_USER = "<s_user>";
        private const string END_TXT_S_USER = "</s_user>";
        private const string START_TXT_S_PASS = "<s_pass>";
        private const string END_TXT_S_PASS = "</s_pass>";
        private const string START_TXT_S_KEY = "<s_key>";
        private const string END_TXT_S_KEY = "</s_key>";
        private const string START_TXT_S_ACC_TITLE = "<s_acctitle>";
        private const string END_TXT_S_ACC_TITLE = "</s_acctitle>";
        private const string START_TXT_S_SIGN_TITLE = "<s_signtitle>";
        private const string END_TXT_S_SIGN_TITLE = "</s_signtitle>";

        /// <summary>
        /// アカウントリスト内に自分のアカウントがあり、デバイス情報も一致しているかどうか
        /// </summary>
        public static bool isMatchMyAccount = false;

        /// <summary>
        /// ログインリスト内に自分のアカウントが無く、作成が必要な場合に建てるフラグ
        /// </summary>
        public static bool isNeedCreateStatus = false;

        /// <summary>
        /// ログインリスト内の自分のアカウントをログイン状態にした段階でネットワークライセンスを付与する
        /// </summary>
        public static bool isUnlockLicence = false;

        private Task<bool> m_coroutine_task;
        private bool m_coroutine_flag = false;
        private string m_serverAccPageUri = "";
        private string m_serverAccPageReq = "";
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
        private string m_localText1 = String.Empty;
        private string m_localText2 = String.Empty;

        private PastebinAPI_nikibobi.User m_userAsync;
        private Uri m_uriAsync;
        private PastebinAPI_nikibobi.Paste m_serverAccPageDelReqAsync;

        private string m_serverSignPageUri = "";
        private string m_serverSignPageReq = "";
        private PastebinAPI_nikibobi.Paste m_serverSignPageDelReqAsync;

        public AccountForm( )
        {
            InitializeComponent( );
        }

        private void LoginForm_Load( object sender , EventArgs e )
        {
            HideInputObjs( false );
            GetCurrentMachineID( );
            LoadSetting( );
            CheckNetwork( );
            CheckServiceInfo( );
            LockLoginOnShowUI( );
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
            if ( !NetworkFunc.CheckNetworkWithMessage( ) )
                this.Close( );
        }

        /// <summary>
        /// サーバー接続設定が取得できているかチェックし、できていなければ更新ボタンを押すよう促す
        /// </summary>
        private void CheckServiceInfo( )
        {
            /*
            if ( ( m_serverUsername == String.Empty ) ||
                    ( m_serverPassword == String.Empty ) ||
                    ( m_serverKey == String.Empty ) ||
                    ( m_serverSignPage == String.Empty ) ||
                    ( m_machineId == String.Empty ) ||
                    ( m_serverSettingInfo == String.Empty ) ||
                    ( m_serverAccPage == String.Empty ) )
                    */
            if ( m_serverSettingInfo == String.Empty )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Get_ServerInfo] +
                                            MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Re_Download_App] , 
                                            MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                this.Close( );
            }
        }

        /// <summary>
        /// ユーザー名、パスワードに使用不可能な文字が含まれているかチェックする
        /// </summary>
        private async Task<bool> CheckUsernameNPassword( )
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
            else if ( this.box_user.Text.Contains( SPACE_SIGN ) )
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
            else if ( this.box_pass.Text.Contains( SPACE_SIGN ) )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Cannot_Use_Password] , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return false;
            }

            return true;
        }

        /// <summary>
        /// 既にログイン済みであれば再ログインできないようにする
        /// </summary>
        private void LockLoginOnShowUI( )
        {
            if ( isUnlockLicence )
            {
                LockLogin( false );
                SetLog( this.lblAppLogText ,  Color.LimeGreen , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Success_Login_noticeBar] );
                SetLog( this.lbl_progressContent , Color.AliceBlue , String.Empty );
            }
        }

        /// <summary>
        /// 既にログイン済みであれば再ログインできないようにする
        /// </summary>
        private void LockLogin( bool onf )
        {
            this.box_user.Enabled = onf;
            this.box_pass.Enabled = onf;
            this.btn_Login.Enabled = onf;
        }

        /// <summary>
        /// ログアウト時に入力可能なフィールドは表示しないようにする
        /// </summary>
        private void HideInputObjs( bool onf )
        {
            bool flag = false;
            if ( onf )
                flag = false;
            else
                flag = true;
            this.box_user.Visible = flag;
            this.box_pass.Visible = flag;
            this.btn_Login.Visible = flag;
        }

        #endregion

        #region サーバー接続処理（非同期）

        /// <summary>
        /// 全処理で使う変数を初期化する
        /// </summary>
        /// <returns></returns>
        private async Task<bool> ClearArgs( )
        {
            try
            {
                SetLog( this.lbl_progressContent , Color.AliceBlue , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Try_Clear_Args] );

                Pastebin.DevKey = String.Empty;
                m_userAsync = null;

                m_serverAccPageUri = String.Empty;
                m_serverAccPageReq = String.Empty;
                m_serverAccPageDelReqAsync = null;
                m_serverSignPageUri = String.Empty;
                m_serverSignPageReq = String.Empty;
                m_serverSignPageDelReqAsync = null;

                m_webClient = null;
                m_uriAsync = null;
                m_serverContent = String.Empty;
                m_startPos = 0;
                m_startPos = 0;
                m_endPos = 0;
                m_length = 0;

                m_decryptContent = String.Empty;

                m_sr = null;
                m_line = String.Empty;

                m_decryptContent = String.Empty;

                m_localText1 = String.Empty;
                m_localText2 = String.Empty;

                isMatchMyAccount = false;
                isNeedCreateStatus = false;
                isUnlockLicence = false;

                await Task.Delay( 1 );
                return true;
            }
            catch( Exception ex )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Irregular_Error] + ex.ToString( ) , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                SetLog( this.lbl_progressContent , Color.AliceBlue , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Clear_Args] );
                return false;
            }
        }

        /// <summary>
        /// サーバーから全
        /// </summary>
        /// <returns></returns>
        private async Task<bool> GetServerSettingsAsync( string uri )
        {
            if ( m_webClient != null )
                m_webClient = null;
            m_webClient = new WebClient( );

            if ( m_uriAsync != null )
                m_uriAsync = null;
            m_uriAsync = new Uri( uri );

            try
            {
                SetLog( this.lbl_progressContent , Color.AliceBlue , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Try_Get_Data_List] );
                m_localText1 = await m_webClient.DownloadStringTaskAsync( m_uriAsync );
                m_localText1 = NetworkFunc.DecryptHtmlWithMessage( m_localText1 , ENCRYPT_SHIFT_SIZE_SERVER_ACC );

                if ( m_localText1.Contains( START_TXT_S_USER ) )
                {
                    m_startPos = m_localText1.IndexOf( START_TXT_S_USER );
                    m_startPos += START_TXT_S_USER.Length;
                    m_endPos = m_localText1.IndexOf( END_TXT_S_USER );
                    m_length = m_endPos - m_startPos;
                    m_serverUsername = m_localText1.Substring( m_startPos , m_length );
                }

                if ( m_localText1.Contains( START_TXT_S_PASS ) )
                {
                    m_startPos = m_localText1.IndexOf( START_TXT_S_PASS );
                    m_startPos += START_TXT_S_PASS.Length;
                    m_endPos = m_localText1.IndexOf( END_TXT_S_PASS );
                    m_length = m_endPos - m_startPos;
                    m_serverPassword = m_localText1.Substring( m_startPos , m_length );
                }

                if ( m_localText1.Contains( START_TXT_S_KEY ) )
                {
                    m_startPos = m_localText1.IndexOf( START_TXT_S_KEY );
                    m_startPos += START_TXT_S_KEY.Length;
                    m_endPos = m_localText1.IndexOf( END_TXT_S_KEY );
                    m_length = m_endPos - m_startPos;
                    m_serverKey = m_localText1.Substring( m_startPos , m_length );
                }

                if ( m_localText1.Contains( START_TXT_S_ACC_TITLE ) )
                {
                    m_startPos = m_localText1.IndexOf( START_TXT_S_ACC_TITLE );
                    m_startPos += START_TXT_S_ACC_TITLE.Length;
                    m_endPos = m_localText1.IndexOf( END_TXT_S_ACC_TITLE );
                    m_length = m_endPos - m_startPos;
                    m_serverAccPage = m_localText1.Substring( m_startPos , m_length );
                }

                if ( m_localText1.Contains( START_TXT_S_SIGN_TITLE ) )
                {
                    m_startPos = m_localText1.IndexOf( START_TXT_S_SIGN_TITLE );
                    m_startPos += START_TXT_S_SIGN_TITLE.Length;
                    m_endPos = m_localText1.IndexOf( END_TXT_S_SIGN_TITLE );
                    m_length = m_endPos - m_startPos;
                    m_serverSignPage = m_localText1.Substring( m_startPos , m_length );
                }

                return true;
            }
            catch ( WebException exc )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Error_Get_Account_Info] + 
                                            MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Send_Error_To_Developer] + 
                                            exc.ToString( ) , 
                                            MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                SetLog( this.lbl_progressContent , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Get_Data_List] );
                return false;
            }
        }

        /// <summary>
        /// サーバー接続設定が取得できているかチェックし、できていなければ更新ボタンを押すよう促す
        /// </summary>
        private async Task<bool> CheckServiceInfoAsync( )
        {
            SetLog( this.lbl_progressContent , Color.AliceBlue , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Try_Get_Data_List] );
            if ( ( m_serverUsername == String.Empty ) ||
                    ( m_serverPassword == String.Empty ) ||
                    ( m_serverKey == String.Empty ) ||
                    ( m_serverSignPage == String.Empty ) ||
                    ( m_machineId == String.Empty ) ||
                    ( m_serverSettingInfo == String.Empty ) ||
                    ( m_serverAccPage == String.Empty ) )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Get_ServerInfo] +
                                            MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Send_Case_To_Developer] ,
                                            MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                SetLog( this.lbl_progressContent , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Get_Data_List] );
                return false;
            }
            await Task.Delay( 1 );
            return true;
        }

        /// <summary>
        /// アカウント管理サーバーにログインする
        /// </summary>
        private async Task<bool> LoginToPastebinAsync( )
        {
            /* 開発者用キーを設定する */
            Pastebin.DevKey = m_serverKey;

            /* ログイン試行 */
            try
            {
                SetLog( this.lbl_progressContent , Color.AliceBlue , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Try_Server_Connect] );
                m_userAsync = await Pastebin.LoginAsync( m_serverUsername , m_serverPassword );
            }
            /* ログインに失敗した場合 */
            catch ( PastebinAPI_nikibobi.PastebinException ex )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Connect_Server] + MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Re_Download_App] , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                SetLog( this.lbl_progressContent , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Server_Connect] );
                return false;
            }

            return true;
        }

        /// <summary>
        /// アカウント管理サーバーからアカウント管理ページのURLを取得する
        /// </summary>
        private async Task<bool> GetListToPastebinAsync( )
        {
            try
            {
                SetLog( this.lbl_progressContent , Color.AliceBlue , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Try_Get_Server_List] );

                foreach ( Paste paste in await m_userAsync.ListPastesAsync( SERVER_MAX_PAGE ) )
                {
                    if ( paste.Title == m_serverAccPage )
                    {
                        m_serverAccPageUri = paste.Url;
                        m_serverAccPageReq = paste.Key;
                        m_serverAccPageDelReqAsync = paste;
                    }
                    else if ( paste.Title == m_serverSignPage )
                    {
                        m_serverSignPageUri = paste.Url;
                        m_serverSignPageReq = paste.Key;
                        m_serverSignPageDelReqAsync = paste;
                    }
                }

                if ( (m_serverAccPageReq != String.Empty) && (m_serverAccPageReq != String.Empty) )
                    return true;
                else
                {
                    messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Connect_Server] + MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Re_Download_App] , MessageForm.MODE_OK );
                    messageForm.ShowDialog( );
                    SetLog( this.lbl_progressContent , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Find_Server_List] );
                    return false;
                }
            }
            /* 取得に失敗した場合 */
            catch ( PastebinAPI_nikibobi.PastebinException ex )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Irregular_Server_Setting] + MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Re_Download_App] , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                SetLog( this.lbl_progressContent , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Server_Connect] );
                return false;
            }
        }

        /// <summary>
        /// 指定されたページのテキストデータを取得する
        /// </summary>
        private async Task<bool> GetHtmlAsync( string uri , string st , string  ed )
        {
            if ( m_webClient != null )
                m_webClient = null;
            m_webClient = new WebClient( );
            
            if ( m_uriAsync != null )
                m_uriAsync = null;
            m_uriAsync = new Uri( uri );

            try
            {
                SetLog( this.lbl_progressContent , Color.AliceBlue , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Try_Get_Data_List] );
                m_serverContent = await m_webClient.DownloadStringTaskAsync( m_uriAsync );
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
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Error_Get_Account_Info] + MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Send_Error_To_Developer] + exc.ToString( ) , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                SetLog( this.lbl_progressContent , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Get_Data_List] );
            }
            return false;
        }

        /// <summary>
        /// 取得したテキストデータを復号化する
        /// </summary>
        private async Task<bool> DecryptHtmlAsync( int shift )
        {
            try
            {
                SetLog( this.lbl_progressContent , Color.AliceBlue , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Try_Decrypt_Data_List] );
                m_decryptContent = "";
                for ( int i = 0 ; i < m_serverContent.Length ; i++ )
                {
                    m_decryptContent += (char)( (int)m_serverContent[i] - shift );
                    await Task.Delay( 1 );
                }
                return true;
            }
            catch ( Exception ex )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Get_Account_Info] +
                                            MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Send_Error_To_Developer] +
                                            ex.Message ,
                                            MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                SetLog( this.lbl_progressContent , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Decrypt_Data_List] );
                return false;
            }
        }

        /// <summary>
        /// 全アカウント情報の中から自分の情報があるか検索する
        /// </summary>
        private async Task<bool> CheckAccountExistAsync( )
        {
            try
            {
                isUnlockLicence = false;
                SetLog( this.lbl_progressContent , Color.AliceBlue , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Try_Find_Account] );

                /* 入力されたユーザー名が存在しないパターン */
                if ( !m_decryptContent.Contains( this.box_user.Text + SPACE_PASS ) )
                {
                    messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Found_Your_Account_Make_Now] , MessageForm.MODE_YN );
                    m_dResult = messageForm.ShowDialog( );
                    if ( m_dResult == DialogResult.Yes )
                        return true;
                    else
                    {
                        SetLog( this.lbl_progressContent , Color.AliceBlue , "" );
                        return false;
                    }
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
                                isMatchMyAccount = true;
                                return true;
                            }
                            /* 違うPCからのログインの場合 */
                            else
                            {
                                SetLog( this.lbl_progressContent , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Account_Device_Mismatch] );
                                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Match_MachineID] , MessageForm.MODE_OK );
                                messageForm.ShowDialog( );
                                return false;
                            }
                        }
                        /* 入力されたユーザー名に対し、パスワードが一致しないパターン */
                        else
                        {
                            SetLog( this.lbl_progressContent , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Account_Password_Mismatch] );
                            messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Match_Password] , MessageForm.MODE_OK );
                            messageForm.ShowDialog( );
                            return false;
                        }
                    }

                    await Task.Delay( 1 );
                }
            }
            catch ( Exception ex )
            {
                SetLog( this.lbl_progressContent , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Find_Account] );
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
            {
                SetLog( this.lbl_progressContent , Color.AliceBlue , "" );
                return false;
            }
        }

        /// <summary>
        /// 自分のアカウント情報を付けたす
        /// </summary>
        private async Task<bool> AddMyInfoToDecryptedHtml( )
        {
            try
            {
                SetLog( this.lbl_progressContent , Color.AliceBlue , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Try_Add_My_Info_To_Data_List] );
                m_decryptContent += "\r\n";
                m_decryptContent += this.box_user.Text + SPACE_PASS;
                m_decryptContent += this.box_pass.Text + SPACE_MCNID;
                m_decryptContent += m_machineId;
                await Task.Delay( 1 );
                return true;
            }
            catch ( Exception ex )
            {
                SetLog( this.lbl_progressContent , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Add_My_Info_To_Data_List] );
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Irregular_Error] +
                                            ex.Message ,
                                            MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return false;
            }
        }

        /// <summary>
        /// 取得したテキストデータを暗号化する
        /// </summary>
        private async Task<bool> EncryptHtmlAsync( int shift )
        {
            try
            {
                SetLog( this.lbl_progressContent , Color.AliceBlue , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Try_Encrypt_Data_List] );
                m_encryptContent = "";
                for ( int i = 0 ; i < m_decryptContent.Length ; i++ )
                {
                    m_encryptContent += (char)( (int)m_decryptContent[i] + shift );
                    await Task.Delay( 1 );
                }
                return true;
            }
            catch ( Exception ex )
            {
                SetLog( this.lbl_progressContent , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Encrypt_Data_List] );
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Irregular_Error] +
                                            ex.Message ,
                                            MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return false;
            }
        }

        /// <summary>
        /// 現在のアカウント管理ページを一旦削除する
        /// </summary>
        private async Task<bool> DeleteHtmlAsync( Paste page )
        {
            try
            {
                SetLog( this.lbl_progressContent , Color.AliceBlue , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Try_Delete_Server] );
                await m_userAsync.DeletePasteAsync( page );
                return true;
            }
            catch ( PastebinAPI_nikibobi.PastebinException ex )
            {
                SetLog( this.lbl_progressContent , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Delete_Server] );
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
        private async Task<bool> MakeHtmlAsync( string tryProgText , string failedProgText , string pageTitle )
        {
            try
            {
                SetLog( this.lbl_progressContent , Color.AliceBlue , tryProgText );
                await m_userAsync.CreatePasteAsync( m_encryptContent , pageTitle , Language.None , Visibility.Unlisted , Expiration.Never );
                isMatchMyAccount = true;
                return true;
            }
            catch ( PastebinAPI_nikibobi.PastebinException ex )
            {
                SetLog( this.lbl_progressContent , Color.Orange , failedProgText );
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Failed_Login] +
                                            MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Re_Login_After] +
                                            MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Send_Case_To_Developer] ,
                                            MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return false;
            }
        }

        /// <summary>
        /// 全サインイン情報の中から自分のアカウントがサインイン済みか検索する
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckAccountLoginAsync( string serverLoginStatus )
        {
            try
            {
                if ( serverLoginStatus == STATUS_SIGNOUT )
                    m_localText1 = STATUS_SIGNIN;
                else if ( serverLoginStatus == STATUS_SIGNIN )
                    m_localText1 = STATUS_SIGNOUT;

                isUnlockLicence = false;
                SetLog( this.lbl_progressContent , Color.AliceBlue , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Try_Find_Login_Status] );

                /* 入力されたユーザー名が存在しないパターン*/
                if ( !m_decryptContent.Contains( this.box_user.Text + SPACE_SIGN ) )
                {
                    isNeedCreateStatus = true;
                    return true;
                }

                if ( m_sr != null )
                    m_sr = null;
                m_sr = new StringReader( m_decryptContent );
                while ( m_sr.Peek( ) > -1 )
                {
                    m_line = m_sr.ReadLine( );

                    /* その行内にユーザー名がマッチ */
                    if ( m_line.Contains( this.box_user.Text + SPACE_SIGN ) )
                    {
                        /* 指定されたログインステータスになっている */
                        if ( m_line.Contains( SPACE_SIGN + serverLoginStatus ) )
                        {
                            isNeedCreateStatus = false;
                            return true;
                        }
                        /* 指定されたログインステータスになっていない場合 */
                        else if ( m_line.Contains( SPACE_SIGN + m_localText1 ) )
                        {
                            /* 前回アプリを正常終了していない形跡があるなら、サーバーステータスと一致しないのは当然なので、このまま処理する */
                            if ( Form1.isExitStatus == Form1.ExitStatus.ForceClosed )
                            {
#if DEBUG
                                MessageBox.Show( "CheckAccountLoginAsync - ForceClosed" );
#endif
                                isNeedCreateStatus = false;
                                return true;
                            }
                            SetLog( this.lbl_progressContent , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Login_Status_Already_Hacked] );
                            messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Failed_Login_Already_Loggedin] , MessageForm.MODE_OK );
                            messageForm.ShowDialog( );
                            return false;
                        }
                    }

                    await Task.Delay( 1 );
                }

                throw new Exception( );
            }
            catch ( Exception ex )
            {
                SetLog( this.lbl_progressContent , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Login_Status] );
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Get_Account_Info] +
                                            MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Send_Error_To_Developer] +
                                            ex.Message ,
                                            MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                
                return false;
            }
        }

        /// <summary>
        /// 自分のアカウントのサインイン情報を付けたす
        /// </summary>
        private async Task<bool> AddMyLoginStatusToDecryptedHtml( )
        {
            try
            {
                SetLog( this.lbl_progressContent , Color.AliceBlue , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Try_Add_My_Login_To_Data_List] );
                m_decryptContent += "\r\n";
                m_decryptContent += this.box_user.Text + SPACE_SIGN + STATUS_SIGNIN;
                await Task.Delay( 1 );
                return true;
            }
            catch ( Exception ex )
            {
                SetLog( this.lbl_progressContent , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Add_My_Login_To_Data_List] );
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Irregular_Error] + ex.Message , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return false;
            }
        }

        /// <summary>
        /// 自分のアカウントのサインイン情報を修正する
        /// </summary>
        private async Task<bool> EditMyLoginStatusToDecryptedHtml( bool loginStatus )
        {
            try
            {
                SetLog( this.lbl_progressContent , Color.AliceBlue , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Try_Add_My_Login_To_Data_List] );
                await Task.Delay( 1 );

                if ( !loginStatus )
                {
                    if ( m_decryptContent.Contains( this.box_user.Text + SPACE_SIGN + STATUS_SIGNIN ) )
                    {
                        m_decryptContent.Replace( this.box_user.Text + SPACE_SIGN + STATUS_SIGNIN , this.box_user.Text + SPACE_SIGN + STATUS_SIGNOUT );
                        return true;
                    }
                    else if ( m_decryptContent.Contains( this.box_user.Text + SPACE_SIGN + STATUS_SIGNOUT ) )
                    {
                        /* 前回アプリを正常終了していない形跡があるなら、サーバーステータスと一致しないのは当然なので、このまま処理する */
                        if ( Form1.isExitStatus == Form1.ExitStatus.ForceClosed )
                        {
#if DEBUG
                            MessageBox.Show( "EditMyLoginStatusToDecryptedHtml - ForceClosed" );
#endif
                            return true;
                        }

                        SetLog( this.lbl_progressContent , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Login_Status_Already_Hacked] );
                        messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Failed_Login_Already_Loggedin] , MessageForm.MODE_OK );
                        messageForm.ShowDialog( );
                        return false;
                    }
                }
                else
                {
                    if ( m_decryptContent.Contains( this.box_user.Text + SPACE_SIGN + STATUS_SIGNOUT ) )
                    {
                        m_decryptContent.Replace( this.box_user.Text + SPACE_SIGN + STATUS_SIGNOUT , this.box_user.Text + SPACE_SIGN + STATUS_SIGNIN );
                        return true;
                    }
                    else if ( m_decryptContent.Contains( this.box_user.Text + SPACE_SIGN + STATUS_SIGNIN ) )
                    {
                        /* 前回アプリを正常終了していない形跡があるなら、サーバーステータスと一致しないのは当然なので、このまま処理する */
                        if ( Form1.isExitStatus == Form1.ExitStatus.ForceClosed )
                        {
#if DEBUG
                            MessageBox.Show( "EditMyLoginStatusToDecryptedHtml - ForceClosed" );
#endif
                            return true;
                        }
                        SetLog( this.lbl_progressContent , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Login_Status_Already_Hacked] );
                        messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Failed_Login_Already_Loggedin] , MessageForm.MODE_OK );
                        messageForm.ShowDialog( );
                        return false;
                    }
                }

                SetLog( this.lbl_progressContent , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Add_My_Login_To_Data_List] );
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Get_Account_Info] +
                                            MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Re_Login_After] +
                                            MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Plz_Send_Case_To_Developer] ,
                                            MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return false;
            }
            catch ( Exception ex )
            {
                SetLog( this.lbl_progressContent , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Add_My_Login_To_Data_List] );
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Irregular_Error] + ex.Message , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return false;
            }
        }

        private void MainAsync( )
        {
            PastebinExampleAsync( ).GetAwaiter( ).GetResult( );
        }

        private async Task PastebinExampleAsync( )
        {
            //before using any class in the api you must enter your api dev key
            Pastebin.DevKey = m_serverKey;
            //you can see yours here: https://pastebin.com/api#1
            try
            {
                // login and get user object
                User me = await Pastebin.LoginAsync( m_serverUsername , m_serverPassword );
                // user contains information like e-mail, location etc...
                Console.WriteLine( "{0}({1}) lives in {2}" , me , me.Email , me.Location );
                // lists all pastes for this user
                foreach ( Paste paste in await me.ListPastesAsync( 3 ) ) // we limmit the results to 3
                {
                    Console.WriteLine( paste.Title );
                }

                string code = "<your fancy &code#() goes here>";
                //creates a new paste and get paste object
                Paste newPaste = await me.CreatePasteAsync( code , "MyPasteTitle" , Language.HTML5 , Visibility.Public , Expiration.TenMinutes );
                //newPaste now contains the link returned from the server
                Console.WriteLine( "URL: {0}" , newPaste.Url );
                Console.WriteLine( "Paste key: {0}" , newPaste.Key );
                Console.WriteLine( "Content: {0}" , newPaste.Text );
                //deletes the paste we just created
                await me.DeletePasteAsync( newPaste );

                //lists all currently trending pastes(similar to me.ListPastes())
                foreach ( Paste paste in await Pastebin.ListTrendingPastesAsync( ) )
                {
                    Console.WriteLine( "{0} - {1}" , paste.Title , paste.Url );
                }
                //you can create pastes directly from Pastebin static class but they are created as guests and you have a limited number of guest uploads
                Paste anotherPaste = await Paste.CreateAsync( "another paste" , "MyPasteTitle2" , Language.CSharp , Visibility.Unlisted , Expiration.OneHour );
                Console.WriteLine( anotherPaste.Title );
            }
            catch ( PastebinAPI_nikibobi.PastebinException ex ) //api throws PastebinException
            {
                //in the Parameter property you can see what invalid parameter was sent
                //here we check if the exeption is thrown because of invalid login details
                if ( ex.Parameter == PastebinAPI_nikibobi.PastebinException.ParameterType.Login )
                {
                    Console.Error.WriteLine( "Invalid username/password" );
                }
                else
                {
                    throw; //all other types are rethrown and not swalowed!
                }
            }
            Console.ReadKey( );
        }

        #endregion

        #region 汎用型スクリプト

        private void SetLog( Label lbl , Color cl , string log )
        {
            lbl.ForeColor = cl;
            lbl.Text = log;
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
                text = NetworkFunc.DecryptHtmlWithMessage( text , ENCRYPT_SHIFT_SIZE_CONFIG_FILE );

                string line = "";
                string target = "";
                int st;
                int ed;
                StringReader rs = new StringReader( text );
                while ( rs.Peek( ) > -1 )
                {
                    line = rs.ReadLine( );

                    /*

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
                    target = "s_acctitle:";
                    if ( line.Contains( target ) )
                    {
                        st = target.Length;
                        ed = line.Length - st;
                        m_serverAccPage = line.Substring( st , ed );
                    }
                    target = "s_signtitle:";
                    if ( line.Contains( target ) )
                    {
                        st = target.Length;
                        ed = line.Length - st;
                        m_serverSignPage = line.Substring( st , ed );
                    }

                    */
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
                    target = "s_settingInfo:";
                    if ( line.Contains( target ) )
                    {
                        st = target.Length;
                        ed = line.Length - st;
                        m_serverSettingInfo = line.Substring( st , ed );
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
            /*
            text = "s_user:" + m_serverUsername + "\r\n";
            text += "s_pass:" + m_serverPassword + "\r\n";
            text += "s_key:" + m_serverKey + "\r\n";
            text += "s_acctitle:" + m_serverAccPage + "\r\n";
            text += "s_signtitle:" + m_serverSignPage + "\r\n";
            text += "l_user:" + this.box_user.Text + "\r\n";
            text += "l_pass:" + this.box_pass.Text + "\r\n";
            */
            text = "l_user:" + this.box_user.Text + "\r\n";
            text += "l_pass:" + this.box_pass.Text + "\r\n";
            text += "s_settingInfo:" + m_serverSettingInfo + "\r\n";

            text = NetworkFunc.EncryptHtmlWithMessage( text , ENCRYPT_SHIFT_SIZE_CONFIG_FILE );
            
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

        public async Task<bool> Logout( )
        {
            HideInputObjs( true );
            // LockLogin( false );
            SetLog( this.lblAppLogText , Color.AliceBlue , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Failed_Login_noticeBar] );

            this.webProgressBar.Maximum = SERVER_FUNC_NUM;
            this.webProgressBar.Value = 0;

            if ( await LoginThink( isSign: false ) == false )
            {
                // LockLogin( true );
                SetLog( this.lblAppLogText , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Failed_Login_noticeBar] );
                return false;
            }
            this.webProgressBar.Value = 0;

            return true;
        }

        /// <summary>
        /// ログイン（アカウント登録）する
        /// </summary>
        private async void btn_Login_Click( object sender , EventArgs e )
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

            LockLogin( false );

            this.webProgressBar.Maximum = SERVER_FUNC_NUM;
            this.webProgressBar.Value = 0;

            if ( await LoginThink( isSign: true ) == false )
            {
                LockLogin( true );
                SetLog( this.lblAppLogText , Color.Orange , MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Failed_Login_noticeBar] );
            }
            this.webProgressBar.Value = 0;
        }

        private async Task<bool> LoginThink( bool isSign )
        {
            /* 1 */
            m_coroutine_task = ClearArgs( );
            m_coroutine_flag = await m_coroutine_task;
            if ( !m_coroutine_flag )
                return m_coroutine_flag;
            this.webProgressBar.Value++;

            /* 2 */
            m_coroutine_task = CheckUsernameNPassword( );
            m_coroutine_flag = await m_coroutine_task;
            if ( !m_coroutine_flag )
                return m_coroutine_flag;
            this.webProgressBar.Value++;

            /* 3 */
            m_coroutine_task = GetServerSettingsAsync( m_serverSettingInfo );
            m_coroutine_flag = await m_coroutine_task;
            if ( !m_coroutine_flag )
                return m_coroutine_flag;
            this.webProgressBar.Value++;

            /* 4 */
            m_coroutine_task = CheckServiceInfoAsync( );
            m_coroutine_flag = await m_coroutine_task;
            if ( !m_coroutine_flag )
                return m_coroutine_flag;
            this.webProgressBar.Value++;

            /* 5 */
            m_coroutine_task = LoginToPastebinAsync( );
            m_coroutine_flag = await m_coroutine_task;
            if ( !m_coroutine_flag )
                return m_coroutine_flag;
            this.webProgressBar.Value++;

            /* 6 */
            m_coroutine_task = GetListToPastebinAsync( );
            m_coroutine_flag = await m_coroutine_task;
            if ( !m_coroutine_flag )
                return m_coroutine_flag;
            this.webProgressBar.Value++;

            /* 7 */
            /* ここからアカウントリストページ関連の処理 */
            m_coroutine_task = GetHtmlAsync( m_serverAccPageUri , START_TXT , END_TXT );
            m_coroutine_flag = await m_coroutine_task;
            if ( !m_coroutine_flag )
                return m_coroutine_flag;
            this.webProgressBar.Value++;

            /* 8 */
            m_coroutine_task = DecryptHtmlAsync( ENCRYPT_SHIFT_SIZE_ACC_PAGE );
            m_coroutine_flag = await m_coroutine_task;
            if ( !m_coroutine_flag )
                return m_coroutine_flag;
            this.webProgressBar.Value++;

            /* 9 */
            m_coroutine_task = CheckAccountExistAsync( );
            m_coroutine_flag = await m_coroutine_task;
            if ( !m_coroutine_flag )
                return m_coroutine_flag;
            this.webProgressBar.Value++;

            /* アカウントが無いため作らなければいけない */
            if ( !isMatchMyAccount )
            {
                /* 10 */
                m_coroutine_task = AddMyInfoToDecryptedHtml( );
                m_coroutine_flag = await m_coroutine_task;
                if ( !m_coroutine_flag )
                    return m_coroutine_flag;
                this.webProgressBar.Value++;

                /* 11 */
                m_coroutine_task = EncryptHtmlAsync( ENCRYPT_SHIFT_SIZE_ACC_PAGE );
                m_coroutine_flag = await m_coroutine_task;
                if ( !m_coroutine_flag )
                    return m_coroutine_flag;
                this.webProgressBar.Value++;

                /* 12 */
                m_coroutine_task = DeleteHtmlAsync( m_serverAccPageDelReqAsync );
                m_coroutine_flag = await m_coroutine_task;
                if ( !m_coroutine_flag )
                    return m_coroutine_flag;
                this.webProgressBar.Value++;

                /* 13 */
                m_localText1 = MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Try_Upload_Account];
                m_localText2 = MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Upload_Account];
                m_coroutine_task = MakeHtmlAsync( m_localText1 , m_localText2 , m_serverAccPage );
                m_coroutine_flag = await m_coroutine_task;
                if ( !m_coroutine_flag )
                    return m_coroutine_flag;
                this.webProgressBar.Value++;
            }
            /* アカウントが既に作られていた場合のみの処理 */
            else
            {
                /* アカウントを作成する処理分プログレスバーを進める */
                await Task.Delay( 1 );
                this.webProgressBar.Value++;
                await Task.Delay( 1 );
                this.webProgressBar.Value++;
                await Task.Delay( 1 );
                this.webProgressBar.Value++;
                await Task.Delay( 1 );
                this.webProgressBar.Value++;
                await Task.Delay( 1 );
            }

            /* 14 */
            /* ここからログインリストページ関連の処理 */
            m_coroutine_task = GetHtmlAsync( m_serverSignPageUri , START_TXT , END_TXT );
            m_coroutine_flag = await m_coroutine_task;
            if ( !m_coroutine_flag )
                return m_coroutine_flag;
            this.webProgressBar.Value++;

            /* 15 */
            m_coroutine_task = DecryptHtmlAsync( ENCRYPT_SHIFT_SIZE_SIGN_PAGE );
            m_coroutine_flag = await m_coroutine_task;
            if ( !m_coroutine_flag )
                return m_coroutine_flag;
            this.webProgressBar.Value++;

            /* 16 */
            string status = String.Empty;
            if ( isSign )
                status = STATUS_SIGNOUT;
            else
                status = STATUS_SIGNIN;
            m_coroutine_task = CheckAccountLoginAsync( status );
            m_coroutine_flag = await m_coroutine_task;
            if ( !m_coroutine_flag )
                return m_coroutine_flag;
            this.webProgressBar.Value++;

            /* 17 */
            /* ログインステータスがまだ追加されてなかった場合 */
            if ( isNeedCreateStatus )
            {
                m_coroutine_task = AddMyLoginStatusToDecryptedHtml( );
                m_coroutine_flag = await m_coroutine_task;
                if ( !m_coroutine_flag )
                    return m_coroutine_flag;
                this.webProgressBar.Value++;
            }
            /* ログインステータスが既に存在していた場合 */
            else
            {
                m_coroutine_task = EditMyLoginStatusToDecryptedHtml( loginStatus: isSign );
                m_coroutine_flag = await m_coroutine_task;
                if ( !m_coroutine_flag )
                    return m_coroutine_flag;
                this.webProgressBar.Value++;
            }

            /* 18 */
            m_coroutine_task = EncryptHtmlAsync( ENCRYPT_SHIFT_SIZE_SIGN_PAGE );
            m_coroutine_flag = await m_coroutine_task;
            if ( !m_coroutine_flag )
                return m_coroutine_flag;
            this.webProgressBar.Value++;

            /* 19 */
            m_coroutine_task = DeleteHtmlAsync( m_serverSignPageDelReqAsync );
            m_coroutine_flag = await m_coroutine_task;
            if ( !m_coroutine_flag )
                return m_coroutine_flag;
            this.webProgressBar.Value++;

            /* 20 */
            m_localText1 = MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Try_Upload_Account];
            m_localText2 = MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Progress_Failed_Upload_Account];
            m_coroutine_task = MakeHtmlAsync( m_localText1 , m_localText2 , m_serverSignPage );
            m_coroutine_flag = await m_coroutine_task;
            if ( !m_coroutine_flag )
                return m_coroutine_flag;
            this.webProgressBar.Value++;

            if ( isSign )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Success_Login_pt1] +
                                            this.box_user.Text +
                                            MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Success_Login_pt2],//Success_Register_pt2] ,
                                            MessageForm.MODE_OK );
                messageForm.ShowDialog( );

                if ( Form1.isExitStatus == Form1.ExitStatus.ForceClosed )
                    Form1.isExitStatus = Form1.ExitStatus.Running;

                isUnlockLicence = true;
            }
            else
            {
                isUnlockLicence = false;
            }

            LockLoginOnShowUI( );
            return m_coroutine_flag;
        }
    }
}
