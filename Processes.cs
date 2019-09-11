using SME;
using SME.Components;
using System;

namespace Lennard_Jones 
{
/*     [ClockedProcess]
    public class Lj : SimpleProcess
    {
        public Lj(int size)
        {
            this.size = size;
            atoms = new int[size];
        }
        [InputBus]
        public Activate active;
        [InputBus]
        public TrueDualPortMemory<int>.IReadResultA ramresult;
        [OutputBus]
        public TrueDualPortMemory<int>.IControlA ramctrl;
        [OutputBus]
        public Arr output = Scope.CreateBus<Arr>();
        

        int i = 0;
        int size;
        int[] atoms;
        int length = 0;
        bool running = false;

        protected override void OnTick()
        {
            output.valid = false;
            ramctrl.Enabled = false;
            if(active.active){
                i = 0;
                length = active.length; 
                running = true;
            }
            if(running)
            {
                ramctrl.Enabled = i < length;
                ramctrl.Address = i;
                ramctrl.Data = 0;
                ramctrl.IsWriting = false;
                //Console.WriteLine("{0}", i);
                 // Number of clock cycles before we can read 
                 // what we sent two clock cycles ago
                if(i >= 2) {
                    atoms[i - 2] = ramresult.Data;
                    //Console.WriteLine("Ramresult: {0}", ramresult.Data);
                    //Console.WriteLine("Inside loop: {0}", i);
                }
                //Console.WriteLine("{0}", tmp[1]);

                if(i >= length + 1) {
                    output.valid = true;
                    running = false;

                    for(int j = 0; j<size; j++){
                    output.arr[j] = atoms[j] + 2;
                    }
                } else {
                    output.valid = false;
                }
                i++;
                GetAccelerations(atoms, 3, 3);
                //int[] positions = new int[input.num_of_steps * input.num_of_bodies];
                //int[] velocity = new int[input.num_of_bodies]
                //accelerations = 

                // for( int j = 0; i < size: j++){
                //     output.arr[j] =
                // }

            }
        }

        //for i in range(0,3):
        //for j in range((i*3)+i+1, ((i*3)+i+1)+(3-(i+1))):
        //print "y1:", y1[j]

    
        private int GetAccelerations(int[] atoms, int mass, int size)
        {
            //accel_x = new int[size];
            for(int i = 0; i < size - 1; i++)
            {
                Console.WriteLine("i is {0}", i);
                for(int j = i + 1; j < size; j++)
                {
                    int r_x = atoms[j] - atoms[i];
                    //force_scalar = lj_force(r_x, 0.0103, 3.4);
                    //force_x = force_scalar * r_x / r_x //?? 
                    //accel_x[i] += force_x / mass;
                    //accel_x[j] -= foce_x / mass;
                    Console.WriteLine("r_x is {0}", r_x);
                }
            }
            Console.WriteLine("---");

            //int[] distance = {10, 20, 30, 40, 50, 60, 70, 80, 90};
            //for(int i = 0; i < size; i++)
            //{
                //Console.WriteLine("i is {0}", i);
              //  for(int j = (i*size)+i+1; j < ((i*size)+i+1)+(size-(i+1)); j++)
                //{
                  //  r_x = atoms[j] - atoms[i];
                   // force_scalar = lj_force(r_x, 0.0103, 3.4);
                   // force_x = force_scalar * r_x / r_x //?? 
                   // accel_x[]
                    //Console.WriteLine("j is {0}", j); 
                    //Console.WriteLine("distance is {0}", distance[j]);
                //}
            //}
        }

        //private int lj_force(int r_x, epsilon, sigma)
        //{
            
        //}

        

    } */


    // Square root process with floating point
    // Input: An input (uint)
    // Output: An output (uint)
    // sqrt(input) = output
    [ClockedProcess]
    public class Sqrt : SimpleProcess 
    {
        [InputBus]
        public ValBus input;

        [OutputBus]
        public ValBus output = Scope.CreateBus<ValBus>();

        protected override void OnTick()
        {
            if(input.valid){
                float float_input = Funcs.FromUint(input.val);
                float float_output = (float) Math.Sqrt(float_input);
                output.val = Funcs.FromFloat(float_output);
                output.valid = true;
            } else {
                output.valid = false;
            }
        }
    }

    ////// FORCE CALCULATIONS //////


    // Minus process with floating point
    // Input: A minuend and a subtrahend (uint)
    // Output: A difference (uint)
    // Minuend - subtrahend = difference
    [ClockedProcess]
    public class Min : SimpleProcess 
    {
        [InputBus]
        public ValBus minuend;

