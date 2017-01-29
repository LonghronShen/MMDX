using BulletX.LinerMath;

namespace BulletX.BulletDynamics.ConstraintSolver
{
    public class TranslationalLimitMotor
    {
        public btVector3 m_lowerLimit;//!< the constraint lower limits
        public btVector3 m_upperLimit;//!< the constraint upper limits
        public btVector3 m_accumulatedImpulse;
        //! Linear_Limit_parameters
        //!@{
        public float m_limitSoftness;//!< Softness for linear limit
        public float m_damping;//!< Damping for linear limit
        public float m_restitution;//! Bounce parameter for linear limit
        public btVector3 m_normalCFM;//!< Constraint force mixing factor
        public btVector3 m_stopERP;//!< Error tolerance factor when joint is at limit
        public btVector3 m_stopCFM;//!< Constraint force mixing factor when joint is at limit
        //!@}
        public bool[] m_enableMotor = new bool[3];
        public btVector3 m_targetVelocity;//!< target motor velocity
        public btVector3 m_maxMotorForce;//!< max force on motor
        public btVector3 m_currentLimitError;//!  How much is violated this limit
        public btVector3 m_currentLinearDiff;//!  Current relative offset of constraint frames
        public int[] m_currentLimit = new int[3];//!< 0=free, 1=at lower limit, 2=at upper limit

        public TranslationalLimitMotor()
        {
            m_lowerLimit = new btVector3(0f, 0f, 0f);
            m_upperLimit = new btVector3(0f, 0f, 0f);
            m_accumulatedImpulse = new btVector3(0f, 0f, 0f);
            m_normalCFM = new btVector3(0f, 0f, 0f);
            m_stopERP = new btVector3(0.2f, 0.2f, 0.2f);
            m_stopCFM = new btVector3(0f, 0f, 0f);

            m_limitSoftness = 0.7f;
            m_damping = 1.0f;
            m_restitution = 0.5f;
            for (int i = 0; i < 3; i++)
            {
                m_enableMotor[i] = false;
            }
            m_targetVelocity = btVector3.Zero;
            m_maxMotorForce = btVector3.Zero;
        }
        public int testLimitValue(int limitIndex, float test_value)
        {
            float loLimit = m_lowerLimit[limitIndex];
            float hiLimit = m_upperLimit[limitIndex];
            if (loLimit > hiLimit)
            {
                m_currentLimit[limitIndex] = 0;//Free from violation
                m_currentLimitError[limitIndex] = 0f;
                return 0;
            }

            if (test_value < loLimit)
            {
                m_currentLimit[limitIndex] = 2;//low limit violation
                m_currentLimitError[limitIndex] = test_value - loLimit;
                return 2;
            }
            else if (test_value > hiLimit)
            {
                m_currentLimit[limitIndex] = 1;//High limit violation
                m_currentLimitError[limitIndex] = test_value - hiLimit;
                return 1;
            };

            m_currentLimit[limitIndex] = 0;//Free from violation
            m_currentLimitError[limitIndex] = 0f;
            return 0;
        }

        public bool isLimited(int limitIndex)
        {
            return (m_upperLimit[limitIndex] >= m_lowerLimit[limitIndex]);
        }

        public bool needApplyForce(int limitIndex)
        {
            if (m_currentLimit[limitIndex] == 0 && m_enableMotor[limitIndex] == false) return false;
            return true;
        }
    }
}
