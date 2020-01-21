using SME;
using System;
using Deflib;

namespace Cache{


    public class Testing_Simulator : SimulationProcess
    {
        [InputBus]
        public FlagBus acceleration_result;
    
        [InputBus]
        public RamResultUint acc_ramresult;
        
        [OutputBus]
        public RamCtrlUint acc_ramctrl;
        
        [OutputBus]
        public ValBus acceleration_ready = Scope.CreateBus<ValBus>();
        
        [OutputBus]
        public ValBus acceleration_input = Scope.CreateBus<ValBus>();

        

        public Testing_Simulator(uint[] positions, uint cache_size)
        {
            this.positions = positions;
            this.size = cache_size;
        }

        private uint[] positions;
        private uint size;

        public override async System.Threading.Tasks.Task Run() 
        {
            // Initial await
            await ClockAsync();
            bool running = true;
            acceleration_ready.val = (uint)positions.Length;
            acceleration_ready.valid = true;
            await ClockAsync();
            acceleration_ready.valid = false;

            float count = 1.0f;
            float[] test_accelerations = new float[positions.Length];
            for(int k = 0; k < positions.Length; k++){
                for(int n = k + 1; n < positions.Length; n++){
                    test_accelerations[k] += count;
                    test_accelerations[n] += - count;
                    count++;
                }
            }
            float input = 1.0f;
            uint i = 0;
            uint j = 0;
            int data_size = positions.Length;
            // int size = 0;
            uint ready_to_read = 0;

            while(running){

                if(input < count){
                    acceleration_input.val = Funcs.FromFloat(input);
                    acceleration_input.valid = true;
                    input++;
                }else{
                    acceleration_input.valid = false;
                }

                acc_ramctrl.Enabled = true;
                acc_ramctrl.Data = 0;
                acc_ramctrl.IsWriting = false;
                if(acceleration_result.valid){
                    ready_to_read += size;
                }

                if(i-j >= 2 || i >= positions.Length){
                    float input_result = Funcs.FromUint(acc_ramresult.Data);
                    if(test_accelerations[j] != input_result)
                        Console.WriteLine("Acceleration result - Got {0}, expected {1} at {2}",
                                input_result, test_accelerations[j], j);
                    j++;
                }

                // TODO: Why does this structure has to be different than
                // velocity simulation?
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