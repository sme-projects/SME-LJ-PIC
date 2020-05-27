using System;
using System.Collections.Generic;

using SME;
using SME.Components;
using Deflib;

using System.Linq;

namespace Velocity_Update{


    public class Testing_Simulation : SimulationProcess
    {

        [InputBus]
        public FlagBus finished;

        [InputBus]
        public TrueDualPortMemory<uint>.IReadResultA velocity_ramresult;

        [OutputBus]
        public FlagBus sim_ready = Scope.CreateBus<FlagBus>();

        [OutputBus]
        public FlagBus data_ready = Scope.CreateBus<FlagBus>();

        [OutputBus]
        public TrueDualPortMemory<uint>.IControlA velocity_ramctrl;

        [OutputBus]
        public RamCtrlArray acceleration_ramctrl;
       



        public Testing_Simulation(uint data_size, float timestep_size, uint cache_size){
            this.data_size = data_size;
            this.timestep = timestep_size;
            this.cache_size = cache_size;
        }

        private uint data_size;
        private float timestep;
        private float cache_size;

        public override async System.Threading.Tasks.Task Run()
        {
            await ClockAsync();
            bool running = true;
            Random rnd = new Random();


            //// Testing data

            // Generate random data for testing
            float[] random_velocity_data = new float[data_size];
            float[] velocity_data = new float[data_size];
            
            for(int i = 0; i < data_size; i++){
                int random_number = rnd.Next(10,1000);
                if(!random_velocity_data.Contains((float) random_number))
                    random_velocity_data[i] = (float) random_number;
                else
                    i--;
                // Non-random data:
                velocity_data[i] = (float) i + 1;
            }

            // Creating random or non-random data variables as the external 
            // variables in the formula
            float[] random_acceleration_data = new float[data_size];
            float[] acceleration_data = new float[data_size];
            
            // Generate random data for testing
            for(uint i = 0; i < data_size; i++){
                // Random data
                int random_number = rnd.Next(10,100);
                if(!random_acceleration_data.Contains((float) random_number))
                    random_acceleration_data[i] = (float) random_number;
                else
                    i--;
                // non-random data
                acceleration_data[i] = (float) i + 1;
            }

            // Write initial velocity data to ram
            for(int i = 0; i < data_size; i++){
                // Data points
                velocity_ramctrl.Enabled = true;
                velocity_ramctrl.Address = i;
                velocity_ramctrl.Data = Funcs.FromFloat(random_velocity_data[i]);
                velocity_ramctrl.IsWriting = true;

                await ClockAsync();
            }

            // Write initial acceleration data to ram
            for (uint i = 0; i < data_size/cache_size; i++){
                acceleration_ramctrl.Enabled = true;
                acceleration_ramctrl.Address = i;
                for (int j = 0; j < cache_size; j++)
                    acceleration_ramctrl.Data[j] = Funcs.FromFloat(random_acceleration_data[j+(i*(uint)cache_size)]);
                acceleration_ramctrl.IsWriting = true;

                await ClockAsync();
            }
            acceleration_ramctrl.Enabled = false;
            acceleration_ramctrl.IsWriting = false;
            velocity_ramctrl.Enabled = false;
            velocity_ramctrl.IsWriting = false;

            float[] updated_velocity = new float[data_size];
            // Calculate data for tests
            for( int i = 0; i < data_size; i++){
                float update_result = Sim_Funcs.Update_Data_Calc(random_velocity_data[i], random_acceleration_data[i], timestep);
                updated_velocity[i] = update_result;
            }

            sim_ready.valid = true;
            await ClockAsync();
            sim_ready.valid = false;

            // Simulate the wait from the cache
            for(int i = 0; i < Math.Abs(data_size/cache_size); i++){
                int random_wait = rnd.Next(5,50);
                for(int j = 0; j < random_wait;j++){
                    await ClockAsync();
                    data_ready.valid = false;
                }
                data_ready.valid = true;
            }
            await ClockAsync();
            data_ready.valid = false;

            int k = 0;
            int n = 0;
            bool receive_data_ready = false;
            while(running){
                if(finished.valid)
                    receive_data_ready = true;
                
                if(receive_data_ready){
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
                        if(updated_velocity[n] - input_result > 0.0f)
                            Console.WriteLine("Update data result - Got {0}, expected {1} at {2}",
                                    input_result, updated_velocity[n], n);
                        n++;
                    }
                    if(n >= acceleration_data.Length){
                        running = false;
                        receive_data_ready = false;
                        // TODO: Add looping functionality - see Lennard_Jones.Simulations
                    }
                }
                await ClockAsync();
                
            }           
        }
    }

}