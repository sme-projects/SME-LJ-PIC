using System;
using SME;
using SME.Components;
using Deflib;

namespace Acceleration
{
    
    class Program
    {

        static void Main(string[] args)
        {
            for(int i = 0; i < 2; i++){
                using(var sim = new Simulation()){

                    // TODO: This fails when data is 1 in size.
                    uint data_size = 10;
                    
                    
                    // RAM
                    var position_ram = new TrueDualPortMemory<uint>((int)data_size);
                    
                    // Multiplexer
                    var multiplexer = new Multiplexer_ControlA();

                    // TODO: Give the data_size to manager as an argument
                    var manager = new Manager();

                    var testing_simulator = new Testing_Simulation(data_size);
                    // TODO: Make the manager handle the result from acceleration and sent to test/cache 
                    Acceleration acceleration = new Acceleration(manager.pos1_output, manager.pos2_output);
                    


                    manager.ready = testing_simulator.ready_signal;
                    testing_simulator.position_ramctrl = multiplexer.first_input;
                    manager.pos1_ramctrl = multiplexer.second_input;
                    multiplexer.output = position_ram.ControlA;
                    manager.pos2_ramctrl = position_ram.ControlB;
                    manager.pos1_ramresult = position_ram.ReadResultA;
                    manager.pos2_ramresult = position_ram.ReadResultB;
                    testing_simulator.testing_result_input = acceleration.output;
                    
                    
                    sim
                    // // .AddTopLevelInputs(acceleration_cache.acceleration_input, acceleration_cache.ready, testing_simulator.acc_ramctrl)
                    // // .AddTopLevelOutputs(testing_simulator.acc_ramresult, acceleration_cache.output)
                    // // .BuildCSVFile()
                    // // .BuildVHDL()
                    .Run()
                    ;
                }
                Console.WriteLine("Simulation number {0}", i);
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

            // Constant processes
            var const_mass_of_argon = new Constants(MASS_OF_ARGON);

            // Calculation processes
            var min             = new Min();
            // TODO: Add calculations for more dimensions 
            Force force = new Force(min.difference);

            // Piping min.difference through force
            var npipe = new nPipe(force.depth);

            var mul             = new Mul();
            var div_mass        = new Div();


            min.minuend                             = input_pos2;
            min.subtrahend                          = input_pos1;
            npipe.first.input                       = min.difference;
            mul.multiplicant                        = force.output;
            mul.multiplier                          = npipe.last.output;
            div_mass.divident                       = mul.product;
            div_mass.divisor                        = const_mass_of_argon.output;
            output                                  = div_mass.quotient;

        }
    }


    public class Force 
    {

        public ValBus input;
        public ValBus output;

        public int depth = 8;

        public Force(ValBus input)
        {   
            this.input = input;


            // Constants
            float SIGMA = 3.4f;
            float EPSILON = 0.0103f;
            float TWELVE = 12.0f;
            float SIX = 6.0f;
            float FOURTEEN = 14.0f;
            float EIGHT = 8.0f;
            float FOURTYEIGHT = 48.0f;
            float TWENTYFOUR = 24.0f;
            float FOUR = 4.0f;
            

            // Constant processes
            var const_sigma = new Constants(SIGMA);
            var const_epsilon = new Constants(EPSILON);
            var const_twelve = new Constants(TWELVE);
            var const_six = new Constants(SIX);
            var const_fourteen = new Constants(FOURTEEN);
            var const_eight = new Constants(EIGHT);
            var const_fourtyeight = new Constants(FOURTYEIGHT);
            var const_twentyfour = new Constants(TWENTYFOUR);
            var const_four = new Constants(FOUR);

            // Calculation processes
            // (48*eps*(sig^12/r^14)) - (24*eps*(sig^6/r^8))
            var abs_sigma       = new Abs();
            var ln_sigma        = new Ln();
            var abs_r           = new Abs();
            var ln_r            = new Ln();
            var mul12           = new Mul();
            var mul6            = new Mul();
            var mul14           = new Mul();
            var mul8            = new Mul();
            var exp12           = new Exp();
            var exp6            = new Exp();
            var exp14           = new Exp();
            var exp8            = new Exp();
            var div_12_14       = new Div();
            var div_6_8         = new Div();
            var mul_eps_12_14   = new Mul();
            var mul_eps_6_8     = new Mul();
            var mul_48          = new Mul();
            var mul_24          = new Mul();
            var minus           = new Min();
            
            
            /* NOTE: e^{x*ln(b)} == b^x*/
            abs_r.input                             = input;
            abs_sigma.input                         = const_sigma.output;
            ln_r.input                              = abs_r.output;
            ln_sigma.input                          = abs_sigma.output;
            
            mul12.multiplicant                      = const_twelve.output;
            mul12.multiplier                        = ln_sigma.output;
            mul6.multiplicant                       = const_six.output;
            mul6.multiplier                         = ln_sigma.output;
            exp12.input                             = mul12.product;
            exp6.input                              = mul6.product;

            mul14.multiplicant                      = const_fourteen.output;
            mul14.multiplier                        = ln_r.output;
            mul8.multiplicant                       = const_eight.output;
            mul8.multiplier                         = ln_r.output;
            exp14.input                             = mul14.product;
            exp8.input                              = mul8.product;

            div_12_14.divident                      = exp12.output;
            div_12_14.divisor                       = exp14.output;

            div_6_8.divident                        = exp6.output;
            div_6_8.divisor                         = exp8.output;

            mul_eps_12_14.multiplicant              = const_epsilon.output;
            mul_eps_12_14.multiplier                = div_12_14.quotient;

            mul_eps_6_8.multiplicant                = const_epsilon.output;
            mul_eps_6_8.multiplier                  = div_6_8.quotient;

            mul_48.multiplicant                     = const_fourtyeight.output;
            mul_48.multiplier                       = mul_eps_12_14.product;

            mul_24.multiplicant                     = const_twentyfour.output;
            mul_24.multiplier                       = mul_eps_6_8.product;

            minus.minuend                           = mul_48.product;
            minus.subtrahend                        = mul_24.product;

            output                                  = minus.difference;
        }
    }
}
