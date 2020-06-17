using SME;
using SME.Components;
using System;
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
                ulong data_size = 20;
                double timestep_size = 10.0;

                // X-dimension

                // RAM
                var position_ram_x = new TrueDualPortMemory<ulong>((int)data_size);
                var velocity_ram_x = new TrueDualPortMemory<ulong>((int)data_size);
                var acceleration_ram_x = new AccelerationDataRam(data_size);
                
                //External simulation process
                var external_simulator_x = new External_Sim(data_size, timestep_size, (ulong)Cache_size.n);

                // Managers
                var acceleration_manager_x = new Acceleration.Manager();
                var velocity_manager_x = new Velocity_Update.Manager(data_size, timestep_size);
                var position_manager_x = new Position_Update.Manager(data_size, timestep_size);
                
                // Multiplexers
                var init_pos_data_multiplexer_x = new Multiplexer_ControlA();
                var pos_data_multiplexer_x = new Multiplexer_ControlA();
                var velocity_data_multiplexer_x = new Multiplexer_ControlB(); 
                var position_update_multiplexer_x = new Multiplexer_ControlB(); 
                var velocity_update_multiplexer_x = new Multiplexer_ControlA(); 


                // Cache
                var acceleration_cache_x = new Cache.AccelerationCache((ulong)Cache_size.n);
                
                var magnitude = new Magnitude.Magnitude();

                // Acceleration class
                Acceleration.Acceleration acceleration_x = 
                    new Acceleration.Acceleration(acceleration_manager_x.pos1_output,
                                     acceleration_manager_x.pos2_output, magnitude.output);
                
                magnitude.input_proc.multiplicant = acceleration_x.mag_input;
                magnitude.input_proc.multiplier = acceleration_x.mag_input;

                // Velocity class
                Velocity_Update.Update_velocity velocity_x = 
                    new Velocity_Update.Update_velocity(velocity_manager_x.prev_velocity, 
                                velocity_manager_x.acceleration_data_point,timestep_size);

                // Position class
                Position_Update.Update_position position_x = 
                    new Position_Update.Update_position(position_manager_x.prev_position, 
                                position_manager_x.velocity_data_point,timestep_size);

                // Connections
                external_simulator_x.position_ramctrl = init_pos_data_multiplexer_x.first_input;
                acceleration_manager_x.pos1_ramctrl = init_pos_data_multiplexer_x.second_input;

                init_pos_data_multiplexer_x.output = pos_data_multiplexer_x.first_input;
                position_manager_x.position_ramctrl = pos_data_multiplexer_x.second_input;
                pos_data_multiplexer_x.output = position_ram_x.ControlA;
                

                acceleration_manager_x.pos2_ramctrl = position_update_multiplexer_x.first_input;
                position_manager_x.updated_position_ramctrl = position_update_multiplexer_x.second_input;
                position_update_multiplexer_x.output = position_ram_x.ControlB;

                acceleration_manager_x.pos1_ramresult = position_ram_x.ReadResultA;
                external_simulator_x.position_ramresult = position_ram_x.ReadResultA;
                position_manager_x.position_ramresult = position_ram_x.ReadResultA;
                acceleration_manager_x.pos2_ramresult = position_ram_x.ReadResultB;

                acceleration_manager_x.ready = external_simulator_x.acc_ready;

                acceleration_cache_x.acceleration_input = acceleration_x.output;
                acceleration_cache_x.ready = acceleration_manager_x.acceleration_ready_output;
                acceleration_cache_x.acc_ramctrl = acceleration_ram_x.ControlA;
                acceleration_cache_x.acc_ramresult = acceleration_ram_x.ReadResultA;

                velocity_manager_x.acceleration_data_point_ramctrl = acceleration_ram_x.ControlB;
                velocity_manager_x.acceleration_data_point_ramresult = acceleration_ram_x.ReadResultB;
                velocity_manager_x.data_ready = acceleration_cache_x.output;

                velocity_manager_x.updated_velocity = velocity_x.updated_data_point;

                velocity_manager_x.velocity_ramctrl = velocity_data_multiplexer_x.first_input;
                position_manager_x.velocity_data_point_ramctrl = velocity_data_multiplexer_x.second_input;
                
                velocity_data_multiplexer_x.output = velocity_ram_x.ControlB;
                velocity_manager_x.velocity_ramresult = velocity_ram_x.ReadResultB;
 
                velocity_manager_x.updated_velocity_ramctrl = velocity_update_multiplexer_x.second_input;
                external_simulator_x.init_velocity_ramctrl = velocity_update_multiplexer_x.first_input;
                velocity_update_multiplexer_x.output = velocity_ram_x.ControlA;

                position_manager_x.velocity_data_point_ramresult = velocity_ram_x.ReadResultB;

                velocity_manager_x.reset = external_simulator_x.velocity_reset; 
                position_manager_x.data_ready = velocity_manager_x.finished;

                position_manager_x.reset = external_simulator_x.position_reset;
                position_manager_x.updated_position = position_x.updated_data_point;
                position_x.prev_data_point = position_manager_x.prev_position;
                position_x.velocity_data_point = position_manager_x.velocity_data_point;

                external_simulator_x.finished = position_manager_x.finished;

                // -------------
                // Y-dimension

                // // RAM
                // var position_ram_y = new TrueDualPortMemory<ulong>((long)data_size);
                // var velocity_ram_y = new TrueDualPortMemory<ulong>((long)data_size);
                // var acceleration_ram_y = new AccelerationDataRam(data_size);
                
                // //External simulation process
                // var external_simulator_y = new External_Sim(data_size, timestep_size, (ulong)Cache_size.n);

                // // Managers
                // var acceleration_manager_y = new Acceleration.Manager();
                // var velocity_manager_y = new Velocity_Update.Manager(data_size, timestep_size);
                // var position_manager_y = new Position_Update.Manager(data_size, timestep_size);
                
                // // Multiplexers
                // var init_pos_data_multiplexer_y = new Multiplexer_ControlA();
                // var pos_data_multiplexer_y = new Multiplexer_ControlA();
                // var velocity_data_multiplexer_y = new Multiplexer_ControlB(); 
                // var position_update_multiplexer_y = new Multiplexer_ControlB(); 
                // var velocity_update_multiplexer_y = new Multiplexer_ControlA(); 


                // // Cache
                // var acceleration_cache_y = new Cache.AccelerationCache((ulong)Cache_size.n);
                
                // // Acceleration class
                // Acceleration.Acceleration acceleration_y = 
                //     new Acceleration.Acceleration(acceleration_manager_y.pos1_output,
                //                      acceleration_manager_y.pos2_output);
                // // Velocity class
                // Velocity_Update.Update_velocity velocity_y = 
                //     new Velocity_Update.Update_velocity(velocity_manager_y.prev_velocity, 
                //                 velocity_manager_y.acceleration_data_point,timestep_size);

                // // Position class
                // Position_Update.Update_position position_y = 
                //     new Position_Update.Update_position(position_manager_y.prev_position, 
                //                 position_manager_y.velocity_data_point,timestep_size);

                // // Connections
                // external_simulator_y.position_ramctrl = init_pos_data_multiplexer_y.first_input;
                // acceleration_manager_y.pos1_ramctrl = init_pos_data_multiplexer_y.second_input;

                // init_pos_data_multiplexer_y.output = pos_data_multiplexer_y.first_input;
                // position_manager_y.position_ramctrl = pos_data_multiplexer_y.second_input;
                // pos_data_multiplexer_y.output = position_ram_y.ControlA;
                

                // acceleration_manager_y.pos2_ramctrl = position_update_multiplexer_y.first_input;
                // position_manager_y.updated_position_ramctrl = position_update_multiplexer_y.second_input;
                // position_update_multiplexer_y.output = position_ram_y.ControlB;

                // acceleration_manager_y.pos1_ramresult = position_ram_y.ReadResultA;
                // external_simulator_y.position_ramresult = position_ram_y.ReadResultA;
                // position_manager_y.position_ramresult = position_ram_y.ReadResultA;
                // acceleration_manager_y.pos2_ramresult = position_ram_y.ReadResultB;

                // acceleration_manager_y.ready = external_simulator_y.acc_ready;

                // acceleration_cache_y.acceleration_input = acceleration_y.output;
                // acceleration_cache_y.ready = acceleration_manager_y.acceleration_ready_output;
                // acceleration_cache_y.acc_ramctrl = acceleration_ram_y.ControlA;
                // acceleration_cache_y.acc_ramresult = acceleration_ram_y.ReadResultA;

                // velocity_manager_y.acceleration_data_point_ramctrl = acceleration_ram_y.ControlB;
                // velocity_manager_y.acceleration_data_point_ramresult = acceleration_ram_y.ReadResultB;
                // velocity_manager_y.data_ready = acceleration_cache_y.output;

                // velocity_manager_y.updated_velocity = velocity_y.updated_data_point;

                // velocity_manager_y.velocity_ramctrl = velocity_data_multiplexer_y.first_input;
                // position_manager_y.velocity_data_point_ramctrl = velocity_data_multiplexer_y.second_input;
                
                // velocity_data_multiplexer_y.output = velocity_ram_y.ControlB;
                // velocity_manager_y.velocity_ramresult = velocity_ram_y.ReadResultB;
 
                // velocity_manager_y.updated_velocity_ramctrl = velocity_update_multiplexer_y.second_input;
                // external_simulator_y.init_velocity_ramctrl = velocity_update_multiplexer_y.first_input;
                // velocity_update_multiplexer_y.output = velocity_ram_y.ControlA;

                // position_manager_y.velocity_data_point_ramresult = velocity_ram_y.ReadResultB;

                // velocity_manager_y.reset = external_simulator_y.velocity_reset; 
                // position_manager_y.data_ready = velocity_manager_y.finished;

                // position_manager_y.reset = external_simulator_y.position_reset;
                // position_manager_y.updated_position = position_y.updated_data_point;
                // position_y.prev_data_point = position_manager_y.prev_position;
                // position_y.velocity_data_point = position_manager_y.velocity_data_point;

                // external_simulator_y.finished = position_manager_y.finished;

                sim.Run();
                Console.WriteLine("Simulation completed");
            }
        }
    }
}
