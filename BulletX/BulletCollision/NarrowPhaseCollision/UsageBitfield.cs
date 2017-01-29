
namespace BulletX.BulletCollision.NarrowPhaseCollision
{
    struct UsageBitfield
    {
        public void reset()
        {
            usedVertexA = false;
            usedVertexB = false;
            usedVertexC = false;
            usedVertexD = false;
        }
        public bool usedVertexA;
        public bool usedVertexB;
        public bool usedVertexC;
        public bool usedVertexD;
        /*public bool unused1;
        public bool unused2;
        public bool unused3;
        public bool unused4;*/
    }
}
