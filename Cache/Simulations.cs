using SME;
using System;
using Deflib;

namespace Cache{


    public class Testing_Simulator : SimulationProcess
    {
        [InputBus]
        public FlagBus acceleration_result;
    
        [InputBus]
        public RamResultUlong acc_ramresult;
        
        [OutputBus]
        public RamCtrlUlong acc_ramctrl;
        
        [OutputBus]
        public ValBus acceleration_ready = Scope.CreateBus<ValBus>();
        
        [OutputBus]
        public ValBus acceleration_input = Scope.CreateBus<ValBus>();

        

        public Testing_Simulator(ulong[] positions, ulong cache_size)
        {
            this.positions = positions;
            this.size = cache_size;
        }

        private ulong[] positions;
        private ulong size;

        public override async System.Threading.Tasks.Task Run() 
        {


            // Initial await
            await ClockAsync();
            bool running = true;
            acceleration_ready.val = (ulong)positions.Length;
            acceleration_ready.valid = true;
            await ClockAsync();
            acceleration_ready.valid = false;

            double count = 1.0;
            double[] test_accelerations = new double[positions.Length];
            for(long k = 0; k < positions.Length; k++){
                for(long n = k + 1; n < positions.Length; n++){
                    test_accelerations[k] += count;
                    test_accelerations[n] += - count;
                    count++;
                }
            }
            double input = 1.0;
            ulong i = 0;
            ulong j = 0;
            long data_size = positions.Length;
            ulong ready_to_read = 0;

            while(running){

                if(input < count){
                    acceleration_input.val = Funcs.FromDouble(input);
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

                if(i-j >= 2 || (int)i >= positions.Length){
                    double input_result = Funcs.FromUlong(acc_ramresult.Data);
                    if(test_accelerations[j] - input_result > 0.0f)
                        Console.WriteLine("Acceleration result - Got {0}, expected {1} at pos {2}",
                                input_result, test_accelerations[j], j);
                    j++;
                }

                // TODO: Why does this structure has to be different than
                // velocity simulation?
                if(i < ready_to_read){
                    acc_ramctrl.Address = i;
                    i++;
                }
                if((int)j >= positions.Length)
                    running = false;
                await ClockAsync();
            }
        }
    }

}