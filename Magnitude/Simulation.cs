using System;
using System.Collections.Generic;
using SME;
using Deflib;

using System.Linq;

namespace Magnitude
{
    public class Testing_Simulation : SimulationProcess
    {

        [InputBus]
        public ValBus input;
        
        [OutputBus]
        public ValBus output = Scope.CreateBus<ValBus>();


        public Testing_Simulation(ulong data_size, ValBus input){
            this.data_size = (uint)data_size;
            this.input = input;
        }

        private uint data_size;

        public override async System.Threading.Tasks.Task Run()
        {

            await ClockAsync();
            bool running = true;
            Random rnd = new Random();

            //// Testing data

            // Generate random data for testing
            double[] coord_data = new double[data_size];
            // for(long k = 0; k < data_size; k++){
            //     long random_number = rnd.Next(10,1000);
            //     if(!xcoord_data.Contains((double) random_number))
            //         xcoord_data[k] = (double) random_number;
            //     else
            //         k--;
            // }

            // Generate data for testing
            for(ulong k = 0; k < data_size; k++)
                coord_data[k] = k + 1;
        
            long i = 0;
            long j = 0;
            Queue<double> calculated_result_queue = new Queue<double>();
            while(running){
                if(i < data_size){
                    output.valid = true;
                    output.val = Funcs.FromDouble(coord_data[i]);
                    i++;
                }
                if(input.valid){
                    double calculated_result = Sim_Funcs.Magnitude_2d_Calc(coord_data[j]);
                    calculated_result_queue.Enqueue(calculated_result);
                    if(j < (uint)data_size){
                        double calc_result = calculated_result_queue.Dequeue();
                        double input_result = Funcs.FromUlong(input.val);

                        // Assertion for unittest
                        System.Diagnostics.Debug.Assert((Math.Abs(calc_result - input_result) < 1/(Math.Pow(10,7))), "SME acceleration did not match C# magnitude");


                        // if(Math.Abs(calc_result - input_result) > 1/(Math.Pow(10,7)))
                        //     Console.WriteLine("Magnitude test sim - Got {0}, Expected {1}", input_result, calc_result);
                        
                        if(j >= (uint)data_size - 1){
                            running = false;
                            Console.WriteLine("Magnitude test successfully completed");
                        }else{
                            j++;
                        }
                    }
                }
                await ClockAsync();
            }
        }

    }
}