using System;
using SME;
using SME.Components;
using Deflib;
using Acceleration;

using System.Linq;

namespace Lennard_Jones 
{

    public class External_Sim : SimulationProcess
    {
        [InputBus]
        public FlagBus result_ready;
    
        [InputBus]
        public RamResultUint acc_ramresult;
        
        [OutputBus]
        public RamCtrlUint acc_ramctrl;
        
        [OutputBus]
        public TrueDualPortMemory<uint>.IControlA pos_ramctrl;

        [OutputBus]
        public ValBus ready = Scope.CreateBus<ValBus>();

        

        public External_Sim(uint data_size, uint cache_size)
        {
            this.data_size = data_size;
            this.cache_size = cache_size;
        }

        private uint data_size;
        private uint cache_size;

        public override async System.Threading.Tasks.Task Run() 
        {
            // Initial await
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

            for(int k = 0; k < positions.Length; k++){
                pos_ramctrl.Address = k;
                pos_ramctrl.Data = Funcs.FromFloat(positions[k]);
                pos_ramctrl.IsWriting = true;
                pos_ramctrl.Enabled = true;

                await ClockAsync();
            }

            bool running = true;
            pos_ramctrl.Enabled = false;
            ready.val = data_size;
            ready.valid = true;
            await ClockAsync();
            ready.valid = false;


            // Calculating data for verifying results
            float[] accelerations = new float[positions.Length];
            for(int k = 0; k < positions.Length; k++){
                for(int n = k + 1; n < positions.Length; n++){
                    float result = Sim_Funcs.Acceleration_Calc(positions[k], positions[n]);
                    accelerations[k] += result;
                    accelerations[n] += - result;
                }
            }

            uint i = 0;
            uint j = 0;
            uint ready_to_read = 0;

            while(running){

                acc_ramctrl.Enabled = true;
                acc_ramctrl.Data = 0;
                acc_ramctrl.IsWriting = false;
                if(result_ready.valid){
                    ready_to_read += cache_size;
                }

                if(i-j >= 2 || i >= positions.Length){
                    float input_result = Funcs.FromUint(acc_ramresult.Data);
                    if(accelerations[j] - input_result > 0.0f)
                        Console.WriteLine("Acceleration result - Got {0}, expected {1} at {2}",
                                input_result, accelerations[j], j);
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