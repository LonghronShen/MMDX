using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MikuMikuDance.Core.Model;
using MikuMikuDance.Core.Model.Physics;
using SlimDX;
using MikuMikuDance.Core.Misc;
using SlimDX.Direct3D9;
using MikuMikuDance.Resource;
using System.IO;
using System.Collections.ObjectModel;

namespace MikuMikuDance.SlimDX.Model
{
    /// <summary>
    /// SlimDX版MMDModelのパーツファクトリー
    /// </summary>
    public class MMDModelFactory : IMMDModelFactory
    {
        private string BuildPath(string modelAbsPath, string resourcePath)
        {
            if (Path.IsPathRooted(resourcePath))
                return resourcePath;
            string dir = Path.GetDirectoryName(modelAbsPath);
            return Path.Combine(dir, resourcePath);
        }

        #region IMMDModelFactory メンバー
        /// <summary>
        /// ファイルからモデルを生成
        /// </summary>
        /// <param name="filename">.pmdファイル名</param>
        /// <param name="opaqueData">不透明データ</param>
        /// <returns>MMDModel</returns>
        /// <remarks>不透明データには"scale"キーで拡大率を入れることができる。ただし、拡大/縮小した際、物理演算の細かいバランスが崩れる場合がある</remarks>
        public MMDModel Load(string filename, Dictionary<string, object> opaqueData)
        {
            SlimMMDModel result;
            MMDRigid[] rigids;
            MMDJoint[] joints;
            List<IMMDModelPart> modelParts;
            MMDBoneManager boneManager;
            MMDFaceManager faceManager;
            string absPath = Path.GetFullPath(filename);
            float scale = 1f;
            if (opaqueData.ContainsKey("scale"))
                scale = (float)opaqueData["scale"];
            MikuMikuDance.Model.Ver1.MMDModel1 iomodel = (MikuMikuDance.Model.Ver1.MMDModel1)MikuMikuDance.Model.ModelManager.Read(absPath, MikuMikuDance.Model.CoordinateType.RightHandedCoordinate, scale);
            MMDVertexNmTx[] vertex=CreateVertex(iomodel.Vertexes);
            modelParts = CreateModelParts(iomodel, absPath, opaqueData);
            boneManager = CreateBoneManager(iomodel);
            faceManager = CreateFaceManager(iomodel);
            CreatePhysics(iomodel, out rigids, out joints);

            result = new SlimMMDModel(vertex, modelParts, boneManager,faceManager, new Dictionary<string, Core.Motion.MMDMotion>(), rigids, joints);
            return result;
        }

        #endregion

        private MMDVertexNmTx[] CreateVertex(MikuMikuDance.Model.Ver1.ModelVertex[] vertexes)
        {
            MMDVertexNmTx[] result = new MMDVertexNmTx[vertexes.LongLength];
            for (long i = 0; i < vertexes.LongLength; ++i)
            {
                result[i] = new MMDVertexNmTx();
                result[i].BlendIndexX = vertexes[i].BoneNum[0];
                result[i].BlendIndexY = vertexes[i].BoneNum[1];
                result[i].BlendWeights = new Vector2((float)vertexes[i].BoneWeight / 100.0f, 1.0f - (float)vertexes[i].BoneWeight / 100.0f);
                result[i].Normal = MMDXMath.ToVector3(vertexes[i].NormalVector);
                result[i].Position = MMDXMath.ToVector3(vertexes[i].Pos);
                result[i].TextureCoordinate = MMDXMath.ToVector2(vertexes[i].UV);
            }

            return result;
        }

