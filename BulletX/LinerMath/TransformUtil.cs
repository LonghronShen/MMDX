using System;

namespace BulletX.LinerMath
{
    static class TransformUtil
    {
        public const float ANGULAR_MOTION_THRESHOLD = (float)(0.5 * BulletGlobal.SIMD_HALF_PI);

        public static void integrateTransform(btTransform curTrans,btVector3 linvel, btVector3 angvel,   float timeStep,out btTransform predictedTransform)
        {
            predictedTransform = btTransform.Identity;
            #region predictedTransform.Origin=curTrans.Origin + linvel * timeStep;
            {
                btVector3 temp;
                btVector3.Multiply(ref linvel, timeStep, out temp);
                btVector3.Add(ref curTrans.Origin, ref temp, out predictedTransform.Origin);
            }
            #endregion
            //Exponential map
            //google for "Practical Parameterization of Rotations Using the Exponential Map", F. Sebastian Grassia

            btVector3 axis;
            float fAngle = angvel.Length;
            //limit the angular motion
            if (fAngle * timeStep > ANGULAR_MOTION_THRESHOLD)
            {
                fAngle = ANGULAR_MOTION_THRESHOLD / timeStep;
            }

            if (fAngle < 0.001f)
            {
                // use Taylor's expansions of sync function
                #region axis = angvel * (0.5f * timeStep - (timeStep * timeStep * timeStep) * (0.020833333333f) * fAngle * fAngle);
                {
                    btVector3.Multiply(ref angvel, (0.5f * timeStep - (timeStep * timeStep * timeStep) * (0.020833333333f) * fAngle * fAngle), out axis);
                }
                #endregion
            }
            else
            {
                // sync(fAngle) = sin(c*fAngle)/t
                #region axis = angvel * ((float)Math.Sin(0.5f * fAngle * timeStep) / fAngle);
                btVector3.Multiply(ref angvel, ((float)Math.Sin(0.5f * fAngle * timeStep) / fAngle), out axis);
                #endregion
            }
            btQuaternion dorn = new btQuaternion(axis.X, axis.Y, axis.Z, (float)Math.Cos(fAngle * timeStep * 0.5f));
            btQuaternion orn0 = curTrans.Rotation;

            btQuaternion predictedOrn;
            btQuaternion.Multiply(ref dorn, ref orn0, out predictedOrn);
            predictedOrn.normalize();
            predictedTransform.Rotation = predictedOrn;
        }
        public static void	calculateVelocity(btTransform transform0,btTransform transform1,float timeStep,out btVector3 linVel, out btVector3 angVel)
	    {
		    linVel = (transform1.Origin - transform0.Origin) / timeStep;
		    btVector3 axis;
		    float  angle;
		    calculateDiffAxisAngle(transform0,transform1,out axis,out angle);
		    angVel = axis * angle / timeStep;
	    }
        public static void calculateDiffAxisAngle(btTransform transform0,btTransform transform1,out btVector3 axis,out float angle)
        {
            btMatrix3x3 dmat;// = transform1.Basis * transform0.Basis.inverse();
            {
                btMatrix3x3 temp;
                transform0.Basis.inverse(out temp);
                btMatrix3x3.Multiply(ref transform1.Basis, ref temp, out dmat);
            }
            btQuaternion dorn;
            dmat.getRotation(out dorn);

            ///floating point inaccuracy can lead to w component > 1..., which breaks 
            dorn.normalize();

            angle = dorn.getAngle();
            axis = new btVector3(dorn.X, dorn.Y, dorn.Z);
            axis.W = 0f;
            //check for axis length
            float len = axis.Length2;
            if (len < BulletGlobal.SIMD_EPSILON * BulletGlobal.SIMD_EPSILON)
                axis = new btVector3(1f, 0f, 0f);
            else
                axis /= (float)Math.Sqrt(len);
        }
    }
}
