using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Content.Pipeline
{

    internal class DisplayNameAttribute
        : Attribute
    {

        public string Name { get; }

        public DisplayNameAttribute(string name)
        {
            this.Name = name;
        }

    }

}