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
            internal_acceleration_sim.input_result = const_mass_of_argon.output;
            //output = const_mass_of_argon.output;

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
                float pos1 = 1.0f;
                float pos2 = 5.0f;
                float pos3 = 10.0f;

                // RAM
                var position_ram1 = new TrueDualPortMemory<uint>(3);
                var position_ram2 = new TrueDualPortMemory<uint>(3);
                // Manager
                var manager = new Manager();

                

                Acceleration acceleration = 
                    new Acceleration(manager.pos1_output,
                                     manager.pos2_output);


                //External simulation process
                var simulator = new External_Sim(pos1, pos2, pos3);

                simulator.pos1_ramctrl = position_ram1.ControlB;
                simulator.pos2_ramctrl = position_ram2.ControlB;
                manager.input = simulator.output;

                manager.pos1_ramctrl = position_ram1.ControlA;
                manager.pos2_ramctrl = position_ram2.ControlA;
                manager.pos1_ramresult = position_ram1.ReadResultA;
                manager.pos2_ramresult = position_ram2.ReadResultA;

                simulator.input = acceleration.output;
                                
                

                // sim.Run(null, () => true);
                sim.Run();
            }
        }
    }
}
