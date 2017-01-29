using System.Collections.Generic;

namespace BulletX.LinerMath
{
    /// <summary>
    /// stackallocの代用変数
    /// </summary>
    struct StackPtr<T>
    {
        public T[] Array;

        public static StackPtr<T> Allocate(int size)
        {
            return new StackPtr<T> { Array = InnerAllocate(size) };
        }
        public void Dispose()
        {
            InnerFree(Array);
        }
        public T this[int index] { get { return Array[index]; } set { Array[index] = value; } }
        public T this[uint index] { get { return Array[index]; } set { Array[index] = value; } }
        public static implicit operator T[](StackPtr<T> ptr)
        {
            return ptr.Array;
        }
        

        static Dictionary<int, Queue<T[]>> ArrayPool = new Dictionary<int, Queue<T[]>>();

        

        static T[] InnerAllocate(int size)
        {
            if (!ArrayPool.ContainsKey(size))
                ArrayPool.Add(size, new Queue<T[]>());
            var pool = ArrayPool[size];
            if (pool.Count > 0)
                return (T[])pool.Dequeue();
            return new T[size];
        }
        static void InnerFree(T[] ptr)
        {
            var pool = ArrayPool[ptr.Length];
            pool.Enqueue(ptr);
        }

        
    }
    
    
}
