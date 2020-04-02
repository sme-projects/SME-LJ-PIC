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
        public FlagBus finished;
        
        [InputBus]
        public TrueDualPortMemory<uint>.IReadResultA position_ramresult;

        [OutputBus]
        public TrueDualPortMemory<uint>.IControlA init_velocity_ramctrl;
    
        [OutputBus]
        public TrueDualPortMemory<uint>.IControlA position_ramctrl;

        [OutputBus]
        public ValBus acc_ready = Scope.CreateBus<ValBus>();

        [OutputBus]
        public FlagBus velocity_reset = Scope.CreateBus<FlagBus>();
        
        [OutputBus]
        public FlagBus position_reset = Scope.CreateBus<FlagBus>();

        

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
                // int random_number = rnd.Next(10,1000);
                // float float_rnd_number = random_number;
                // if(!positions.Contains((float) random_number))
                //     positions[i] = (float) random_number;
                // else
                //     i--;
                // Non-random data:
                positions[i] = (float) i + 1;
                // Console.WriteLine("position: {0}", positions[i]);
            }

            for(int i = 0; i < positions.Length; i++){
                // Write initial data to position ram
                position_ramctrl.Address = i;
                position_ramctrl.Data = Funcs.FromFloat(positions[i]);
                position_ramctrl.IsWriting = true;
                position_ramctrl.Enabled = true;

                // Write initial data to velocity ram
                init_velocity_ramctrl.Address = i;
                // Initial value is 0
                init_velocity_ramctrl.Data = Funcs.FromFloat(0.0f); 
                init_velocity_ramctrl.IsWriting = true;
                init_velocity_ramctrl.Enabled = true;

                await ClockAsync();
            }

            bool running = true;
            position_ramctrl.Enabled = false;
            init_velocity_ramctrl.Enabled = false;
            acc_ready.val = data_size;
            acc_ready.valid = true;
            velocity_reset.valid = true;
            position_reset.valid = true;
            await ClockAsync();
            acc_ready.valid = false;
            velocity_reset.valid = false;
            position_reset.valid = false;


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

            float[] updated_positions = new float[data_size];
            // Calculate data for tests
            for( int i = 0; i < data_size; i++){
                float update_result = Sim_Funcs.Update_Data_Calc(positions[i], updated_velocity[i], timestep);
                updated_positions[i] = update_result;
            }

            int k = 0;
            int n = 0;

            while(running){
                if(finished.valid){
                    
                    if(k < data_size){
                        position_ramctrl.Enabled = true;
                        position_ramctrl.Data = 0;
                        position_ramctrl.IsWriting = false;
                        position_ramctrl.Address = k;
                        k++;
                    }else{
                        position_ramctrl.Enabled = false;
                    }

                    if(k-n > 2 || k >= data_size){
                        float input_result = Funcs.FromUint(position_ramresult.Data);
                        if(Math.Abs(updated_positions[n] - input_result) > 0.0f)
                            Console.WriteLine("Update position result - Got {0}, expected {1} at {2}",
                                    input_result, updated_positions[n], n);
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