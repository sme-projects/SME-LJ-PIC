using System;
using System.Collections.Generic;

using SME;
using SME.Components;
using Deflib;

using System.Linq;

namespace Velocity{


    public class Testing_Simulation : SimulationProcess
    {

        [InputBus]
        public FlagBus finished;

        [InputBus]
        public TrueDualPortMemory<uint>.IReadResultB velocity_ramresult;

        [OutputBus]
        public FlagBus sim_ready = Scope.CreateBus<FlagBus>();

        [OutputBus]
        public FlagBus acc_ready = Scope.CreateBus<FlagBus>();

        [OutputBus]
        public TrueDualPortMemory<uint>.IControlB velocity_ramctrl;
        [OutputBus]
        public TrueDualPortMemory<uint>.IControlB acceleration_ramctrl;

       



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
            // Testing data
            // Positions array is kept float so that the uint bitstream can be
            // generated correctly
            float[] positions = new float[data_size];
            // Generate random accelerations for testing
            for(uint i = 0; i < data_size; i++){
                int random_number = rnd.Next(10,1000);
                if(!positions.Contains((float) random_number))
                    positions[i] = (float) random_number;
                    // positions[i] = i + 1 ;
                else
                    i--;
            }
            // Calculate acceleration and add to previous calculations
            float[] acceleration = new float[positions.Length];
            for(int i = 0; i < positions.Length - 1; i++){
                for(int j = i + 1; j < positions.Length; j++){
                    float calculated_result = Sim_Funcs.Acceleration_Calc(positions[i], positions[j]);
                    acceleration[i] += calculated_result;
                    acceleration[j] -= calculated_result;
                }
            }

            // Generate random velocities for testing
            float[] velocities = new float[positions.Length];
            for(int i = 0; i < positions.Length; i++){
                int random_number = rnd.Next(10,1000);
                velocities[i] = (float) random_number;
            }

            // Write initial data to ram
            for(int i = 0; i < positions.Length; i++){
                acceleration_ramctrl.Enabled = true;
                acceleration_ramctrl.Address = i;
                acceleration_ramctrl.Data = Funcs.FromFloat(acceleration[i]);
                acceleration_ramctrl.IsWriting = true;

                velocity_ramctrl.Enabled = true;
                velocity_ramctrl.Address = i;
                // velocity_ramctrl.Data = Funcs.FromFloat(0.0f);
                velocity_ramctrl.Data = Funcs.FromFloat(velocities[i]);
                velocity_ramctrl.IsWriting = true;
                await ClockAsync();
            }
            acceleration_ramctrl.Enabled = false;
            acceleration_ramctrl.IsWriting = false;
            velocity_ramctrl.Enabled = false;
            velocity_ramctrl.IsWriting = false;

            float[] updated_velocities = new float[positions.Length];
            // Calculate velocity for tests
            for( int i = 0; i < positions.Length; i++){
                float velocity_result = Sim_Funcs.Velocity_Calc(velocities[i], acceleration[i], timestep);
                updated_velocities[i] = velocity_result;
            }

            sim_ready.valid = true;
            await ClockAsync();
            sim_ready.valid = false;
            acc_ready.valid = true;
            await ClockAsync();
            acc_ready.valid = false;

            int k = 0;
            int n = 0;
            while(running){
                if(finished.valid){
                    
                    if(k < positions.Length){
                        velocity_ramctrl.Enabled = true;
                        velocity_ramctrl.Data = 0;
                        velocity_ramctrl.IsWriting = false;
                        velocity_ramctrl.Address = k;
                        k++;
                    }else{
                        velocity_ramctrl.Enabled = false;
                    }

                    if(k-n > 2 || k >= positions.Length){
                        float input_result = Funcs.FromUint(velocity_ramresult.Data);
                        if(updated_velocities[n] != input_result)
                            Console.WriteLine("Velocity result - Got {0}, expected {1} at {2}",
                                    input_result, updated_velocities[n], n);
                        n++;
                    }
                    if(n >= positions.Length)
                        running = false;
                }
                await ClockAsync();
                
            }           
        }
    }


    public class Internal_Velocity_Sim : SimulationProcess 
    {
        [InputBus]
        public ValBus prev_velocity;
        [InputBus]
        public ValBus acceleration;
        [InputBus]
        public ValBus input_velocity_result;

        public Internal_Velocity_Sim(float timestep)
        {
            this.timestep = timestep;
        }

        private float timestep;

        public override async System.Threading.Tasks.Task Run()
        {
            Queue<float> input_queue = new Queue<float>();
            while(true){
                while(!prev_velocity.valid && !acceleration.valid) {
                        await ClockAsync();    
                }
                float prev_velocity_val = Funcs.FromUint(prev_velocity.val);
                float acceleration_val = Funcs.FromUint(acceleration.val);
                //Velocity test results
                float velocity_result = Sim_Funcs.Velocity_Calc(prev_velocity_val, acceleration_val, timestep);
                input_queue.Enqueue(velocity_result);
                if(input_velocity_result.valid){
                    float result = Funcs.FromUint(input_velocity_result.val);
                    float calc_result = input_queue.Dequeue();
                    if (calc_result != result){
                        Console.WriteLine("Internal Velocity sim: Got {0}, expected {1}", result, calc_result);
                    }
                }
                await ClockAsync();
            }
            
        }
    }

}