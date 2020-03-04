using System;
using System.Collections.Generic;

using SME;
using SME.Components;
using Deflib;

using System.Linq;

namespace Position_Update{


    public class Testing_Simulation : SimulationProcess
    {

        [InputBus]
        public FlagBus finished;

        [InputBus]
        public TrueDualPortMemory<uint>.IReadResultB data_point_ramresult;

        [OutputBus]
        public FlagBus sim_ready = Scope.CreateBus<FlagBus>();

        [OutputBus]
        public FlagBus data_ready = Scope.CreateBus<FlagBus>();

        [OutputBus]
        public TrueDualPortMemory<uint>.IControlB data_point_ramctrl;
        [OutputBus]
        public TrueDualPortMemory<uint>.IControlB velocity_data_point_ramctrl;

       



        public Testing_Simulation(uint data_size, float timestep_size){
            this.data_size = data_size;
            this.timestep = timestep_size;
        }

        private uint data_size;
        private float timestep;

        public override async System.Threading.Tasks.Task Run()
        {
            await ClockAsync();
            bool running = true;
            Random rnd = new Random();


            //// Testing data

            // Generate random data for testing
            float[] position_data = new float[data_size];
            for(int i = 0; i < data_size; i++){
                int random_number = rnd.Next(10,1000);
                if(!position_data.Contains((float) random_number))
                    position_data[i] = (float) random_number;
                else
                    i--;
            }

            // Creating random or non-random data variables as the external 
            // variables in the formula
            float[] random_velocity_data = new float[data_size];
            float[] velocity_data = new float[data_size];
            
            // Generate random data for testing
            for(uint i = 0; i < data_size; i++){
                int random_number = rnd.Next(10,100);
                if(!random_velocity_data.Contains((float) random_number))
                    random_velocity_data[i] = (float) random_number;
                else
                    i--;
            }

            // Generate data for testing
            for(uint i = 0; i < data_size; i++)
                velocity_data[i] = i + 1;


            // Write initial data to ram
            for(int i = 0; i < data_size; i++){
                // Data points
                data_point_ramctrl.Enabled = true;
                data_point_ramctrl.Address = i;
                data_point_ramctrl.Data = Funcs.FromFloat(position_data[i]);
                data_point_ramctrl.IsWriting = true;

                // External variable data points
                velocity_data_point_ramctrl.Enabled = true;
                velocity_data_point_ramctrl.Address = i;
                velocity_data_point_ramctrl.Data = Funcs.FromFloat(random_velocity_data[i]);
                velocity_data_point_ramctrl.IsWriting = true;

                await ClockAsync();
            }
            velocity_data_point_ramctrl.Enabled = false;
            velocity_data_point_ramctrl.IsWriting = false;
            data_point_ramctrl.Enabled = false;
            data_point_ramctrl.IsWriting = false;

            float[] updated_data_points = new float[data_size];
            // Calculate data for tests
            for( int i = 0; i < data_size; i++){
                float update_result = Sim_Funcs.Update_Data_Calc(position_data[i], random_velocity_data[i], timestep);
                updated_data_points[i] = update_result;
            }

            sim_ready.valid = true;
            await ClockAsync();
            sim_ready.valid = false;
            data_ready.valid = true;
            await ClockAsync();
            data_ready.valid = false;

            int k = 0;
            int n = 0;
            while(running){
                if(finished.valid){
                    
                    if(k < random_velocity_data.Length){
                        data_point_ramctrl.Enabled = true;
                        data_point_ramctrl.Data = 0;
                        data_point_ramctrl.IsWriting = false;
                        data_point_ramctrl.Address = k;
                        k++;
                    }else{
                        data_point_ramctrl.Enabled = false;
                    }

                    if(k-n > 2 || k >= random_velocity_data.Length){
                        float input_result = Funcs.FromUint(data_point_ramresult.Data);
                        if(updated_data_points[n] - input_result > 0.0f)
                            Console.WriteLine("Update data result - Got {0}, expected {1} at {2}",
                                    input_result, updated_data_points[n], n);
                        n++;
                    }
                    if(n >= random_velocity_data.Length)
                        running = false;
                }
                await ClockAsync();
                
            }           
        }
    }

}