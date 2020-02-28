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
                uint data_size = 40;

                // RAM
                var position_ram = new TrueDualPortMemory<uint>((int)data_size);
                // var velocity_ram = new TrueDualPortMemory<uint>(positions.Length);
                var acceleration_ram = new AccelerationDataRam(data_size);
                
                //External simulation process
                var external_simulator = new External_Sim(data_size, (uint)Cache_size.n);

                // Managers
                var acceleration_manager = new Acceleration.Manager();
                
                // Multiplexers
                var init_data_multiplexer = new Multiplexer_ControlA();

                // Cache
                var acceleration_cache = new Cache.AccelerationCache((uint)Cache_size.n);
                
                // Acceleration class
                Acceleration.Acceleration acceleration = 
                    new Acceleration.Acceleration(acceleration_manager.pos1_output,
                                     acceleration_manager.pos2_output);


                // Connections
                external_simulator.pos_ramctrl = init_data_multiplexer.first_input;
                acceleration_manager.pos1_ramctrl = init_data_multiplexer.second_input;

                init_data_multiplexer.output = position_ram.ControlA;
                acceleration_manager.ready = external_simulator.ready;

                // acceleration_manager.pos1_ramctrl = position_ram.ControlA;
                acceleration_manager.pos2_ramctrl = position_ram.ControlB;
                acceleration_manager.pos1_ramresult = position_ram.ReadResultA;
                acceleration_manager.pos2_ramresult = position_ram.ReadResultB;

                // var testprocess = new Test();
                acceleration_cache.acceleration_input = acceleration.output;
                // acceleration_cache.acceleration_input = testprocess.output;
                acceleration_cache.ready = acceleration_manager.acceleration_ready_output;
                acceleration_cache.acc_ramctrl = acceleration_ram.ControlA;
                acceleration_cache.acc_ramresult = acceleration_ram.ReadResultA;
                external_simulator.acc_ramctrl = acceleration_ram.ControlB;
                external_simulator.acc_ramresult = acceleration_ram.ReadResultB;
                external_simulator.result_ready = acceleration_cache.output;

                // sim.Run(null, () => true);
                sim.Run();
            }
        }
    }
}
