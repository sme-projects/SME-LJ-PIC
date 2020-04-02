using System;
using SME;
using SME.Components;
using Deflib;

namespace Position_Update
{

    class Program
    {
        static void Main(string[] args)
        {
            using(var sim = new Simulation())
            {
                int data_size = 24;
                float timestep_size = 10.1f;

                var position_ram = new TrueDualPortMemory<uint>(data_size);
                var velocity_ram = new TrueDualPortMemory<uint>(data_size);

                var testing_simulator = new Testing_Simulation((uint)data_size, timestep_size);
                var position_manager = new Manager((uint)data_size, timestep_size);
                
                

                Update_position update_position = 
                    new Update_position(position_manager.prev_position, 
                                position_manager.velocity_data_point,timestep_size);
                
                var multiplexer = new Multiplexer_ControlB();

                position_manager.reset = testing_simulator.sim_ready;
                position_manager.data_ready = testing_simulator.data_ready;
                position_manager.updated_position = update_position.updated_data_point;
                testing_simulator.finished = position_manager.finished;

                position_manager.position_ramctrl = position_ram.ControlA;
                position_manager.position_ramresult = position_ram.ReadResultA;

                position_manager.velocity_data_point_ramctrl = velocity_ram.ControlB;
                position_manager.velocity_data_point_ramresult = velocity_ram.ReadResultB;

                position_manager.updated_position_ramctrl = multiplexer.second_input;
                testing_simulator.data_point_ramctrl = multiplexer.first_input;
                multiplexer.output = position_ram.ControlB;
                
                testing_simulator.velocity_data_point_ramctrl = velocity_ram.ControlA;
                testing_simulator.data_point_ramresult = position_ram.ReadResultB;

                sim.Run();
                Console.WriteLine("Simulation completed");
            }
        }
    }

    public class Update_position
    {
        public ValBus prev_data_point;
        public ValBus velocity_data_point;
        public ValBus updated_data_point;

        public Update_position(ValBus prev_data_point, ValBus velocity_data_point, float timestep)
        {
            this.prev_data_point = prev_data_point;
            this.velocity_data_point = velocity_data_point;

            // Constants
            var const_timestep = new Constants(timestep);

            // Calculation processes
            var mul = new Mul();
            var add = new Add();
            var pipe = new PipelineRegister();

            mul.multiplicant                            = velocity_data_point;
            mul.multiplier                              = const_timestep.output;
            pipe.input                                  = prev_data_point;

            add.augend                                  = mul.product;
            add.addend                                  = pipe.output;

            updated_data_point                          = add.sum;
        }
    }
}