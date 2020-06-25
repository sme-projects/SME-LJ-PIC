using System;
using SME;
using SME.Components;
using Deflib;
using Acceleration;

using System.Linq;

namespace Lennard_Jones 
{

    public class External_LJ_Sim : SimulationProcess
    {
        [InputBus]
        public FlagBus finished;
        
        [InputBus]
        public TrueDualPortMemory<ulong>.IReadResultA position_ramresult;

        [OutputBus]
        public TrueDualPortMemory<ulong>.IControlA init_velocity_ramctrl;
    
        [OutputBus]
        public TrueDualPortMemory<ulong>.IControlA position_ramctrl;

        [OutputBus]
        public ValBus acc_ready = Scope.CreateBus<ValBus>();

        [OutputBus]
        public FlagBus velocity_reset = Scope.CreateBus<FlagBus>();
        
        [OutputBus]
        public FlagBus position_reset = Scope.CreateBus<FlagBus>();

        

        public External_LJ_Sim(ulong data_size, double timestep_size, ulong cache_size)
        {
            this.data_size = (uint)data_size;
            this.timestep = timestep_size;
            this.cache_size = cache_size;
        }

        private uint data_size;
        private double timestep;
        private ulong cache_size;

        public override async System.Threading.Tasks.Task Run() 
        {
            // Initial await
            await ClockAsync();


            Random rnd = new Random();
            // Testing data
            // Positions array is kept double so that the ulong bitstream can be
            // generated correctly
            double[] positions = new double[data_size];
            
            for(ulong i = 0; i < data_size; i++){
                // long random_number = rnd.Next(10,1000);
                // double double_rnd_number = random_number;
                // if(!positions.Contains((double) random_number))
                //     positions[i] = (double) random_number;
                // else
                //     i--;
                // Non-random data:
                positions[i] = (double) i + 1;
                // Console.WriteLine("position: {0}", positions[i]);
            }

            double[] velocity = new double[data_size];
            for(ulong i = 0; i < data_size; i++){
                velocity[i] = 0.0;
            }

            for(int i = 0; i < positions.Length; i++){
                // Write initial data to position ram
                position_ramctrl.Address = i;
                position_ramctrl.Data = Funcs.FromDouble(positions[i]);
                position_ramctrl.IsWriting = true;
                position_ramctrl.Enabled = true;

                // Write initial data to velocity ram
                init_velocity_ramctrl.Address = i;
                // Initial value is 0
                init_velocity_ramctrl.Data = Funcs.FromDouble(0.0f); 
                init_velocity_ramctrl.IsWriting = true;
                init_velocity_ramctrl.Enabled = true;

                await ClockAsync();
            }

            // LJ loop
            for(int k = 0; k < (uint)Number_of_loops.n; k++){

                bool running = true;
                position_ramctrl.Enabled = false;
                position_ramctrl.Data = 0;
                position_ramctrl.IsWriting = false;
                init_velocity_ramctrl.Enabled = false;
                init_velocity_ramctrl.Data = 0;
                init_velocity_ramctrl.IsWriting = false;
                acc_ready.val = data_size;
                acc_ready.valid = true;
                velocity_reset.valid = true;
                position_reset.valid = true;
                await ClockAsync();
                acc_ready.valid = false;
                velocity_reset.valid = false;
                position_reset.valid = false;


                // Calculating data for verifying results
                double[] accelerations = new double[positions.Length];
                for(long i = 0; i < positions.Length; i++){
                    for(long j = i + 1; j < positions.Length; j++){
                        double result = Sim_Funcs.Acceleration_2d_Calc(positions[i], positions[j]);
                        accelerations[i] += result;
                        accelerations[j] += - result;
                    }
                }

                double[] updated_velocity = new double[data_size];
                // Calculate data for tests
                for( long i = 0; i < data_size; i++){
                    // Initial velocity is 0
                    double update_result = Sim_Funcs.Update_Data_Calc(velocity[i], accelerations[i], timestep);
                    updated_velocity[i] = update_result;
                }

                double[] updated_positions = new double[data_size];
                // Calculate data for tests
                for( long i = 0; i < data_size; i++){
                    double update_result = Sim_Funcs.Update_Data_Calc(positions[i], updated_velocity[i], timestep);
                    updated_positions[i] = update_result;
                }

                int m = 0;
                int n = 0;
                bool data_ready = false;

                while(running){
                    if(finished.valid)
                        data_ready = true;
                        
                    if(data_ready){
                        if(m < data_size){
                            position_ramctrl.Enabled = true;
                            position_ramctrl.Data = 0;
                            position_ramctrl.IsWriting = false;
                            position_ramctrl.Address = m;
                            m++;
                        }else{
                            position_ramctrl.Enabled = false;
                        }

                        if(m-n > 2 || m >= data_size){
                            double input_result = Funcs.FromUlong(position_ramresult.Data);
                            if(Math.Abs(updated_positions[n] - input_result) > 0.00001f)
                                Console.WriteLine("Update position result - Got {0}, expected {1} at {2}",
                                        input_result, updated_positions[n], n);
                            n++;
                        }
                        if(n >= data_size){
                            running = false;
                            data_ready = false;
                            positions = updated_positions;
                            velocity = updated_velocity;
                        }
                    }
                    await ClockAsync();
                }
            Console.WriteLine("Loop {0} finished", k);
            }
        }
    }
    
}