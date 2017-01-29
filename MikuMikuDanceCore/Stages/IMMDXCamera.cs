using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if XNA
using Microsoft.Xna.Framework;
#elif SlimDX
using SlimDX;
#endif

namespace MikuMikuDance.Core.Stages
{
    /// <summary>
    /// �J�������C���^�[�t�F�C�X
    /// </summary>
    public interface IMMDXCamera
    {
        /// <summary>
        /// �J�������
        /// </summary>
        /// <param name="aspectRatio">�A�X�y�N�g��</param>
        /// <param name="view">�r���[���</param>
        /// <param name="proj">�v���W�F�N�V�������</param>
        void GetCameraParam(float aspectRatio, out  Matrix view, out Matrix proj);
        /// <summary>
        /// �J�����ʒu
        /// </summary>
        Vector3 Position { get; set; }
        /// <summary>
        /// ��]�̐ݒ�
        /// </summary>
        /// <param name="rotate">��]</param>
        void SetRotation(Quaternion rotate);
        /// <summary>
        /// ����p�̐ݒ�/�擾
        /// </summary>
        float FieldOfView { get; set; }
        /// <summary>
        /// Near��
        /// </summary>
        float Near { get; set; }
        /// <summary>
        /// Far��
        /// </summary>
        float Far { get; set; }
        
        /// <summary>
        /// �J�����x�N�g���̐ݒ�
        /// </summary>
        /// <param name="newVector">�J�����x�N�g��</param>
        void SetVector(Vector3 newVector);
    }
}
