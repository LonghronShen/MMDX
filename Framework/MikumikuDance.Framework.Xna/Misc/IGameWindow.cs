using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework
{
    
    public interface IGameWindow
    {

        event EventHandler<EventArgs> ClientSizeChanged;

    }

}