        internal List<IMMDModelPart> CreateModelParts(MikuMikuDance.Model.Ver1.MMDModel1 iomodel, string modelAbsPath, Dictionary<string, object> opaqueData)
        {
            List<IMMDModelPart> result = new List<IMMDModelPart>();
            int indicesStart = 0;

            //インデックスバッファ
            IndexBuffer indexbuffer = new IndexBuffer(SlimMMDXCore.Instance.Device, (int)(sizeof(ushort) * iomodel.FaceVertexes.Length), Usage.WriteOnly, Pool.Managed, true);
            DataStream stream = indexbuffer.Lock(0, (int)(sizeof(ushort) * iomodel.FaceVertexes.Length), LockFlags.None);
            stream.WriteRange(iomodel.FaceVertexes, indicesStart, (int)iomodel.FaceVertexes.Length);
            indexbuffer.Unlock();
            

            for (long materialId = 0; materialId < iomodel.Materials.LongLength; ++materialId)
            {
                Effect effect = null;
                MikuMikuDance.Model.Ver1.ModelMaterial material = iomodel.Materials[materialId];
                //エフェクトの読み込み
                if (opaqueData.ContainsKey("Effect") && opaqueData["Effect"] is string)
                {
                    string file = Path.GetFullPath((string)opaqueData["Effect"]);
                    if (File.Exists(file))
                    {
                        effect = Effect.FromFile(SlimMMDXCore.Instance.Device, file,
#if DEBUG
 ShaderFlags.SkipOptimization | ShaderFlags.Debug
#else
                            ShaderFlags.OptimizationLevel3
#endif
);
                    }
                }
                if (effect == null)
                {
                    effect = Effect.FromMemory(SlimMMDXCore.Instance.Device, MMDXResource.MMDWinEffect,
#if DEBUG
 ShaderFlags.SkipOptimization | ShaderFlags.Debug
#else
                        ShaderFlags.OptimizationLevel3
#endif
);
                }
                //マテリアル設定
                effect.SetValue<Vector3>("DiffuseColor", MMDXMath.ToVector3(material.DiffuseColor));
                effect.SetValue("Alpha", material.Alpha);
                effect.SetValue("EmissiveColor", MMDXMath.ToVector3(material.MirrorColor));
                effect.SetValue("SpecularColor", MMDXMath.ToVector3(material.SpecularColor));
                effect.SetValue("SpecularPower", material.Specularity);
                int shaderIndex;
                //テクスチャ設定
                if (!string.IsNullOrEmpty(material.TextureFileName))
                {
                    //テクスチャを読み込んではめ込み
                    string fulltexPath = BuildPath(modelAbsPath, material.TextureFileName);
                    if (!File.Exists(fulltexPath))
                        throw new FileNotFoundException("テクスチャファイルが見つかりません", fulltexPath);
                    Texture tex = Texture.FromFile(SlimMMDXCore.Instance.Device, fulltexPath, Usage.None, Pool.Managed);
                    effect.SetTexture("Texture", tex);
                    shaderIndex = 2;//本来pmd以外のモデルを考慮すべきだが、('A`)ﾏﾝﾄﾞｸｾ
                }
                else
                {
                    shaderIndex = 0;
                }
                if (!string.IsNullOrEmpty(material.SphereTextureFileName))
                {
                    if (Path.GetExtension(material.SphereTextureFileName) == ".sph")
                    {
                        effect.SetValue("UseSphere", 1);
                    }
                    else if (Path.GetExtension(material.SphereTextureFileName) == ".spa")
                    {
                        effect.SetValue("UseSphere", 2);
                    }
                    else
                        throw new MMDXException("スフィアマップファイルは.sphまたは.spa拡張子である必要があります");
                    //テクスチャを読み込んではめ込み
                    Texture tex = Texture.FromFile(SlimMMDXCore.Instance.Device, BuildPath(modelAbsPath, material.SphereTextureFileName), Usage.None, Pool.Managed);
                    effect.SetTexture("Sphere", tex);
                }
                else
                {
                    effect.SetValue("UseSphere", 0);
                }
                //トゥーン設定
                //トゥーンを読み込み
                string toonPath = ToonTexManager.GetToonTexPath(material.ToonIndex, iomodel.ToonFileNames, modelAbsPath);
                if (!string.IsNullOrEmpty(toonPath))
                {
                    string fulltexPath = BuildPath(modelAbsPath, toonPath);
                    if (!File.Exists(fulltexPath))
                        throw new FileNotFoundException("スフィアマップファイル:" + fulltexPath + "が見つかりません", fulltexPath);
                    Texture tex = Texture.FromFile(SlimMMDXCore.Instance.Device, BuildPath(modelAbsPath, toonPath), Usage.None, Pool.Managed);
                    effect.SetTexture("ToonTex", tex);
                    effect.SetValue("UseToon", true);
                }
                else
                {
                    effect.SetValue("UseToon", false);
                }
                //シェーダインデックス設定
                effect.SetValue("ShaderIndex", shaderIndex);
                result.Add(new MMDModelPart(iomodel.Vertexes.Length,indicesStart, (int)(material.FaceVertCount / 3), effect, indexbuffer));
                indicesStart += (int)material.FaceVertCount;
            }
            return result;
        }

        

