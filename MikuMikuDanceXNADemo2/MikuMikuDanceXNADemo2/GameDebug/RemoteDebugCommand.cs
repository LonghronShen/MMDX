#if XBOX360 || WINDOWS

#region Using ステートメント

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;

#endregion

namespace DebugSample
{
    /// <summary>
    /// リモートデバッグコマンドコンポーネント
    /// </summary>
    /// <remarks>
    /// オリジナルのデバッグコマンドはXbox 360にキーボードを接続することで使えたが、
    /// キーボードが二つない場合や、Xbox 360本体が開発しているPCよりも遠くにある場合に
    /// 不便なので、NetworkSessionを使ってPCサイドからリモート接続するのが、
    /// このコンポーネントである。
    /// 
    /// Xbox 360、Windowsで同じゲームを走らせておいた状態で、"remote"コマンドを
    /// 使うと、WindowsからXbox 360へと接続する
    /// 
    /// 接続が完了するとWindows側でタイプされたコマンドはXbox 360側に送られ実行される
    /// 実行結果はXbox 360、Windowsの両方で表示される
    /// 
    /// "quit"コマンドでリモート接続を終了する
    /// </remarks>
    public class RemoteDebugCommand : GameComponent,
        IDebugCommandExecutioner, IDebugEchoListner
    {
        #region プロパティ

        /// <summary>
        /// 接続に使うNetworkSessionの取得と設定
        /// </summary>
        public NetworkSession NetworkSession { get; set; }

        /// <summary>
        /// このコンポーネントがNetworkSessionのオーナーか？
        /// </summary>
        public bool OwnsNetworkSession { get; private set; }

        #endregion

        #region 定数宣言

        const string StartPacketHeader = "RmtStart";
        const string ExecutePacketHeader = "RmtCmd";
        const string EchoPacketHeader = "RmtEcho";
        const string ErrorPacketHeader = "RmtErr";
        const string WarningPacketHeader = "RmtWrn";
        const string QuitPacketHeader = "RmtQuit";

        #endregion

        #region フィールド

        IDebugCommandHost commandHost;

#if WINDOWS
        bool IsHost = false;
#else
        bool IsHost = true;
#endif

        Regex packetRe = new Regex(@"\$(?<header>[^$]+)\$:(?<text>.+)");

        PacketReader packetReader = new PacketReader();
        PacketWriter packetWriter = new PacketWriter();

        IAsyncResult asyncResult;

        enum ConnectionPahse
        {
            None,
            EnsureSignedIn,
            FindSessions,
            Joining,
        }

        ConnectionPahse phase = ConnectionPahse.None;

        #endregion

        #region 初期化

        public RemoteDebugCommand(Game game)
            : base(game)
        {
            commandHost =
                game.Services.GetService(typeof(IDebugCommandHost)) as IDebugCommandHost;

            if (!IsHost)
            {
                commandHost.RegisterCommand("remote", "Start remote command",
                                                ExecuteRemoteCommand);
            }
        }

        public override void Initialize()
        {
            if (IsHost)
            {
                commandHost.RegisterEchoListner(this);

                // Create network session if NetworkSession is not setted.
                if (NetworkSession == null)
                {
                    GamerServicesDispatcher.WindowHandle = Game.Window.Handle;
                    GamerServicesDispatcher.Initialize(Game.Services);
                    NetworkSession =
                        NetworkSession.Create(NetworkSessionType.SystemLink, 1, 2);

                    OwnsNetworkSession = true;
                }
            }

            base.Initialize();
        }

        #endregion

        /// <summary>
        /// パケット文字の処理
        /// </summary>
        /// <remarks>ゲーム側でNetworkSessionを保持している場合に受け取ったリモート
        /// デバッグコマンド用のパケットはこのメソッドで処理する
        /// </remarks>
        /// <param name="packetString"></param>
        /// <returns>指定されたパケット文字が処理されたか？</returns>
        public bool ProcessRecievedPacket(string packetString)
        {
            bool processed = false;

            Match mc = packetRe.Match(packetString);
            if (mc.Success)
            {
                string packetHeader = mc.Groups["header"].Value;
                string text = mc.Groups["text"].Value;
                switch (packetHeader)
                {
                    case ExecutePacketHeader:
                        commandHost.ExecuteCommand(text);
                        processed = true;
                        break;
                    case EchoPacketHeader:
                        commandHost.Echo(text);
                        processed = true;
                        break;
                    case ErrorPacketHeader:
                        commandHost.EchoError(text);
                        processed = true;
                        break;
                    case WarningPacketHeader:
                        commandHost.EchoWarning(text);
                        processed = true;
                        break;
                    case StartPacketHeader:
                        ConnectedToRemote();
                        commandHost.Echo(text);
                        processed = true;
                        break;
                    case QuitPacketHeader:
                        commandHost.Echo(text);
                        DisconnectedFromRemote();
                        processed = true;
                        break;
                }
            }

            return processed;
        }

        #region 実装

        /// <summary>
        /// 更新
        /// </summary>
        public override void Update(GameTime gameTime)
        {
            // 複数のフェーズ処理
            switch (phase)
            {
                case ConnectionPahse.EnsureSignedIn:
                    GamerServicesDispatcher.Update();
                    break;

                case ConnectionPahse.FindSessions:
                    GamerServicesDispatcher.Update();
                    if (asyncResult.IsCompleted)
                    {
                        AvailableNetworkSessionCollection sessions =
                            NetworkSession.EndFind(asyncResult);

                        if (sessions.Count > 0)
                        {
                            asyncResult = NetworkSession.BeginJoin(sessions[0],
                                                                    null, null);
                            commandHost.EchoError("Connecting to the host...");
                            phase = ConnectionPahse.Joining;
                        }
                        else
                        {
                            commandHost.EchoError("Couldn't find a session.");
                            phase = ConnectionPahse.None;
                        }
                    }
                    break;
                case ConnectionPahse.Joining:
                    GamerServicesDispatcher.Update();
                    if (asyncResult.IsCompleted)
                    {
                        NetworkSession = NetworkSession.EndJoin(asyncResult);
                        NetworkSession.SessionEnded +=
                            new EventHandler<NetworkSessionEndedEventArgs>(
                                                            NetworkSession_SessionEnded);

                        OwnsNetworkSession = true;
                        commandHost.EchoError("Connected to the host.");
                        phase = ConnectionPahse.None;
                        asyncResult = null;

                        ConnectedToRemote();
                    }
                    break;
            }

            // NetworkSessionの更新
            if (OwnsNetworkSession)
            {
                GamerServicesDispatcher.Update();
                NetworkSession.Update();

                if (NetworkSession != null)
                {
                    // 受け取ったパケットの処理
                    foreach (LocalNetworkGamer gamer in NetworkSession.LocalGamers)
                    {
                        while (gamer.IsDataAvailable)
                        {
                            NetworkGamer sender;
                            gamer.ReceiveData(packetReader, out sender);
                            if (!sender.IsLocal)
                                ProcessRecievedPacket(packetReader.ReadString());
                        }
                    }
                }
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// デバッグコマンドパケットを送信する
        /// </summary>
        void SendPacket(string header, string text)
        {
            if (NetworkSession != null)
            {
                packetWriter.Write("$" + header + "$:" + text);
                NetworkSession.LocalGamers[0].SendData(packetWriter,
                    SendDataOptions.ReliableInOrder);
            }
        }

        /// <summary>
        /// リモートデバッグコマンドの開始
        /// </summary>
        void ConnectedToRemote()
        {
            DebugCommandUI commandUI = commandHost as DebugCommandUI;

            if (IsHost)
            {
                if (commandUI != null)
                    commandUI.Prompt = "[Host]>";
            }
            else
            {
                if (commandUI != null)
                    commandUI.Prompt = "[Client]>";

                commandHost.PushExecutioner(this);

                SendPacket(StartPacketHeader, "Remote Debug Command Started!!");
            }

            commandHost.RegisterCommand("quit", "Quit from remote command",
                                            ExecuteQuitCommand);
        }

        /// <summary>
        /// リモートデバッグコマンドの終了
        /// </summary>
        void DisconnectedFromRemote()
        {
            DebugCommandUI commandUI = commandHost as DebugCommandUI;
            if (commandUI != null)
                commandUI.Prompt = DebugCommandUI.DefaultPrompt;

            commandHost.UnregisterCommand("quit");

            if (!IsHost)
            {
                commandHost.PopExecutioner();

                if (OwnsNetworkSession)
                {
                    NetworkSession.Dispose();
                    NetworkSession = null;
                    OwnsNetworkSession = false;
                }
            }
        }

        #region デバッグコマンドの実装

        private void ExecuteRemoteCommand(IDebugCommandHost host, string command,
                                                            IList<string> arguments)
        {
            if (NetworkSession == null)
            {
                try
                {
                    GamerServicesDispatcher.WindowHandle = Game.Window.Handle;
                    GamerServicesDispatcher.Initialize(Game.Services);
                }
                catch { }

                if (SignedInGamer.SignedInGamers.Count > 0)
                {
                    commandHost.Echo("Finding available sessions...");

                    asyncResult = NetworkSession.BeginFind(
                            NetworkSessionType.SystemLink, 1, null, null, null);

                    phase = ConnectionPahse.FindSessions;
                }
                else
                {
                    host.Echo("Please signed in.");
                    phase = ConnectionPahse.EnsureSignedIn;
                }
            }
            else
            {
                ConnectedToRemote();
            }
        }

        private void ExecuteQuitCommand(IDebugCommandHost host, string command,
                                                            IList<string> arguments)
        {
            SendPacket(QuitPacketHeader, "End Remote Debug Command.");
            DisconnectedFromRemote();
        }

        #endregion

        #region IDebugCommandExecutionerとIDebugEchoListnerの実装

        public void ExecuteCommand(string command)
        {
            SendPacket(ExecutePacketHeader, command);
        }

        public void Echo(DebugCommandMessage messageType, string text)
        {
            switch (messageType)
            {
                case DebugCommandMessage.Standard:
                    SendPacket(EchoPacketHeader, text);
                    break;
                case DebugCommandMessage.Warning:
                    SendPacket(WarningPacketHeader, text);
                    break;
                case DebugCommandMessage.Error:
                    SendPacket(ErrorPacketHeader, text);
                    break;
            }
        }

        #endregion

        /// <summary>
        /// ホストが消失したときの処理
        /// </summary>
        void NetworkSession_SessionEnded(object sender, NetworkSessionEndedEventArgs e)
        {
            DisconnectedFromRemote();
            commandHost.EchoWarning("Disconnected from the Host.");
        }

        #endregion
    }
}

#endif