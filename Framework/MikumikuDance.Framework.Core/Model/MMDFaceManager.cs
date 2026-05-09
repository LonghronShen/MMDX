using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Misc;
#if !XBOX
using System.Threading.Tasks;
using MikumikuDance.Framework.Abstractions;
#endif

namespace MikuMikuDance.Core.Model
{
    /// <summary>
    /// 表情頂点データ
    /// </summary>
    public class SkinVertSet
    {
        /// <summary>
        /// 頂点インデックス/baseインデックス
        /// </summary>
        public int index;
        /// <summary>
        /// デフォルト頂点位置/移動量
        /// </summary>
        /// <remarks>詳しくはPMDのフォーマットを参照</remarks>
        public MMDVector3 vector;
    }
    /// <summary>
    /// 表情マネージャ
    /// </summary>
    public class MMDFaceManager : IMMDFaceManager
    {
        Dictionary<int, MMDVector3> updateVerts;

        /// <summary>
        /// 表情とその適応割合
        /// </summary>
        protected Dictionary<string, float[]> faceRates;
        //SkinVertSetは最初baseにする
        /// <summary>
        /// 頂点情報
        /// </summary>
        protected Dictionary<string, SkinVertSet[]> vertData ;

        /// <summary>
        /// 不透明データへの参照（DI対応、null時はMMDCore.Current.OpaqueDataを使用）
        /// </summary>
        protected Dictionary<string, object> OpaqueData { get; set; }

        /// <summary>
        /// 不透明データを解決する
        /// </summary>
        private Dictionary<string, object> ResolveOpaqueData()
        {
            if (OpaqueData != null) return OpaqueData;
            if (MMDCore.Current != null) return MMDCore.Current.OpaqueData;
            return null;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="vertData">頂点データ</param>
        /// <param name="opaqueData">不透明データ（省略時はMMDCore.Current.OpaqueData）</param>
        public MMDFaceManager(Dictionary<string, SkinVertSet[]> vertData, Dictionary<string, object> opaqueData = null)
        {
            this.OpaqueData = opaqueData;
            this.vertData = vertData;
            this.faceRates = new Dictionary<string, float[]>();
            if (vertData != null && vertData.ContainsKey("base"))
            {
                foreach (var it in vertData)
                    if (it.Key != "base")
                        this.faceRates.Add(it.Key, new float[2] { 0.0f, 0.0f });
                updateVerts = new Dictionary<int, MMDVector3>(vertData["base"].Length);
            }
            else
            {
                updateVerts = new Dictionary<int, MMDVector3>();
            }
        }
        /// <summary>
        /// 表情適用割合の取得/設定
        /// </summary>
        /// <param name="facename">表情名</param>
        /// <returns>表情適用割合</returns>
        public float this[string facename]
        {
            get
            {
                return faceRates[facename][0];
            }
            set
            {
                faceRates[facename][0] = value;
            }
        }
        /// <summary>
        /// 表情数
        /// </summary>
        public int Count
        {
            get
            {
                return faceRates.Count;
            }
        }
        /// <summary>
        /// 表情があるかどうか
        /// </summary>
        /// <param name="facename">表情名</param>
        /// <returns>あればtrue</returns>
        public bool ContainsKey(string facename)
        {
            return faceRates.ContainsKey(facename);
        }
        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update()
        {
#if !XBOX
            Dictionary<string, object> opaque = ResolveOpaqueData();
            bool strictFaceVert = opaque != null && opaque.ContainsKey("StrictFaceVert");
            updateVerts.Clear();
            //更新する頂点とその量をリストアップ
            foreach (var facerate in faceRates)
            {
                if (Math.Abs(facerate.Value[0] - facerate.Value[1]) > 0.05f ||
                    strictFaceVert)
                {//変化をキャッチ
                    foreach (var skinvert in vertData[facerate.Key])
                    {//その分を適用
                        MMDVector3 v = MMDVector3.Zero, vmove, vnext;
                        if (strictFaceVert)
                            MMDVector3.Multiply(ref skinvert.vector, facerate.Value[0], out vmove);
                        else
                            MMDVector3.Multiply(ref skinvert.vector, facerate.Value[0] - facerate.Value[1], out vmove);
                        if (updateVerts.ContainsKey(skinvert.index))
                        {
                            v = updateVerts[skinvert.index];
                            MMDVector3.Add(ref v, ref vmove, out vnext);
                            updateVerts[skinvert.index] = vnext;
                        }
                        else
                            updateVerts.Add(skinvert.index, vmove);
                    }
                    facerate.Value[1] = facerate.Value[0];
                }

            }
#endif
        }
#if !XBOX
        /// <summary>
        /// 頂点に表情によるモーフィングを適用
        /// </summary>
        /// <param name="faceManager">表情マネージャ</param>
        /// <param name="vert">頂点</param>
        /// <param name="indices">PMD→MMDXの頂点変換マップ</param>
        public static void ApplyToVertex(MMDFaceManager faceManager, MMDVertexNm[] vert, Dictionary<long,int[]> indices      = null)
        {
            if (!faceManager.vertData.ContainsKey("base"))
                return;
            Dictionary<string, object> opaque = faceManager.ResolveOpaqueData();
            bool strictFaceVert = opaque != null && opaque.ContainsKey("StrictFaceVert");
            if (strictFaceVert)
            {
                foreach (var skinvert in faceManager.vertData["base"])
                {
                    if (indices == null)
                    {
                        vert[skinvert.index].Position = skinvert.vector;
                    }
                    else
                    {
                        if (indices.ContainsKey(skinvert.index))
                        {
                            foreach (var it in indices[skinvert.index])
                            {
                                vert[it].Position += skinvert.vector;
                            }
                        }
                    }
                }
            }
            SkinVertSet[] baseVertSet = faceManager.vertData["base"];
            foreach (var updateVert in faceManager.updateVerts)
            {
                SkinVertSet baseVert = baseVertSet[updateVert.Key];
                //MMDVector3 Total = baseVert.vector + updateVert.Value;
                if (indices == null)
                {
                    vert[baseVert.index].Position += updateVert.Value;
                }
                else
                {
                    if (indices.ContainsKey(baseVert.index))
                    {
                        foreach (var it in indices[baseVert.index])
                        {
                            vert[it].Position += updateVert.Value;
                        }
                    }
                }
            }
            
        }
#endif
        
    }
}
