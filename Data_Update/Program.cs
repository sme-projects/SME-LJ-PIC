using System;
using SME;
using SME.Components;
using Deflib;

namespace Data_Update
{

    class Program
    {
        static void Main(string[] args)
        {
            using(var sim = new Simulation())
            {
                int data_size = 24;
                float timestep_size = 10.1f;

                var velocity_ram = new TrueDualPortMemory<uint>(data_size);
                var acceleration_ram = new TrueDualPortMemory<uint>(data_size);

                var testing_simulator = new Testing_Simulation((uint)data_size, timestep_size);
                var velocity_manager = new Manager((uint)data_size, timestep_size);

                Update velocity = 
                    new Update(velocity_manager.prev_data_point, 
                                velocity_manager.external_data_point,timestep_size);
                
                var multiplexer = new Multiplexer();

                velocity_manager.sim_ready = testing_simulator.sim_ready;
                velocity_manager.data_ready = testing_simulator.data_ready;
                velocity_manager.updated_data_point = velocity.updated_data_point;
                testing_simulator.finished = velocity_manager.finished;

                velocity_manager.data_point_ramctrl = velocity_ram.ControlA;
                velocity_manager.data_point_ramresult = velocity_ram.ReadResultA;

                velocity_manager.external_data_point_ramctrl = acceleration_ram.ControlA;
                velocity_manager.external_data_point_ramresult = acceleration_ram.ReadResultA;

                velocity_manager.updated_data_point_ramctrl = multiplexer.first_input;
                testing_simulator.data_point_ramctrl = multiplexer.second_input;
                multiplexer.output = velocity_ram.ControlB;
                
                testing_simulator.external_data_point_ramctrl = acceleration_ram.ControlB;
                testing_simulator.data_point_ramresult = velocity_ram.ReadResultB;

                sim.Run();
            }
        }
    }

    public class Update
    {
        public ValBus prev_data_point;
        public ValBus external_data_point;
        public ValBus updated_data_point;

        public Update(ValBus prev_data_point, ValBus external_data_point, float timestep)
        {
            this.prev_data_point = prev_data_point;
            this.external_data_point = external_data_point;

            // Internal simulation process
            var internal_simulation = 
                new Internal_Simulation(timestep);
            internal_simulation.prev_data_point = prev_data_point;
            internal_simulation.external_data_point = external_data_point;

            // Constants
            var const_timestep = new Constants(timestep);

            // Calculation processes
            var mul = new Mul();
            var add = new Add();
            var pipe = new Pipelineregister();

            mul.multiplicant                            = external_data_point;
            mul.multiplier                              = const_timestep.output;
            pipe.input                                  = prev_data_point;

            add.augend                                  = mul.product;
            add.addend                                  = pipe.output;

            updated_data_point                          = add.sum;
            internal_simulation.input_result            = add.sum;

        }
    }
}