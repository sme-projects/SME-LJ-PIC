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
        public TrueDualPortMemory<ulong>.IReadResultB data_point_ramresult;

        [OutputBus]
        public FlagBus sim_ready = Scope.CreateBus<FlagBus>();

        [OutputBus]
        public FlagBus data_ready = Scope.CreateBus<FlagBus>();

        [OutputBus]
        public TrueDualPortMemory<ulong>.IControlB data_point_ramctrl;
        [OutputBus]
        public TrueDualPortMemory<ulong>.IControlA velocity_data_point_ramctrl;

       



        public Testing_Simulation(ulong data_size, double timestep_size){
            this.data_size = (uint)data_size;
            this.timestep = timestep_size;
        }

        private uint data_size;
        private double timestep;

        public override async System.Threading.Tasks.Task Run()
        {
            await ClockAsync();
            bool running = true;
            Random rnd = new Random();


            //// Testing data

            // Generate random data for testing
            double[] position_data = new double[data_size];
            for(long i = 0; i < data_size; i++){
                long random_number = rnd.Next(10,1000);
                if(!position_data.Contains((double) random_number))
                    position_data[i] = (double) random_number;
                else
                    i--;
            }

            // Creating random or non-random data variables as the external 
            // variables in the formula
            double[] random_velocity_data = new double[data_size];
            double[] velocity_data = new double[data_size];
            
            // Generate random data for testing
            for(ulong i = 0; i < data_size; i++){
                long random_number = rnd.Next(10,100);
                if(!random_velocity_data.Contains((double) random_number))
                    random_velocity_data[i] = (double) random_number;
                else
                    i--;
            }

            // Generate data for testing
            for(ulong i = 0; i < data_size; i++)
                velocity_data[i] = i + 1;


            // Write initial data to ram
            for(int i = 0; i < data_size; i++){
                // Data points
                data_point_ramctrl.Enabled = true;
                data_point_ramctrl.Address = i;
                data_point_ramctrl.Data = Funcs.FromDouble(position_data[i]);
                data_point_ramctrl.IsWriting = true;

                // External variable data points
                velocity_data_point_ramctrl.Enabled = true;
                velocity_data_point_ramctrl.Address = i;
                velocity_data_point_ramctrl.Data = Funcs.FromDouble(random_velocity_data[i]);
                velocity_data_point_ramctrl.IsWriting = true;

                await ClockAsync();
            }
            velocity_data_point_ramctrl.Enabled = false;
            velocity_data_point_ramctrl.IsWriting = false;
            data_point_ramctrl.Enabled = false;
            data_point_ramctrl.IsWriting = false;

            double[] updated_data_points = new double[data_size];
            // Calculate data for tests
            for( long i = 0; i < data_size; i++){
                double update_result = Sim_Funcs.Update_Data_Calc(position_data[i], random_velocity_data[i], timestep);
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
            bool receive_data_ready = false;
            while(running){
                if(finished.valid)
                    receive_data_ready = true;
                
                if(receive_data_ready){
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
                        double input_result = Funcs.FromUlong(data_point_ramresult.Data);
                        if(updated_data_points[n] - input_result > 0.0f)
                            Console.WriteLine("Update data result - Got {0}, expected {1} at {2}",
                                    input_result, updated_data_points[n], n);
                        n++;
                    }
                    if(n >= random_velocity_data.Length){
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