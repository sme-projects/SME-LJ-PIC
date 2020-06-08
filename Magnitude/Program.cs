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

                var testing_simulator = new Testing_Simulation(data_size);

                Magnitude magnitude = new Magnitude(testing_simulator.output);

                testing_simulator.input = magnitude.output;

                sim
                .Run();
                Console.WriteLine("Simulation completed");
            }

        }
    }

    public class Magnitude
    {
        public ValBus x_coord;
        // public ValBus y_coord;
        // public ValBus z_coord;

        public ValBus output;

        public Magnitude(ValBus x_coord){
            this.x_coord = x_coord;

            var mul = new Mul();
            var sqrt = new Sqrt();

            mul.multiplicant = x_coord;
            mul.multiplier = x_coord;

            sqrt.input = mul.product;
            output = sqrt.output;


        }

    }

}