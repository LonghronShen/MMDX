using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Model;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MikuMikuDance.XNA.Misc;

namespace MikuMikuDance.XNA.Model
{
    class MMDXBoxFaceManager : IMMDFaceManager
    {
        //表情一覧
        float[] faceRates = new float[100];
        //表情情報を入れた頂点バッファ
        VertexBuffer vertexBuffer;
        public float[] FaceRates { get { return faceRates; } }
        public VertexBuffer FaceVertBuffer { get { return vertexBuffer; } }
        //表情と番号の対応
        Dictionary<string, int> FaceDict = new Dictionary<string, int>();
        //データ持っておく
        VertexFaceVert[] vertData;
        #region IMMDFaceManager メンバー

        public bool ContainsKey(string facename)
        {
            return FaceDict.ContainsKey(facename);
        }

        public int Count
        {
            get { return FaceDict.Count; }
        }

        public float this[string facename]
        {
            get
            {
                return faceRates[FaceDict[facename]];
            }
            set
            {
                faceRates[FaceDict[facename]] = value;
            }
        }
        public void Update() { }//なにもしない
        #endregion
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="vertData">表情データ</param>
        /// <param name="FaceDict">表情辞書</param>
        public MMDXBoxFaceManager(Vector4[] vertData, Dictionary<string, int> FaceDict)
        {
            this.FaceDict = FaceDict;
            this.vertData = new VertexFaceVert[vertData.Length];
            for (int i = 0; i < this.vertData.Length; ++i)
                this.vertData[i].FaceData = vertData[i];
        }

        public void SetUp(GraphicsDevice graphics)
        {
            vertexBuffer = new VertexBuffer(graphics, typeof(VertexFaceVert), vertData.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertData);
        }
        
    }
}
