using SME;
using SME.Components;
using System;

namespace Lennard_Jones 
{


 ////// RAM PROCESS //////

    [ClockedProcess]
    public class Ram : SimpleProcess
    {
        [InputBus]
        public TrueDualPortMemory<uint>.IControlA ControlA = Scope.CreateBus<TrueDualPortMemory<uint>.IControlA>();
        [InputBus]
        public TrueDualPortMemory<uint>.IControlB ControlB = Scope.CreateBus<TrueDualPortMemory<uint>.IControlB>();
        [OutputBus]
        public TrueDualPortMemory<uint>.IReadResultA ReadResultA = Scope.CreateBus<TrueDualPortMemory<uint>.IReadResultA>();
        [OutputBus]
        public TrueDualPortMemory<uint>.IReadResultB ReadResultB = Scope.CreateBus<TrueDualPortMemory<uint>.IReadResultB>();

        uint[] mem;

        public Ram(uint size){
            mem = new uint[size];
        }

        protected override void OnTick(){
            if(ControlA.Enabled) {
                ReadResultA.Data = mem[ControlA.Address];
                if(ControlA.IsWriting) {
                    mem[ControlA.Address] = ControlA.Data;
                }
            }
            if(ControlB.Enabled) {
                ReadResultB.Data = mem[ControlB.Address];
                if(ControlB.IsWriting) {
                    mem[ControlB.Address] = ControlB.Data;
                }
            }
            
        }
    }


 ////// RAM MANAGER //////
    [ClockedProcess]
    public class Manager : SimpleProcess
    {
        [InputBus]
        public ValBus input;

        [InputBus]
        public TrueDualPortMemory<uint>.IReadResultA pos1_ramresult;
        
        [InputBus]
        public TrueDualPortMemory<uint>.IReadResultA pos2_ramresult;

        [OutputBus]
        public ValBus pos1_output = Scope.CreateBus<ValBus>();

        [OutputBus]
        public ValBus pos2_output = Scope.CreateBus<ValBus>();
        
        [OutputBus]
        public ValBus acceleration_ready_output = Scope.CreateBus<ValBus>();

        [OutputBus]
        public TrueDualPortMemory<uint>.IControlA pos1_ramctrl;

        [OutputBus]
        public TrueDualPortMemory<uint>.IControlA pos2_ramctrl;


        bool running = false;
        int clock_count = 0;
        int i = 0;
        int j = 1;
        uint length = 0;


        protected override void OnTick() {
            pos1_ramctrl.Enabled = false;
            pos2_ramctrl.Enabled = false;

            // The input is only valid once for each simulation. 
            if(input.valid){
                clock_count = 0;
                i = 0;
                j = 1;
                length = input.val; 
                running = true;
                acceleration_ready_output.val = length;
                acceleration_ready_output.valid = true;
            }else {
                acceleration_ready_output.valid = false;
            }
            if(running){
                pos1_ramctrl.Enabled = i < length;
                pos1_ramctrl.Address = i;
                pos1_ramctrl.Data = 0;
                pos1_ramctrl.IsWriting = false;
                
                pos2_ramctrl.Enabled = j < length;
                pos2_ramctrl.Address = j;
                pos2_ramctrl.Data = 0;
                pos2_ramctrl.IsWriting = false;
                if(clock_count >= 2){
                    pos1_output.val = pos1_ramresult.Data;
                    pos2_output.val = pos2_ramresult.Data;
                    pos1_output.valid = true;
                    pos2_output.valid = true;
                }
                
                if(clock_count >= length + 1) {
                    running = false;
                }
                if(j >= length -1)
                {
                    i++;
                    j = i + 1;
                }else{
                    j++;
                }
                clock_count++;
            }
            
        }

    }

     ////// RAM MANAGER //////
    [ClockedProcess]
    public class AccelerationResultManager : SimpleProcess
    {
        [InputBus]
        public ValBus manager_input;
        [InputBus]
        public ValBus acceleration_input;
        
        [InputBus]
        public TrueDualPortMemory<uint>.IReadResultA acc_ramresult;
        
        [OutputBus]
        public TrueDualPortMemory<uint>.IControlA acc_ramctrl;

        [OutputBus]
        public ValBus output = Scope.CreateBus<ValBus>();

        float[] cache_A;
        float[] cache_B;
        float[] cache_C;
        int cache_size;

        public AccelerationResultManager(uint size){
            cache_A = new float[size];
            cache_B = new float[size];
            cache_C = new float[size];
            cache_size = (int)size;

        }

        // bool write_cache_A = false;
        // bool write_cache_B = false;
        // bool write_cache_C = false;
        // uint index_cache_A = 0;
        // uint index_cache_B = 0;
        // uint index_cache_C = 0;
        // uint dirty_counter = 3;
        int i = 0;
        // int j = 0;
        uint length = 0;

        bool running = false;


        protected override void OnTick() {
            if(manager_input.valid){
                length = manager_input.val;
                output.valid = false;
                running = true;
                // dirty_counter = 3;
                // // clear caches
                // Array.Clear(cache_A, 0, cache_size);
                // Array.Clear(cache_B, 0, cache_size);
                // Array.Clear(cache_C, 0, cache_size);

            }
            // acc_ramctrl.Enabled = false;

            if(acceleration_input.valid && running){
                acc_ramctrl.Enabled = i < length;
                acc_ramctrl.Address = i;
                acc_ramctrl.Data = acceleration_input.val;
                acc_ramctrl.IsWriting = true;
                i++;

            } 
            if(running && i >= length + 2){

                output.valid = true;
                running = false;
            }
            
        }

    }



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
            if(minuend.valid && subtrahend.valid ){
                float float_minuend = Funcs.FromUint(minuend.val);
                float float_subtrahend = Funcs.FromUint(subtrahend.val);
                float float_difference = float_minuend - float_subtrahend;
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
                float float_result = (float) Math.Exp(float_val);
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
                float float_multiplicant = Funcs.FromUint(multiplicant.val);
                float float_multiplier = Funcs.FromUint(multiplier.val);
                float float_product = float_multiplicant * float_multiplier;
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
                float float_val = Funcs.FromUint(input.val);
                float float_result = (float) Math.Log(float_val);
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
            if(divident.valid && divisor.valid){
                float float_divident = Funcs.FromUint(divident.val);
                float float_divisor = Funcs.FromUint(divisor.val);
                float float_quotient = float_divident / float_divisor;
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
            this.val = Funcs.FromFloat(elem);
        }

        private uint val;


        protected override void OnTick()
        {
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