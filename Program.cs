using System;
using SME;

namespace Lennard_Jones
{
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
            var internal_simulator = new Internal_Force_Sim(SIGMA, EPSILON);

            internal_simulator.input_r = input;
            
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
            internal_simulator.input_result     = mul4.product;

        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            using(var sim = new Simulation())
            {
                float r = 1.0f;

                //External simulation process
                var external_force_simulator = new External_Force_Sim(r);
                Force force = new Force(external_force_simulator.output);
                external_force_simulator.input = force.output;

                sim.Run();
            }
        }
    }
}
