
namespace BulletX.LinerMath
{
    public class DefaultMotionState : IMotionState
    {
        public btTransform m_graphicsWorldTrans;
        public btTransform m_centerOfMassOffset;
        public btTransform m_startWorldTrans;
        public object UserData { get; set; }

        public DefaultMotionState()
        {
            m_graphicsWorldTrans = btTransform.Identity; 
            m_centerOfMassOffset = btTransform.Identity;
            m_startWorldTrans = btTransform.Identity;
            UserData = null;
        }
        public DefaultMotionState(btTransform startTrans)
        {
            m_graphicsWorldTrans = startTrans;
            m_centerOfMassOffset = btTransform.Identity;
            m_startWorldTrans = startTrans;
            UserData = null;
        }
        public DefaultMotionState(btTransform startTrans, btTransform centerOfMassOffset)
        {
            m_graphicsWorldTrans = startTrans;
            m_centerOfMassOffset = centerOfMassOffset;
            m_startWorldTrans = startTrans;
            UserData = null;

        }

        ///synchronizes world transform from user to physics
	    public virtual void	getWorldTransform(out btTransform centerOfMassWorldTrans ) 
	    {
			    centerOfMassWorldTrans = 	m_centerOfMassOffset.inverse() * m_graphicsWorldTrans ;
	    }

	    ///synchronizes world transform from physics to user
	    ///Bullet only calls the update of worldtransform for active objects
        public virtual void setWorldTransform(btTransform centerOfMassWorldTrans)
	    {
			    m_graphicsWorldTrans = centerOfMassWorldTrans * m_centerOfMassOffset ;
	    }
    }
}
