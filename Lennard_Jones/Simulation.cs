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
        public TrueDualPortMemory<uint>.IReadResultB velocity_ramresult;

        [OutputBus]
        public TrueDualPortMemory<uint>.IControlB velocity_ramctrl;
    
        [OutputBus]
        public TrueDualPortMemory<uint>.IControlA pos_ramctrl;

        [OutputBus]
        public ValBus acc_ready = Scope.CreateBus<ValBus>();

        [OutputBus]
        public FlagBus sim_ready_velocity = Scope.CreateBus<FlagBus>();

        

        public External_Sim(uint data_size, float timestep_size, uint cache_size)
        {
            this.data_size = data_size;
            this.timestep = timestep_size;
            this.cache_size = cache_size;
        }

        private uint data_size;
        private float timestep;
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
            for(uint i = 0; i < data_size; i++){
                int random_number = rnd.Next(10,1000);
                float float_rnd_number = random_number;
                if(!positions.Contains((float) random_number))
                    positions[i] = (float) random_number;
                else
                    i--;
                // Non-random data:
                // positions[i] = (float) i + 1;
            }

            for(int i = 0; i < positions.Length; i++){
                // Write initial data to position ram
                pos_ramctrl.Address = i;
                pos_ramctrl.Data = Funcs.FromFloat(positions[i]);
                pos_ramctrl.IsWriting = true;
                pos_ramctrl.Enabled = true;

                // Write initial data to velocity ram
                velocity_ramctrl.Address = i;
                // Initial value is 0
                velocity_ramctrl.Data = Funcs.FromFloat(0.0f); 
                velocity_ramctrl.IsWriting = true;
                velocity_ramctrl.Enabled = true;

                await ClockAsync();
            }

            bool running = true;
            pos_ramctrl.Enabled = false;
            velocity_ramctrl.Enabled = false;
            acc_ready.val = data_size;
            acc_ready.valid = true;
            sim_ready_velocity.valid = true;
            await ClockAsync();
            acc_ready.valid = false;
            sim_ready_velocity.valid = false;


            // Calculating data for verifying results
            float[] accelerations = new float[positions.Length];
            for(int i = 0; i < positions.Length; i++){
                for(int j = i + 1; j < positions.Length; j++){
                    float result = Sim_Funcs.Acceleration_Calc(positions[i], positions[j]);
                    accelerations[i] += result;
                    accelerations[j] += - result;
                }
            }

            float[] updated_velocity = new float[data_size];
            // Calculate data for tests
            for( int i = 0; i < data_size; i++){
                // Initial velocity is 0
                float update_result = Sim_Funcs.Update_Data_Calc(0.0f, accelerations[i], timestep);
                updated_velocity[i] = update_result;
            }

            int k = 0;
            int n = 0;

            while(running){
                if(result_ready.valid){
                    
                    if(k < data_size){
                        velocity_ramctrl.Enabled = true;
                        velocity_ramctrl.Data = 0;
                        velocity_ramctrl.IsWriting = false;
                        velocity_ramctrl.Address = k;
                        k++;
                    }else{
                        velocity_ramctrl.Enabled = false;
                    }

                    if(k-n > 2 || k >= data_size){
                        float input_result = Funcs.FromUint(velocity_ramresult.Data);
                        if(Math.Abs(updated_velocity[n] - input_result) > 0.0f)
                            Console.WriteLine("Update velocity result - Got {0}, expected {1} at {2}",
                                    input_result, updated_velocity[n], n);
                        n++;
                    }
                    if(n >= data_size)
                        running = false;
                }
                await ClockAsync();
                
            }
        }
    }
    
    
}