using System;
using SME;

namespace Lennard_Jones
{


    public class Force 
    {
        public ValBus input;
        public ValBus output;

        public Force(float r, float sigma, float epsilon, ValBus input)
        {
            this.input = input;

            // Constant processes
            var const_sigma = new Constants(sigma);
            //Console.WriteLine("const sigma: {0}", sigma);
            var const_epsilon = new Constants(epsilon);
            var const_twelve = new Constants(12.0f);
            var const_six = new Constants(6.0f);
            var const_four = new Constants(4.0f);


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

            
            div.divisor             = input;
            div.divident            = const_sigma.output;
            ln.input                = div.quotient;
            mul12.multiplicant      = ln.output;
            mul12.multiplier        = const_twelve.output;
            mul6.multiplicant       = ln.output;
            mul6.multiplier         = const_six.output;
            exp12.input             = mul12.product;
            exp6.input              = mul6.product;
            minus.minuend           = exp12.output;
            minus.subtrahend        = exp6.output;
            mulepsilon.multiplicant = const_epsilon.output;
            mulepsilon.multiplier   = minus.difference;
            mul4.multiplicant       = const_four.output;
            mul4.multiplier         = mulepsilon.product;
            output                  = mul4.product;

            /* if(ln.input == div.quotient){
                Console.WriteLine("");
            } */
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            using(var sim = new Simulation())
            {
                float SIGMA = 3.4f;
                float EPSILON = 0.0103f;
                float r = 1.0f;
                var simulator = new Sim(r, SIGMA, EPSILON);
                Force force = new Force(r, SIGMA, EPSILON, simulator.output);
            
                simulator.input = force.output;

                sim.Run();
            }
        }
    }
}
