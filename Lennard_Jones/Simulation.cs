using System;
using SME;
using SME.Components;
using Deflib;
using Acceleration;

namespace Lennard_Jones 
{

    public class External_Sim : SimulationProcess
    {
        [InputBus]
        public FlagBus input;
    
        [InputBus]
        public RamResultUint acc_ramresult;
        
        [OutputBus]
        public RamCtrlUint acc_ramctrl;
        
        
        [OutputBus]
        public TrueDualPortMemory<uint>.IControlB pos1_ramctrl;
        [OutputBus]
        public TrueDualPortMemory<uint>.IControlB pos2_ramctrl;

        [OutputBus]
        public ValBus output = Scope.CreateBus<ValBus>();

        

        public External_Sim(float[] positions, uint cache_size)
        {
            this.positions = positions;
            this.cache_size = cache_size;
        }

        private float[] positions;
        private uint cache_size;

        public override async System.Threading.Tasks.Task Run() 
        {
            // Initial await
            await ClockAsync();

            for(int i = 0; i < positions.Length; i++){
                pos1_ramctrl.Enabled = true;
                pos1_ramctrl.Address = i;
                pos1_ramctrl.Data = Funcs.FromFloat(positions[i]);
                pos1_ramctrl.IsWriting = true;

                pos2_ramctrl.Enabled = true;
                pos2_ramctrl.Address = i;
                pos2_ramctrl.Data = Funcs.FromFloat(positions[i]);
                pos2_ramctrl.IsWriting = true;
                await ClockAsync();
            }

            bool running = true;
            pos1_ramctrl.Enabled = false;
            pos2_ramctrl.Enabled = false;
            output.val = (uint)positions.Length;
            output.valid = true;
            await ClockAsync();
            output.valid = false;
            // Calculating data for checking results
            float[] accelerations = new float[positions.Length];
            for(int i = 0; i < positions.Length; i++){
                for(int j = i + 1; j < positions.Length; j++){
                    float result = Sim_Funcs.Acceleration_Calc(positions[i], positions[j]);
                    accelerations[i] += result;
                    accelerations[j] += - result;
                }
            }

            // uint count = 29;
            // float[] test_accelerations = new float[positions.Length];
            // for(int i = 0; i < positions.Length; i++){
            //     for(int j = i + 1; j < positions.Length; j++){
            //         // float result = co
            //         test_accelerations[i] += count;
            //         test_accelerations[j] += - count;
            //         count++;
            //     }
            // }
            // uint k = 0;

            while(running){
                uint i = 0;
                int data_size = positions.Length;
                acc_ramctrl.Enabled = true;
                acc_ramctrl.Data = 0;
                acc_ramctrl.IsWriting = false;
                while(i < data_size){
                    Console.WriteLine("i : {0}", i);
                    while(!input.valid) {
                        await ClockAsync();    
                    }
                    // uint size = input.val;
                    for(uint j = 0; j < cache_size+1; j++){
                        acc_ramctrl.Address = i + j;
                        await ClockAsync();
                        if(j >= 1){
                            float input_result = Funcs.FromUint(acc_ramresult.Data);
                            if(accelerations[i+j-1] != input_result)
                            // if(test_accelerations[i+j-1] != input_result)

                                 Console.WriteLine("Acceleration result - Got {0}, expected {1} at {2}", 
                                 input_result, accelerations[i+j-1], i+j-1);
                                //  input_result, test_accelerations[i+j-1], i+j-1);
                            // Console.WriteLine("result is: {0}", Funcs.FromUint(acc_ramresult.Data));
                        }
                    }
                    i += (uint)cache_size;

                }
                
                // TODO: Must be able to repeat which is not possible now
                running = false;
                // if(Funcs.FromUint(acc_ramresult.Data) == -2460.20117){
                //     running = false;
                // }
                // await ClockAsync();
                // await ClockAsync();
                
                // // float input_result = Funcs.FromUint(acc_ramresult.Data);
                // // Dequeueing the data along to check the results against the C# calculation
                // (uint, uint) tuple = position_queue.Dequeue();
                // float acceleration_calc_result = Sim_Funcs.Acceleration_Calc(pos1, pos2);
                // if(acceleration_calc_result != input_result){
                //     Console.WriteLine("Acceleration result - Got {0}, expected {1}", input_result, acceleration_calc_result);
                // }else{
                //     Console.WriteLine("Acceleration result - Got {0}, expected {1}", input_result, acceleration_calc_result);
                // }
                // if(position_queue.Count == 0){
                //     running = false;
                // }
                // k++;
                
                // await ClockAsync();
            }
        }
    }
    
    
}