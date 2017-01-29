using System;
using System.Collections.Generic;
using MikuMikuDance.Core.Misc;
namespace MikuMikuDance.Core.Model
{
    /// <summary>
    /// モデルパーツのアブストラクトファクトリー
    /// </summary>
    public interface IMMDModelPartFactory
    {
        /// <summary>
        /// モデルパーツの作成
        /// </summary>
        /// <param name="triangleCount">ポリゴン数</param>
        /// <param name="Vertices">頂点一覧</param>
        /// <param name="OpaqueData">不透明データ。インデックスバッファなどが含まれる</param>
        /// <returns>モデルパーツ</returns>
        IMMDModelPart Create(int triangleCount, MMDVertexNm[] Vertices, Dictionary<string, object> OpaqueData);
    }
}
