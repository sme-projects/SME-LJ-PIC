using SME;
using System;

namespace Lennard_Jones
{

    public enum Constant : int 
    {
        n = 3,
    }


    public interface Arr : IBus
    {
        [InitialValue(false)]
        bool valid {get; set;}
        [FixedArrayLength((int)Constant.n)]
        IFixedArray<int> arr {get; set;}
    }

    [InitializedBus]
    public interface Activate : IBus
    {
        bool active {get; set;}
        int length {get; set;}
    }


    [InitializedBus]
    public interface ValBus : IBus
    {
        uint val { get; set; }
        [InitialValue(false)]
        bool valid { get; set; }
    }

}