using SME;
using SME.Components;
using Deflib;
using Acceleration;
using Velocity_Update;

namespace Lennard_Jones
{
    class Program
    {
        static void Main(string[] args)
        {
            using(var sim = new Simulation())
            {
                uint data_size = 28;
                float timestep_size = 10.0f;

                // RAM
                var position_ram = new TrueDualPortMemory<uint>((int)data_size);
                var velocity_ram = new TrueDualPortMemory<uint>((int)data_size);
                var acceleration_ram = new AccelerationDataRam(data_size);
                
                //External simulation process
                var external_simulator = new External_Sim(data_size, timestep_size, (uint)Cache_size.n);

                // Managers
                var acceleration_manager = new Acceleration.Manager();
                var velocity_manager = new Velocity_Update.Manager(data_size, timestep_size);
                
                // Multiplexers
                var init_pos_data_multiplexer = new Multiplexer_ControlA();
                var velocity_data_multiplexer = new Multiplexer_ControlB();

                // Cache
                var acceleration_cache = new Cache.AccelerationCache((uint)Cache_size.n);
                
                // Acceleration class
                Acceleration.Acceleration acceleration = 
                    new Acceleration.Acceleration(acceleration_manager.pos1_output,
                                     acceleration_manager.pos2_output);
                // Velocity class
                Velocity_Update.Update_velocity velocity = 
                    new Velocity_Update.Update_velocity(velocity_manager.prev_velocity, 
                                velocity_manager.acceleration_data_point,timestep_size);

                // Connections
                external_simulator.pos_ramctrl = init_pos_data_multiplexer.first_input;
                acceleration_manager.pos1_ramctrl = init_pos_data_multiplexer.second_input;

                init_pos_data_multiplexer.output = position_ram.ControlA;

                acceleration_manager.pos2_ramctrl = position_ram.ControlB;
                acceleration_manager.pos1_ramresult = position_ram.ReadResultA;
                acceleration_manager.pos2_ramresult = position_ram.ReadResultB;

                acceleration_manager.ready = external_simulator.acc_ready;

                acceleration_cache.acceleration_input = acceleration.output;
                acceleration_cache.ready = acceleration_manager.acceleration_ready_output;
                acceleration_cache.acc_ramctrl = acceleration_ram.ControlA;
                acceleration_cache.acc_ramresult = acceleration_ram.ReadResultA;

                velocity_manager.acceleration_data_point_ramctrl = acceleration_ram.ControlB;
                velocity_manager.acceleration_data_point_ramresult = acceleration_ram.ReadResultB;
                velocity_manager.data_ready = acceleration_cache.output;

                velocity_manager.updated_velocity = velocity.updated_data_point;

                velocity_manager.velocity_ramctrl = velocity_ram.ControlA;
                velocity_manager.velocity_ramresult = velocity_ram.ReadResultA;

                velocity_manager.updated_velocity_ramctrl = velocity_data_multiplexer.second_input;
                external_simulator.velocity_ramctrl = velocity_data_multiplexer.first_input;
                velocity_data_multiplexer.output = velocity_ram.ControlB;

                external_simulator.velocity_ramresult = velocity_ram.ReadResultB;

                velocity_manager.sim_ready = external_simulator.sim_ready_velocity;
                external_simulator.result_ready = velocity_manager.finished;

                // sim.Run(null, () => true);
                sim.Run();
            }
        }
    }
}
