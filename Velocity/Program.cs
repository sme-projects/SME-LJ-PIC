using System;
using SME;
using SME.Components;
using Deflib;

namespace Velocity
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

                Velocity velocity = 
                    new Velocity(velocity_manager.prev_velocity, 
                                velocity_manager.acceleration,timestep_size);
                
                var multiplexer = new Multiplexer();

                velocity_manager.sim_ready = testing_simulator.sim_ready;
                velocity_manager.acc_ready = testing_simulator.acc_ready;
                velocity_manager.updated_velocity = velocity.updated_velocity;
                testing_simulator.finished = velocity_manager.finished;

                velocity_manager.velocity_ramctrl = velocity_ram.ControlA;
                velocity_manager.velocity_ramresult = velocity_ram.ReadResultA;

                velocity_manager.acc_ramctrl = acceleration_ram.ControlA;
                velocity_manager.acc_ramresult = acceleration_ram.ReadResultA;

                velocity_manager.updated_velocity_ramctrl = multiplexer.first_input;
                testing_simulator.velocity_ramctrl = multiplexer.second_input;
                multiplexer.output = velocity_ram.ControlB;
                
                testing_simulator.acceleration_ramctrl = acceleration_ram.ControlB;
                testing_simulator.velocity_ramresult = velocity_ram.ReadResultB;

                sim.Run();
            }
        }
    }

    public class Velocity
    {
        public ValBus prev_velocity;
        public ValBus acceleration;
        public ValBus updated_velocity;

        public Velocity(ValBus prev_velocity, ValBus acceleration, float timestep)
        {
            this.prev_velocity = prev_velocity;
            this.acceleration = acceleration;

            // Internal simulation process
            var internal_velocity_sim = 
                new Internal_Velocity_Sim(timestep);
            internal_velocity_sim.prev_velocity = prev_velocity;
            internal_velocity_sim.acceleration = acceleration;

            // Constants
            var const_timestep = new Constants(timestep);

            // Calculation processes
            var mul = new Mul();
            var add = new Add();
            var pipe = new Pipelineregister();

            mul.multiplicant                            = acceleration;
            mul.multiplier                              = const_timestep.output;
            pipe.input                                  = prev_velocity;

            add.augend                                  = mul.product;
            add.addend                                  = pipe.output;

            updated_velocity                            = add.sum;
            internal_velocity_sim.input_velocity_result = add.sum;

        }
    }
}