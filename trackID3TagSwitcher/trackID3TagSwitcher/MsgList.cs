﻿using System;

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

            Not_Load_Album_Number,
            Not_Found_Wave_Link,

            /* ========== ここからLogin Form用 ========== */

            Not_Connect_Network,

            Not_Get_ServerInfo,
            Not_Connect_Server,
            Irregular_Server_Setting,
            Plz_Re_Download_App,

            Error_Get_Account_Info,
            Plz_Send_Error_To_Developer,
            Not_Get_Account_Info,
            Plz_Re_Login_After,
            Plz_Send_Case_To_Developer,

            Not_Found_Your_Account_Make_Now,
            Not_Match_Password,
            Not_Match_MachineID,
            Success_Login_pt1,
            Success_Login_pt2,
            Already_Login_pt1,
            Already_Login_pt2,
            Failed_Login,
            Success_Register_pt2,
            Cannot_Use_Username,
            Cannot_Use_Password,
            Success_Login_noticeBar,
            Failed_Login_noticeBar,
            Failed_Login_Already_Loggedin,

            Error_Get_MachineID,
            Plz_Tell_Case_To_Developer,

            Progress_Try_Clear_Args,
            Progress_Failed_Clear_Args,
            Progress_Try_Server_Connect,
            Progress_Failed_Server_Connect,
            Progress_Try_Get_Server_List,
            Progress_Failed_Get_Server_List,
            Progress_Failed_Find_Server_List,
            Progress_Try_Get_Data_List,
            Progress_Failed_Get_Data_List,
            Progress_Try_Decrypt_Data_List,
            Progress_Failed_Decrypt_Data_List,
            Progress_Try_Find_Account,
            Progress_Failed_Find_Account,
            Progress_Failed_Account_Password_Mismatch,
            Progress_Failed_Account_Device_Mismatch,
            Progress_Try_Add_My_Info_To_Data_List,
            Progress_Failed_Add_My_Info_To_Data_List,
            Progress_Try_Encrypt_Data_List,
            Progress_Failed_Encrypt_Data_List,
            Progress_Try_Delete_Server,
            Progress_Failed_Delete_Server,
            Progress_Try_Upload_Account,
            Progress_Failed_Upload_Account,
            Progress_Try_Find_Login_Status,
            Progress_Failed_Login_Status_Already_Hacked,
            Progress_Failed_Login_Status,
            Progress_Try_Add_My_Login_To_Data_List,
            Progress_Failed_Add_My_Login_To_Data_List,
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

            "想定外のエラーが発生しました。\r\n今後の本ソフトウェア安定性向上のため、製作者にスクリーンショットを添えてご報告お願いします。\r\n\r\n_______________\r\n",
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

            "アルバムナンバーを読み込めていません。\r\n再度アルバムをロードし直してください。\r\n",
            "チェックリンク先が指定されていないため、この機能は利用できません。",
            
            /* ========== ここからLogin Form用 ========== */

            "ネットワークに接続されていません。\r\n接続状況を確認し、繋ぎ直してください。",

            "サーバーの設定を確認できませんでした。\r\n",
            "サーバーに接続できませんでした。\r\n",
            "サーバー設定が間違っている可能性があります。\r\n",
            "もし症状が治らない場合は、アプリケーションをダウンロードし直してください。\r\n",

            "アカウント情報の取得中にエラーが発生しました。\r\n",
            "下記エラーコードをコピーし、本アプリの制作者にご送信お願いいたします。\r\n\r\n_______________\r\n",
            "アカウント情報を取得できませんでした。\r\n",
            "ネットワーク状況による可能性もあるため、しばらくしてから再度ログインを試してみてください。\r\n",
            "このエラーが多発するようであれば、本アプリの制作者にお問合せをお願いいたします。\r\n",

            "あなたのアカウントはまだ登録されていません。\r\n登録しますか？\r\n",
            "パスワードが一致しません。\r\n再度パスワードを入力し直してください。\r\n",
            "アカウントを登録したPCと一致しません。\r\n本アプリのオンラインサービスは複数の機器での利用はできません。\r\n本アプリの制作者に使用するPCの変更のお問合せをお願いいたします。\r\n",
            "アカウント【　",
            "　】にログインしました。\r\n",
            "既にアカウント【　",
            "　】にログイン済みです。\r\n",
            "アカウントのログインに失敗しました\r\n",
            "　】の登録が完了しました。\r\n",
            "ユーザー名に使用できない文字が含まれています。\r\n",
            "パスワードに使用できない文字が含まれています。\r\n",
            "ログインしました\r\n",
            "ログインに失敗しました\r\n",
            "このアカウントのサーバー上のログイン状態が一致しません。\r\n" + 
                "アプリケーションの強制終了の履歴が確認できなかったため、アカウントのハッキングを受けた可能性があります。\r\n" +
                "至急開発者にご連絡お願い致します。\r\n\r\n" + 
                "_______________\r\n" +
                "【連絡時に必要な情報】\r\n" + 
                "・ユーザー名\r\n",

            "使用中のPC本体の情報の取得に失敗しました。\r\n",
            "このエラーが発生したことを本アプリの制作者にお問合せお願いいたします。\r\n",

            "キャッシュを削除中…",
            "キャッシュの削除に失敗",
            "サーバーに接続中…",
            "サーバーの接続に失敗",
            "サーバーデータを取得中…",
            "サーバーデータの取得に失敗",
            "サーバーデータがありません",
            "データリストを取得中…",
            "データリストの取得に失敗",
            "データを復号化しています…",
            "データの復号化に失敗",
            "アカウント情報を照会中…",
            "アカウント情報の照会に失敗",
            "パスワードが一致しません",
            "登録したデバイスと一致しません",
            "アカウント情報を挿入中…",
            "アカウント情報の挿入に失敗",
            "データを暗号化しています…",
            "データの暗号化に失敗",
            "サーバーへ更新リクエストを送信中…",
            "サーバーのリクエスト送信に失敗",
            "アカウント情報をアップロード中…",
            "アカウント情報の反映に失敗",
            "ログイン状態を取得中…",
            "ログイン情報が一致しません",
            "ログイン状態の取得に失敗",
            "ログイン情報を挿入中…",
            "ログイン情報の挿入に失敗",
        };
    }
}