        private MMDBoneManager CreateBoneManager(MikuMikuDance.Model.Ver1.MMDModel1 iomodel)
        {
            MMDBoneManager result;
            List<MMDBone> bones = new List<MMDBone>();
            List<MMDIK> iks = new List<MMDIK>();
            Matrix[] absPoses = new Matrix[iomodel.Bones.LongLength];
            //各ボーンの絶対座標を計算
            for (long i = 0; i < iomodel.Bones.LongLength; ++i)
            {
                Matrix.Translation(iomodel.Bones[i].BoneHeadPos[0],
                                iomodel.Bones[i].BoneHeadPos[1],
                                iomodel.Bones[i].BoneHeadPos[2],
                                out absPoses[i]);
            }
            for (long i = 0; i < iomodel.Bones.LongLength; ++i)
            {
                Matrix localMatrix;
                if (iomodel.Bones[i].ParentBoneIndex != 0xffff)
                {
                    Matrix parentInv;
                    Matrix.Invert(ref absPoses[iomodel.Bones[i].ParentBoneIndex], out parentInv);
                    Matrix.Multiply(ref parentInv, ref absPoses[i], out localMatrix);
                }
                else
                {
                    localMatrix = absPoses[i];
                }
                SQTTransform bindPose = SQTTransform.FromMatrix(localMatrix);
                Matrix inverseBindPose;
                Matrix.Invert(ref absPoses[i], out inverseBindPose);
                bones.Add(new MMDBone(iomodel.Bones[i].BoneName, bindPose, inverseBindPose, iomodel.Bones[i].ParentBoneIndex));
            }
            for (long i = 0; i < iomodel.IKs.LongLength; ++i)
            {
                List<int> ikChildBones = new List<int>();
                foreach (var ikc in iomodel.IKs[i].IKChildBoneIndex)
                    ikChildBones.Add(ikc);
                iks.Add(new MMDIK(iomodel.IKs[i].IKBoneIndex, iomodel.IKs[i].IKTargetBoneIndex, iomodel.IKs[i].Iterations, iomodel.IKs[i].AngleLimit, ikChildBones));
            }
            //ボーンインデックス→ボーンオブジェクト化
            SkinningHelpers.IKSetup(iks, bones);
            
            result = new MMDBoneManager(bones, iks);
            return result;
        }
        private MMDFaceManager CreateFaceManager(MikuMikuDance.Model.Ver1.MMDModel1 pmdmodel)
        {
            Dictionary<string, SkinVertSet[]> vertData = new Dictionary<string, SkinVertSet[]>();
            for (int i = 0; i < pmdmodel.Skins.Length; ++i)
            {
                List<SkinVertSet> list = new List<SkinVertSet>();
                for (long j = 0; j < pmdmodel.Skins[i].SkinVertDatas.LongLength; ++j)
                {
                    list.Add(new SkinVertSet { index = (int)pmdmodel.Skins[i].SkinVertDatas[j].SkinVertIndex, vector = MMDXMath.ToVector3(pmdmodel.Skins[i].SkinVertDatas[j].SkinVertPos) });
                }
                vertData.Add(pmdmodel.Skins[i].SkinName, list.ToArray());
            }

            return new MMDFaceManager(vertData);

        }
        
