using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{

#if NETPORTABLE
    public class SerializableAttribute
        : Attribute
    {
    }
#endif

}