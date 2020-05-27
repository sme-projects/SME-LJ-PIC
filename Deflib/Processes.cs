using System;
using SME;
using SME.Components;

namespace Deflib {

    ////// CALCULATIONS //////

    
    // Absolute process with floating point
    // Input: An input (negative or positive) (uint)
    // Output: The absolute value of the input (uint)
    [ClockedProcess]
    public class Abs : SimpleProcess 
    {
        [InputBus]
        public ValBus input;

        [OutputBus]
        public ValBus output = Scope.CreateBus<ValBus>();

        protected override void OnTick()
        {
            if(input.valid){
                float float_input = Funcs.FromUint(input.val);
                float float_output = (float) Math.Abs(float_input);
                output.val = Funcs.FromFloat(float_output);
                output.valid = true;
            } else {
                output.valid = false;
            }
        }
    }

    // Add process with floating point
    // Input: A Augend and a Addend (uint)
    // Output: A sum (uint)
    // Augend + Addend = Sum
    [ClockedProcess]
    public class Add : SimpleProcess 
    {
        [InputBus]
        public ValBus augend;

        [InputBus]
        public ValBus addend;


        [OutputBus]
        public ValBus sum = Scope.CreateBus<ValBus>();

        protected override void OnTick()
        {
            if(augend.valid && addend.valid ){
                float float_augend = Funcs.FromUint(augend.val);
                float float_addend = Funcs.FromUint(addend.val);
                float float_sum = float_augend + float_addend;
                sum.val = Funcs.FromFloat(float_sum);
                sum.valid = true;
            } else {
                sum.valid = false;
            }
        }
    }


    // Minus process with floating point
    // Input: A minuend and a subtrahend (uint)
    // Output: A difference (uint)
    // minuend - subtrahend = difference
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


    // TODO: Handle div by 0!!

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


    // MISCELLANEOUS PROCESS

    [ClockedProcess]
    public class PipelineRegister : SimpleProcess
    {

        [InputBus]
        public ValBus input;

        [OutputBus]
        public ValBus output = Scope.CreateBus<ValBus>();

        protected override void OnTick()
        {
            if(input.valid){
                output.valid = true;
                output.val = input.val;
            }
        }
    }

    public class nPipe
    {
        public PipelineRegister first;
        public PipelineRegister last;

        public nPipe(int n)
        {
            PipelineRegister[] regs = new PipelineRegister[n];
            for (int i = 0; i < n; i++)
                regs[i] = new PipelineRegister();
            for (int i = 1; i < n; i++)
                regs[i].input = regs[i-1].output;
            first = regs[0];
            last = regs[regs.Length-1];
        }
    }

    // A multiplexer which should not be a clocked process 
    public class Multiplexer_ControlA : SimpleProcess
    {
        [InputBus]
        public TrueDualPortMemory<uint>.IControlA first_input = Scope.CreateBus<TrueDualPortMemory<uint>.IControlA>();
        
        [InputBus]
        public TrueDualPortMemory<uint>.IControlA second_input = Scope.CreateBus<TrueDualPortMemory<uint>.IControlA>();

        [OutputBus]
        public TrueDualPortMemory<uint>.IControlA output;


        protected override void OnTick()
        {
            if(first_input.Enabled){
                output.Enabled = true;
                output.Data = first_input.Data;
                output.Address = first_input.Address;
                output.IsWriting = first_input.IsWriting;
            }else if(second_input.Enabled){
                output.Enabled = true;
                output.Data = second_input.Data;
                output.Address = second_input.Address;
                output.IsWriting = second_input.IsWriting;
            }else{
                output.Enabled = false;
                output.Data = 0;
                output.Address = 0;
                output.IsWriting = false;
            }
        }
    }

    // A multiplexer which should not be a clocked process 
    public class Multiplexer_ControlB : SimpleProcess
    {
        [InputBus]
        public TrueDualPortMemory<uint>.IControlB first_input = Scope.CreateBus<TrueDualPortMemory<uint>.IControlB>();
        
        [InputBus]
        public TrueDualPortMemory<uint>.IControlB second_input = Scope.CreateBus<TrueDualPortMemory<uint>.IControlB>();

        [OutputBus]
        public TrueDualPortMemory<uint>.IControlB output;


        protected override void OnTick()
        {
            if(first_input.Enabled){
                output.Enabled = true;
                output.Data = first_input.Data;
                output.Address = first_input.Address;
                output.IsWriting = first_input.IsWriting;
            }else if(second_input.Enabled){
                output.Enabled = true;
                output.Data = second_input.Data;
                output.Address = second_input.Address;
                output.IsWriting = second_input.IsWriting;
            }else{
                output.Enabled = false;
                output.Data = 0;
                output.Address = 0;
                output.IsWriting = false;
            }
        }
    }



    ////// RAM PROCESS //////


    [ClockedProcess]
    public class AccelerationDataRam : SimpleProcess
    {
        public AccelerationDataRam(ulong size)
        {
            mem_size = size;
            mem = new ulong[size];
        }

        [InputBus]
        public RamCtrlArray ControlA = Scope.CreateBus<RamCtrlArray>();

        [InputBus]
        public RamCtrlUlong ControlB = Scope.CreateBus<RamCtrlUlong>(); 

        [OutputBus]
        public RamResultArray ReadResultA = 
            Scope.CreateBus<RamResultArray>();

        [OutputBus]
        public RamResultUlong ReadResultB = Scope.CreateBus<RamResultUlong>();
       
        ulong mem_size;
        ulong[] mem;

        protected override void OnTick()
        {
            if (ControlB.Enabled)
                ReadResultB.Data = ControlB.Address < mem_size ? mem[ControlB.Address]: (ulong)0xFFFFFFFF;
            
            if (ControlA.Enabled)
                for (int i = 0; i < ControlA.Data.Length; i++)
                    if((ulong)((int)ControlA.Address*ControlA.Data.Length + i) < mem_size){
                        ReadResultA.Data[i] = mem[(int)ControlA.Address*ControlA.Data.Length + i];
                    }else{
                        // Data outside of memory
                        ReadResultA.Data[i] = (ulong)0xFFFFFFFF;
                    }
                

            if (ControlB.Enabled && ControlB.IsWriting)
                mem[ControlB.Address] = (ulong)ControlB.Data;
            
            if (ControlA.Enabled && ControlA.IsWriting)
                for (int i = 0; i < ControlA.Data.Length; i++)
                    mem[(int)ControlA.Address*ControlA.Data.Length + i] = (ulong)ControlA.Data[i];
        }
    }
}