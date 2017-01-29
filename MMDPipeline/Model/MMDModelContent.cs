using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using MikuMikuDance.XNA.Motion;
using Microsoft.Xna.Framework.Content;
using MikuMikuDance.XNA.Misc;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.IO;
using MikuMikuDance.Resource;

namespace MikuMikuDance.XNA.Model
{
    /// <summary>
    /// MMDモデルクラス
    /// </summary>
    public class MMDModelContent
    {
        /// <summary>
        /// このモデルのメッシュパーツ
        /// </summary>
        public List<MMDModelPartContent> ModelParts = new List<MMDModelPartContent>();
        /// <summary>
        /// ボーンマネージャ
        /// </summary>
        public MMDBoneManagerContent BoneManager;
        /// <summary>
        /// 表情マネージャ
        /// </summary>
        public MMDFaceManagerContent FaceManager;
        /// <summary>
        /// 付随モーション
        /// </summary>
        public Dictionary<string, MMDMotionContent> AttachedMotionData;
        /// <summary>
        /// 剛体情報
        /// </summary>
        public MMDRigidContent[] Rigids;
        /// <summary>
        /// 間接情報
        /// </summary>
        public MMDJointContent[] Joints;
        /// <summary>
        /// エッジ描画用のエフェクト
        /// </summary>
        public static CompiledEffectContent EdgeEffect = null;
        /// <summary>
        /// モデルパーツ追加
        /// </summary>
        /// <param name="triangleCount">三角形の追加</param>
        /// <param name="indexCollection">インデックスコレクション</param>
        /// <param name="vertMap">元の頂点番号との対応表</param>
        /// <param name="vertices">頂点データ</param>
        /// <param name="material">マテリアルデータ</param>
        public void AddModelPart(int triangleCount, IndexCollection indexCollection, Dictionary<long, int[]> vertMap, MMDVertexNmContent[] vertices, MaterialContent material)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            ModelParts.Add(new MMDModelPartContent
            {
                TriangleCount = triangleCount,
                IndexCollection = indexCollection,
                VertMap = vertMap,
                Vertices = vertices,
                extVertices = new Microsoft.Xna.Framework.Vector2[vertices.Length],
                Material = material,
            });
        }
        /// <summary>
        /// 共用のエッジエフェクトの読み込み
        /// </summary>
        /// <param name="context">コンテントプロセッサー</param>
        public static void ReadEdgeEffect(ContentProcessorContext context)
        {
            if (MMDModelContent.EdgeEffect == null)
            {
                FileStream fs;
                fs = new FileStream(Path.Combine("ext", "MMDEdgeEffect.fx"), FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(MMDXResource.MMDEdgeEffect);
                bw.Close();

                EffectContent edgeEffect = context.BuildAndLoadAsset<EffectContent, EffectContent>(new ExternalReference<EffectContent>(Path.Combine("ext", "MMDEdgeEffect.fx")), null);
                MMDModelContent.EdgeEffect = context.Convert<EffectContent, CompiledEffectContent>(edgeEffect, "EffectProcessor");
            }
        }
    }
    
}
