using BulletX.LinerMath;

namespace BulletX.BulletCollision.BroadphaseCollision
{
    public struct DbvtAabbMm
    {
        public btVector3 mi;
        public btVector3 mx;

#if false
        private void				AddSpan(btVector3 d,float smi,float smx);
	
        public DBVT_INLINE btVector3			Center() const	{ return((mi+mx)/2); }
	    public DBVT_INLINE btVector3			Lengths() const	{ return(mx-mi); }
	    public DBVT_INLINE btVector3			Extents() const	{ return((mx-mi)/2); }
	    public DBVT_INLINE const btVector3&	Mins() const	{ return(mi); }
	    public DBVT_INLINE const btVector3&	Maxs() const	{ return(mx); }
	    public static inline btDbvtAabbMm		FromCE(const btVector3& c,const btVector3& e);
	    public static inline btDbvtAabbMm		FromCR(const btVector3& c,btScalar r);
#endif
        public static void FromMM(ref btVector3 mi,ref btVector3 mx, out DbvtAabbMm box)
        {
            //DbvtAabbMm box;
            box.mi = mi; box.mx = mx;
            //return (box);
        }

#if false
	    public static inline btDbvtAabbMm		FromPoints(const btVector3* pts,int n);
	    public static inline btDbvtAabbMm		FromPoints(const btVector3** ppts,int n);
#endif
        public void Expand(ref btVector3 e)
        {
            //mi -= e; mx += e;
            mi.Subtract(ref e);
            mx.Add(ref e);
        }
        public void SignedExpand(ref btVector3 e)
        {
            if (e.X > 0)
                mx.X = (mx.X + e.X);
            else
                mi.X = (mi.X + e.X);
            if (e.Y > 0)
                mx.Y = (mx.Y + e.Y);
            else
                mi.Y = (mi.Y + e.Y);
            if (e.Z > 0)
                mx.Z = (mx.Z + e.Z);
            else
                mi.Z = (mi.Z + e.Z);
        }
	    public bool Contain(ref DbvtAabbMm a)
        {
	        return(	(mi.X<=a.mi.X)&&
		        (mi.Y<=a.mi.Y)&&
		        (mi.Z<=a.mi.Z)&&
		        (mx.X>=a.mx.X)&&
		        (mx.Y>=a.mx.Y)&&
		        (mx.Z>=a.mx.Z));
        }
#if false
	    public DBVT_INLINE int					Classify(const btVector3& n,btScalar o,int s) const;
	    public DBVT_INLINE btScalar			ProjectMinimum(const btVector3& v,unsigned signs) const;
	    public DBVT_INLINE friend bool			Intersect(	const btDbvtAabbMm& a,
		    const btDbvtAabbMm& b);
    	
	    public DBVT_INLINE friend bool			Intersect(	const btDbvtAabbMm& a,
		    const btVector3& b);

	    public DBVT_INLINE friend btScalar		Proximity(	const btDbvtAabbMm& a,
		    const btDbvtAabbMm& b);
	    public DBVT_INLINE friend int			Select(		const btDbvtAabbMm& o,
		    const btDbvtAabbMm& a,
		    const btDbvtAabbMm& b);
	    public DBVT_INLINE friend void			Merge(		const btDbvtAabbMm& a,
		    const btDbvtAabbMm& b,
		    btDbvtAabbMm& r);
	    public DBVT_INLINE friend bool			NotEqual(	const btDbvtAabbMm& a,
		    const btDbvtAabbMm& b);
#endif

    }
}
