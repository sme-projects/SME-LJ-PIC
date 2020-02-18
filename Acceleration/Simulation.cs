using System;
using System.Collections.Generic;
using SME;
using SME.Components;
using Deflib;

using System.Linq;

namespace Acceleration{


    public class Testing_Simulation : SimulationProcess
    {
        [InputBus]
        public ValBus input;

        [OutputBus]
        public TrueDualPortMemory<uint>.IControlB pos1_ramctrl;
        [OutputBus]
        public TrueDualPortMemory<uint>.IControlB pos2_ramctrl;

        [OutputBus]
        public ValBus output = Scope.CreateBus<ValBus>();

        public Testing_Simulation(uint data_size){
            this.data_size = data_size;
        }

        private uint data_size;

        public override async System.Threading.Tasks.Task Run()
        {
            await ClockAsync();

            Random rnd = new Random();
            // Testing data
            // Positions array is kept float so that the uint bitstream can be
            // generated correctly
            float[] positions = new float[data_size];
            for(uint k = 0; k < data_size; k++){
                int random_number = rnd.Next(10,1000);
                float float_rnd_number = random_number;
                if(!positions.Contains((float) random_number))
                    positions[k] = (float) random_number;
                else
                    k--;
                // Non-random data:
                // positions[k] = (float) k + 1;
            }
            
            for(int k = 0; k < positions.Length; k++){
                pos1_ramctrl.Enabled = true;
                pos1_ramctrl.Address = k;
                pos1_ramctrl.Data = Funcs.FromFloat(positions[k]);
                pos1_ramctrl.IsWriting = true;

                pos2_ramctrl.Enabled = true;
                pos2_ramctrl.Address = k;
                pos2_ramctrl.Data = Funcs.FromFloat(positions[k]);
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

            int i = 0;
            int j = 1;
            Queue<float> calculated_result_queue = new Queue<float>();
            while(running){

                if(input.valid){
                    // TODO: Rename the control result to something with "control result"
                    float calculated_result = Sim_Funcs.Acceleration_Calc(positions[i], positions[j]);
                    calculated_result_queue.Enqueue(calculated_result);
                    if(i <= data_size -2 && j <= data_size -1){
                        float calc_result = calculated_result_queue.Dequeue();
                        float input_result = Funcs.FromUint(input.val);

                        // TODO: Figure out what the expected and accepted difference can be
                        if(Math.Abs(calc_result - input_result) > 1/(Math.Pow(10,7))){
                            Console.WriteLine("pos1 {0}: {1}, pos2 {2}: {3}", i, positions[i], j, positions[j]);
                            Console.WriteLine("Acceleration test sim - Got {0}, Expected {0}", input_result, calc_result);
                            // Console.WriteLine($"Acc difference : Got {Math.Abs(calc_result - input_result)}");
                        }
                        if(i >= data_size - 2){
                            running = false;
                            Console.WriteLine("Acceleration test successfully completed");
                        }
                        if(j >= data_size -1 ){
                            i++;
                            j = i + 1;
                        }else{
                            j++;
                        }
                    }
                }
                await ClockAsync();
            }
        }
    }


    // FORCE SIMULATIONS

    // Currently not in use
    public class External_Force_Sim : SimulationProcess 
    {
        [InputBus]
        public ValBus input;
        [OutputBus]
        public ValBus output = Scope.CreateBus<ValBus>();

        public External_Force_Sim(float r)
        {
            this.val = Funcs.FromFloat(r);
        }

        private uint val;

        public override async System.Threading.Tasks.Task Run()
        {
            await ClockAsync();
            output.val = val;
            output.valid = true;
            while(!input.valid) {
                    await ClockAsync();    
            }
        }
    }


    public class Internal_Force_Sim : SimulationProcess 
    {

        [InputBus]
        public ValBus input_r;
        [InputBus]
        public ValBus input_result;

        
        public Internal_Force_Sim(float sigma, float epsilon)
        {
            this.SIGMA = sigma;
            this.EPSILON = epsilon;
        }

        
        private float SIGMA;
        private float EPSILON;

        public override async System.Threading.Tasks.Task Run()
        {
            Queue<float> input_queue = new Queue<float>();
            while(true){
                while(!input_r.valid) {
                        await ClockAsync();    
                }
                float float_r = Funcs.FromUint(input_r.val);
            
                 // Force test results
                 // TODO: Rename the control result to something with "control result"
                float force_result = Sim_Funcs.Force_Calc(float_r);
                input_queue.Enqueue(force_result);
                if(input_result.valid){
                    float float_val = Funcs.FromUint(input_result.val);
                    float calc_result = input_queue.Dequeue();
                    // TODO: Figure out what the expected and accepted difference can be
                    if(Math.Abs(calc_result - float_val) > 1/(Math.Pow(10,7))) {
                        Console.WriteLine("Internal force sim: Got {0}, expected {1}", float_val, calc_result);
                        Console.WriteLine($"Internal force diff : Got {Math.Abs(calc_result - float_val)}");
                    }
                }
                await ClockAsync();
            }
                
            
        }
    }

    // ACCELERATION SIMULATIONS

    // Currently not in use
    public class External_Acceleration_Sim : SimulationProcess 
    {

        [InputBus]
        public ValBus input;
        [OutputBus]
        public ValBus output_pos1 = Scope.CreateBus<ValBus>();
        [OutputBus]
        public ValBus output_pos2 = Scope.CreateBus<ValBus>();


        public External_Acceleration_Sim(float pos1, float pos2)
        {
            this.pos1 = Funcs.FromFloat(pos1);
            this.pos2 = Funcs.FromFloat(pos2);
            
        }

        private uint pos1;
        private uint pos2;

        public override async System.Threading.Tasks.Task Run()
        {
            await ClockAsync();
            output_pos1.val = pos1;
            output_pos1.valid = true;
            output_pos2.val = pos2;
            output_pos2.valid = true;
            while(!input.valid) {
                    await ClockAsync();    
            }

        }
    }

     public class Internal_Acceleration_Sim : SimulationProcess 
    {
        [InputBus]
        public ValBus input_pos1;
        [InputBus]
        public ValBus input_pos2;
        [InputBus]
        public ValBus input_result;

        
        public Internal_Acceleration_Sim(float mass_of_argon, float sigma, float epsilon)
        {

            this.MASS = mass_of_argon;
            this.SIGMA = sigma;
            this.EPSILON = epsilon;
        }

        private float MASS;
        private float SIGMA;
        private float EPSILON;

        public override async System.Threading.Tasks.Task Run()
        {
            Queue<float> input_queue = new Queue<float>();
            while(true){
                while(!input_pos1.valid && !input_pos2.valid) {
                        await ClockAsync();    
                }
                float pos1 = Funcs.FromUint(input_pos1.val);
                float pos2 = Funcs.FromUint(input_pos2.val);
                //Acceleration test results
                // TODO: Rename the control result to something with "control result"
                float acceleration_result = Sim_Funcs.Acceleration_Calc(pos1, pos2);
                input_queue.Enqueue(acceleration_result);
                if(input_result.valid){
                    float result = Funcs.FromUint(input_result.val);
                    float calc_result = input_queue.Dequeue();
                    // TODO: Figure out what the expected and accepted difference can be
                    if (Math.Abs(calc_result - result) > 1/(Math.Pow(10,7))){
                        Console.WriteLine("Internal Acceleration sim: Got {0}, expected {1}", result, calc_result);
                        Console.WriteLine($"Internal Acc diff: Got {Math.Abs(calc_result - result)}");
                    }
                }
                await ClockAsync();
            }
            
        }
    }
}