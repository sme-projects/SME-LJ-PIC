using System;
using SME;
using SME.Components;
using Deflib;

namespace Velocity_Update
{

    class Program
    {
        static void Main(string[] args)
        {
            using(var sim = new Simulation())
            {
                // TODO: Fix to handle data_size not divisable with cache_size
                int data_size = 48;
                float timestep_size = 10.1f;

                var velocity_ram = new TrueDualPortMemory<uint>(data_size);
                var acceleration_ram = new Deflib.AccelerationDataRam((uint)data_size);
                
                var testing_simulator = new Testing_Simulation((uint)data_size, timestep_size, (uint)Cache_size.n);
                var velocity_manager = new Manager((uint)data_size, timestep_size);
                
                

                Update_velocity velocity = 
                    new Update_velocity(velocity_manager.prev_data_point, 
                                velocity_manager.external_data_point,timestep_size);
                
                var multiplexer = new Multiplexer_ControlB();

                velocity_manager.sim_ready = testing_simulator.sim_ready;
                velocity_manager.data_ready = testing_simulator.data_ready;
                velocity_manager.updated_data_point = velocity.updated_data_point;
                testing_simulator.finished = velocity_manager.finished;

                velocity_manager.data_point_ramctrl = velocity_ram.ControlA;
                velocity_manager.data_point_ramresult = velocity_ram.ReadResultA;

                velocity_manager.external_data_point_ramctrl = acceleration_ram.ControlB;
                velocity_manager.external_data_point_ramresult = acceleration_ram.ReadResultB;

                velocity_manager.updated_data_point_ramctrl = multiplexer.first_input;
                testing_simulator.velocity_ramctrl = multiplexer.second_input;
                multiplexer.output = velocity_ram.ControlB;
                
                testing_simulator.acceleration_ramctrl = acceleration_ram.ControlA;
                testing_simulator.velocity_ramresult = velocity_ram.ReadResultB;

                sim.Run();
                Console.WriteLine("Simulation completed");
            }
        }
    }

    public class Update_velocity
    {
        public ValBus prev_data_point;
        public ValBus external_data_point;
        public ValBus updated_data_point;

        public Update_velocity(ValBus prev_data_point, ValBus external_data_point, float timestep)
        {
            this.prev_data_point = prev_data_point;
            this.external_data_point = external_data_point;

            // Constants
            var const_timestep = new Constants(timestep);

            // Calculation processes
            var mul = new Mul();
            var add = new Add();
            var pipe = new PipelineRegister();

            mul.multiplicant                            = external_data_point;
            mul.multiplier                              = const_timestep.output;
            pipe.input                                  = prev_data_point;

            add.augend                                  = mul.product;
            add.addend                                  = pipe.output;

            updated_data_point                          = add.sum;
        }
    }
}