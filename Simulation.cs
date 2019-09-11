using SME;
using SME.Components;
using System;
using System.Linq;

namespace Lennard_Jones 
{
    public class Sim : SimulationProcess 
    {

        [ InputBus]
        public ValBus input;
        [OutputBus]
        public ValBus output = Scope.CreateBus<ValBus>();

        public Sim(float r, float sigma, float epsilon)
        {
            this.r = r;
            this.SIGMA = sigma;
            this.EPSILON = epsilon;
        }

        private float r;
        private float SIGMA;
        private float EPSILON;

        public override async System.Threading.Tasks.Task Run()
        {
            await ClockAsync();
            float div = SIGMA / r;
            //float pow12 = (float) Math.Pow(div, 12);
            //float pow6 = (float) Math.Pow(div,6);
            float ln = (float) Math.Log(div);
            float mul12 = ln * 12;
            float mul6 = ln * 6;
            float exp12 = (float) Math.Exp(mul12);
            float exp6 = (float) Math.Exp(mul6);
            //float min = pow12 - pow6;
            float min = exp12 - exp6;
            float fourepsilon = 4 * EPSILON;
            //float force_result = fourepsilon * min;
            float force_result = 4 * EPSILON * (((float) Math.Exp(((float)Math.Log(SIGMA  / r)) * 12)) - ((float) Math.Exp(((float)Math.Log(SIGMA  / r)) * 6)));

            output.val = Funcs.FromFloat(r);
            output.valid = true;
            await ClockAsync();
            output.valid = false;
            while(!input.valid) {
                    await ClockAsync();    
            }
            float float_val = Funcs.FromUint(input.val);
            if(float_val != force_result) {
                    Console.WriteLine("Got {0}, expected {1}", float_val, force_result);
            }
            //Console.WriteLine("Got {0}, expected {1}", float_val, force_result);

        }
    }
}