        private void CreatePhysics(MikuMikuDance.Model.Ver1.MMDModel1 iomodel, out MMDRigid[] rigids, out MMDJoint[] joints)
        {
            if (iomodel.RigidBodies==null || iomodel.Joints==null)
            {
                rigids = new MMDRigid[0];
                joints = new MMDJoint[0];
                return;
            }
            rigids = new MMDRigid[iomodel.RigidBodies.LongLength];
            joints = new MMDJoint[iomodel.Joints.LongLength];
            for (long i = 0; i < iomodel.RigidBodies.LongLength; ++i)
            {
                MMDRigid rigid = new MMDRigid();
                rigid.AngularDamping = iomodel.RigidBodies[i].AngularDamping;
                rigid.Friction = iomodel.RigidBodies[i].Friction;
                rigid.GroupIndex = iomodel.RigidBodies[i].GroupIndex;
                rigid.GroupTarget = iomodel.RigidBodies[i].GroupTarget;
                rigid.LinerDamping = iomodel.RigidBodies[i].LinerDamping;
                rigid.Name = iomodel.RigidBodies[i].Name;
                rigid.Position = new float[3];
                Array.Copy(iomodel.RigidBodies[i].Position,rigid.Position,3);
                if (iomodel.RigidBodies[i].RelatedBoneIndex < iomodel.Bones.LongLength)
                    rigid.RelatedBoneName = iomodel.Bones[iomodel.RigidBodies[i].RelatedBoneIndex].BoneName;
                else
                    rigid.RelatedBoneName = null;
                rigid.Restitution = iomodel.RigidBodies[i].Restitution;
                rigid.Rotation = new float[3];
                Array.Copy(iomodel.RigidBodies[i].Rotation, rigid.Rotation, 3);
                rigid.ShapeDepth = iomodel.RigidBodies[i].ShapeDepth;
                rigid.ShapeHeight = iomodel.RigidBodies[i].ShapeHeight;
                rigid.ShapeType = iomodel.RigidBodies[i].ShapeType;
                rigid.ShapeWidth = iomodel.RigidBodies[i].ShapeWidth;
                rigid.Type = iomodel.RigidBodies[i].Type;
                rigid.Weight = iomodel.RigidBodies[i].Weight;
                rigids[i] = rigid;
            }
            for (long i = 0; i < iomodel.Joints.LongLength; ++i)
            {
                MMDJoint joint = new MMDJoint();
                joint.ConstrainPosition1 = new float[3];
                Array.Copy(iomodel.Joints[i].ConstrainPosition1, joint.ConstrainPosition1, 3);
                joint.ConstrainPosition2 = new float[3];
                Array.Copy(iomodel.Joints[i].ConstrainPosition2, joint.ConstrainPosition2, 3);
                joint.ConstrainRotation1 = new float[3];
                Array.Copy(iomodel.Joints[i].ConstrainRotation1, joint.ConstrainRotation1, 3);
                joint.ConstrainRotation2 = new float[3];
                Array.Copy(iomodel.Joints[i].ConstrainRotation2, joint.ConstrainRotation2, 3);
                joint.Name = iomodel.Joints[i].Name;
                joint.Position = new float[3];
                Array.Copy(iomodel.Joints[i].Position, joint.Position, 3);
                joint.RigidBodyA = iomodel.Joints[i].RigidBodyA;
                joint.RigidBodyB = iomodel.Joints[i].RigidBodyB;
                joint.Rotation = new float[3];
                Array.Copy(iomodel.Joints[i].Rotation, joint.Rotation, 3);
                joint.SpringPosition = new float[3];
                Array.Copy(iomodel.Joints[i].SpringPosition, joint.SpringPosition, 3);
                joint.SpringRotation = new float[3];
                Array.Copy(iomodel.Joints[i].SpringRotation, joint.SpringRotation, 3);

                MMDXMath.CheckMinMax(joint.ConstrainPosition1, joint.ConstrainPosition2);
                MMDXMath.CheckMinMax(joint.ConstrainRotation1, joint.ConstrainRotation2);

                joints[i] = joint;
            }
        }
    }
}
