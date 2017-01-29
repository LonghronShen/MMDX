using System;
namespace MikuMikuDance.Core.Model
{
    /// <summary>
    /// 表情マネージャ
    /// </summary>
    public interface IMMDFaceManager
    {
        /// <summary>
        /// 表情があるかどうか
        /// </summary>
        /// <param name="facename">表情名</param>
        /// <returns>あればtrue</returns>
        bool ContainsKey(string facename);
        /// <summary>
        /// 表情数
        /// </summary>
        int Count { get; }
        /// <summary>
        /// 表情適用割合の取得/設定
        /// </summary>
        /// <param name="facename">表情名</param>
        /// <returns>表情適用割合</returns>
        float this[string facename] { get; set; }
        /// <summary>
        /// 更新
        /// </summary>
        void Update();
    }
}
