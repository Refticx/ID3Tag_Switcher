using System;

namespace trackID3TagSwitcher
{
    public class MsgList
    {
        /// <summary>
        /// メッセージの番号
        /// </summary>
        public enum STRNUM
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
            Process_Use_Error,

            Plz_Check_Artwork,
            Is_This_OK,
            Not_Found_Artwork,

            Break_ID3List,
            Plz_Make_ID3List,

            Not_Loaded_Album,
            Not_Loaded_Album_Reason,

            Success_Load_Album,
            Success_Convert_Song_ID3,
            Failed_Convert_Song_ID3,

            Found_Cannot_Use_Word,
            Cannot_Word_Replace_Another_Word,
            Select_Replace_Way,
            Target_Word,

            /* ========== ここからLogin Form用 ========== */

            Not_Connect_Network,

            Not_Get_ServerInfo,
            Not_Connect_Server,
            Irregular_Server_Setting,
            Plz_Press_Reflesh,

            Error_Get_Account_Info,
            Plz_Send_Error_To_Developer,
            Not_Get_Account_Info,
            Plz_Re_Login_After,
            Plz_Send_Case_To_Developer,

            Not_Found_Your_Account_Make_Now,
            Not_Match_Password,
            Not_Match_MachineID,

            Error_Get_MachineID,
            Plz_Tell_Case_To_Developer,
        }

        /// <summary>
        /// メッセージの内容
        /// </summary>
        public static string[] SYS_MSG_LIST =
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
            "変換中の音源ファイルを、他のアプリケーションが使用中のため、処理を続行できません。\r\nWindows Media PlayerやiTunesなど、アクセスしている可能性のあるアプリを閉じてから再度変換を行ってください。\r\n\r\n--以下Windowsエラーコード--\r\n",

            "取得されたアートワークの確認です。\r\n",
            "こちらでよろしいでしょうか？\r\n",
            "設定可能なアートワークを取得できませんでした。",

            "ID3リストファイルが破損しています。",
            "\r\nデータを読み取ることができませんでした。\r\n再度作り直してください。",

            "アルバムを読み込んでいないため、開始できません。",
            "\r\n以下の原因が考えられます。\r\n\r\n・楽曲のあるディレクトリを指定していない。\r\n・ID3リストを作っていない。\r\n・ID3リストはあるが、データが破損している。",

            "アルバムを読み込みました。",
            "曲方式の変換に成功しました。",
            "曲方式の変換に失敗しました。",

            "ファイル名に使用できない文字が含まれています。\r\n",
            "ユーザー設定により空白か全角文字に置き換えられます。\r\n",
            "「はい」の場合は空白に、「いいえ」の場合は全角に置き換え実行します。\r\n\r\n",
            "対象文字：",
            
            /* ========== ここからLogin Form用 ========== */

            "ネットワークに接続されていません。\r\n接続状況を確認し、繋ぎ直してください。",

            "サーバーの設定を確認できませんでした。\r\n",
            "サーバーに接続できませんでした。\r\n",
            "サーバー設定が間違っている可能性があります。\r\n",
            "更新ボタンを押してください。\r\nもし症状が治らない場合は、アプリケーションをダウンロードし直してください。",

            "アカウント情報の取得中にエラーが発生しました。\r\n",
            "下記エラーコードをコピーし、本アプリの制作者にご送信お願いいたします。\r\n\r\n_______________\r\n",
            "アカウント情報を取得できませんでした。\r\n",
            "ネットワーク状況による可能性もあるため、しばらくしてから再度ログインを試してみてください。\r\n",
            "このエラーが多発するようであれば、本アプリの制作者にお問合せをお願いいたします。\r\n",

            "あなたのアカウントはまだ登録されていません。\r\n登録しますか？\r\n",
            "パスワードが一致しません。\r\n再度パスワードを入力し直してください。\r\n",
            "アカウントを登録したPCと一致しません。\r\n本アプリのオンラインサービスは複数の機器での利用はできません。\r\n本アプリの制作者に使用するPCの変更のお問合せをお願いいたします。",

            "使用中のPC本体の情報の取得に失敗しました。\r\n",
            "このエラーが発生したことを本アプリの制作者にお問合せお願いいたします。\r\n",
        };
    }
}