using System;
using SME;

namespace Deflib{
    public class Sim_Funcs
    {
        static float MASS_OF_ARGON = 39.948f;
        static float SIGMA = 3.4f;
        static float EPSILON = 0.0103f;

        public static float Velocity_Calc(float prev_velocity, float acceleration, float timestep)
        {
            float result = prev_velocity + (acceleration * timestep);
            return result;
        }


        public static float Acceleration_Calc(float pos1, float pos2)
        {
            float r = pos2 - pos1;
            float f = Force_Calc(r);
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
            float force_result = 4 * EPSILON 
                                 * (((float) Math.Exp(((float)Math.Log(Math.Abs(SIGMA  / r))) 
                                 * 12)) - ((float) Math.Exp(((float)Math.Log(Math.Abs(SIGMA  / r))) * 6)));
            return force_result;
        }
    }
}


