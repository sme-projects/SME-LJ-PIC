using System;
using SME;
using Deflib;

namespace Cache
{
    public class Program
    {

        static void Main(string[] args)
        {


            // TODO: Implement the structure to be able to handle data size not dividable by 4
            // TODO: Implement so that the structure can run more than one time
            // TODO: Update the cache to be able to use both simple data and 
            // actual acceleration data

            // TODO: Fix cache to handle data_size 4 and 8. 

            /* NOTE: The Cache running independently does not use the acceleration
            module and therefore the data stored in the cache are created by the 
            simulation. This also means that the cache results cannot be compared
            to other MD acceleration results when testing the cache module alone. */


            for(ulong data_size = 12; data_size < 200; data_size = data_size * 2){
                Console.WriteLine("data_size is {0} and cache_size is {1}", data_size, (long)Cache_size.n);
                using(var sim = new Simulation()){
                    ulong[] positions = new ulong[data_size];
                    for(long k = 0; k < (int)data_size; k++){
                        positions[k] = (ulong)k+1;
                    }

                    //External simulation process
                    var testing_simulator = new Testing_Simulator(positions, (ulong)Cache_size.n);

                    // RAM
                    var acceleration_ram = new Deflib.AccelerationDataRam((ulong)positions.Length);
                    

                    var acceleration_cache = new AccelerationCache((ulong)Cache_size.n);
                    
                    
                    acceleration_cache.acceleration_input = testing_simulator.acceleration_input;
                    acceleration_cache.ready = testing_simulator.acceleration_ready;
                    acceleration_cache.acc_ramctrl = acceleration_ram.ControlA;
                    acceleration_cache.acc_ramresult = acceleration_ram.ReadResultA;
                    
                    testing_simulator.acc_ramctrl = acceleration_ram.ControlB;
                    testing_simulator.acc_ramresult = acceleration_ram.ReadResultB;
                    testing_simulator.acceleration_result = acceleration_cache.output;

                    sim
                    // .AddTopLevelInputs(acceleration_cache.acceleration_input, acceleration_cache.ready, testing_simulator.acc_ramctrl)
                    // .AddTopLevelOutputs(testing_simulator.acc_ramresult, acceleration_cache.output)
                    // .BuildCSVFile()
                    // .BuildVHDL()
                    .Run()
                    ;
                }
            }
            Console.WriteLine("Simulation completed");
        }
    }
}