        [InputBus]
        public ValBus subtrahend;


        [OutputBus]
        public ValBus difference = Scope.CreateBus<ValBus>();

        protected override void OnTick()
        {
            if(minuend.valid && subtrahend.valid){
                float float_minuend = Funcs.FromUint(minuend.val);
                float float_subtrahend = Funcs.FromUint(subtrahend.val);
                float float_difference = float_minuend - float_subtrahend;
                //Console.WriteLine("difference value: {0}", float_difference);
                difference.val = Funcs.FromFloat(float_difference);
                difference.valid = true;
            } else {
                difference.valid = false;
            }
        }
    }

    // Exponentiel process with floating point
    // Input: An value (uint)
    // Output: A result (uint)
    // e^input = result
    [ClockedProcess]
    public class Exp : SimpleProcess 
    {
        [InputBus]
        public ValBus input;

        [OutputBus]
        public ValBus output = Scope.CreateBus<ValBus>();

        protected override void OnTick()
        {
            if(input.valid){
                float float_val = Funcs.FromUint(input.val);
                //Console.WriteLine("exp double value: {0}", Math.Exp((double)float_val));
                float float_result = (float) Math.Exp(float_val);
                //Console.WriteLine("exp value: {0}", float_result);
                output.val = Funcs.FromFloat(float_result);
                output.valid = true;
            } else {
                output.valid = false;
            }
        }
    }

    // Multiplication process with floating point
    // Input: A multiplicant and a multiplier (uint)
    // Output: A product (uint)
    // multiplicant * multiplier = product
    [ClockedProcess]
    public class Mul : SimpleProcess 
    {
        [InputBus]
        public ValBus multiplicant;

        [InputBus]
        public ValBus multiplier;

        [OutputBus]
        public ValBus product = Scope.CreateBus<ValBus>();

        protected override void OnTick()
        {
            if(multiplicant.valid && multiplier.valid){
                //Console.WriteLine("running");
                float float_multiplicant = Funcs.FromUint(multiplicant.val);
                float float_multiplier = Funcs.FromUint(multiplier.val);
                float float_product = float_multiplicant * float_multiplier;
                //Console.WriteLine("product value: {0}", float_product);
                product.val = Funcs.FromFloat(float_product);
                product.valid = true;
            } else {
                product.valid = false;
            }
        }
    }

    // Natural logarithm process with floating point
    // Input: A value (uint)
    // Output: A result (uint)
    // ln(input) = output
    [ClockedProcess]
    public class Ln : SimpleProcess 
    {
        [InputBus]
        public ValBus input;

        [OutputBus]
        public ValBus output = Scope.CreateBus<ValBus>();

        protected override void OnTick()
        {
            if(input.valid){
                //Console.WriteLine("running");
                float float_val = Funcs.FromUint(input.val);
                float float_result = (float) Math.Log(float_val);
                //Console.WriteLine("ln value: {0}", float_result);
                output.val = Funcs.FromFloat(float_result);
                output.valid = true;
            } else {
                output.valid = false;
            }
        }
    }



    // Division process with floating point
    // Input: A divident and a divisor (uint)
    // Output: A quotient (uint)
    // divident / divisor = quotient
    [ClockedProcess]
    public class Div : SimpleProcess 
    {
        [InputBus]
        public ValBus divident;

        [InputBus]
        public ValBus divisor;

        [OutputBus]
        public ValBus quotient = Scope.CreateBus<ValBus>();

        protected override void OnTick()
        {
            /* if(divisor.valid){
                Console.WriteLine("divisor val: {0}", divisor.valid);
            } */
            if(divident.valid && divisor.valid){
                
                float float_divident = Funcs.FromUint(divident.val);
                float float_divisor = Funcs.FromUint(divisor.val);
                float float_quotient = float_divident / float_divisor;
                //Console.WriteLine("quotient value: {0}", float_quotient);
                quotient.val = Funcs.FromFloat(float_quotient);
                quotient.valid = true;
            } else {
                quotient.valid = false;
            }
        }
    }


    [ClockedProcess]
    public class Constants : SimpleProcess
    {
        [OutputBus]
        public ValBus output = Scope.CreateBus<ValBus>();

        public Constants(float elem)
        {
            //Console.WriteLine("elem: {0}", elem);
            this.val = Funcs.FromFloat(elem);
            //Console.WriteLine("val: {0}", val);
        }

        private uint val;


        protected override void OnTick()
        {
            //Console.WriteLine("val in OnTick(): {0}", val);
            output.val = val;
            output.valid = true;
        }
    }


    public class Funcs
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
    }
}