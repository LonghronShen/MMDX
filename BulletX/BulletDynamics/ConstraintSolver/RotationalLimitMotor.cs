
namespace BulletX.BulletDynamics.ConstraintSolver
{
    public class RotationalLimitMotor
    {
        //! limit_parameters
        //!@{
        public float m_loLimit;//!< joint limit
        public float m_hiLimit;//!< joint limit
        public float m_targetVelocity;//!< target motor velocity
        public float m_maxMotorForce;//!< max force on motor
        public float m_maxLimitForce;//!< max force on limit
        public float m_damping;//!< Damping.
        public float m_limitSoftness;//! Relaxation factor
        public float m_normalCFM;//!< Constraint force mixing factor
        public float m_stopERP;//!< Error tolerance factor when joint is at limit
        public float m_stopCFM;//!< Constraint force mixing factor when joint is at limit
        public float m_bounce;//!< restitution factor
        public bool m_enableMotor;

        //!@}

        //! temp_variables
        //!@{
        public float m_currentLimitError;//!  How much is violated this limit
        public float m_currentPosition;     //!  current value of angle 
        public int m_currentLimit;//!< 0=free, 1=at lo limit, 2=at hi limit
        public float m_accumulatedImpulse;
        //!@}

        public RotationalLimitMotor()
        {
            m_accumulatedImpulse = 0f;
            m_targetVelocity = 0;
            m_maxMotorForce = 0.1f;
            m_maxLimitForce = 300.0f;
            m_loLimit = 1.0f;
            m_hiLimit = -1.0f;
            m_normalCFM = 0f;
            m_stopERP = 0.2f;
            m_stopCFM = 0f;
            m_bounce = 0.0f;
            m_damping = 1.0f;
            m_limitSoftness = 0.5f;
            m_currentLimit = 0;
            m_currentLimitError = 0;
            m_enableMotor = false;
        }
        public void Reset()
        {
            m_accumulatedImpulse = 0f;
            m_targetVelocity = 0;
            m_maxMotorForce = 0.1f;
            m_maxLimitForce = 300.0f;
            m_loLimit = 1.0f;
            m_hiLimit = -1.0f;
            m_normalCFM = 0f;
            m_stopERP = 0.2f;
            m_stopCFM = 0f;
            m_bounce = 0.0f;
            m_damping = 1.0f;
            m_limitSoftness = 0.5f;
            m_currentLimit = 0;
            m_currentLimitError = 0;
            m_enableMotor = false;

            m_currentLimitError = 0f;
            m_currentPosition = 0f;
            m_currentLimit = 0;
            m_accumulatedImpulse = 0f;
        }

        //! calculates  error
        /*!
        calculates m_currentLimit and m_currentLimitError.
        */
        public int testLimitValue(float test_value)
        {
            if (m_loLimit > m_hiLimit)
            {
                m_currentLimit = 0;//Free from violation
                return 0;
            }
            if (test_value < m_loLimit)
            {
                m_currentLimit = 1;//low limit violation
                m_currentLimitError = test_value - m_loLimit;
                return 1;
            }
            else if (test_value > m_hiLimit)
            {
                m_currentLimit = 2;//High limit violation
                m_currentLimitError = test_value - m_hiLimit;
                return 2;
            };

            m_currentLimit = 0;//Free from violation
            return 0;

        }
        //! Need apply correction
        public bool needApplyTorques()
        {
            if (m_currentLimit == 0 && m_enableMotor == false) return false;
            return true;
        }


        
    }
}
