using System;
using System.Collections.Generic;
using SME;
using SME.Components;
using Deflib;

using System.Linq;

namespace Acceleration{


    public class Testing_Simulation : SimulationProcess
    {
        [InputBus]
        public ValBus acceleration_ready_signal;
        
        [InputBus]
        public ValBus testing_result_input;

        [OutputBus]
        public TrueDualPortMemory<ulong>.IControlA position_ramctrl;

        [OutputBus]
        public ValBus ready_signal = Scope.CreateBus<ValBus>();

        public Testing_Simulation(ulong data_size){
            this.data_size = data_size;
        }

        private ulong data_size;

        public override async System.Threading.Tasks.Task Run()
        {
            await ClockAsync();

            Random rnd = new Random();
            // Testing data
            // Positions array is kept double so that the ulong bitstream can be
            // generated correctly
            double[] positions = new double[data_size];
            for(ulong k = 0; k < data_size; k++){
                // long random_number = rnd.Next(10,1000);
                // double double_rnd_number = random_number;
                // if(!positions.Contains((double) random_number))
                //     positions[k] = (double) random_number;
                // else
                //     k--;
                // Non-random data:
                positions[k] = (double) k + 1;
            }
            
            // Write data to ram
            for(long k = 0; k < positions.Length; k++){
                position_ramctrl.Address = (int)k;
                position_ramctrl.Data = Funcs.FromDouble(positions[k]);
                position_ramctrl.IsWriting = true;
                position_ramctrl.Enabled = true;
                await ClockAsync();
            }
            bool running = true;
            position_ramctrl.Enabled = false;
            ready_signal.val = data_size;
            ready_signal.valid = true;
            await ClockAsync();
            ready_signal.valid = false;

            long i = 0;
            long j = 1;
            Queue<double> calculated_result_queue = new Queue<double>();
            while(running){

                if(testing_result_input.valid){
                    // TODO: Rename the control result to something with "control result"
                    double calculated_result = Sim_Funcs.Acceleration_Calc(positions[i], positions[j]);
                    calculated_result_queue.Enqueue(calculated_result);
                    if(i <= (uint)data_size -2 && j <= (uint)data_size -1){
                        double calc_result = calculated_result_queue.Dequeue();
                        double input_result = Funcs.FromUlong(testing_result_input.val);

                        // TODO: Figure out what the expected and accepted difference can be
                        if(Math.Abs(calc_result - input_result) > 1/(Math.Pow(10,7))){
                            Console.WriteLine("pos1 {0}: {1}, pos2 {2}: {3}", i, positions[i], j, positions[j]);
                            Console.WriteLine("Acceleration test sim - Got {0}, Expected {0}", input_result, calc_result);
                            // Console.WriteLine($"Acc difference : Got {Math.Abs(calc_result - input_result)}");
                        }
                        if(i >= (uint)data_size - 2){
                            running = false;
                            Console.WriteLine("Acceleration test successfully completed");
                        }
                        if(j >= (uint)data_size -1 ){
                            i++;
                            j = i + 1;
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