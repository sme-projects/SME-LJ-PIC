using SME;
using System;

namespace Lennard_Jones
{
    [InitializedBus]
    public interface ValBus : IBus
    {
        uint val { get; set; }
        [InitialValue(false)]
        bool valid { get; set; }
    }

}