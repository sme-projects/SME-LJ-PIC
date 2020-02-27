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
        public TrueDualPortMemory<uint>.IControlA position_ramctrl;

        [OutputBus]
        public ValBus ready_signal = Scope.CreateBus<ValBus>();

        public Testing_Simulation(uint data_size){
            this.data_size = data_size;
        }

        private uint data_size;

        public override async System.Threading.Tasks.Task Run()
        {
            await ClockAsync();

            Random rnd = new Random();
            // Testing data
            // Positions array is kept float so that the uint bitstream can be
            // generated correctly
            float[] positions = new float[data_size];
            for(uint k = 0; k < data_size; k++){
                int random_number = rnd.Next(10,1000);
                float float_rnd_number = random_number;
                if(!positions.Contains((float) random_number))
                    positions[k] = (float) random_number;
                else
                    k--;
                // Non-random data:
                // positions[k] = (float) k + 1;
            }
            
            // Write data to ram
            for(int k = 0; k < positions.Length; k++){
                position_ramctrl.Address = k;
                position_ramctrl.Data = Funcs.FromFloat(positions[k]);
                position_ramctrl.IsWriting = true;
                position_ramctrl.Enabled = true;
                await ClockAsync();
            }
            bool running = true;
            position_ramctrl.Enabled = false;
            ready_signal.val = (uint)positions.Length;
            ready_signal.valid = true;
            await ClockAsync();
            ready_signal.valid = false;

            int i = 0;
            int j = 1;
            Queue<float> calculated_result_queue = new Queue<float>();
            while(running){

                if(testing_result_input.valid){
                    // TODO: Rename the control result to something with "control result"
                    float calculated_result = Sim_Funcs.Acceleration_Calc(positions[i], positions[j]);
                    calculated_result_queue.Enqueue(calculated_result);
                    if(i <= data_size -2 && j <= data_size -1){
                        float calc_result = calculated_result_queue.Dequeue();
                        float input_result = Funcs.FromUint(testing_result_input.val);

                        // TODO: Figure out what the expected and accepted difference can be
                        if(Math.Abs(calc_result - input_result) > 1/(Math.Pow(10,7))){
                            Console.WriteLine("pos1 {0}: {1}, pos2 {2}: {3}", i, positions[i], j, positions[j]);
                            Console.WriteLine("Acceleration test sim - Got {0}, Expected {0}", input_result, calc_result);
                            // Console.WriteLine($"Acc difference : Got {Math.Abs(calc_result - input_result)}");
                        }
                        if(i >= data_size - 2){
                            running = false;
                            Console.WriteLine("Acceleration test successfully completed");
                        }
                        if(j >= data_size -1 ){
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