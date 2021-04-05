using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace trackID3TagSwitcher
{
    public class Defines
    {
        public const string APP_DOWNLOAD_URL = "https://chaoticbootstrage.wixsite.com/scene/id3-tool";     /* アプリ更新があった時に開かせるURL */
        public const string VERSION_INFO_URL = "https://chaoticbootstrage.wixsite.com/scene/idtool-ver";   /* アプリ更新があるかを確認するページ */
        public const string ALBUM_LICENSE_URL = "https://chaoticbootstrage.wixsite.com/scene/idtool-ver";   /* 変換するアルバムのサウンドレイヤーの権限を確認するページ */
        public const string CURRENT_VERSION = "1.20";                                                       /* 現在のアプリのバージョン */

        /* 文字列 */
        public const string CONFIG_JACKET_NAME = "Jacket_Name";
        public const string CONFIG_TRACK_FOLDER = "Track_Folder";
        public const string CONFIG_IS_DOT = "Is_Dot";
        public const string CONFIG_AUTO_SEARCH = "Is_AutoSearch";
        public const string CONFIG_REPLACE_REGISTER_WORD = "Is_ReplaceRegisterWord";
        public const string CONFIG_APP_EXIT = "App_Exit";
        public const string CONFIG_CONVERT_EXT = "Convert_Ext";
        public const string CONFIG_ACC_DEC = "AccDec";
        public const string ACC_SERVER_USERNAME = "s_user";
        public const string ACC_SERVER_PASSWORD = "s_pass";
        public const string ACC_SERVER_KEY = "s_key";
        public const string ACC_SERVER_ACOUNT_LIST = "s_acctitle";
        public const string ACC_SERVER_LOGIN_LIST = "s_signtitle";
        public const string ACC_USERNAME = "l_user";
        public const string ACC_PASSWORD = "l_pass";
        public const string TRACKINFO_ID3_TYPE = "Is_Type";
        public const string TRACKINFO_WAVE_TYPE = "Wave_Type";
        public const string TRACKINFO_ALBUM_LABEL = "Album_Label";
        public const string TRACKINFO_ALBUM_NAME = "Album_Name";
        public const string TRACKINFO_ALBUM_NUMBER = "Album_Number";
        public const string TRACKINFO_WAVE_LINK = "Wave_Link";

        /* 配列参照時の要素数 */
        public const int ARN_NAME = 0;
        public const int FYS_NAME = 1;
        public const int SUBTITLE = 2;
        public const int ARTIST = 3;
        public const int COMMENT = 4;
        public const int CREATOR = 5;
        public const int BPM = 6;
        public const int GENRE = 7;
        public const int CUSTOM = 8;

        public const int MAX_TRACK = 20;   /* 一括変更できる最大曲数、ここを変更すると曲情報画面の縦ボックス数を変更できる */
        public const int MAX_TAG = 9;      /* 設定する項目数、ここを変更すると曲情報画面の横ボックス数を変更できる */

        /* デザイナーサイズ */
        public const int EXIT_Y = 3;
        public const int DESIGN_DEF_X = 0;
        public const int DESIGN_DEF_Y = 0;
        public const int MAIN_AREA_Y = 32;
        public const int MAIN_AREA_HIDE_X = -( APP_WIDTH + 20 );
        public const int SUB_AREA_HIDE_X = ( APP_WIDTH + 10 );
        public const int LINE_HEIGHT = 2;
        public const int CURR_INFO_AREA_Y = ( DESIGN_DEF_Y + 126 );
        public const int CURR_ID3_AREA_Y = ( CURR_INFO_AREA_Y + 126 );
        public const int EXEC_CONV_AREA_Y = ( CURR_ID3_AREA_Y + 126 );

        /* 設定パネル */
        public const int SETTING_AREA_HEIGHT = 400;
        public const int SETTING_AREA_HIDE_Y = ( DESIGN_DEF_Y - ( SETTING_AREA_HEIGHT + 20 ) );

        public const int APP_WIDTH = 550;
        public const int APP_HEIGHT = ( APP_WIDTH + 50 );
        public const int FOOTER_Y = ( APP_HEIGHT - 20 );
        public const int FOOTER_LINE_Y = ( FOOTER_Y - 2 );
        public const int HEADERT_BTN_EXIT_X = ( APP_WIDTH - 28 );
        public const int HEADERT_BTN_ACCOUNT_X = ( HEADERT_BTN_EXIT_X - 40 );
        public const int HEADERT_BTN_SETTING_X = ( HEADERT_BTN_ACCOUNT_X - 40 );
        public const int HEADERT_BTN_EXIT_MOVE_X = ( APP_WIDTH - 90 );
        public const int HEADERT_BTN_ACCOUNT_MOVE_X = ( HEADERT_BTN_EXIT_MOVE_X - 40 );
        public const int HEADERT_BTN_SETTING_MOVE_X = ( HEADERT_BTN_ACCOUNT_MOVE_X - 40 );

        public const int APP_BIG_WIDTH = 760;
        public const int APP_BIG_HEIGHT = ( APP_BIG_WIDTH - 10 );
        public const int FOOTER_BIG_Y = ( APP_BIG_HEIGHT - 20 );
        public const int FOOTER_LINE_BIG_Y = ( FOOTER_BIG_Y - 2 );
        public const int HEADERT_BTN_EXIT_BIG_X = ( APP_BIG_WIDTH - 28 );
        public const int HEADERT_BTN_ACCOUNT_BIG_X = ( HEADERT_BTN_EXIT_BIG_X - 40 );
        public const int HEADERT_BTN_SETTING_BIG_X = ( HEADERT_BTN_ACCOUNT_BIG_X - 40 );
        public const int HEADERT_BTN_EXIT_MOVE_BIG_X = ( APP_BIG_WIDTH - 90 );
        public const int HEADERT_BTN_ACCOUNT_MOVE_BIG_X = ( HEADERT_BTN_EXIT_MOVE_BIG_X - 40 );
        public const int HEADERT_BTN_SETTING_MOVE_BIG_X = ( HEADERT_BTN_ACCOUNT_MOVE_BIG_X - 40 );

        public enum ConvertExt
        {
            flac,
            mp3,
            wav,
        }

        /* アプリケーション終了状態 */
        public enum ExitStatus
        {
            Launching,
            Running,
            Closing,
            ForceClosed,
        }

        /* アルバムライセンサー */
        public enum AlbumLicense
        {
            Non_License,
            Need_License,
        }


        /* プログレスバー設定 */
        /// <summary>
        /// プログレスバーに割り当てる、async処理内のファンクション数
        /// </summary>
        public const int SERVER_FUNC_NUM = 20;

        /* ==================== サーバー設定 ==================== */

        /// <summary>
        /// サーバーから取得するページの最大件数
        /// </summary>
        public const int SERVER_MAX_PAGE = 3;

        /// <summary>
        /// アカウントリストのページの暗号化の際の移動オフセット量
        /// </summary>
        public const int ENCRYPT_SHIFT_SIZE_ACC_PAGE = 2;

        /// <summary>
        /// サインインリストのページの暗号化の際の移動オフセット量
        /// </summary>
        public const int ENCRYPT_SHIFT_SIZE_SIGN_PAGE = 3;

        /// <summary>
        /// アカウントコンフィグファイルの暗号化の際の移動オフセット量
        /// </summary>
        public static int Encrypt_Shift_Size_Config_File = 5;

        /// <summary>
        /// サーバーアカウントデータの暗号化の際の移動オフセット量
        /// </summary>
        public const int ENCRYPT_SHIFT_SIZE_SERVER_ACC = 6;

        /* 各関数処理用変数 */
        public const string START_TXT = "<textarea class=\"textarea\">";
        public const string END_TXT = "</textarea>";
        public const string SPACE_PASS = "[+@]";
        public const string SPACE_MCNID = "[+=]";
        public const string SPACE_UNQID = "[+!]";
        public const string SPACE_SIGN = "[+*]";
        public const string STATUS_SIGNIN = "1";
        public const string STATUS_SIGNOUT = "0";
        public const string START_TXT_S_USER = "<s_user>";
        public const string END_TXT_S_USER = "</s_user>";
        public const string START_TXT_S_PASS = "<s_pass>";
        public const string END_TXT_S_PASS = "</s_pass>";
        public const string START_TXT_S_KEY = "<s_key>";
        public const string END_TXT_S_KEY = "</s_key>";
        public const string START_TXT_S_ACC_TITLE = "<s_acctitle>";
        public const string END_TXT_S_ACC_TITLE = "</s_acctitle>";
        public const string START_TXT_S_SIGN_TITLE = "<s_signtitle>";
        public const string END_TXT_S_SIGN_TITLE = "</s_signtitle>";
    }
}
