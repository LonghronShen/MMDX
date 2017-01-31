using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Content.Pipeline
{

    internal class DescriptionAttribute
        : Attribute
    {

        public string Description { get; }

        public DescriptionAttribute(string description)
        {
            this.Description = description;
        }

    }

}