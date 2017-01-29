#region Using ステートメント

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace DebugSample
{
    /// <summary>
    /// デバッグ用コマンドウィンドウクラス
    /// </summary>
    /// <remarks>
    /// ゲーム内で動作するデバックコマンドウィンドウUI部分
    /// キーボード入力によってコマンドを入力、実行することができる。
    /// Xbox 360でもUSBキーボードを接続することで動作可能。
    /// 
    /// 使用方法:
    /// 1)このコンポーネントをゲームに追加。
    /// 2)RegisterCommandメソッドを使ってコマンドを登録する
    /// 3)Tabキーでデバッグウィンドウの開閉しコマンド入力
    /// </remarks>
    public class DebugCommandUI : DrawableGameComponent, IDebugCommandHost
    {
        #region 定数宣言

        /// <summary>
        /// 最大行数
        /// </summary>
        const int MaxLineCount = 20;

        /// <summary>
        /// コマンドヒストリ数
        /// </summary>
        const int MaxCommandHistory = 32;

        /// <summary>
        /// カーソル文字。ここではUnicodeのBlock Eleemntsからカーソルっぽいものを使用
        /// http://www.unicode.org/charts/PDF/U2580.pdf
        /// </summary>
        const string Cursor = "\u2582";

        /// <summary>
        /// デフォルトのコマンドプロンプト文字列
        /// </summary>
        public const string DefaultPrompt = "CMD>";

        #endregion

        #region プロパティ

        /// <summary>
        /// コマンドプロンプト文字列
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// キー入力待機状態か
        /// </summary>
        public bool Focused { get { return state != State.Closed; } }

        #endregion

        #region フィールド

        // コマンドウィンドウのステート
        enum State
        {
            Closed,     // 閉じている
            Opening,    // 開いている途中
            Opened,     // 開いている(コマンド入力待機中)
            Closing     // 閉じている途中
        }

        /// <summary>
        /// コマンド実行用情報格納用のクラス
        /// </summary>
        class CommandInfo
        {
            public CommandInfo(
                string command, string description, DebugCommandExecute callback)
            {
                this.command = command;
                this.description = description;
                this.callback = callback;
            }

            // コマンド名
            public string command;

            // コマンド詳細
            public string description;

            // コマンド実行用のデリゲート
            public DebugCommandExecute callback;
        }

        // デバッグマネージャーへの参照
        private DebugManager debugManager;

        // 現在のステート
        private State state = State.Closed;

        // ステート移行用のタイマー
        private float stateTransition;

        // 登録されているEchoリスナー
        List<IDebugEchoListner> listenrs = new List<IDebugEchoListner>();

        // 登録されているコマンド実行者
        Stack<IDebugCommandExecutioner> executioners = new Stack<IDebugCommandExecutioner>();

        // 登録されているコマンド
        private Dictionary<string, CommandInfo> commandTable =
                                                new Dictionary<string, CommandInfo>();

        // 現在入力中のコマンドライン文字列と、カーソル位置
        private string commandLine = String.Empty;
        private int cursorIndex = 0;

        // コマンドライン表示文字列
        private Queue<string> lines = new Queue<string>();

        // コマンド履歴用バッファ
        private List<string> commandHistory = new List<string>();

        // 現在選択されている履歴インデックス
        private int commandHistoryIndex;

        #region キーボード入力処理用の変数群

        //　前フレームのキーボードステート
        private KeyboardState prevKeyState;

        //　最後に押されたキー
        private Keys pressedKey;

        //　キーリピートタイマー
        private float keyRepeatTimer;

        // 最初のキー押下時のリピート時間(秒)
        private float keyRepeatStartDuration = 0.3f;

        // ２回目以降のキーリピート時間(秒)
        private float keyRepeatDuration = 0.03f;

        #endregion

        #endregion

        #region 初期化

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DebugCommandUI(Game game)
            : base(game)
        {
            Prompt = DefaultPrompt;

            // サービスとして追加する
            Game.Services.AddService(typeof(IDebugCommandHost), this);

            // 基本コマンドの追加

            // ヘルプコマンド
            // 登録されているコマンド情報の表示
            RegisterCommand("help", "Show Command helps",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                int maxLen = 0;
                foreach (CommandInfo cmd in commandTable.Values)
                    maxLen = Math.Max(maxLen, cmd.command.Length);

                string fmt = String.Format("{{0,-{0}}}    {{1}}", maxLen);

                foreach (CommandInfo cmd in commandTable.Values)
                {
                    Echo(String.Format(fmt, cmd.command, cmd.description));
                }
            });

            // クリアスクリーン
            // コマンド画面クリア
            RegisterCommand("cls", "Clear Screen",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                lines.Clear();
            });

            // Echoコマンド
            RegisterCommand("echo", "Display Messages",
            delegate(IDebugCommandHost host, string command, IList<string> args)
            {
                Echo(command.Substring(5));
            });
        }

        /// <summary>
        /// コンポーネントの初期化
        /// </summary>
        public override void Initialize()
        {
            debugManager =
                Game.Services.GetService(typeof(DebugManager)) as DebugManager;

            if (debugManager == null)
                throw new InvalidOperationException("DebugManagerが見つかりません。");

            base.Initialize();
        }

        #endregion

        #region IDebugCommandHostインターフェースの実装

        public void RegisterCommand(
            string command, string description, DebugCommandExecute callback)
        {
            string lowerCommand = command.ToLower();
            if (commandTable.ContainsKey(lowerCommand))
            {
                throw new InvalidOperationException(
                    String.Format("{0}は既に登録されています", command));
            }

            commandTable.Add(
                lowerCommand, new CommandInfo(command, description, callback));
        }

        public void UnregisterCommand(string command)
        {
            string lowerCommand = command.ToLower();
            if (!commandTable.ContainsKey(lowerCommand))
            {
                throw new InvalidOperationException(
                    String.Format("{0}は登録されていません", command));
            }

            commandTable.Remove(command);
        }

        public void ExecuteCommand(string command)
        {
            // 他のコマンド実行者が登録されている場合は、最新の登録者にコマンドを
            // 実行させる。
            if (executioners.Count != 0)
            {
                executioners.Peek().ExecuteCommand(command);
                return;
            }

            // コマンドの実行
            char[] spaceChars = new char[] { ' ' };

            Echo(Prompt + command);

            command = command.TrimStart(spaceChars);

            List<string> args = new List<string>(command.Split(spaceChars));
            string cmdText = args[0];
            args.RemoveAt(0);

            CommandInfo cmd;
            if (commandTable.TryGetValue(cmdText.ToLower(), out cmd))
            {
                try
                {
                    // 登録されているコマンドのデリゲートを呼び出す
                    cmd.callback(this, command, args);
                }
                catch (Exception e)
                {
                    // 例外がコマンド実行中に発生
                    EchoError("Unhandled Exception occured");

                    string[] lines = e.Message.Split(new char[] { '\n' });
                    foreach (string line in lines)
                        EchoError(line);
                }
            }
            else
            {
                Echo("Unknown Command");
            }

            // コマンドヒストリに追加する
            commandHistory.Add(command);
            while (commandHistory.Count > MaxCommandHistory)
                commandHistory.RemoveAt(0);

            commandHistoryIndex = commandHistory.Count;
        }

        public void RegisterEchoListner(IDebugEchoListner listner)
        {
            listenrs.Add(listner);
        }

        public void UnregisterEchoListner(IDebugEchoListner listner)
        {
            listenrs.Remove(listner);
        }

        public void Echo(DebugCommandMessage messageType, string text)
        {
            lines.Enqueue(text);
            while (lines.Count >= MaxLineCount)
                lines.Dequeue();

            // 登録されているリスナーを呼び出す
            foreach (IDebugEchoListner listner in listenrs)
                listner.Echo(messageType, text);
        }

        public void Echo(string text)
        {
            Echo(DebugCommandMessage.Standard, text);
        }

        public void EchoWarning(string text)
        {
            Echo(DebugCommandMessage.Warning, text);
        }

        public void EchoError(string text)
        {
            Echo(DebugCommandMessage.Error, text);
        }

        public void PushExecutioner(IDebugCommandExecutioner executioner)
        {
            executioners.Push(executioner);
        }

        public void PopExecutioner()
        {
            executioners.Pop();
        }

        #endregion

        #region 更新と描画

        /// <summary>
        /// デバッグコマンドウィンドウを表示する。
        /// </summary>
        public void Show()
        {
            if (state == State.Closed)
            {
                stateTransition = 0.0f;
                state = State.Opening;
            }
        }

        /// <summary>
        /// デバッグコマンドウィンドウを非表示にする。
        /// </summary>
        public void Hide()
        {
            if (state == State.Opened)
            {
                stateTransition = 1.0f;
                state = State.Closing;
            }
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            const float OpenSpeed = 8.0f;
            const float CloseSpeed = 8.0f;

            switch (state)
            {
                case State.Closed:
                    if (keyState.IsKeyDown(Keys.Tab))
                        Show();
                    break;
                case State.Opening:
                    stateTransition += dt * OpenSpeed;
                    if (stateTransition > 1.0f)
                    {
                        stateTransition = 1.0f;
                        state = State.Opened;
                    }
                    break;
                case State.Opened:
                    ProcessKeyInputs(dt);
                    break;
                case State.Closing:
                    stateTransition -= dt * CloseSpeed;
                    if (stateTransition < 0.0f)
                    {
                        stateTransition = 0.0f;
                        state = State.Closed;
                    }
                    break;
            }

            prevKeyState = keyState;

            base.Update(gameTime);
        }

        /// <summary>
        /// キー入力処理
        /// </summary>
        /// <param name="dt"></param>
        public void ProcessKeyInputs(float dt)
        {
            KeyboardState keyState = Keyboard.GetState();
            Keys[] keys = keyState.GetPressedKeys();

            bool shift = keyState.IsKeyDown(Keys.LeftShift) ||
                            keyState.IsKeyDown(Keys.RightShift);

            foreach (Keys key in keys)
            {
                if (!IsKeyPressed(key, dt)) continue;

                char ch;
                if (KeyboardUtils.KeyToString(key, shift, out ch))
                {
                    // 通常文字の入力
                    commandLine = commandLine.Insert(cursorIndex, new string(ch, 1));
                    cursorIndex++;
                }
                else
                {
                    switch (key)
                    {
                        case Keys.Back:
                            if (cursorIndex > 0)
                                commandLine = commandLine.Remove(--cursorIndex, 1);
                            break;
                        case Keys.Delete:
                            if (cursorIndex < commandLine.Length)
                                commandLine = commandLine.Remove(cursorIndex, 1);
                            break;
                        case Keys.Left:
                            if (cursorIndex > 0)
                                cursorIndex--;
                            break;
                        case Keys.Right:
                            if (cursorIndex < commandLine.Length)
                                cursorIndex++;
                            break;
                        case Keys.Enter:
                            // コマンドの実行
                            ExecuteCommand(commandLine);
                            commandLine = string.Empty;
                            cursorIndex = 0;
                            break;
                        case Keys.Up:
                            // ヒストリ表示
                            if (commandHistory.Count > 0)
                            {
                                commandHistoryIndex =
                                    Math.Max(0, commandHistoryIndex - 1);

                                commandLine = commandHistory[commandHistoryIndex];
                                cursorIndex = commandLine.Length;
                            }
                            break;
                        case Keys.Down:
                            // ヒストリ表示
                            if (commandHistory.Count > 0)
                            {
                                commandHistoryIndex = Math.Min(commandHistory.Count - 1,
                                                                commandHistoryIndex + 1);
                                commandLine = commandHistory[commandHistoryIndex];
                                cursorIndex = commandLine.Length;
                            }
                            break;
                        case Keys.Tab:
                            Hide();
                            break;
                    }
                }
            }

        }

        /// <summary>
        /// キーリピート付きキー押下チェック
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsKeyPressed(Keys key, float dt)
        {
            // 前フレームでキーが押されていなければ、キーが押されていると判定
            if (prevKeyState.IsKeyUp(key))
            {
                keyRepeatTimer = keyRepeatStartDuration;
                pressedKey = key;
                return true;
            }

            // 前フレームでキーが押されていた場合はリピート処理
            if (key == pressedKey)
            {
                keyRepeatTimer -= dt;
                if (keyRepeatTimer <= 0.0f)
                {
                    keyRepeatTimer += keyRepeatDuration;
                    return true;
                }
            }

            return false;
        }

        public override void Draw(GameTime gameTime)
        {
            // コマンドウィンドウが完全に閉じている場合は描画処理をしない
            if (state == State.Closed)
                return;

            SpriteFont font = debugManager.DebugFont;
            SpriteBatch spriteBatch = debugManager.SpriteBatch;
            Texture2D whiteTexture = debugManager.WhiteTexture;

            // コマンドウィンドウのサイズ計算と描画
            float w = GraphicsDevice.Viewport.Width;
            float h = GraphicsDevice.Viewport.Height;
            float topMargin = h * 0.1f;
            float leftMargin = w * 0.1f;

            Rectangle rect = new Rectangle();
            rect.X = (int)leftMargin;
            rect.Y = (int)topMargin;
            rect.Width = (int)(w * 0.8f);
            rect.Height = (int)(MaxLineCount * font.LineSpacing);

            Matrix mtx = Matrix.CreateTranslation(
                        new Vector3(0, -rect.Height * (1.0f - stateTransition), 0));

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, mtx);

            spriteBatch.Draw(whiteTexture, rect, new Color(0, 0, 0, 200));

            // 文字列の描画
            Vector2 pos = new Vector2(leftMargin, topMargin);
            foreach (string line in lines)
            {
                spriteBatch.DrawString(font, line, pos, Color.White);
                pos.Y += font.LineSpacing;
            }

            // プロンプト文字列の描画
            string leftPart = Prompt + commandLine.Substring(0, cursorIndex);
            Vector2 cursorPos = pos + font.MeasureString(leftPart);
            cursorPos.Y = pos.Y;

            spriteBatch.DrawString(font,
                String.Format("{0}{1}", Prompt, commandLine), pos, Color.White);
            spriteBatch.DrawString(font, Cursor, cursorPos, Color.White);

            spriteBatch.End();
        }

        #endregion

    }
}
