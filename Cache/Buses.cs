using SME;
using System;

namespace Cache
{

    [InitializedBus]
    public interface ValBus : IBus
    {
        [InitialValue(0)]
        int val { get; set; }
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
    public interface RamCtrlInt : IBus
    {
        bool Enabled { get; set; }
        int Address { get; set; }
        int Data { get; set; }
        bool IsWriting { get; set; }
        
    }

    [InitializedBus]
    public interface RamResultInt : IBus
    {
        int Data { get; set; }
    }

    
    
    public interface RamCtrlArray : IBus
    {
        [InitialValue(false)]
        bool Enabled { get; set; }
        [InitialValue(0)]
        int Address { get; set; }
        [InitialValue(false)]
        bool IsWriting { get; set; }
        [FixedArrayLength((int)Cache_size.n)] // width of array bus
        IFixedArray<int> Data { get; set; }
    }

    public interface RamResultArray : IBus
    {
        [FixedArrayLength((int)Cache_size.n)] // width of array bus
        IFixedArray<int> Data { get; set; }
    }


}