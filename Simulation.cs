using SME;
using SME.Components;
using System;
using System.Linq;

namespace Lennard_Jones 
{

    public class External_Force_Sim : SimulationProcess 
    {

        [OutputBus]
        public ValBus output = Scope.CreateBus<ValBus>();
        [ InputBus]
        public ValBus input;


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

        [ InputBus]
        public ValBus input_r;
        [ InputBus]
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
}