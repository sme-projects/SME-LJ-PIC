using SME;
using SME.Components;
using System;
using System.Collections.Generic;

namespace Lennard_Jones 
{

    public class External_Sim : SimulationProcess
    {
        [InputBus]
        public ValBus input;
    
        [InputBus]
        public TrueDualPortMemory<uint>.IReadResultB acc_ramresult;
        [OutputBus]
        public TrueDualPortMemory<uint>.IControlB acc_ramctrl;
        
        [OutputBus]
        public TrueDualPortMemory<uint>.IControlB pos1_ramctrl;
        [OutputBus]
        public TrueDualPortMemory<uint>.IControlB pos2_ramctrl;

        [OutputBus]
        public ValBus output = Scope.CreateBus<ValBus>();

        

        public External_Sim(float[] positions)
        {
            this.positions = positions;
        }

        private float[] positions;

        public override async System.Threading.Tasks.Task Run() 
        {
            // Initial await
            await ClockAsync();
            for(int i = 0; i < positions.Length; i++){
                pos1_ramctrl.Enabled = true;
                pos1_ramctrl.Address = i;
                pos1_ramctrl.Data = Funcs.FromFloat(positions[i]);
                pos1_ramctrl.IsWriting = true;

                pos2_ramctrl.Enabled = true;
                pos2_ramctrl.Address = i;
                pos2_ramctrl.Data = Funcs.FromFloat(positions[i]);
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
            // Calculating tuples for the check queue
            Queue<(uint, uint)> position_queue = new Queue<(uint, uint)>();
            for(int i = 0; i < positions.Length; i++){
                for(int j = i + 1; j < positions.Length; j++){
                    position_queue.Enqueue((Funcs.FromFloat(positions[i]), Funcs.FromFloat(positions[j])));
                }
            }
            int k = 0;
            while(running){
                while(!input.valid) {
                    await ClockAsync();    
                }
                acc_ramctrl.Enabled = k < positions.Length;
                acc_ramctrl.Address = k;
                acc_ramctrl.Data = 0;
                acc_ramctrl.IsWriting = false;
                await ClockAsync();
                await ClockAsync();
                float input_result = Funcs.FromUint(acc_ramresult.Data);
                // Dequeueing the data along to check the results against the C# calculation
                (uint, uint) tuple = position_queue.Dequeue();
                float pos1 = Funcs.FromUint(tuple.Item1);
                float pos2 = Funcs.FromUint(tuple.Item2);
                float acceleration_calc_result = Sim_Funcs.Acceleration_Calc(pos1, pos2);
                if(acceleration_calc_result != input_result){
                    Console.WriteLine("Acceleration result - Got {0}, expected {1}", input_result, acceleration_calc_result);
                }
                if(position_queue.Count == 0){
                    running = false;
                }
                k++;
                
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
                float force_result = Sim_Funcs.Force_Calc(float_r);
                input_queue.Enqueue(force_result);
                if(input_result.valid){
                    float float_val = Funcs.FromUint(input_result.val);
                    float calc_result = input_queue.Dequeue();
                    if(float_val != calc_result) {
                        Console.WriteLine("Force sim: Got {0}, expected {1}", float_val, calc_result);
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
                float acceleration_result = Sim_Funcs.Acceleration_Calc(pos1, pos2);
                input_queue.Enqueue(acceleration_result);
                if(input_result.valid){
                    float result = Funcs.FromUint(input_result.val);
                    float calc_result = input_queue.Dequeue();
                    if (calc_result != result){
                        Console.WriteLine("Acceleration sim: Got {0}, expected {1}", result, calc_result);
                    }
                }
                await ClockAsync();
            }
            
        }
    }

    public class Sim_Funcs
    {
        static float MASS_OF_ARGON = 39.948f;
        static float SIGMA = 3.4f;
        static float EPSILON = 0.0103f;

        public static float Acceleration_Calc(float pos1, float pos2)
        {
            float r = pos2 - pos1;
            float f = Force_Calc(r);
            float result = f / MASS_OF_ARGON;
            return result;
        }

        public static float Force_Calc(float r)
        {
            float div = SIGMA / r;
            float ln = (float) Math.Log(div);
            float mul12 = ln * 12;
            float mul6 = ln * 6;
            float exp12 = (float) Math.Exp(mul12);
            float exp6 = (float) Math.Exp(mul6);
            float min = exp12 - exp6;
            float fourepsilon = 4 * EPSILON;
            float force_result = 4 * EPSILON 
                                 * (((float) Math.Exp(((float)Math.Log(SIGMA  / r)) 
                                 * 12)) - ((float) Math.Exp(((float)Math.Log(SIGMA  / r)) * 6)));
            return force_result;
        }
    }
}