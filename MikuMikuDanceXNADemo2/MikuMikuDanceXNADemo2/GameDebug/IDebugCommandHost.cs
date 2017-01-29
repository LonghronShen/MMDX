#region Using ステートメント

using System.Collections.Generic;

#endregion

namespace DebugSample
{
    /// <summary>
    /// デバッグコマンドのメッセージタイプ
    /// </summary>
    public enum DebugCommandMessage
    {
        // 標準出力
        Standard = 1,

        // エラー出力
        Error = 2,

        // 警告出力
        Warning = 3
    }

    /// <summary>
    /// デバッグコマンド実行用のデリゲーション
    /// </summary>
    /// <param name="host">実行を発行したホスト</param>
    /// <param name="command">コマンド</param>
    /// <param name="arguments">コマンドの引数</param>
    public delegate void DebugCommandExecute(IDebugCommandHost host, string command,
                                                            IList<string> arguments);

    /// <summary>
    /// デバッグコマンド実行者インターフェース
    /// </summary>
    public interface IDebugCommandExecutioner
    {
        /// <summary>
        /// コマンドの実行
        /// </summary>
        /// <param name="command">実行コマンド</param>
        void ExecuteCommand(string command);
    }

    /// <summary>
    /// デバッグコマンドメッセージのリスナー用インターフェース
    /// </summary>
    public interface IDebugEchoListner
    {
        /// <summary>
        /// メッセージの出力
        /// </summary>
        /// <param name="messageType">メッセージの種類</param>
        /// <param name="text">メッセージ</param>
        void Echo(DebugCommandMessage messageType, string text);
    }

    /// <summary>
    /// デバッグコマンドホスト用のインターフェース
    /// </summary>
    public interface IDebugCommandHost : IDebugEchoListner, IDebugCommandExecutioner
    {
        /// <summary>
        /// コマンドの登録
        /// </summary>
        /// <param name="command">コマンド</param>
        /// <param name="description">コマンドの説明</param>
        /// <param name="callback">実行時のデリゲーション</param>
        void RegisterCommand(string command, string description,
                                                        DebugCommandExecute callback);

        /// <summary>
        /// コマンドの登録解除
        /// </summary>
        /// <param name="command">コマンド</param>
        void UnregisterCommand(string command);


        /// <summary>
        /// 標準メッセージ出力
        /// </summary>
        /// <param name="text"></param>
        void Echo(string text);

        /// <summary>
        /// 警告メッセージ出力
        /// </summary>
        /// <param name="text"></param>
        void EchoWarning(string text);

        /// <summary>
        /// エラーメッセージ出力
        /// </summary>
        /// <param name="text"></param>
        void EchoError(string text);

        /// <summary>
        /// メッセージ出力リスナーの登録
        /// </summary>
        /// <param name="listner"></param>
        void RegisterEchoListner(IDebugEchoListner listner);

        /// <summary>
        /// メッセージ出力リスナーの登録解除
        /// </summary>
        /// <param name="listner"></param>
        void UnregisterEchoListner(IDebugEchoListner listner);

        /// <summary>
        /// コマンド実行者の追加
        /// </summary>
        void PushExecutioner(IDebugCommandExecutioner executioner);

        /// <summary>
        /// コマンド実行者の削除
        /// </summary>
        void PopExecutioner();
    }

}
