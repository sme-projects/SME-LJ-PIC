using System;
using SME;
using SME.Components;

namespace Deflib {

    ////// CALCULATIONS //////


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

    // Square root process with floating point
    // Input: An input (uint)
    // Output: An output (uint)
    // sqrt(input) = output
    // [ClockedProcess]
    // public class Sqrt : SimpleProcess 
    // {
    //     [InputBus]
    //     public ValBus input;

    //     [OutputBus]
    //     public ValBus output = Scope.CreateBus<ValBus>();

    //     protected override void OnTick()
    //     {
    //         if(input.valid){
    //             float float_input = Funcs.FromUint(input.val);
    //             float float_output = (float) Math.Sqrt(float_input);
    //             output.val = Funcs.FromFloat(float_output);
    //             output.valid = true;
    //         } else {
    //             output.valid = false;
    //         }
    //     }
    // }


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

    [ClockedProcess]
    public class AccelerationDataRam : SimpleProcess
    {
        public AccelerationDataRam(uint size)
        {
            mem_size = size;
            mem = new uint[size];
        }

        [InputBus]
        public RamCtrlArray ControlA = Scope.CreateBus<RamCtrlArray>();

        [InputBus]
        public RamCtrlUint ControlB = Scope.CreateBus<RamCtrlUint>(); 

        [OutputBus]
        public RamResultArray ReadResultA = 
            Scope.CreateBus<RamResultArray>();

        [OutputBus]
        public RamResultUint ReadResultB = Scope.CreateBus<RamResultUint>();
       
        uint mem_size;
        uint[] mem;

        protected override void OnTick()
        {
            if (ControlB.Enabled)
                ReadResultB.Data = ControlB.Address < mem_size ? mem[ControlB.Address]: (uint)0xFFFFFFFF;
            
            if (ControlA.Enabled)
                for (int i = 0; i < ControlA.Data.Length; i++)
                    if((ControlA.Address*ControlA.Data.Length + i) < mem_size){
                        ReadResultA.Data[i] = mem[ControlA.Address*ControlA.Data.Length + i];
                    }else{
                        // Data outside of memory
                        ReadResultA.Data[i] = (uint)0xFFFFFFFF;
                    }
                

            if (ControlB.Enabled && ControlB.IsWriting)
                mem[ControlB.Address] = ControlB.Data;
            
            if (ControlA.Enabled && ControlA.IsWriting)
                for (int i = 0; i < ControlA.Data.Length; i++)
                    mem[ControlA.Address*ControlA.Data.Length + i] = ControlA.Data[i];
        }
    }
}