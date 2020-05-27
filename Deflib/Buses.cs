using SME;
using System;

namespace Deflib
{

    [InitializedBus]
    public interface ValBus : IBus
    {
        [InitialValue(0)]
        ulong val { get; set; }
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
    public interface RamCtrlUlong : IBus
    {
        bool Enabled { get; set; }
        ulong Address { get; set; }
        ulong Data { get; set; }
        bool IsWriting { get; set; }
        
    }

    [InitializedBus]
    public interface RamResultUlong : IBus
    {
        ulong Data { get; set; }
    }

    
    
    public interface RamCtrlArray : IBus
    {
        [InitialValue(false)]
        bool Enabled { get; set; }
        [InitialValue(0)]
        ulong Address { get; set; }
        [InitialValue(false)]
        bool IsWriting { get; set; }
        [FixedArrayLength((int)Cache_size.n)] // width of array bus
        IFixedArray<ulong> Data { get; set; }
    }

    public interface RamResultArray : IBus
    {
        [FixedArrayLength((int)Cache_size.n)] // width of array bus
        IFixedArray<ulong> Data { get; set; }
    }


}