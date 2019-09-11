using SME;
using System;

namespace Lennard_Jones
{
    //[InitializedBus]
    //public interface Data : IBus
    //{
    //    int val { get; set;}
    //    bool valid { get; set;}
    //    int dt { get; set;}
    //    int num_of_steps { get; set;}
    //    int initial_temp { get; set;}
    //    int num_of_bodies { get; set;}
    //}

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