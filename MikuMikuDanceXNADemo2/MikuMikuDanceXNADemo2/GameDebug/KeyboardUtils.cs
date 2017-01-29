#region Using ステートメント

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

#endregion

namespace DebugSample
{
    /// <summary>
    /// キーボード入力関連のユーティリティクラス
    /// </summary>
    public static class KeyboardUtils
    {
        #region フィールド

        /// <summary>
        /// 通常の文字とシフトキーが押下された時の文字を保持する文字ペアクラス
        /// </summary>
        class CharPair
        {
            public CharPair(char normalChar, Nullable<char> shiftChar)
            {
                this.NormalChar = normalChar;
                this.ShiftChar = shiftChar;
            }

            public char NormalChar;
            public Nullable<char> ShiftChar;
        }

        // キー:Keys, 値:CharPair
        static private Dictionary<Keys, CharPair> keyMap =
                                                    new Dictionary<Keys, CharPair>();

        #endregion

        /// <summary>
        /// キー情報から文字を取得する
        /// </summary>
        /// <param name="key">押下されたキー</param>
        /// <param name="shitKeyPressed">シフトキーが押されていたか?</param>
        /// <param name="character">キー入力から変換された文字</param>
        /// <returns>文字取得が成功した場合trueを返す</returns>
        public static bool KeyToString(Keys key, bool shitKeyPressed,
                                                                    out char character)
        {
            bool result = false;
            character = ' ';
            CharPair charPair;

            if ((Keys.A <= key && key <= Keys.Z) || key == Keys.Space)
            {
                // A～Z、スペースキーはそのまま文字コードとして使用する
                character = (shitKeyPressed) ? (char)key : Char.ToLower((char)key);
                result = true;
            }
            else if (keyMap.TryGetValue(key, out charPair))
            {
                // それ以外の場合はKeyMapの情報を元に変換する
                if (!shitKeyPressed)
                {
                    character = charPair.NormalChar;
                    result = true;
                }
                else if (charPair.ShiftChar.HasValue)
                {
                    character = charPair.ShiftChar.Value;
                    result = true;
                }
            }

            return result;
        }

        #region 初期化

        static KeyboardUtils()
        {
            InitializeKeyMap();
        }

        /// <summary>
        /// 英字以外のキーの文字マップの初期化
        /// </summary>
        /// <remarks>ここではUSキーボード用のものを宣言しているので、
        /// 日本語キーボードでは変更する必要があるかも</remarks>
        static void InitializeKeyMap()
        {
            // 英語キーボードの上から1列目
            AddKeyMap(Keys.OemTilde, "`~");
            AddKeyMap(Keys.D1, "1!");
            AddKeyMap(Keys.D2, "2@");
            AddKeyMap(Keys.D3, "3#");
            AddKeyMap(Keys.D4, "4$");
            AddKeyMap(Keys.D5, "5%");
            AddKeyMap(Keys.D6, "6^");
            AddKeyMap(Keys.D7, "7&");
            AddKeyMap(Keys.D8, "8*");
            AddKeyMap(Keys.D9, "9(");
            AddKeyMap(Keys.D0, "0)");
            AddKeyMap(Keys.OemMinus, "-_");
            AddKeyMap(Keys.OemPlus, "=+");

            // 英語キーボードの上から2列目
            AddKeyMap(Keys.OemOpenBrackets, "[{");
            AddKeyMap(Keys.OemCloseBrackets, "]}");
            AddKeyMap(Keys.OemPipe, "\\|");

            // 英語キーボードの上から3列目
            AddKeyMap(Keys.OemSemicolon, ";:");
            AddKeyMap(Keys.OemQuotes, "'\"");
            AddKeyMap(Keys.OemComma, ",<");
            AddKeyMap(Keys.OemPeriod, ".>");
            AddKeyMap(Keys.OemQuestion, "/?");

            // 英語キーボードのキーパッドのキー
            AddKeyMap(Keys.NumPad1, "1");
            AddKeyMap(Keys.NumPad2, "2");
            AddKeyMap(Keys.NumPad3, "3");
            AddKeyMap(Keys.NumPad4, "4");
            AddKeyMap(Keys.NumPad5, "5");
            AddKeyMap(Keys.NumPad6, "6");
            AddKeyMap(Keys.NumPad7, "7");
            AddKeyMap(Keys.NumPad8, "8");
            AddKeyMap(Keys.NumPad9, "9");
            AddKeyMap(Keys.NumPad0, "0");
            AddKeyMap(Keys.Add, "+");
            AddKeyMap(Keys.Divide, "/");
            AddKeyMap(Keys.Multiply, "*");
            AddKeyMap(Keys.Subtract, "-");
            AddKeyMap(Keys.Decimal, ".");
        }

        /// <summary>
        ///　キーボードのキーと文字マップの追加
        /// </summary>
        /// <param name="key">キーボードのキー</param>
        /// <param name="charPair">
        /// 文字、２文字の場合はシフトキー無し、有り順番に記述する</param>
        static void AddKeyMap(Keys key, string charPair)
        {
            char char1 = charPair[0];
            Nullable<char> char2 = null;
            if (charPair.Length > 1)
                char2 = charPair[1];

            keyMap.Add(key, new CharPair(char1, char2));
        }

        #endregion

    }

}
