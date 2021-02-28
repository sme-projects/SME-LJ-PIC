using SME;
using SME.Components;
using System;
using Deflib;

namespace Molecular_Dynamics
{
    class Program
    {
        static void Main(string[] args)
        {
            using(var sim = new Simulation())
            {
                ulong data_size = 20;
                double timestep_size = 0.10;

                var magnitude = new Magnitude.Magnitude();

                var simulation = new External_Sim(data_size, timestep_size);

                var lennard = new MD[(int)Deflib.Dimensions.n];
                for (int i = 0; i < (int)Deflib.Dimensions.n; i++){
                    lennard[i] = new MD(data_size, timestep_size, magnitude.output);
                }

                for (int i = 0; i < (int)Deflib.Dimensions.n; i++)
                {
                    magnitude.input_procs[i].multiplicant = lennard[i].acc.mag_input;
                    magnitude.input_procs[i].multiplier = lennard[i].acc.mag_input;
                }

                for (int i = 0; i < (int)Deflib.Dimensions.n; i++){
                    lennard[i].sim.init_position = simulation.init_position[i];
                    lennard[i].sim.init_velocity = simulation.init_velocity[i];
                    
                }
                
                for (int i = 0; i < (int)Deflib.Dimensions.n; i++){
                    simulation.finished[i] = lennard[i].sim.sim_finished;
                }
                int ticks = 0;
                sim
                    .AddTopLevelInputs(simulation.init_velocity[0],
                                       simulation.init_velocity[1],
                                       simulation.init_velocity[2],
                                       simulation.init_position[0], 
                                       simulation.init_position[1], 
                                       simulation.init_position[2])
                    .AddTopLevelOutputs(simulation.finished[0],simulation.finished[1],simulation.finished[2])
                    .AddTicker(s => ticks++)
                    .BuildCSVFile()
                    // .BuildVHDL()
                    .Run();
                Console.WriteLine($"number of ticks: {ticks}");
                Console.WriteLine("Simulation completed");
            }
        }
    }



    public class MD{

        public Acceleration.Acceleration acc;
        public External_MD_Sim sim;

        public MD(ulong data_size, double timestep_size, ValBus mag_output){
            // RAM
            var position_ram = new TrueDualPortMemory<ulong>((int)data_size);
            var velocity_ram = new TrueDualPortMemory<ulong>((int)data_size);
            var acceleration_ram = new AccelerationDataRam(data_size);
            
            //External simulation process
            var external_simulator = new External_MD_Sim(data_size, timestep_size, (ulong)Cache_size.n);

            // Connect External_MD_Simulation field with simulation proccess
            sim = external_simulator;

            // Managers
            var acceleration_manager = new Acceleration.Acc_manager();
            var velocity_manager = new Velocity_Update.Vel_manager(data_size, timestep_size);
            var position_manager = new Position_Update.Pos_manager(data_size, timestep_size);
            
            // Multiplexers
            var init_pos_data_multiplexer = new Multiplexer_ControlA();
            var pos_data_multiplexer = new Multiplexer_ControlA();
            var velocity_data_multiplexer = new Multiplexer_ControlB(); 
            var position_update_multiplexer = new Multiplexer_ControlB(); 
            var velocity_update_multiplexer = new Multiplexer_ControlA(); 


            // Cache
            var acceleration_cache = new Cache.AccelerationCache((ulong)Cache_size.n);
            

            // Acceleration class
            Acceleration.Acceleration acceleration = 
                new Acceleration.Acceleration(acceleration_manager.pos1_output,
                                    acceleration_manager.pos2_output, mag_output);
            
            // Connect Acceleration field with acceleration process
            acc = acceleration;
            
            // Velocity class
            Velocity_Update.Update_velocity velocity = 
                new Velocity_Update.Update_velocity(velocity_manager.prev_velocity, 
                            velocity_manager.acceleration_data_point,timestep_size);

            // Position class
            Position_Update.Update_position position = 
                new Position_Update.Update_position(position_manager.prev_position, 
                            position_manager.velocity_data_point,timestep_size);

            // Connections
            external_simulator.position_ramctrl = init_pos_data_multiplexer.first_input;
            acceleration_manager.pos1_ramctrl = init_pos_data_multiplexer.second_input;

            init_pos_data_multiplexer.output = pos_data_multiplexer.first_input;
            position_manager.position_ramctrl = pos_data_multiplexer.second_input;
            pos_data_multiplexer.output = position_ram.ControlA;
            

            acceleration_manager.pos2_ramctrl = position_update_multiplexer.first_input;
            position_manager.updated_position_ramctrl = position_update_multiplexer.second_input;
            position_update_multiplexer.output = position_ram.ControlB;

            acceleration_manager.pos1_ramresult = position_ram.ReadResultA;
            external_simulator.position_ramresult = position_ram.ReadResultA;
            position_manager.position_ramresult = position_ram.ReadResultA;
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

            velocity_manager.velocity_ramctrl = velocity_data_multiplexer.first_input;
            position_manager.velocity_data_point_ramctrl = velocity_data_multiplexer.second_input;
            
            velocity_data_multiplexer.output = velocity_ram.ControlB;
            velocity_manager.velocity_ramresult = velocity_ram.ReadResultB;

            velocity_manager.updated_velocity_ramctrl = velocity_update_multiplexer.second_input;
            external_simulator.init_velocity_ramctrl = velocity_update_multiplexer.first_input;
            velocity_update_multiplexer.output = velocity_ram.ControlA;

            position_manager.velocity_data_point_ramresult = velocity_ram.ReadResultB;

            velocity_manager.reset = external_simulator.velocity_reset; 
            position_manager.data_ready = velocity_manager.finished;

            position_manager.reset = external_simulator.position_reset;
            position_manager.updated_position = position.updated_data_point;
            position.prev_data_point = position_manager.prev_position;
            position.velocity_data_point = position_manager.velocity_data_point;

            external_simulator.finished = position_manager.finished;
        }
                

    }
}
