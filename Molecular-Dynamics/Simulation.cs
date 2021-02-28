using System;
using SME;
using SME.Components;
using Deflib;
using Acceleration;

using System.Linq;

namespace Molecular_Dynamics
{

        public class External_Sim : SimulationProcess
    {
        [InputBus]
        public FlagBus[] finished;
        
        [OutputBus]
        public ValBus[] init_position;

        [OutputBus]
        public ValBus[] init_velocity;


        public External_Sim(ulong data_size, double timestep_size)
        {
            this.data_size = (uint)data_size;
            this.timestep = timestep_size;

            init_position = new ValBus[(int)Deflib.Dimensions.n];
            init_velocity = new ValBus[(int)Deflib.Dimensions.n];
            finished = new FlagBus[(int)Deflib.Dimensions.n];
            for (int i= 0; i < (int)Deflib.Dimensions.n; i++){
                init_position[i] = Scope.CreateBus<ValBus>();
                init_velocity[i] = Scope.CreateBus<ValBus>();
            }
        }

        private uint data_size;
        private double timestep;

        public override async System.Threading.Tasks.Task Run() 
        {
            // Initial await
            await ClockAsync();


            Random rnd = new Random();
            // Testing data
            // Positions array is kept double so that the ulong bitstream can be
            // generated correctly
            double[,] positions = new double[(int)Deflib.Dimensions.n, data_size];
            double[,] velocity = new double[(int)Deflib.Dimensions.n, data_size];
            for(ulong i = 0; i < data_size; i++){
                for (int j = 0; j < (int)Deflib.Dimensions.n; j++){
                    // Non-random data:
                    positions[j, i] = (double) i + 1;
                    velocity[j,i] = 0.0;
                }
                
            }

            for(int i = 0; i < data_size; i++){
                for (int j = 0; j < (int)Deflib.Dimensions.n; j++){
                    // Write initial positions to external sim
                    init_position[j].valid = true;
                    init_position[j].val = Funcs.FromDouble(positions[j,i]);
                    
                    // Write initial velocity to external sim
                    init_velocity[j].valid = true;
                    init_velocity[j].val = Funcs.FromDouble(velocity[j,i]);
                }
                await ClockAsync();
            }

            bool running = true;
            while(running){
                bool isfinished = true;
                for (int i = 0; i < (int)Deflib.Dimensions.n; i++ ){
                    if(finished[i].valid && isfinished)
                        isfinished = true;
                    else
                        isfinished = false;
                }
                if(isfinished){
                    running = false;
                    Console.WriteLine("All dimensions have completed");
                }
                else
                    await ClockAsync();
            }
           
        }
    }

    public class External_MD_Sim : SimulationProcess
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


        // Busses to communicate with master simulation process
        [InputBus]
        public ValBus init_position;

        [InputBus]
        public ValBus init_velocity;

        [OutputBus]
        public FlagBus sim_finished = Scope.CreateBus<FlagBus>();
        

        public External_MD_Sim(ulong data_size, double timestep_size, ulong cache_size)
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

            double[] positions = new double[data_size];
            double[] velocities = new double[data_size];

            for(int i = 0; i < data_size; i++){
                if(init_position.valid && init_velocity.valid){
                    // Get data from master simulation process
                    ulong position = init_position.val;
                    positions[i] = Funcs.FromUlong(position);
                    ulong velocity = init_velocity.val;
                    velocities[i] = Funcs.FromUlong(velocity);

                    // Write initial data to position ram
                    position_ramctrl.Address = i;
                    position_ramctrl.Data = position;
                    position_ramctrl.IsWriting = true;
                    position_ramctrl.Enabled = true;

                    // Write initial data to velocity ram
                    init_velocity_ramctrl.Address = i;
                    // Initial value is 0
                    init_velocity_ramctrl.Data = velocity; 
                    init_velocity_ramctrl.IsWriting = true;
                    init_velocity_ramctrl.Enabled = true;
                }else{
                    i--;
                }
                await ClockAsync();
            }

            // MD loop
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
                double[] accelerations = new double[data_size];
                for(long i = 0; i < data_size; i++){
                    for(long j = i + 1; j < data_size; j++){
                        double result = Sim_Funcs.Acceleration_2d_Calc(positions[i], positions[j]);
                        accelerations[i] += result;
                        accelerations[j] += - result;
                    }
                }

                double[] updated_velocity = new double[data_size];
                // Calculate data for tests
                for( long i = 0; i < data_size; i++){
                    // Initial velocity is 0
                    double update_result = Sim_Funcs.Update_Data_Calc(velocities[i], accelerations[i], timestep);
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
                            velocities = updated_velocity;
                        }
                    }
                    await ClockAsync();
                }
            Console.WriteLine("Loop {0} finished", k);
            }
            sim_finished.valid = true;
        }
    }
    
}