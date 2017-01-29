using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using MikuMikuDance.Model.Ver1;
using MikuMikuDance.XNA.Misc;
using Microsoft.Xna.Framework;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MikuMikuDance.XNA.Model
{
    class MMDModelScene
    {
        NodeContent root = new NodeContent();
        public NodeContent Root { get { return root; } }
        MMDRigidContent[] rigids;
        public MMDRigidContent[] Rigids { get { return rigids; } }
        MMDJointContent[] joints;
        public MMDJointContent[] Joints { get { return joints; } }
        MMDFaceManagerContent faceManager;
        public MMDFaceManagerContent FaceManager { get { return faceManager; } }
        private MMDModelScene() { }
        public static MMDModelScene Create(MMDModel1 model, string filename)
        {
            MMDModelScene result = new MMDModelScene();
            result.Build(model, filename);
            return result;
        }

        private void Build(MMDModel1 model, string filename)
        {
            ContentIdentity identity = new ContentIdentity(filename);
            //メッシュのビルド
            MeshContent mesh = MMDMeshBuilder.BuildMesh(model, filename);
            if (mesh != null)
                root.Children.Add(mesh);
            //ボーンのビルド
            BoneContent bone = BuildSkelton(model, identity);
            root.Children.Add(bone);
            //メッシュ以外の情報の整理
            BuildFace(model);
            BuildPhysics(model);
            //このクラスを不透明データに入れる
            root.OpaqueData.Add("MMDScene", this);
        }

        private BoneContent BuildSkelton(MMDModel1 model, ContentIdentity identity)
        {
            //ルートボーン生成(MMDのルートボーンはpmdの中に存在せず、0,0,0にあるこちらで用意する必要がある)
            BoneContent rootBone = new BoneContent();
            rootBone.Name = CreateRootBoneName(model);
            rootBone.Identity = identity;
            rootBone.Transform = Matrix.Identity;
            //pmdの構造合わせ用の変数
            BoneContent[] boneMap = new BoneContent[model.Bones.Length];
            Array.Clear(boneMap, 0, model.Bones.Length);
            try
            {//このルートボーンの子はUint16.MaxValue値になっている
                BuildSkelton(rootBone, UInt16.MaxValue, model, boneMap);
            }
            catch (StackOverflowException)
            {
                throw new InvalidContentException("ボーンがループしている");
            }
            //ボーンマップチェック
            foreach (var bone in boneMap)
                if (bone == null)
                    throw new InvalidContentException("ボーンマップ生成失敗。ルートとつながっていないボーン系統がある？");
            //MMDのボーン用タグを作成
            MMDBoneTag[] boneTags = new MMDBoneTag[model.Bones.Length];
            for (int i = 0; i < boneTags.Length; i++)
                boneTags[i] = new MMDBoneTag();
            //タグにIK情報を入れる
            for (int i = 0; i < model.IKs.Length; i++)
            {
                MMDBoneIKTag ikTag = new MMDBoneIKTag();
                ikTag.Iteration = model.IKs[i].Iterations;
                ikTag.ControlWeight = model.IKs[i].AngleLimit;
                ikTag.IKChildBones = new BoneContent[model.IKs[i].IKChildBoneIndex.Length];
                for (int j = 0; j < ikTag.IKChildBones.Length; j++)
                {
                    ikTag.IKChildBones[j] = boneMap[model.IKs[i].IKChildBoneIndex[j]];
                }
                ikTag.IKTargetBone = boneMap[model.IKs[i].IKTargetBoneIndex];
                //IK情報を対応するタグに挿入
                boneTags[model.IKs[i].IKBoneIndex].IKs.Add(ikTag);
            }
            //ボーンにMMD用のタグを挿入
            for (int i = 0; i < boneTags.Length; i++)
            {
                boneMap[i].OpaqueData.Add("MMDBoneTag", boneTags[i]);
            }
            //ボーン返却
            return rootBone;
        }

        //ボーンを再帰的に組み立てる
        private void BuildSkelton(BoneContent parentBone, ushort parentIndex, MMDModel1 model, BoneContent[] boneMap)
        {
            for (UInt16 boneIndex = 0; boneIndex < model.Bones.Length; boneIndex++)
            {
                if (model.Bones[boneIndex].ParentBoneIndex == parentIndex)
                {//子ボーン発見
                    BoneContent bone = new BoneContent();
                    bone.Identity = parentBone.Identity;
                    bone.Name = model.Bones[boneIndex].BoneName;
                    //pmdのボーンは絶対座標。相対座標に直す
                    Matrix absTrans = Matrix.CreateTranslation(model.Bones[boneIndex].BoneHeadPos[0], model.Bones[boneIndex].BoneHeadPos[1], model.Bones[boneIndex].BoneHeadPos[2]);
                    bone.Transform = Matrix.Invert(parentBone.AbsoluteTransform) * absTrans;
                    //その他ボーン情報登録用にpmd→XNAの対応表を作成
                    boneMap[boneIndex] = bone;
                    //親に登録
                    parentBone.Children.Add(bone);
                    //再帰呼び出し
                    BuildSkelton(bone, boneIndex, model, boneMap);
                }
            }
        }

        private string CreateRootBoneName(MMDModel1 model)
        {
            //既存のボーンと被らないようにルート名を生成
            string result = "root";
            int i = 0;
            int aliasIndex = 1;
            while (i < model.Bones.Length)
            {
                if (model.Bones[i].BoneName == result)
                {
                    i = 0;
                    result = "root" + (aliasIndex++).ToString();
                    continue;
                }
                ++i;
            }
            return result;
        }
        
        

        private void BuildFace(MMDModel1 model)
        {
            //表情の格納
            faceManager = new MMDFaceManagerContent();
            //Windows用表情データ作成
            for (int i = 0; i < model.Skins.Length; ++i)
            {
                List<SkinVertSet> tempList = new List<SkinVertSet>();
                foreach (var it in model.Skins[i].SkinVertDatas)
                {
                    tempList.Add(new SkinVertSet { index = (int)it.SkinVertIndex, vector = MMDXMath.ToVector3(it.SkinVertPos) });
                }
                faceManager.vertData.Add(model.Skins[i].SkinName, tempList.ToArray());
            }
            
            
            int BaseFaceIndex = -1;
            Dictionary<long, List<SkinVertSet2>> vertTemp = new Dictionary<long, List<SkinVertSet2>>();
            for (int i = 0; i < model.Skins.Length; ++i)
            {
                if (model.Skins[i].SkinName != "base")
                {
                    faceManager.faceDict.Add(model.Skins[i].SkinName, faceManager.faceDict.Count);
                }
                else
                {
                    BaseFaceIndex = i;
                    for (long j = 0; j < model.Skins[i].SkinVertDatas.LongLength; ++j)
                    {
                        List<SkinVertSet2> list = new List<SkinVertSet2>();
                        list.Add(new SkinVertSet2 { FaceName = model.Skins[i].SkinName, vector = MMDXMath.ToVector3(model.Skins[i].SkinVertDatas[j].SkinVertPos) });
                        vertTemp.Add(model.Skins[i].SkinVertDatas[j].SkinVertIndex, list);
                    }
                }
            }
            for (int i = 0; i < model.Skins.Length; ++i)
            {
                if (model.Skins[i].SkinName != "base")
                {
                    for (long j = 0; j < model.Skins[i].SkinVertDatas.LongLength; ++j)
                    {
                        vertTemp[model.Skins[BaseFaceIndex].SkinVertDatas[model.Skins[i].SkinVertDatas[j].SkinVertIndex].SkinVertIndex].Add(
                            new SkinVertSet2 { FaceName = model.Skins[i].SkinName, vector = MMDXMath.ToVector3(model.Skins[i].SkinVertDatas[j].SkinVertPos) });
                    }
                }
            }
            foreach (var it in vertTemp)
            {
                faceManager.vertData2.Add(it.Key, it.Value.ToArray());
            }
            int pos = 0;
            faceManager.vertDataXBox = new Vector4[(from it in faceManager.vertData select it.Value.Length - 1).Sum()];
            //XBox用にビルド
            foreach (var it in faceManager.vertData2)
            {
                SkinVertPtr ptr = new SkinVertPtr();
                ptr.Pos = pos;
                int i = 0;
                for (int j = 0; j < it.Value.Length; ++j)
                {
                    if (it.Value[j].FaceName != "base")
                    {
                        faceManager.vertDataXBox[pos + i] = new Vector4(it.Value[j].vector, faceManager.faceDict[it.Value[j].FaceName]);
                        ++i;
                    }
                }
                ptr.Count = i;
                faceManager.vertPtr.Add(it.Key, ptr);
                pos += i;
            }
        }

        private void BuildPhysics(MMDModel1 model)
        {
            rigids = new MMDRigidContent[0];
            if (model.RigidBodies != null)
            {
                rigids = new MMDRigidContent[model.RigidBodies.LongLength];
                for (UInt32 i = 0; i < model.RigidBodies.LongLength; i++)
                {
                    MMDRigidContent rigid = new MMDRigidContent();
                    rigid.AngularDamping = model.RigidBodies[i].AngularDamping;
                    rigid.Friction = model.RigidBodies[i].Friction;
                    rigid.GroupIndex = model.RigidBodies[i].GroupIndex;
                    rigid.GroupTarget = model.RigidBodies[i].GroupTarget;
                    rigid.LinerDamping = model.RigidBodies[i].LinerDamping;
                    rigid.Name = model.RigidBodies[i].Name;
                    rigid.Position = new float[model.RigidBodies[i].Position.Length];
                    for (int j = 0; j < rigid.Position.Length; j++)
                        rigid.Position[j] = model.RigidBodies[i].Position[j];
                    if (model.RigidBodies[i].RelatedBoneIndex < model.Bones.LongLength)
                        rigid.RelatedBoneName = model.Bones[model.RigidBodies[i].RelatedBoneIndex].BoneName;
                    else
                        rigid.RelatedBoneName = null;
                    rigid.Restitution = model.RigidBodies[i].Restitution;
                    rigid.Rotation = new float[model.RigidBodies[i].Rotation.Length];
                    for (int j = 0; j < rigid.Rotation.Length; j++)
                        rigid.Rotation[j] = model.RigidBodies[i].Rotation[j];
                    rigid.ShapeDepth = model.RigidBodies[i].ShapeDepth;
                    rigid.ShapeHeight = model.RigidBodies[i].ShapeHeight;
                    rigid.ShapeType = model.RigidBodies[i].ShapeType;
                    rigid.ShapeWidth = model.RigidBodies[i].ShapeWidth;
                    rigid.Type = model.RigidBodies[i].Type;
                    rigid.Weight = model.RigidBodies[i].Weight;
                    rigids[i] = rigid;
                }
            }
            joints = new MMDJointContent[0];
            if (model.Joints != null)
            {
                joints = new MMDJointContent[model.Joints.LongLength];
                for (UInt32 i = 0; i < model.Joints.LongLength; i++)
                {
                    MMDJointContent joint = new MMDJointContent();
                    joint.ConstrainPosition1 = new float[model.Joints[i].ConstrainPosition1.Length];
                    for (int j = 0; j < joint.ConstrainPosition1.Length; j++)
                        joint.ConstrainPosition1[j] = model.Joints[i].ConstrainPosition1[j];
                    joint.ConstrainPosition2 = new float[model.Joints[i].ConstrainPosition2.Length];
                    for (int j = 0; j < joint.ConstrainPosition2.Length; j++)
                        joint.ConstrainPosition2[j] = model.Joints[i].ConstrainPosition2[j];
                    joint.ConstrainRotation1 = new float[model.Joints[i].ConstrainRotation1.Length];
                    for (int j = 0; j < joint.ConstrainRotation1.Length; j++)
                        joint.ConstrainRotation1[j] = model.Joints[i].ConstrainRotation1[j];
                    joint.ConstrainRotation2 = new float[model.Joints[i].ConstrainRotation2.Length];
                    for (int j = 0; j < joint.ConstrainRotation2.Length; j++)
                        joint.ConstrainRotation2[j] = model.Joints[i].ConstrainRotation2[j];
                    MMDXMath.CheckMinMax(joint.ConstrainPosition1, joint.ConstrainPosition2);
                    MMDXMath.CheckMinMax(joint.ConstrainRotation1, joint.ConstrainRotation2);

                    joint.Name = model.Joints[i].Name;
                    joint.Position = new float[model.Joints[i].Position.Length];
                    for (int j = 0; j < joint.Position.Length; j++)
                        joint.Position[j] = model.Joints[i].Position[j];
                    joint.RigidBodyA = model.Joints[i].RigidBodyA;
                    joint.RigidBodyB = model.Joints[i].RigidBodyB;
                    joint.Rotation = new float[model.Joints[i].Rotation.Length];
                    for (int j = 0; j < joint.Rotation.Length; j++)
                        joint.Rotation[j] = model.Joints[i].Rotation[j];
                    joint.SpringPosition = new float[model.Joints[i].SpringPosition.Length];
                    for (int j = 0; j < joint.SpringPosition.Length; j++)
                        joint.SpringPosition[j] = model.Joints[i].SpringPosition[j];
                    joint.SpringRotation = new float[model.Joints[i].SpringRotation.Length];
                    for (int j = 0; j < joint.SpringRotation.Length; j++)
                        joint.SpringRotation[j] = model.Joints[i].SpringRotation[j];
                    joints[i] = joint;
                }
            }
            
        }

        
        
        
    }
}
