using SME;
using System;

namespace Deflib
{

    [InitializedBus]
    public interface ValBus : IBus
    {
        [InitialValue(0)]
        uint val { get; set; }
        [InitialValue(false)]
        bool valid { get; set; }
    }

    [InitializedBus]
    public interface FlagBus : IBus
    {
        [InitialValue(false)]
        bool valid { get; set; }
    }


    [InitializedBus]
    public interface RamCtrlUint : IBus
    {
        bool Enabled { get; set; }
        uint Address { get; set; }
        uint Data { get; set; }
        bool IsWriting { get; set; }
        
    }

    [InitializedBus]
    public interface RamResultUint : IBus
    {
        uint Data { get; set; }
    }

    
    
    public interface RamCtrlArray : IBus
    {
        [InitialValue(false)]
        bool Enabled { get; set; }
        [InitialValue(0)]
        uint Address { get; set; }
        [InitialValue(false)]
        bool IsWriting { get; set; }
        [FixedArrayLength((int)Cache_size.n)] // width of array bus
        IFixedArray<uint> Data { get; set; }
    }

    public interface RamResultArray : IBus
    {
        [FixedArrayLength((int)Cache_size.n)] // width of array bus
        IFixedArray<uint> Data { get; set; }
    }


}