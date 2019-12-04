using System;
using SME;

namespace Cache
{
    public enum Cache_size : int{
        n = 4,
    };
    
    class Program
    {

        static void Main(string[] args)
        {


            // TODO: Implement the structure to be able to handle data size not dividable by 4
            // TODO: Implement so that the structure can run more than one time

            for(int data_size = 4; data_size < 100; data_size = data_size * 2){
                Console.WriteLine("data_size is {0} and cache_size is {1}", data_size, (int)Cache_size.n);
                using(var sim = new Simulation()){
                    int[] positions = new int[data_size];
                    for(int k = 0; k < data_size; k++){
                        positions[k] = (int)k+1;
                    }

                    //External simulation process
                    var testing_simulator = new Testing_Simulator(positions, (int)Cache_size.n);

                    // RAM
                    var acceleration_ram = new AccelerationDataRam(positions.Length);
                    

                    var acceleration_cache = new AccelerationCache((int)Cache_size.n);
                    
                    
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

        }
    }
}
