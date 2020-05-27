using System;
using SME;

namespace Deflib{
    public class Sim_Funcs
    {
        static double MASS_OF_ARGON = 39.948;
        static double SIGMA = 3.4;
        static double EPSILON = 0.0103;

        public static double Update_Data_Calc(double prev_data_point, double external_data_point, double timestep)
        {
            double result = prev_data_point + (external_data_point * timestep);
            return result;
        }


        public static double Acceleration_Calc(double pos1, double pos2)
        {
            double r = pos2 - pos1;
            double f_calc = Force_Calc(r);
            double f = f_calc * r;
            double result = f / MASS_OF_ARGON;
            return result;
        }

        public static double Force_Calc(double r)
        {
            ulong abs_r = Funcs.FromDouble(Math.Abs(r));
            ulong abs_sigma = Funcs.FromDouble(Math.Abs(SIGMA));
            ulong ln_r = Funcs.FromDouble((double) Math.Log(Funcs.FromUlong(abs_r)));
            ulong ln_sigma = Funcs.FromDouble((double) Math.Log(Funcs.FromUlong(abs_sigma)));

            ulong mul_12 = Funcs.FromDouble(12 * Funcs.FromUlong(ln_sigma));
            ulong mul_6 = Funcs.FromDouble(6 * Funcs.FromUlong(ln_sigma));
            ulong mul_14 = Funcs.FromDouble(14 * Funcs.FromUlong(ln_r));
            ulong mul_8 = Funcs.FromDouble(8 * Funcs.FromUlong(ln_r));
            
            ulong exp_12 = Funcs.FromDouble((double) Math.Exp(Funcs.FromUlong(mul_12))); 
            ulong exp_6 = Funcs.FromDouble((double) Math.Exp(Funcs.FromUlong(mul_6))); 
            ulong exp_14 = Funcs.FromDouble((double) Math.Exp(Funcs.FromUlong(mul_14))); 
            ulong exp_8 = Funcs.FromDouble((double) Math.Exp(Funcs.FromUlong(mul_8))); 

            ulong div_12_14 = Funcs.FromDouble(Funcs.FromUlong(exp_12) / Funcs.FromUlong(exp_14)); 
            ulong div_6_8 = Funcs.FromDouble(Funcs.FromUlong(exp_6) / Funcs.FromUlong(exp_8)); 
            ulong mul_epsilon_12_14 = Funcs.FromDouble(EPSILON * Funcs.FromUlong(div_12_14));
            ulong mul_epsilon_6_8 = Funcs.FromDouble(EPSILON * Funcs.FromUlong(div_6_8));
            
            
            ulong mul_48 = Funcs.FromDouble(48 * Funcs.FromUlong(mul_epsilon_12_14));
            ulong mul_24 = Funcs.FromDouble(24 * Funcs.FromUlong(mul_epsilon_6_8));

            ulong min = Funcs.FromDouble(Funcs.FromUlong(mul_48) - Funcs.FromUlong(mul_24));

            double force_result = Funcs.FromUlong(min);

            // double force_result = (48 * EPSILON * ((double) Math.Exp((double)Math.Log(Math.Abs(SIGMA)) * 12)  / (double) Math.Exp((double)Math.Log(Math.Abs(r)) * 14))) - (24 * EPSILON * ((double) Math.Exp((double)Math.Log(Math.Abs(SIGMA)) * 6)  / (double) Math.Exp((double)Math.Log(Math.Abs(r)) * 8)));
            return force_result;
        }
    }
}


