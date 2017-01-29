using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if XNA
using Microsoft.Xna.Framework;
#elif SlimDX
using SlimDX;
#endif
using MikuMikuDance.Core.Misc;
#if !XBOX
using System.Threading.Tasks;
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
        public Vector3 vector;
    }
    /// <summary>
    /// 表情マネージャ
    /// </summary>
    public class MMDFaceManager : IMMDFaceManager
    {
        Dictionary<int, Vector3> updateVerts;

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
        /// コンストラクタ
        /// </summary>
        /// <param name="vertData">頂点データ</param>
        public MMDFaceManager(Dictionary<string, SkinVertSet[]> vertData)
        {
            this.vertData = vertData;
            this.faceRates = new Dictionary<string, float[]>();
            if (vertData != null && vertData.ContainsKey("base"))
            {
                foreach (var it in vertData)
                    if (it.Key != "base")
                        this.faceRates.Add(it.Key, new float[2] { 0.0f, 0.0f });
                updateVerts = new Dictionary<int, Vector3>(vertData["base"].Length);
            }
            else
            {
                updateVerts = new Dictionary<int, Vector3>();
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
            updateVerts.Clear();
            //更新する頂点とその量をリストアップ
            foreach (var facerate in faceRates)
            {
                if (Math.Abs(facerate.Value[0] - facerate.Value[1]) > 0.05f ||
                    MMDCore.Instance.OpaqueData.ContainsKey("StrictFaceVert"))
                {//変化をキャッチ
                    foreach (var skinvert in vertData[facerate.Key])
                    {//その分を適用
                        Vector3 v = Vector3.Zero, vmove, vnext;
                        if (MMDCore.Instance.OpaqueData.ContainsKey("StrictFaceVert"))
                            Vector3.Multiply(ref skinvert.vector, facerate.Value[0], out vmove);
                        else
                            Vector3.Multiply(ref skinvert.vector, facerate.Value[0] - facerate.Value[1], out vmove);
                        if (updateVerts.ContainsKey(skinvert.index))
                        {
                            v = updateVerts[skinvert.index];
                            Vector3.Add(ref v, ref vmove, out vnext);
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
            if (MMDCore.Instance.OpaqueData.ContainsKey("StrictFaceVert"))
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
                //Vector3 Total = baseVert.vector + updateVert.Value;
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
