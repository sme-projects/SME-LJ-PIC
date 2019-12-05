using System;
using SME;
using Deflib;

namespace Acceleration
{
    
    class Program
    {

        static void Main(string[] args)
        {
            using(var sim = new Simulation()){
                // uint[] positions = new uint[data_size];
                // for(int k = 0; k < data_size; k++){
                //     positions[k] = (uint)k+1;
                // }

                // //External simulation process
                // var testing_simulator = new Testing_Simulator(positions, (uint)Cache_size.n);

                // // RAM
                // var acceleration_ram = new AccelerationDataRam((uint)positions.Length);
                

                // var acceleration_cache = new AccelerationCache((uint)Cache_size.n);
                
                
                // acceleration_cache.acceleration_input = testing_simulator.acceleration_input;
                // acceleration_cache.ready = testing_simulator.acceleration_ready;
                // acceleration_cache.acc_ramctrl = acceleration_ram.ControlA;
                // acceleration_cache.acc_ramresult = acceleration_ram.ReadResultA;
                
                // testing_simulator.acc_ramctrl = acceleration_ram.ControlB;
                // testing_simulator.acc_ramresult = acceleration_ram.ReadResultB;
                // testing_simulator.acceleration_result = acceleration_cache.output;

                // sim
                // // .AddTopLevelInputs(acceleration_cache.acceleration_input, acceleration_cache.ready, testing_simulator.acc_ramctrl)
                // // .AddTopLevelOutputs(testing_simulator.acc_ramresult, acceleration_cache.output)
                // // .BuildCSVFile()
                // // .BuildVHDL()
                // .Run()
                // ;
            }
        }
    }

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
}
