using BulletX.LinerMath;

namespace BulletX.BulletCollision.NarrowPhaseCollision
{
    interface ISimplexSolver
    {
        void reset();

        bool inSimplex(btVector3 w);

        void addVertex(btVector3 w, btVector3 p, btVector3 q);

        bool closest(out btVector3 v);

        void backup_closest(ref btVector3 v);

        bool fullSimplex { get; }

        void compute_points(out btVector3 pointOnA,out btVector3 pointOnB);
    }
}
