using System;
using System.Collections.Generic;

using SME;
using SME.Components;
using Deflib;

using System.Linq;

namespace Data_Update{


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
        public TrueDualPortMemory<uint>.IControlB external_data_point_ramctrl;

       



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
            float[] random_data = new float[data_size];
            for(int i = 0; i < data_size; i++){
                int random_number = rnd.Next(10,1000);
                if(!random_data.Contains((float) random_number))
                    random_data[i] = (float) random_number;
                else
                    i--;
            }

            // Creating random or non-random data variables as the external 
            // variables in the formula
            float[] random_external_data = new float[data_size];
            float[] external_data = new float[data_size];
            
            // Generate random data for testing
            for(uint i = 0; i < data_size; i++){
                int random_number = rnd.Next(10,100);
                if(!random_external_data.Contains((float) random_number))
                    random_external_data[i] = (float) random_number;
                else
                    i--;
            }

            // Generate data for testing
            for(uint i = 0; i < data_size; i++)
                external_data[i] = i + 1;


            // Write initial data to ram
            for(int i = 0; i < data_size; i++){
                // Data points
                data_point_ramctrl.Enabled = true;
                data_point_ramctrl.Address = i;
                data_point_ramctrl.Data = Funcs.FromFloat(random_data[i]);
                data_point_ramctrl.IsWriting = true;

                // External variable data points
                external_data_point_ramctrl.Enabled = true;
                external_data_point_ramctrl.Address = i;
                external_data_point_ramctrl.Data = Funcs.FromFloat(random_external_data[i]);
                external_data_point_ramctrl.IsWriting = true;

                await ClockAsync();
            }
            external_data_point_ramctrl.Enabled = false;
            external_data_point_ramctrl.IsWriting = false;
            data_point_ramctrl.Enabled = false;
            data_point_ramctrl.IsWriting = false;

            float[] updated_data_points = new float[data_size];
            // Calculate data for tests
            for( int i = 0; i < data_size; i++){
                float update_result = Sim_Funcs.Update_Data_Calc(random_data[i], random_external_data[i], timestep);
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
                    
                    if(k < random_external_data.Length){
                        data_point_ramctrl.Enabled = true;
                        data_point_ramctrl.Data = 0;
                        data_point_ramctrl.IsWriting = false;
                        data_point_ramctrl.Address = k;
                        k++;
                    }else{
                        data_point_ramctrl.Enabled = false;
                    }

                    if(k-n > 2 || k >= random_external_data.Length){
                        float input_result = Funcs.FromUint(data_point_ramresult.Data);
                        // TODO: Change comparison method
                        if(updated_data_points[n] != input_result)
                            Console.WriteLine("Update data result - Got {0}, expected {1} at {2}",
                                    input_result, updated_data_points[n], n);
                        n++;
                    }
                    if(n >= random_external_data.Length)
                        running = false;
                }
                await ClockAsync();
                
            }           
        }
    }

    public class Internal_Simulation : SimulationProcess 
    {
        [InputBus]
        public ValBus prev_data_point;
        [InputBus]
        public ValBus external_data_point;
        [InputBus]
        public ValBus input_result;

        public Internal_Simulation(float timestep)
        {
            this.timestep = timestep;
        }

        private float timestep;

        public override async System.Threading.Tasks.Task Run()
        {
            Queue<float> input_queue = new Queue<float>();
            while(true){
                while(!prev_data_point.valid && !external_data_point.valid) {
                        await ClockAsync();    
                }
                float prev_data_point_val = Funcs.FromUint(prev_data_point.val);
                float external_data_point_val = Funcs.FromUint(external_data_point.val);
                //Test results
                float Update_result = Sim_Funcs.Update_Data_Calc(prev_data_point_val, external_data_point_val, timestep);
                input_queue.Enqueue(Update_result);
                if(input_result.valid){
                    float result = Funcs.FromUint(input_result.val);
                    float calc_result = input_queue.Dequeue();
                    // TODO: Change comparison method
                    if (calc_result != result){
                        Console.WriteLine("Internal update data sim: Got {0}, expected {1}", result, calc_result);
                    }
                }
                await ClockAsync();
            }
            
        }
    }

}