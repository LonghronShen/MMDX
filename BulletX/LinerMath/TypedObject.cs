
namespace BulletX.LinerMath
{
    public class TypedObject<T>
        where T : struct
    {
        protected T m_objectType;
        public T ObjectType { get { return m_objectType; } }
        public TypedObject(T objectType)
        {
            m_objectType = objectType;
        }
    }
}
