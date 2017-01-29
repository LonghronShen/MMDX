using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MikuMikuDance.Core.Misc
{
    /// <summary>
    /// MMDXエラー
    /// </summary>
    public class MMDXException : Exception
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MMDXException() : base() { }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="message">メッセージ</param>
        public MMDXException(string message) : base(message) { }
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="message">メッセージ</param>
        /// <param name="innerException">内部エラー</param>
        public MMDXException(string message, Exception innerException) : base(message, innerException) { }
    }
}
