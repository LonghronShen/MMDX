#define USE_PATH_COMPRESSION
using System.Collections.Generic;

namespace BulletX.BulletCollision.CollisionDispatch
{
    public struct Element
    {
        public int m_id;
        public int m_sz;
    }
    public class UnionFind
    {
        List<Element> m_elements = new List<Element>();

        public int NumElements { get { return m_elements.Count; } }

        public void unite(int p, int q)
        {
            int i = find(p), j = find(q);
            if (i == j)
                return;
#if! USE_PATH_COMPRESSION
			//weighted quick union, this keeps the 'trees' balanced, and keeps performance of unite O( log(n) )
			if (m_elements[i].m_sz < m_elements[j].m_sz)
			{ 
				m_elements[i].m_id = j; m_elements[j].m_sz += m_elements[i].m_sz; 
			}
			else 
			{ 
				m_elements[j].m_id = i; m_elements[i].m_sz += m_elements[j].m_sz; 
			}
#else
            m_elements[i] = new Element { m_id = j, m_sz = m_elements[i].m_sz };

            //m_elements[i].m_id = j;
            m_elements[j] = new Element { m_id = m_elements[j].m_id, m_sz = m_elements[j].m_sz + m_elements[i].m_sz };
            //m_elements[j].m_sz += m_elements[i].m_sz; 
#endif
        }
        public int find(int x)
        {
            //btAssert(x < m_N);
            //btAssert(x >= 0);

            while (x != m_elements[x].m_id)
            {
                //not really a reason not to use path compression, and it flattens the trees/improves find performance dramatically

#if USE_PATH_COMPRESSION
		        m_elements[x] = new Element { m_id = m_elements[m_elements[x].m_id].m_id, m_sz = m_elements[x].m_sz };
                x = m_elements[m_elements[x].m_id].m_id;
#else//
				x = m_elements[x].m_id;
#endif
                //btAssert(x < m_N);
                //btAssert(x >= 0);

            }
            return x;
        }
        public void reset(int n)
        {
            m_elements.Clear();
            for (int i = 0; i < n; i++)
            {
                m_elements.Add(new Element { m_id = i, m_sz = 1 });
            }
        }
        UnionFindElementSortPredicate pred = new UnionFindElementSortPredicate();
        public void sortIslands()
        {
            //first store the original body index, and islandId
            int numElements = m_elements.Count;

            for (int i = 0; i < numElements; i++)
            {
                m_elements[i] = new Element { m_id = find(i), m_sz = i };
                //m_elements[i].m_id = find(i);
                //m_elements[i].m_sz = i;
            }

            // Sort the vector using predicate and std::sort
            //std::sort(m_elements.begin(), m_elements.end(), btUnionFindElementSortPredicate);
            m_elements.Sort(pred);
        }

        public Element getElement(int index)
        {
            return m_elements[index];
        }
    }
}
