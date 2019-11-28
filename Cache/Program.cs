using System;
using SME;

namespace Cache
{
    class Program
    {
        static void Main(string[] args)
        {
            using(var sim = new Simulation()){

                int size = 16;
                int[] positions = new int[size];
                for(int i = 0; i < size; i++){
                    positions[i] = (int)i+1;
                }

                //External simulation process
                var testing_simulator = new Testing_Simulator(positions);

                // RAM
                var acceleration_ram = new AccelerationDataRam(positions.Length);
                
                int cache_size = 4;
                var acceleration_cache = new AccelerationCache(cache_size);
                
                
                acceleration_cache.acceleration_input = testing_simulator.acceleration_input;
                acceleration_cache.ready = testing_simulator.acceleration_ready;
                acceleration_cache.acc_ramctrl = acceleration_ram.ControlA;
                acceleration_cache.acc_ramresult = acceleration_ram.ReadResultA;
                
                testing_simulator.acc_ramctrl = acceleration_ram.ControlB;
                testing_simulator.acc_ramresult = acceleration_ram.ReadResultB;
                testing_simulator.acceleration_result = acceleration_cache.output;

                sim
                .AddTopLevelInputs(acceleration_cache.acceleration_input, acceleration_cache.ready, testing_simulator.acc_ramctrl)
                .AddTopLevelOutputs(testing_simulator.acc_ramresult, acceleration_cache.output)
                .BuildCSVFile()
                .BuildVHDL()
                .Run()
                ;
            }
        }
    }
}
