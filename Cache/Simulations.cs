using SME;
using System;
using System.Collections.Generic;

namespace Cache{


    public class Testing_Simulator : SimulationProcess
    {
        [InputBus]
        public ValBus acceleration_result;
    
        [InputBus]
        public RamResultInt acc_ramresult;
        
        [OutputBus]
        public RamCtrlInt acc_ramctrl;
        
        [OutputBus]
        public ValBus acceleration_ready = Scope.CreateBus<ValBus>();
        
        [OutputBus]
        public ValBus acceleration_input = Scope.CreateBus<ValBus>();

        

        public Testing_Simulator(int[] positions)
        {
            this.positions = positions;
        }

        private int[] positions;

        public override async System.Threading.Tasks.Task Run() 
        {
            // Initial await
            await ClockAsync();
            bool running = true;
            acceleration_ready.val = positions.Length;
            acceleration_ready.valid = true;
            await ClockAsync();
            acceleration_ready.valid = false;

            int count = 0;
            int[] test_accelerations = new int[positions.Length];
            for(int k = 0; k < positions.Length; k++){
                for(int n = k + 1; n < positions.Length; n++){
                    test_accelerations[k] += count;
                    test_accelerations[n] += - count;
                    count++;
                }
            }
            int input = 0;
            int i = 0;
            int j = 0;
            int data_size = positions.Length;
            int size = 0;
            int ready_to_read = 0;

            while(running){

                if(input < count){
                    acceleration_input.val = input;
                    acceleration_input.valid = true;
                    input++;
                }else{
                    acceleration_input.valid = false;
                }

                acc_ramctrl.Enabled = true;
                acc_ramctrl.Data = 0;
                acc_ramctrl.IsWriting = false;
                if(acceleration_result.valid){
                    size = acceleration_result.val;
                    ready_to_read += size;
                }

                if(i-j >= 2 || i >= positions.Length){
                    int input_result = acc_ramresult.Data;
                    if(test_accelerations[j] == input_result)
                        Console.WriteLine("Acceleration result - Got {0}, expected {1} at {2}",
                                input_result, test_accelerations[j], j);
                    j++;
                }

                if(i < ready_to_read){
                    acc_ramctrl.Address = i;
                    i++;
                }
                if(j >= positions.Length)
                    running = false;
                await ClockAsync();
            }
        }
    }

}