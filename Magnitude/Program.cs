using System;
using SME;
using SME.Components;
using Deflib;

namespace Magnitude
{

    class Program
    {
        static void Main(string[] args)
        {
            using(var sim = new Simulation()){

                ulong data_size = 20;

                // sqrt(x^2 + y^2 +z^2)

                Magnitude magnitude = new Magnitude();

                // The simulator will be acting as the acceleration class
                var tests = new Testing_Simulation[(int)Deflib.Dimensions.n];

                for (int i = 0; i < (int)Deflib.Dimensions.n; i++){
                    tests[i] = new Testing_Simulation(data_size, magnitude.output);
                }
                // var testing_simulator = 
                //     new Testing_Simulation(data_size, magnitude.output);

                magnitude.input_procs[0].multiplicant = tests[0].output;
                magnitude.input_procs[0].multiplier = tests[0].output;
                magnitude.input_procs[1].multiplicant = tests[1].output;
                magnitude.input_procs[1].multiplier = tests[1].output;

                // TODO: Add other dimensions to test
                

                // tests.input = magnitude.output;
                sim
                .Run();
                Console.WriteLine("Simulation completed");
            }

        }
    }

    public class Magnitude
    {
        public Mul[] input_procs;

        public ValBus output;

        public Magnitude(){
            input_procs = new Mul[(int)Deflib.Dimensions.n];
            for (int i= 0; i < (int)Deflib.Dimensions.n; i++){
                input_procs[i] = new Mul();
            }

            
            var add = new Add();
            var sqrt = new Sqrt();

            add.addend = input_procs[0].product;
            add.augend = input_procs[1].product;

            sqrt.input = add.sum;
            output = sqrt.output;


        }

    }

}