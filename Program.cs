using System;
using SME;
using SME.Components;

namespace Lennard_Jones
{

    public class Acceleration 
    {
        public ValBus input_pos1;
        public ValBus input_pos2;
        public ValBus output;




        public Acceleration(ValBus input_pos1, ValBus input_pos2)
        {
            this.input_pos1 = input_pos1;
            this.input_pos2 = input_pos2;

            // Constants
            float MASS_OF_ARGON = 39.948f;
            float SIGMA = 3.4f;
            float EPSILON = 0.0103f;

            // Constant processes
            var const_mass_of_argon = new Constants(MASS_OF_ARGON);

            //Internal simulation process
            var internal_acceleration_sim = 
                new Internal_Acceleration_Sim(MASS_OF_ARGON, SIGMA, EPSILON);
            internal_acceleration_sim.input_pos1 = input_pos1;
            internal_acceleration_sim.input_pos2 = input_pos2;

            // Calculation processes
             var min         = new Min();
            // Adding sqrt and mul maybe? 
            Force force = new Force(min.difference);
            //var mul        = new Mul();
            //var div        = new Div();
            var div_mass       = new Div();


            min.minuend                             = input_pos2;
            min.subtrahend                          = input_pos1;

            div_mass.divident                       = force.output;
            div_mass.divisor                        = const_mass_of_argon.output;
            output                                  = div_mass.quotient;
            internal_acceleration_sim.input_result  = div_mass.quotient;


            
        }
    }


    public class Force 
    {

        public ValBus input;
        public ValBus output;

        public Force(ValBus input)
        {   
            this.input = input;


            // Constants
            float SIGMA = 3.4f;
            float EPSILON = 0.0103f;
            float TWELVE = 12.0f;
            float SIX = 6.0f;
            float FOUR = 4.0f;
            

            // Constant processes
            var const_sigma = new Constants(SIGMA);
            var const_epsilon = new Constants(EPSILON);
            var const_twelve = new Constants(TWELVE);
            var const_six = new Constants(SIX);
            var const_four = new Constants(FOUR);

            // Calculation processes
            var div         = new Div();
            var ln          = new Ln();
            var mul12       = new Mul();
            var mul6        = new Mul();
            var exp12       = new Exp();
            var exp6        = new Exp();
            var minus       = new Min();
            var mulepsilon  = new Mul();
            var mul4        = new Mul();

            //Internal simulation process
            var internal_force_simulation = new Internal_Force_Sim(SIGMA, EPSILON);

            internal_force_simulation.input_r = input;
            
            /* NOTE: e^{x*ln*b} == b^x*/
            div.divisor                         = input;
            div.divident                        = const_sigma.output;
            ln.input                            = div.quotient;
            mul12.multiplicant                  = ln.output;
            mul12.multiplier                    = const_twelve.output;
            mul6.multiplicant                   = ln.output;
            mul6.multiplier                     = const_six.output;
            exp12.input                         = mul12.product;
            exp6.input                          = mul6.product;
            minus.minuend                       = exp12.output;
            minus.subtrahend                    = exp6.output;
            mulepsilon.multiplicant             = const_epsilon.output;
            mulepsilon.multiplier               = minus.difference;
            mul4.multiplicant                   = const_four.output;
            mul4.multiplier                     = mulepsilon.product;
            output                              = mul4.product;
            internal_force_simulation.input_result     = mul4.product;

        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            using(var sim = new Simulation())
            {
                uint size = 20;
                float[] positions = new float[size];
                // size : 12
                for(int i = 0; i < size; i++){
                    positions[i] = i+1;
                    // Console.WriteLine(i);
                }

                // RAM
                var position_ram1 = new TrueDualPortMemory<uint>(positions.Length);
                var position_ram2 = new TrueDualPortMemory<uint>(positions.Length);
                var acceleration_ram = new AccelerationDataRam((uint)positions.Length);
                // Manager
                var manager = new Manager();
                uint cache_size = 4;
                var acceleration_manager = new AccelerationResultManager(cache_size);
                
                

                Acceleration acceleration = 
                    new Acceleration(manager.pos1_output,
                                     manager.pos2_output);


                //External simulation process
                var external_simulator = new External_Sim(positions);

                external_simulator.pos1_ramctrl = position_ram1.ControlB;
                external_simulator.pos2_ramctrl = position_ram2.ControlB;
                manager.input = external_simulator.output;

                manager.pos1_ramctrl = position_ram1.ControlA;
                manager.pos2_ramctrl = position_ram2.ControlA;
                manager.pos1_ramresult = position_ram1.ReadResultA;
                manager.pos2_ramresult = position_ram2.ReadResultA;

                // var testprocess = new Test();
                acceleration_manager.acceleration_input = acceleration.output;
                // acceleration_manager.acceleration_input = testprocess.output;
                acceleration_manager.manager_input = manager.acceleration_ready_output;
                acceleration_manager.acc_ramctrl = acceleration_ram.ControlA;
                acceleration_manager.acc_ramresult = acceleration_ram.ReadResultA;
                external_simulator.acc_ramctrl = acceleration_ram.ControlB;
                external_simulator.acc_ramresult = acceleration_ram.ReadResultB;
                external_simulator.input = acceleration_manager.output;

                // sim.Run(null, () => true);
                sim.Run();
            }
        }
    }
}
