using BulletX.LinerMath;

namespace BulletX.BulletCollision.NarrowPhaseCollision
{
    class SubSimplexClosestResult
    {
        public btVector3 m_closestPointOnSimplex;
        //MASK for m_usedVertices
        //stores the simplex vertex-usage, using the MASK, 
        // if m_usedVertices & MASK then the related vertex is used
        public UsageBitfield m_usedVertices;
        public float[] m_barycentricCoords = new float[4];
        public bool m_degenerate;

        public void reset()
        {
            m_degenerate = false;
            setBarycentricCoordinates();
            m_usedVertices.reset();
            m_closestPointOnSimplex = btVector3.Zero;
        }
        public bool isValid()
        {
            bool valid = (m_barycentricCoords[0] >= 0f) &&
                (m_barycentricCoords[1] >= 0f) &&
                (m_barycentricCoords[2] >= 0f) &&
                (m_barycentricCoords[3] >= 0f);


            return valid;
        }
        public void setBarycentricCoordinates()
        {
            m_barycentricCoords[0] = 0f;
            m_barycentricCoords[1] = 0f;
            m_barycentricCoords[2] = 0f;
            m_barycentricCoords[3] = 0f;
        }
        public void setBarycentricCoordinates(float a, float b)
        {
            m_barycentricCoords[0] = a;
            m_barycentricCoords[1] = b;
            m_barycentricCoords[2] = 0f;
            m_barycentricCoords[3] = 0f;
        }
        public void setBarycentricCoordinates(float a, float b, float c)
        {
            m_barycentricCoords[0] = a;
            m_barycentricCoords[1] = b;
            m_barycentricCoords[2] = c;
            m_barycentricCoords[3] = 0f;
        }
        public void setBarycentricCoordinates(float a, float b, float c, float d)
        {
            m_barycentricCoords[0] = a;
            m_barycentricCoords[1] = b;
            m_barycentricCoords[2] = c;
            m_barycentricCoords[3] = d;
        }



        
    }
}
