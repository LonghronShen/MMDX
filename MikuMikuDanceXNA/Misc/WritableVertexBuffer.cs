using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace MikuMikuDance.XNA.Misc
{
    /// <summary>
    /// 連続書き込み用の頂点クラス
    /// </summary>
    /// <remarks>
    /// 頂点バッファへの動的書きこみはGPUの読み取りとバッティングしないようにする必要がある
    /// vFetchサンプルより。
    /// 詳しくはvFetchサンプル、及び
    /// http://blogs.msdn.com/ito/archive/2009/03/25/how-gpus-works-01.aspx
    /// </remarks>
    public class WritableVertexBuffer
    {
        //頂点バッファ
        DynamicVertexBuffer vb;
        //現在の書きこみ位置
        int currentPosition;
        //最大頂点数
        int maxElementCount;
        /// <summary>
        /// 頂点バッファの取得
        /// </summary>
        public DynamicVertexBuffer VertexBuffer { get { return vb; } }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="graphics">GraphicsDevice</param>
        /// <param name="maxElementCount">バッファ長(頂点数単位)</param>
        /// <param name="vertexType">書き込み型</param>
        public WritableVertexBuffer(GraphicsDevice graphics, int maxElementCount, Type vertexType)
        {
            vb = new DynamicVertexBuffer(graphics, vertexType, maxElementCount, 0);
            this.maxElementCount = maxElementCount;
        }
        /// <summary>
        /// 頂点データの書きこみ
        /// </summary>
        /// <param name="data">頂点データ</param>
        /// <typeparam name="T">書き込み型</typeparam>
        /// <returns>頂点データのオフセット</returns>
        public int SetData<T>(T[] data)
            where T : struct
        {
            return SetData(data, 0, data.Length);
        }
        /// <summary>
        /// 頂点データの書きこみ
        /// </summary>
        /// <param name="data">頂点データ</param>
        /// <param name="startIndex">書き込むデータの先頭インデックス</param>
        /// <param name="elementCount">書き込む要素数</param>
        /// <typeparam name="T">書き込み型</typeparam>
        /// <returns>書き込んだデータの先頭オフセット</returns>
        public int SetData<T>(T[] data, int startIndex, int elementCount)
            where T : struct
        {
            int position = currentPosition;
            SetDataOptions option = SetDataOptions.NoOverwrite;
            //最大要素数を超えるならDiscard
            if (position + elementCount > maxElementCount)
            {
                position = 0;
                option = SetDataOptions.Discard;
            }
            int strideSize = vb.VertexDeclaration.VertexStride;
            vb.SetData(position * strideSize, data, startIndex, elementCount, strideSize, option);
            currentPosition = position + elementCount;
            return position;
        }
    }
}
