using System;
using SME;
using SME.Components;

namespace Deflib {

    ////// CALCULATIONS //////

    
    // Absolute process with floating point
    // Input: An input (negative or positive) (ulong)
    // Output: The absolute value of the input (ulong)
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
                double double_input = Funcs.FromUlong(input.val);
                double double_output = (double) Math.Abs(double_input);
                output.val = Funcs.FromDouble(double_output);
                output.valid = true;
            } else {
                output.valid = false;
            }
        }
    }

    // Add process with floating point
    // Input: A Augend and a Addend (ulong)
    // Output: A sum (ulong)
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
                double double_augend = Funcs.FromUlong(augend.val);
                double double_addend = Funcs.FromUlong(addend.val);
                double double_sum = double_augend + double_addend;
                sum.val = Funcs.FromDouble(double_sum);
                sum.valid = true;
            } else {
                sum.valid = false;
            }
        }
    }


    // Minus process with floating point
    // Input: A minuend and a subtrahend (ulong)
    // Output: A difference (ulong)
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
                double double_minuend = Funcs.FromUlong(minuend.val);
                double double_subtrahend = Funcs.FromUlong(subtrahend.val);
                double double_difference = double_minuend - double_subtrahend;
                difference.val = Funcs.FromDouble(double_difference);
                difference.valid = true;
            } else {
                difference.valid = false;
            }
        }
    }

    // Exponentiel process with floating point
    // Input: An value (ulong)
    // Output: A result (ulong)
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
                double double_val = Funcs.FromUlong(input.val);
                double double_result = (double) Math.Exp(double_val);
                output.val = Funcs.FromDouble(double_result);
                output.valid = true;
            } else {
                output.valid = false;
            }
        }
    }

    // Multiplication process with floating point
    // Input: A multiplicant and a multiplier (ulong)
    // Output: A product (ulong)
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
                double double_multiplicant = Funcs.FromUlong(multiplicant.val);
                double double_multiplier = Funcs.FromUlong(multiplier.val);
                double double_product = double_multiplicant * double_multiplier;
                product.val = Funcs.FromDouble(double_product);
                product.valid = true;
            } else {
                product.valid = false;
            }
        }
    }

    // Natural logarithm process with floating point
    // Input: A value (ulong)
    // Output: A result (ulong)
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
                double double_val = Funcs.FromUlong(input.val);
                double double_result = (double) Math.Log(double_val);
                output.val = Funcs.FromDouble(double_result);
                output.valid = true;
            } else {
                output.valid = false;
            }
        }
    }


    // TODO: Handle div by 0!!

    // Division process with floating point
    // Input: A divident and a divisor (ulong)
    // Output: A quotient (ulong)
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
                double double_divident = Funcs.FromUlong(divident.val);
                double double_divisor = Funcs.FromUlong(divisor.val);
                double double_quotient = double_divident / double_divisor;
                quotient.val = Funcs.FromDouble(double_quotient);
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

        public Constants(double elem)
        {
            this.val = Funcs.FromDouble(elem);
        }

        private ulong val;


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

        public nPipe(long n)
        {
            PipelineRegister[] regs = new PipelineRegister[n];
            for (long i = 0; i < n; i++)
                regs[i] = new PipelineRegister();
            for (long i = 1; i < n; i++)
                regs[i].input = regs[i-1].output;
            first = regs[0];
            last = regs[regs.Length-1];
        }
    }

    // A multiplexer which should not be a clocked process 
    public class Multiplexer_ControlA : SimpleProcess
    {
        [InputBus]
        public TrueDualPortMemory<ulong>.IControlA first_input = Scope.CreateBus<TrueDualPortMemory<ulong>.IControlA>();
        
        [InputBus]
        public TrueDualPortMemory<ulong>.IControlA second_input = Scope.CreateBus<TrueDualPortMemory<ulong>.IControlA>();

        [OutputBus]
        public TrueDualPortMemory<ulong>.IControlA output;


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
        public TrueDualPortMemory<ulong>.IControlB first_input = Scope.CreateBus<TrueDualPortMemory<ulong>.IControlB>();
        
        [InputBus]
        public TrueDualPortMemory<ulong>.IControlB second_input = Scope.CreateBus<TrueDualPortMemory<ulong>.IControlB>();

        [OutputBus]
        public TrueDualPortMemory<ulong>.IControlB output;


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