using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// 表情セット
    /// </summary>
    [ContentSerializerRuntimeType("MikuMikuDance.Core.Model.SkinVertSet, MikuMikuDanceCore")]
    public class SkinVertSet
    {
        /// <summary>
        /// 頂点インデックス/baseインデックス
        /// </summary>
        public int index;
        /// <summary>
        /// 頂点位置の差分
        /// </summary>
        /// <remarks>base表情のみデフォルト頂点位置</remarks>
        public Vector3 vector;
    }

    /// <summary>
    /// 表情セット(XBox用)
    /// </summary>
    public class SkinVertSet2
    {
        /// <summary>
        /// 表情名
        /// </summary>
        public string FaceName;
        /// <summary>
        /// 頂点位置の差分
        /// </summary>
        /// <remarks>base表情のみデフォルト頂点位置</remarks>
        public Vector3 vector;
    }
    /// <summary>
    /// 表情ポインタデータ
    /// </summary>
    public class SkinVertPtr
    {
        /// <summary>
        /// 開始位置
        /// </summary>
        public int Pos;
        /// <summary>
        /// カウント数
        /// </summary>
        public int Count;
    }

    /// <summary>
    /// 表情マネージャ
    /// </summary>
    public class MMDFaceManagerContent
    {
        
        /// <summary>
        /// 頂点情報
        /// </summary>
        public Dictionary<string, SkinVertSet[]> vertData = new Dictionary<string, SkinVertSet[]>();
        /// <summary>
        /// 頂点情報2(XBox用)
        /// </summary>
        public Dictionary<long, SkinVertSet2[]> vertData2 = new Dictionary<long, SkinVertSet2[]>();
        /// <summary>
        /// 表情辞書(XBox用)
        /// </summary>
        public Dictionary<string, int> faceDict = new Dictionary<string, int>();
        /// <summary>
        /// 表情データ(XBox用)
        /// </summary>
        /// <remarks>頂点データの修正量(xyz)と表情番号(w)が入っている</remarks>
        public Vector4[] vertDataXBox = null;
        /// <summary>
        /// 表情データ(XBox用)のポインタ情報
        /// </summary>
        public Dictionary<long, SkinVertPtr> vertPtr = new Dictionary<long, SkinVertPtr>();

    }
}
