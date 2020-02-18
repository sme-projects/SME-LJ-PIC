using System;
using SME;

namespace Deflib{
    public class Sim_Funcs
    {
        static float MASS_OF_ARGON = 39.948f;
        static float SIGMA = 3.4f;
        static float EPSILON = 0.0103f;

        public static float Update_Data_Calc(float prev_data_point, float external_data_point, float timestep)
        {
            float result = prev_data_point + (external_data_point * timestep);
            return result;
        }


        public static float Acceleration_Calc(float pos1, float pos2)
        {
            float r = pos2 - pos1;
            float f_calc = Force_Calc(r);
            float f = f_calc * r;
            // Console.WriteLine("f: {0}", f);
            float result = f / MASS_OF_ARGON;
            return result;
        }

        public static float Force_Calc(float r)
        {
            // float div = SIGMA / r;
            // float abs = (float) Math.Abs(div);
            // float ln = (float) Math.Log(abs);
            // float mul12 = ln * 12;
            // float mul6 = ln * 6;
            // float exp12 = (float) Math.Exp(mul12);
            // float exp6 = (float) Math.Exp(mul6);
            // float min = exp12 - exp6;
            // float fourepsilon = 4 * EPSILON;
            
            // float force_result = 4 * EPSILON 
            //                      * (((float) Math.Exp(((float)Math.Log(Math.Abs(SIGMA  / r))) 
            //                      * 12)) - ((float) Math.Exp(((float)Math.Log(Math.Abs(SIGMA  / r))) * 6)));
            
            uint abs_r = Funcs.FromFloat(Math.Abs(r));
            uint abs_sigma = Funcs.FromFloat(Math.Abs(SIGMA));
            uint ln_r = Funcs.FromFloat((float) Math.Log(Funcs.FromUint(abs_r)));
            uint ln_sigma = Funcs.FromFloat((float) Math.Log(Funcs.FromUint(abs_sigma)));
            
            uint mul_12 = Funcs.FromFloat(12 * Funcs.FromUint(ln_sigma));
            uint mul_6 = Funcs.FromFloat(6 * Funcs.FromUint(ln_sigma));
            uint mul_14 = Funcs.FromFloat(14 * Funcs.FromUint(ln_r));
            uint mul_8 = Funcs.FromFloat(8 * Funcs.FromUint(ln_r));
            
            uint exp_12 = Funcs.FromFloat((float) Math.Exp(Funcs.FromUint(mul_12))); 
            uint exp_6 = Funcs.FromFloat((float) Math.Exp(Funcs.FromUint(mul_6))); 
            uint exp_14 = Funcs.FromFloat((float) Math.Exp(Funcs.FromUint(mul_14))); 
            uint exp_8 = Funcs.FromFloat((float) Math.Exp(Funcs.FromUint(mul_8))); 

            uint div_12_14 = Funcs.FromFloat(Funcs.FromUint(exp_12) / Funcs.FromUint(exp_14)); 
            uint div_6_8 = Funcs.FromFloat(Funcs.FromUint(exp_6) / Funcs.FromUint(exp_8)); 

            uint mul_epsilon_12_14 = Funcs.FromFloat(EPSILON * Funcs.FromUint(div_12_14));
            uint mul_epsilon_6_8 = Funcs.FromFloat(EPSILON * Funcs.FromUint(div_6_8));
            
            
            uint mul_48 = Funcs.FromFloat(48 * Funcs.FromUint(mul_epsilon_12_14));
            uint mul_24 = Funcs.FromFloat(24 * Funcs.FromUint(mul_epsilon_6_8));
            
            // Console.WriteLine("r: {0}", r);
            // Console.WriteLine("Mul 48: {0}", Funcs.FromUint(mul_48));
            // Console.WriteLine("Mul 24: {0}", Funcs.FromUint(mul_24));
            

            uint min = Funcs.FromFloat(Funcs.FromUint(mul_48) - Funcs.FromUint(mul_24));

            float force_result = Funcs.FromUint(min);

            // float force_result = (48 * EPSILON * ((float) Math.Exp((float)Math.Log(Math.Abs(SIGMA)) * 12)  / (float) Math.Exp((float)Math.Log(Math.Abs(r)) * 14))) - (24 * EPSILON * ((float) Math.Exp((float)Math.Log(Math.Abs(SIGMA)) * 6)  / (float) Math.Exp((float)Math.Log(Math.Abs(r)) * 8)));
            return force_result;
        }
    }
}


