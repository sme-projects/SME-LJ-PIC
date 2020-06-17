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
            for(long i = 0; i < 2; i++){
                using(var sim = new Simulation()){

                    // TODO: This fails when data is 1 in size.
                    ulong data_size = 20;
                    
                    
                    // RAM
                    var position_ram = new TrueDualPortMemory<ulong>((int)data_size);
                    
                    // Multiplexer
                    var multiplexer = new Multiplexer_ControlA();

                    // TODO: Give the data_size to manager as an argument
                    var manager = new Manager();

                    var testing_simulator = new Testing_Simulation(data_size);

                    var testing_magnitude = new Testing_Magnitude(data_size);

                    // TODO: Make the manager handle the result from acceleration and sent to test/cache 
                    Acceleration acceleration = 
                        new Acceleration(manager.pos1_output, manager.pos2_output, 
                                         testing_magnitude.output);

                    testing_magnitude.x_coord = acceleration.mag_input;

                    manager.ready = testing_simulator.ready_signal;
                    testing_simulator.position_ramctrl = multiplexer.first_input;
                    manager.pos1_ramctrl = multiplexer.second_input;
                    multiplexer.output = position_ram.ControlA;
                    manager.pos2_ramctrl = position_ram.ControlB;
                    manager.pos1_ramresult = position_ram.ReadResultA;
                    manager.pos2_ramresult = position_ram.ReadResultB;
                    testing_simulator.testing_result_input = acceleration.output;
                    testing_magnitude.finished = testing_simulator.mag_sim_finished;
                    
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
        public ValBus mag_input;
        public ValBus mag_output;
        public ValBus output;




        public Acceleration(ValBus input_pos1, ValBus input_pos2,
                            ValBus magnitude_output)
        {
            this.input_pos1 = input_pos1;
            this.input_pos2 = input_pos2;
            this.mag_output = magnitude_output;

            // Constants
            double MASS_OF_ARGON = 39.948;

            // Constant processes
            var const_mass_of_argon = new Constants(MASS_OF_ARGON);

            // Calculation processes
            var sub             = new Sub();

            // TODO: Add calculations for more dimensions 
            Force force = new Force(mag_output);

            // Piping sub.difference through magnitude
            var magnitude_pipe = new nPipe((long)Deflib.Magnitude_depth.n);


            // Piping sub.difference through force
            var force_pipe = new nPipe((long)Deflib.Force_depth.n);

            var mul             = new Mul();
            var div_mass        = new Div();


            sub.minuend                             = input_pos2;
            sub.subtrahend                          = input_pos1;
            // magnitude calculation
            mag_input                         = sub.difference;
            // pipe sub.difference through magnitude
            magnitude_pipe.first.input              = sub.difference;
            // pipe sub.difference through force
            force_pipe.first.input                  = magnitude_pipe.last.output;

            mul.multiplicant                        = force.output;
            mul.multiplier                          = force_pipe.last.output;
            div_mass.divident                       = mul.product;
            div_mass.divisor                        = const_mass_of_argon.output;
            output                                  = div_mass.quotient;

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
            double SIGMA = 3.4;
            double EPSILON = 0.0103;
            double TWELVE = 12.0;
            double SIX = 6.0;
            double FOURTEEN = 14.0;
            double EIGHT = 8.0;
            double FOURTYEIGHT = 48.0;
            double TWENTYFOUR = 24.0;
            double FOUR = 4.0;
            

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
            var sub             = new Sub();
            
            
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

            sub.minuend                             = mul_48.product;
            sub.subtrahend                          = mul_24.product;

            output                                  = sub.difference;
        }
    }
}
