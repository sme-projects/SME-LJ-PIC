using System;

namespace Deflib
{
    public static class Funcs
    {
        public static uint FromFloat(float input)
        {
            byte[] tmp = BitConverter.GetBytes(input);
            uint result = 0;
            for (int i = 0; i < tmp.Length; i++)
            {
                result |= (uint)(tmp[i] << (i*8));
            }
            return result;
        }

        public static float FromUint(uint input)
        {
            byte[] tmp = new byte[4];
            for (int i = 0; i < tmp.Length; i++)
            {
                tmp[i] = (byte)((input >> (i*8)) & 0xFF);
            }
            return BitConverter.ToSingle(tmp);
        }


        public static void Tester()
        {
            float[] positions = new float[] {2,6};
            float[] velocity = new float[2] {0,0};
            
            for (int m = 0; m < 2; m++){
                // Calculating data for verifying results
                float[] accelerations = new float[2];
                for(int i = 0; i < 2; i++){
                    for(int j = i + 1; j < 2; j++){
                        float result = Sim_Funcs.Acceleration_Calc(positions[i], positions[j]);
                        accelerations[i] += result;
                        accelerations[j] += - result;
                    }
                }
                Console.WriteLine("accel i: {0}, accel j {1}", accelerations[0],accelerations[1]);

                // Calculate data for tests
                for( int i = 0; i < 2; i++){
                    // Initial velocity is 0
                    float update_result = Sim_Funcs.Update_Data_Calc(velocity[i], accelerations[i], (float)10.0);
                    velocity[i] = update_result;
                }
                Console.WriteLine("velocity i: {0}, velocity j {1}", velocity[0],velocity[1]);

                float[] updated_positions = new float[2];
                // Calculate data for tests
                for( int i = 0; i < 2; i++){
                    float update_result = Sim_Funcs.Update_Data_Calc(positions[i], velocity[i], (float)10.0);
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