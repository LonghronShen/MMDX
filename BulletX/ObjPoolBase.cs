using System.Collections.Generic;

namespace BulletX
{
    /// <summary>
    /// オブジェクトプール実装用の抽象クラス
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ObjPoolBase<T>
        where T : class, new()
    {
        protected static Queue<T> ObjPool = new Queue<T>();
        internal static T GetFromPool()
        {
            T result;
            if (ObjPool.Count == 0)
                result = new T();
            else
                result = ObjPool.Dequeue();
            return result;
        }
        internal abstract void Free();
        
    }
}
