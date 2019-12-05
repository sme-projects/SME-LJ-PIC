using SME;
using SME.Components;
using Deflib;
using Acceleration;

namespace Lennard_Jones
{
    class Program
    {
        static void Main(string[] args)
        {
            using(var sim = new Simulation())
            {
                // TODO: It should handle edge cases, such as lower than 12 in 
                // amount of data
                uint size = 40;
                float[] positions = new float[size];
                // size : 12
                for(int i = 0; i < size; i++){
                    positions[i] = i+1;
                    // Console.WriteLine(i);
                }

                // int cache_size = 4;

                // RAM
                var position_ram1 = new TrueDualPortMemory<uint>(positions.Length);
                var position_ram2 = new TrueDualPortMemory<uint>(positions.Length);
                var acceleration_ram = new AccelerationDataRam((uint)positions.Length);
                // Manager
                var manager = new Manager();
                
                var acceleration_cache = new Cache.AccelerationCache((uint)Cache_size.n);
                
                

                Acceleration.Acceleration acceleration = 
                    new Acceleration.Acceleration(manager.pos1_output,
                                     manager.pos2_output);


                //External simulation process
                var external_simulator = new External_Sim(positions, (uint)Cache_size.n);

                external_simulator.pos1_ramctrl = position_ram1.ControlB;
                external_simulator.pos2_ramctrl = position_ram2.ControlB;
                manager.input = external_simulator.output;

                manager.pos1_ramctrl = position_ram1.ControlA;
                manager.pos2_ramctrl = position_ram2.ControlA;
                manager.pos1_ramresult = position_ram1.ReadResultA;
                manager.pos2_ramresult = position_ram2.ReadResultA;

                // var testprocess = new Test();
                acceleration_cache.acceleration_input = acceleration.output;
                // acceleration_cache.acceleration_input = testprocess.output;
                acceleration_cache.ready = manager.acceleration_ready_output;
                acceleration_cache.acc_ramctrl = acceleration_ram.ControlA;
                acceleration_cache.acc_ramresult = acceleration_ram.ReadResultA;
                external_simulator.acc_ramctrl = acceleration_ram.ControlB;
                external_simulator.acc_ramresult = acceleration_ram.ReadResultB;
                external_simulator.input = acceleration_cache.output;

                // sim.Run(null, () => true);
                sim.Run();
            }
        }
    }
}
