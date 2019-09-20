using SME;
using SME.Components;
using System;
using System.Linq;

namespace Lennard_Jones 
{
    
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
            // Simulation data for Force
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
            // Data for checking result from Force
            while(!input_r.valid) {
                    await ClockAsync();    
            }
            float float_r = Funcs.FromUint(input_r.val);
            float force_result = force_calc(float_r);

            // Getting data from Force process calculation
            while(!input_result.valid) {
                    await ClockAsync();    
            }
            float float_val = Funcs.FromUint(input_result.val);
            
            if(float_val == force_result) {
                    Console.WriteLine("Got {0}, expected {1}", float_val, force_result);
            }
        }

        private float force_calc(float r)
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

    // ACCELERATION SIMULATIONS


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
        [OutputBus]
        public ValBus input_result;

        
        public Internal_Acceleration_Sim(float mass_of_argon)
        {

            this.MASS = Funcs.FromFloat(mass_of_argon);
        }

        private uint MASS;

        public override async System.Threading.Tasks.Task Run()
        {
            
            await ClockAsync();
            while(!input_pos1.valid && !input_pos2.valid) {
                    await ClockAsync();    
            }
            float pos1 = Funcs.FromUint(input_pos1.val);
            float pos2 = Funcs.FromUint(input_pos2.val);
            Console.WriteLine("pos1 : {0}, pos2 : {1}", pos1, pos2);

            while(!input_result.valid) {
                    await ClockAsync();    
            }
             float result = Funcs.FromUint(input_result.val);
            Console.WriteLine("input_result (mass) : {0}", result);

            /* //Second test
            float r_x = pos2 - pos1;
            // <- Force calculation
            float force_x = force_result * r_x / r_x;
            float a1 = force_x / MASS;
            float a2 = (- force_x) / MASS;
            //Console.WriteLine("Got {0}, expected {1}", float_val, force_result);
             */
        }

    }
}