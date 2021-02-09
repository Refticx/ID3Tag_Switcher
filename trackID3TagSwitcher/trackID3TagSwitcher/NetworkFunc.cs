using System;
using System.Threading.Tasks;

namespace trackID3TagSwitcher
{
    public static class NetworkFunc
    {
        /// <summary>
        /// ネットワークに接続されているかチェックし、接続されていなかったらアプリを閉じる
        /// </summary>
        private static MessageForm messageForm = new MessageForm( );    /* ダイアログ用フォームを作成しておく */
        private static string m_localText;

        /// <summary>
        /// ネットワークに接続されているかチェックし、接続されていなかったらメッセージを表示する
        /// </summary>
        public static bool CheckNetworkWithMessage( )
        {
            if ( !System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable( ) )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Not_Connect_Network] , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return false;
            }
            return true;
        }


        /// <summary>
        /// 取得したテキストデータを復号化する
        /// </summary>
        public static async Task<string> DecryptHtmlAsyncWithMessage( string m_str , int shift )
        {
            try
            {
                m_localText = String.Empty;
                for ( int i = 0 ; i < m_str.Length ; i++ )
                {
                    m_localText += (char)( (int)m_str[i] - shift );
                    await Task.Delay( 1 );
                }
                return m_localText;
            }
            catch ( Exception ex )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Irregular_Error] + ex.ToString( ) , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return String.Empty;
            }
        }

        /// <summary>
        /// 取得したテキストデータを暗号化する
        /// </summary>
        public static async Task<string> EncryptHtmlAsyncWithMessage( string m_str , int shift )
        {
            try
            {
                m_localText = String.Empty;
                for ( int i = 0 ; i < m_str.Length ; i++ )
                {
                    m_localText += (char)( (int)m_str[i] + shift );
                    await Task.Delay( 1 );
                }
                return m_localText;
            }
            catch ( Exception ex )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Irregular_Error] + ex.ToString( ) , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return String.Empty;
            }
        }


        /// <summary>
        /// 取得したテキストデータを復号化する
        /// </summary>
        public static string DecryptHtmlWithMessage( string m_str , int shift )
        {
            try
            {
                m_localText = String.Empty;
                for ( int i = 0 ; i < m_str.Length ; i++ )
                {
                    m_localText += (char)( (int)m_str[i] - shift );
                }
                return m_localText;
            }
            catch ( Exception ex )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Irregular_Error] + ex.ToString( ) , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return String.Empty;
            }
        }

        /// <summary>
        /// 取得したテキストデータを暗号化する
        /// </summary>
        public static string EncryptHtmlWithMessage( string m_str , int shift )
        {
            try
            {
                m_localText = String.Empty;
                for ( int i = 0 ; i < m_str.Length ; i++ )
                {
                    m_localText += (char)( (int)m_str[i] + shift );
                }
                return m_localText;
            }
            catch ( Exception ex )
            {
                messageForm.SetFormState( MsgList.SYS_MSG_LIST[(int)MsgList.STRNUM.Irregular_Error] + ex.ToString( ) , MessageForm.MODE_OK );
                messageForm.ShowDialog( );
                return String.Empty;
            }
        }
    }
}