using System;

namespace Deflib
{
    public static class Funcs
    {
        public static ulong FromDouble(double input)
        {
            byte[] tmp = BitConverter.GetBytes(input);
            ulong result = 0;
            for (int i = 0; i < tmp.Length; i++)
            {
                result |= (ulong)((ulong)tmp[i] << (i*8));
                
            }
            return result;
        }

        public static double FromUlong(ulong input)
        {
            byte[] tmp = new byte[8];
            for (int i = 0; i < tmp.Length; i++)
            {
                tmp[i] = (byte)((input >> (i*8)) & 0xFF);
            }
            return BitConverter.ToDouble(tmp);
        }


        public static void Tester()
        {
            double[] positions = new double[] {2,6};
            double[] velocity = new double[2] {0,0};
            
            for (long m = 0; m < 2; m++){
                // Calculating data for verifying results
                double[] accelerations = new double[2];
                for(long i = 0; i < 2; i++){
                    for(long j = i + 1; j < 2; j++){
                        double result = Sim_Funcs.Acceleration_Calc(positions[i], positions[j]);
                        accelerations[i] += result;
                        accelerations[j] += - result;
                    }
                }
                Console.WriteLine("accel i: {0}, accel j {1}", accelerations[0],accelerations[1]);

                // Calculate data for tests
                for( long i = 0; i < 2; i++){
                    // Initial velocity is 0
                    double update_result = Sim_Funcs.Update_Data_Calc(velocity[i], accelerations[i], (double)10.0);
                    velocity[i] = update_result;
                }
                Console.WriteLine("velocity i: {0}, velocity j {1}", velocity[0],velocity[1]);

                double[] updated_positions = new double[2];
                // Calculate data for tests
                for( long i = 0; i < 2; i++){
                    double update_result = Sim_Funcs.Update_Data_Calc(positions[i], velocity[i], (double)10.0);
                    updated_positions[i] = update_result;
                }
                Console.WriteLine("pos i: {0}, pos j {1}", updated_positions[0],updated_positions[1]);
                positions = updated_positions;
                Console.WriteLine(positions[0]);
                Console.WriteLine(positions[1]);
            }

            return;
        }

    }
}