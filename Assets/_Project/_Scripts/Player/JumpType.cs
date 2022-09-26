using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Starwalker.Player
{
    [Flags]
    public enum JumpType
    {
        Grounded        = 0x0,
        Aerial          = 0x1,
        WallJumpLeft    = 0x2,
        WallJumpRight   = 0x4,
    }
}
