#region Using ステートメント

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace DebugSample
{
    /// <summary>
    /// CPU処理速度のリアルタイム測定用ツール
    /// </summary>
    /// <remarks>
    /// このツールを使うことでボトルネックの発見と、あとどれだけの処理ができるのかが
    /// 視覚的に判る。
    /// また、リアルタイムプロファイラーなのでゲーム中に瞬間的に大量の処理をする場合の
    /// 様子も把握することができる。
    /// 
    /// TimeRulerがサポートしている機能は以下の通り:
    ///  * 最大8個(変更可)のバー表示
    ///  * 各マーカーに任意の色をつけることができる
    ///  * マーカーログ表示機能
    ///  * TRACE設定を外すとBeginMark/EndMark等のメソッドの呼び出し自体をしない
    ///  * 最大32個(変更可)のBeginMarkのネスト呼び出しに対応
    ///  * マルチスレッド対応
    ///  * 負荷状態による表示フレーム数の自動変化機能
    ///  
    /// 基本的な使用方法は、Game.ComponentsにTimeRulerのインスタンスを追加し、
    /// Game.Updateメソッドの先頭でtimerRuler.StartFrame()メソッドを呼ぶようにする。
    /// 
    /// 後は測定したい部分の前後でBeginMark,EndMarkを呼び出す。
    /// 
    /// timeRuler.BeginMark( "Update", Color.Blue );
    /// // 測定したい処理
    /// timerRuler.EndMark( "Update" );
    /// 
    /// また、BeginMarkには測定結果を表示するバーのインデックスを指定できる(規定値は0)
    /// 
    /// timeRuler.BeginMark( 1, "Update", Color.Blue );
    /// 
    /// プロファイルに使用するメソッド自体にはConditionalAttributeを指定しているので、
    /// BeginMark/EndMark等のメソッドはTRACEが設定されていないとメソッド呼び出しのコード
    /// を生成しないようになっている。実際のリリース時にはビルド設定の
    /// TRACE定数の定義のチェックボックスをクリアするのを忘れないようにすること。
    /// 
    /// </remarks>
    public class TimeRuler : DrawableGameComponent
    {
        #region 定数宣言

        /// <summary>
        /// 最大バー表示数
        /// </summary>
        const int MaxBars = 8;

        /// <summary>
        /// バーひとつあたりの最大サンプル数
        /// </summary>
        const int MaxSamples = 256;

        /// <summary>
        /// バーひとつあたりの最大ネスト数
        /// </summary>
        const int MaxNestCall = 32;

        /// <summary>
        /// 最多表示フレーム数
        /// </summary>
        const int MaxSampleFrames = 4;

        /// <summary>
        /// ログのスナップを取る間隔(フレーム数)
        /// </summary>
        const int LogSnapDuration = 120;

        /// <summary>
        /// バーの高さ(ピクセル)
        /// </summary>
        const int BarHeight = 8;

        /// <summary>
        /// バーのパディング(ピクセル)
        /// </summary>
        const int BarPadding = 2;

        /// <summary>
        /// 自動表示フレーム調整に掛かるフレーム数
        /// </summary>
        const int AutoAdjustDelay = 30;

        #endregion

        #region プロパティ

        /// <summary>
        /// ログの表示設定の設定と取得
        /// </summary>
        public bool ShowLog { get; set; }

        /// <summary>
        /// 目標表示フレーム数の取得と設定
        /// </summary>
        public int TargetSampleFrames { get; set; }

        /// <summary>
        /// TimeRuler描画位置の取得と設定
        /// </summary>
        public Vector2 Position { get { return position; } set { position = value; } }

        /// <summary>
        /// TimeRuler描画幅の取得と設定
        /// </summary>
        public int Width { get; set; }

        #endregion

        #region フィールド

#if TRACE

        /// <summary>
        /// マーカー構造体
        /// </summary>
        private struct Marker
        {
            public int MarkerId;
            public float BeginTime;
            public float EndTime;
            public Color Color;
        }

        /// <summary>
        /// マーカーコレクション
        /// </summary>
        private class MarkerCollection
        {
            // マーカーコレクション
            public Marker[] Markers = new Marker[MaxSamples];
            public int MarkCount;

            // マーカーネスト情報
            public int[] MarkerNests = new int[MaxNestCall];
            public int NestCount;
        }

        /// <summary>
        /// フレームのログ
        /// </summary>
        private class FrameLog
        {
            // バー情報
            public MarkerCollection[] Bars;

            public FrameLog()
            {
                // マーカーコレクション配列の初期化
                Bars = new MarkerCollection[MaxBars];
                for (int i = 0; i < MaxBars; ++i)
                    Bars[i] = new MarkerCollection();
            }
        }

        /// <summary>
        /// マーカー情報
        /// </summary>
        private class MarkerInfo
        {
            // マーカー名
            public string Name;

            // マーカーログ
            public MarkerLog[] Logs = new MarkerLog[MaxBars];

            public MarkerInfo(string name)
            {
                Name = name;
            }
        }

        /// <summary>
        /// マーカーログ情報
        /// </summary>
        private struct MarkerLog
        {
            public float SnapMin;
            public float SnapMax;
            public float SnapAvg;

            public float Min;
            public float Max;
            public float Avg;

            public int Samples;

            public Color Color;

            public bool Initialized;
        }

        // デバッグマネージャー
        DebugManager debugManager;

        // フレーム毎のログ
        FrameLog[] logs;

        // 前フレームのログ
        FrameLog prevLog;

        // 測定中のフレームログ
        FrameLog curLog;

        // 現在のフレーム数
        int frameCount;

        // 計測に使用するストップウォッチ
        Stopwatch stopwatch = new Stopwatch();

        // マーカー情報配列
        List<MarkerInfo> markers = new List<MarkerInfo>();

        // マーカー名からマーカーIDへの変換マップ
        Dictionary<string, int> markerNameToIdMap = new Dictionary<string, int>();

        // サンプルフレーム自動調整用のカウンタ
        int frameAdjust;

        // 現在の表示フレーム数
        int sampleFrames;

        // マーカーログ表示文字列
        StringBuilder logString = new StringBuilder(512);

        // You want to call StartFrame at beginning of Game.Update method.
        // But Game.Update gets calls multiple time when game runs slow in fixed time step mode.
        // In this case, we should ignore StartFrame call.
        // To do this, we just keep tracking of number of StartFrame calls untile Draw gets called.
        int updateCount;

#endif
        // TimerRulerの表示位置
        Vector2 position;

        #endregion

        #region 初期化

        public TimeRuler(Game game)
            : base(game)
        {
            // サービスとして登録する
            // timeLine.BeginMarkを使っているラインはTRACE設定なしの場合でも残ってる
            // 場合があるので、サービス登録は必要になる
            Game.Services.AddService(typeof(TimeRuler), this);
        }

        public override void Initialize()
        {
#if TRACE
            debugManager =
                Game.Services.GetService(typeof(DebugManager)) as DebugManager;

            if (debugManager == null)
                throw new InvalidOperationException("DebugManagerが登録されていません");

            // DebugCommandHostが登録されているのなら、コマンドを登録
            IDebugCommandHost host =
                                Game.Services.GetService(typeof(IDebugCommandHost))
                                                                    as IDebugCommandHost;

            if (host != null)
            {
                host.RegisterCommand("tr", "TimeRuler", this.CommandExecute);
                this.Visible = false;
                this.Enabled = false;
            }

            // パラメーターの初期化
            logs = new FrameLog[2];
            for (int i = 0; i < logs.Length; ++i)
                logs[i] = new FrameLog();

            sampleFrames = TargetSampleFrames = 1;

            // Time-Ruler's update method doesn't need to get called.
            this.Enabled = false;
#endif
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Width = (int)(GraphicsDevice.Viewport.Width * 0.8f);

            Layout layout = new Layout(GraphicsDevice.Viewport);
            position = layout.Place(new Vector2(Width, BarHeight),
                                                    0, 0.01f, Alignment.BottomCenter);

            base.LoadContent();
        }

#if TRACE
        /// <summary>
        /// TimeRulerコマンド処理
        /// </summary>
        void CommandExecute(IDebugCommandHost host, string command,
                                                                IList<string> arguments)
        {
            bool previousVisible = Visible;

            if (arguments.Count == 0)
                Visible = !Visible;

            char[] subArgSeparator = new[] { ':' };
            foreach (string orgArg in arguments)
            {
                string arg = orgArg.ToLower();
                string[] subargs = arg.Split(subArgSeparator);
                switch (subargs[0])
                {
                    case "on":
                        Visible = true;
                        break;
                    case "off":
                        Visible = false;
                        break;
                    case "reset":
                        ResetLog();
                        break;
                    case "log":
                        if (subargs.Length > 1)
                        {
                            if (String.Compare(subargs[1], "on") == 0)
                                ShowLog = true;
                            if (String.Compare(subargs[1], "off") == 0)
                                ShowLog = false;
                        }
                        else
                        {
                            ShowLog = !ShowLog;
                        }
                        break;
                    case "frame":
                        int a = Int32.Parse(subargs[1]);
                        a = Math.Max(a, 1);
                        a = Math.Min(a, MaxSampleFrames);
                        TargetSampleFrames = a;
                        break;
                    case "/?":
                    case "--help":
                        host.Echo("tr [log|on|off|reset|frame]");
                        host.Echo("Options:");
                        host.Echo("       on     Display TimeRuler.");
                        host.Echo("       off    Hide TimeRuler.");
                        host.Echo("       log    Show/Hide marker log.");
                        host.Echo("       reset  Reset marker log.");
                        host.Echo("       frame:sampleFrames");
                        host.Echo("              Change target sample frame count");
                        break;
                    default:
                        break;
                }
            }

            // Reset update count when Visible state changed.
            if (Visible != previousVisible)
            {
                Interlocked.Exchange(ref updateCount, 0);
            }
        }
#endif

        #endregion

        #region 測定用メソッド

        /// <summary>
        /// 新しいフレームの開始
        /// </summary>
        [Conditional("TRACE")]
        public void StartFrame()
        {
#if TRACE
            lock (this)
            {
                // Game.IsFixedTimeStepがtrueの場合、Game.Updateが複数回呼ばれることがある。
                // http://blogs.msdn.com/b/ito/archive/2007/03/08/2-update.aspx
                // このケースに対処する為に、タイムルーラーの描画が呼び出されずにStartFrameが複数回呼ばれた場合は
                // フレームリセットするのではなく、測定を継続するようになっている。
                int count = Interlocked.Increment(ref updateCount);
                if (Visible && (1 < count && count < MaxSampleFrames))
                    return;

                // 現フレームログの更新
                prevLog = logs[frameCount++ & 0x1];
                curLog = logs[frameCount & 0x1];

                float endFrameTime = (float)stopwatch.Elapsed.TotalMilliseconds;

                // マーカーの更新とログ生成
                for (int barIdx = 0; barIdx < prevLog.Bars.Length; ++barIdx)
                {
                    MarkerCollection prevBar = prevLog.Bars[barIdx];
                    MarkerCollection nextBar = curLog.Bars[barIdx];

                    // 前フレームでEndMarkを呼んでいないマーカーを閉じ、現フレームで
                    // 再度開く。
                    for (int nest = 0; nest < prevBar.NestCount; ++nest)
                    {
                        int markerIdx = prevBar.MarkerNests[nest];

                        prevBar.Markers[markerIdx].EndTime = endFrameTime;

                        nextBar.MarkerNests[nest] = nest;
                        nextBar.Markers[nest].MarkerId =
                            prevBar.Markers[markerIdx].MarkerId;
                        nextBar.Markers[nest].BeginTime = 0;
                        nextBar.Markers[nest].EndTime = -1;
                        nextBar.Markers[nest].Color = prevBar.Markers[markerIdx].Color;
                    }

                    // マーカーログの更新
                    for (int markerIdx = 0; markerIdx < prevBar.MarkCount; ++markerIdx)
                    {
                        float duration = prevBar.Markers[markerIdx].EndTime -
                                            prevBar.Markers[markerIdx].BeginTime;

                        int markerId = prevBar.Markers[markerIdx].MarkerId;
                        MarkerInfo m = markers[markerId];

                        m.Logs[barIdx].Color = prevBar.Markers[markerIdx].Color;

                        if (!m.Logs[barIdx].Initialized)
                        {
                            // 最初のフレームの処理
                            m.Logs[barIdx].Min = duration;
                            m.Logs[barIdx].Max = duration;
                            m.Logs[barIdx].Avg = duration;

                            m.Logs[barIdx].Initialized = true;
                        }
                        else
                        {
                            // ２フレーム目以降の処理
                            m.Logs[barIdx].Min = Math.Min(m.Logs[barIdx].Min, duration);
                            m.Logs[barIdx].Max = Math.Min(m.Logs[barIdx].Max, duration);
                            m.Logs[barIdx].Avg += duration;
                            m.Logs[barIdx].Avg *= 0.5f;

                            if (m.Logs[barIdx].Samples++ >= LogSnapDuration)
                            {
                                m.Logs[barIdx].SnapMin = m.Logs[barIdx].Min;
                                m.Logs[barIdx].SnapMax = m.Logs[barIdx].Max;
                                m.Logs[barIdx].SnapAvg = m.Logs[barIdx].Avg;
                                m.Logs[barIdx].Samples = 0;
                            }
                        }
                    }

                    nextBar.MarkCount = prevBar.NestCount;
                    nextBar.NestCount = prevBar.NestCount;
                }

                // このフレームの測定開始
                stopwatch.Reset();
                stopwatch.Start();
            }
#endif
        }

        /// <summary>
        /// マーカーの開始
        /// </summary>
        /// <param name="markerName">マーカー名</param>
        /// <param name="color">カラー</param>
        [Conditional("TRACE")]
        public void BeginMark(string markerName, Color color)
        {
#if TRACE
            BeginMark(0, markerName, color);
#endif
        }

        /// <summary>
        /// マーカーの開始
        /// </summary>
        /// <param name="barIndex">バーのインデックス値</param>
        /// <param name="markerName">マーカー名</param>
        /// <param name="color">カラー</param>
        [Conditional("TRACE")]
        public void BeginMark(int barIndex, string markerName, Color color)
        {
#if TRACE
            lock (this)
            {
                if (barIndex < 0 || barIndex >= MaxBars)
                    throw new ArgumentOutOfRangeException("barIndex");

                MarkerCollection bar = curLog.Bars[barIndex];

                if (bar.MarkCount >= MaxSamples)
                {
                    throw new OverflowException(
                        "サンプル数がMaxSampleを超えました。\n" +
                        "TimeRuler.MaxSmpaleの値を大きくするか、" +
                        "サンプル数を少なくしてください。");
                }

                if (bar.NestCount >= MaxNestCall)
                {
                    throw new OverflowException(
                        "ネスト数がMaxNestCallを超えました。\n" +
                        "TimeRuler.MaxNestCallの値を大きくするか、" +
                        "ネスト呼び出し数を減らしてください。");
                }

                // 登録されているマーカーを取得
                int markerId;
                if (!markerNameToIdMap.TryGetValue(markerName, out markerId))
                {
                    // 登録されていなければ新たに登録する
                    markerId = markers.Count;
                    markerNameToIdMap.Add(markerName, markerId);
                    markers.Add(new MarkerInfo(markerName));
                }

                // 測定開始
                bar.MarkerNests[bar.NestCount++] = bar.MarkCount;

                // マーカーのパラメーターを設定
                bar.Markers[bar.MarkCount].MarkerId = markerId;
                bar.Markers[bar.MarkCount].Color = color;
                bar.Markers[bar.MarkCount].BeginTime =
                                        (float)stopwatch.Elapsed.TotalMilliseconds;

                bar.Markers[bar.MarkCount].EndTime = -1;

                bar.MarkCount++;
            }
#endif
        }

        /// <summary>
        /// マーカーの終了
        /// </summary>
        /// <param name="markerName">マーカー名</param>
        [Conditional("TRACE")]
        public void EndMark(string markerName)
        {
#if TRACE
            EndMark(0, markerName);
#endif
        }

        /// <summary>
        /// マーカーの終了
        /// </summary>
        /// <param name="barIndex">バーのインデックス値</param>
        /// <param name="markerName">マーカー名</param>
        [Conditional("TRACE")]
        public void EndMark(int barIndex, string markerName)
        {
#if TRACE
            lock (this)
            {
                if (barIndex < 0 || barIndex >= MaxBars)
                    throw new ArgumentOutOfRangeException("barIndex");

                MarkerCollection bar = curLog.Bars[barIndex];

                if (bar.NestCount <= 0)
                {
                    throw new InvalidOperationException(
                        "EndMarkを呼び出す前に、BeginMarkメソッドを呼んでください。");
                }

                int markerId;
                if (!markerNameToIdMap.TryGetValue(markerName, out markerId))
                {
                    throw new InvalidOperationException(
                        String.Format("マーカー名「{0}」は登録されていません。" +
                            "BeginMarkで使った名前と同じ名前か確認してください。",
                            markerName));
                }

                int markerIdx = bar.MarkerNests[--bar.NestCount];
                if (bar.Markers[markerIdx].MarkerId != markerId)
                {
                    throw new InvalidOperationException(
                    "BeginMark/EndMarkの呼び出し順序が不正です。" +
                    "BeginMark(A), BeginMark(B), EndMark(B), EndMark(A)の" +
                    "のようには呼べますが、" +
                    "BeginMark(A), BeginMark(B), EndMark(A), EndMark(B)のようには" +
                    "呼べません。");
                }

                bar.Markers[markerIdx].EndTime =
                    (float)stopwatch.Elapsed.TotalMilliseconds;
            }
#endif
        }

        /// <summary>
        /// 指定された測定バーインデックスとマーカー名の平均処理時間を返す。
        /// </summary>
        /// <param name="barIndex">測定バーのインデックス</param>
        /// <param name="markerName">マーカー名</param>
        /// <returns>平均処理時間(ミリ秒)</returns>
        public float GetAverageTime(int barIndex, string markerName)
        {
#if TRACE
            if (barIndex < 0 || barIndex >= MaxBars)
                throw new ArgumentOutOfRangeException("barIndex");

            float result = 0;
            int markerId;
            if (markerNameToIdMap.TryGetValue(markerName, out markerId))
                result = markers[markerId].Logs[barIndex].Avg;

            return result;
#endif
        }

        /// <summary>
        /// マーカーログのリセット
        /// </summary>
        [Conditional("TRACE")]
        public void ResetLog()
        {
#if TRACE
            lock (this)
            {
                foreach (MarkerInfo markerInfo in markers)
                {
                    for (int i = 0; i < markerInfo.Logs.Length; ++i)
                    {
                        markerInfo.Logs[i].Initialized = false;
                        markerInfo.Logs[i].SnapMin = 0;
                        markerInfo.Logs[i].SnapMax = 0;
                        markerInfo.Logs[i].SnapAvg = 0;

                        markerInfo.Logs[i].Min = 0;
                        markerInfo.Logs[i].Max = 0;
                        markerInfo.Logs[i].Avg = 0;

                        markerInfo.Logs[i].Samples = 0;
                    }
                }
            }
#endif
        }

        #endregion

        #region 描画

        public override void Draw(GameTime gameTime)
        {
            Draw(position, Width);
            base.Draw(gameTime);
        }

        [Conditional("TRACE")]
        public void Draw(Vector2 position, int width)
        {
#if TRACE
            // 更新カウントをリセットする。
            Interlocked.Exchange(ref updateCount, 0);

            // SpriteBatch, SpriteFont, WhiteTextureをDebugManagerから取得する。
            SpriteBatch spriteBatch = debugManager.SpriteBatch;
            SpriteFont font = debugManager.DebugFont;
            Texture2D texture = debugManager.WhiteTexture;

            // 表示するべきバーの数によって表示サイズと位置を変更する
            int height = 0;
            float maxTime = 0;
            foreach (MarkerCollection bar in prevLog.Bars)
            {
                if (bar.MarkCount > 0)
                {
                    height += BarHeight + BarPadding * 2;
                    maxTime = Math.Max(maxTime,
                                            bar.Markers[bar.MarkCount - 1].EndTime);
                }
            }

            // 表示フレーム数の自動調整
            // 例えば16.6msで処理が間に合わなかった状態が一定時間以上続くと
            // 自動的に表示する時間間隔を33.3msに調整する
            const float frameSpan = 1.0f / 60.0f * 1000f;
            float sampleSpan = (float)sampleFrames * frameSpan;

            if (maxTime > sampleSpan)
                frameAdjust = Math.Max(0, frameAdjust) + 1;
            else
                frameAdjust = Math.Min(0, frameAdjust) - 1;

            if (Math.Abs(frameAdjust) > AutoAdjustDelay)
            {
                sampleFrames = Math.Min(MaxSampleFrames, sampleFrames);
                sampleFrames =
                    Math.Max(TargetSampleFrames, (int)(maxTime / frameSpan) + 1);

                frameAdjust = 0;
            }

            // ミリ秒からピクセルに変換する係数を計算
            float msToPs = (float)width / sampleSpan;

            // 描画開始位置
            int startY = (int)position.Y - (height - BarHeight);

            // 現在のy座標
            int y = startY;

            spriteBatch.Begin();

            // 背景の半透明の矩形を描画
            Rectangle rc = new Rectangle((int)position.X, y, width, height);
            spriteBatch.Draw(texture, rc, new Color(0, 0, 0, 128));

            // 各バーのマーカーを描画
            rc.Height = BarHeight;
            foreach (MarkerCollection bar in prevLog.Bars)
            {
                rc.Y = y + BarPadding;
                if (bar.MarkCount > 0)
                {
                    for (int j = 0; j < bar.MarkCount; ++j)
                    {
                        float bt = bar.Markers[j].BeginTime;
                        float et = bar.Markers[j].EndTime;
                        int sx = (int)(position.X + bt * msToPs);
                        int ex = (int)(position.X + et * msToPs);
                        rc.X = sx;
                        rc.Width = Math.Max(ex - sx, 1);

                        spriteBatch.Draw(texture, rc, bar.Markers[j].Color);
                    }
                }

                y += BarHeight + BarPadding;
            }

            // グリッドを描画する
            // ミリ秒単位のグリッド描画
            rc = new Rectangle((int)position.X, (int)startY, 1, height);
            for (float t = 1.0f; t < sampleSpan; t += 1.0f)
            {
                rc.X = (int)(position.X + t * msToPs);
                spriteBatch.Draw(texture, rc, Color.Gray);
            }

            // フレーム単位のグリッド描画
            for (int i = 0; i <= sampleFrames; ++i)
            {
                rc.X = (int)(position.X + frameSpan * (float)i * msToPs);
                spriteBatch.Draw(texture, rc, Color.White);
            }

            // ログの表示
            if (ShowLog)
            {
                // 表示する文字列をStringBuilderで生成する
                y = startY - font.LineSpacing;
                logString.Length = 0;
                foreach (MarkerInfo markerInfo in markers)
                {
                    for (int i = 0; i < MaxBars; ++i)
                    {
                        if (markerInfo.Logs[i].Initialized)
                        {
                            if (logString.Length > 0)
                                logString.Append("\n");

                            logString.Append(" Bar ");
                            logString.AppendNumber(i);
                            logString.Append(" ");
                            logString.Append(markerInfo.Name);

                            logString.Append(" Avg.:");
                            logString.AppendNumber(markerInfo.Logs[i].SnapAvg);
                            logString.Append("ms ");

                            y -= font.LineSpacing;
                        }
                    }
                }

                // 表示する文字列の背景の矩形サイズの計算と描画
                Vector2 size = font.MeasureString(logString);
                rc = new Rectangle((int)position.X, (int)y, (int)size.X + 12, (int)size.Y);
                spriteBatch.Draw(texture, rc, new Color(0, 0, 0, 128));

                // ログ文字列の描画
                spriteBatch.DrawString(font, logString,
                                        new Vector2(position.X + 12, y), Color.White);

                // ログカラーボックスの描画
                y += (int)((float)font.LineSpacing * 0.3f);
                rc = new Rectangle((int)position.X + 4, y, 10, 10);
                Rectangle rc2 = new Rectangle((int)position.X + 5, y + 1, 8, 8);
                foreach (MarkerInfo markerInfo in markers)
                {
                    for (int i = 0; i < MaxBars; ++i)
                    {
                        if (markerInfo.Logs[i].Initialized)
                        {
                            rc.Y = y;
                            rc2.Y = y + 1;
                            spriteBatch.Draw(texture, rc, Color.White);
                            spriteBatch.Draw(texture, rc2, markerInfo.Logs[i].Color);

                            y += font.LineSpacing;
                        }
                    }
                }


            }

            spriteBatch.End();
#endif
        }

        #endregion

    }
}
