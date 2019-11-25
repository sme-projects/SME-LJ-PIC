using SME;
using SME.Components;
using System;

namespace Lennard_Jones 
{



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
                pos1_ramctrl.Enabled = i < length - 1;
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
                if(i >= length +1  && j >= length +2) {
                    running = false;
                    pos1_output.valid = false;
                    pos2_output.valid = false;
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
        public RamResultArray acc_ramresult;

        [OutputBus]
        public RamCtrlArray acc_ramctrl;
        [OutputBus]
        public ValBus output = Scope.CreateBus<ValBus>();

        float[] cache_A1;
        float[] cache_A2;
        float[] cache_B;
        float[] cache_C;
        
        uint cache_size;


        public AccelerationResultManager(uint size){
            cache_A1 = new float[size];
            cache_A2 = new float[size];
            cache_B = new float[size];
            cache_C = new float[size];
            
            cache_size = size;

        }
        bool request_cache = false;
        bool receive_cache = false;
        bool write_cache = false;

        bool write_cache_A = false;
        bool write_cache_B = false;
        bool write_cache_C = false;
        uint index_cache_A1 = 0;
        uint index_cache_A2 = 0;
        uint index_cache_B = 1;
        uint index_cache_C = 2;
        uint amount_of_data = 0;

        bool cache_A1_or_A2 = false;
        bool copy_cache_A = false;

        bool running = false;
        uint current_i = 0;
        uint current_j = 0;
        
        // counting the index of blocks we have read from RAM
        uint dirty_blocks = 0;
        
        // uint current_cache_index = 0;
        uint number_of_blocks = 0;
        uint next_cache_address = 0;
        

        bool initial_clock_cycle = false;

        uint current_cache_index_i;
        uint current_cache_index_j;
        uint current_index_i;
        uint current_index_j;

        uint last_cache_index_i;
        uint last_cache_index_j;

        uint received_data_address;
        bool last_cache_blocks;



        uint count = 0;

        

        protected override void OnTick() {
            if(manager_input.valid){
                
                amount_of_data = manager_input.val;
                number_of_blocks = amount_of_data / cache_size;
                // Output not ready to read
                output.valid = false;
                
                // Put in initial clock cycle loop
                // running = true;
                
                // The maximum index of the four caches
                // current_cache_index = 3;
                // clear caches
                // clear_cache_A1 = true;
                // clear_cache_A2 = true;
                // clear_cache_B = true;
                // clear_cache_C = true;

                initial_clock_cycle = true;
                

                index_cache_A1 = 0;
                index_cache_A2 = 0;
                index_cache_B = 1;
                index_cache_C = 2;
                cache_A1_or_A2 = true; // A1 == true, A2 == false
                copy_cache_A = false;

                current_i = 0;
                current_j = 0;

                current_cache_index_i = 0;
                current_cache_index_j = 0;
                current_index_i = 0;
                current_index_j = 1;
                last_cache_blocks = false;

                last_cache_index_i = 0;
                last_cache_index_j = 0;
                count = 0;
                
                /*  NOTE: The Acceleration manager will never write to more than 
                    there are amount of data. Therefore it is only a problem when 
                    we want to read a new cacheline from RAM since it will try to
                    read from the elements that does not exists, because it will
                    always read cache_size number of items. So the AM does not 
                    need to handle any cases where the data does not fit in the
                    cache lines. Instead the RAM should be changed to handle any
                    read that wants to read at an unknown address. This should 
                    fix our problem. 
                */
               
            }
            if(initial_clock_cycle){
                running = true;
            }

            if(acceleration_input.valid && running){

                if(count == 114){

                }
                count++;
                
                
                output.valid = false;
                
                // Find current i and j
                if (current_j == amount_of_data - 1){
                //    j is at the end of data and i is moved one forward 
                    current_i++;
                    current_j = current_i + 1;
                }else{
                    current_j++;
                }
                if (current_i == current_j){
                    Console.WriteLine("Something wrong - i is equal to j");
                    running = false;
                }

                // last clock cycle result
                last_cache_index_i = current_cache_index_i;
                last_cache_index_j = current_cache_index_j;
                uint last_index_i = current_index_i;
                uint last_index_j = current_index_j;

                // Calculate current cacheblock
                current_cache_index_i = current_i / cache_size;
                current_cache_index_j = current_j / cache_size;
                // Calculate which index within the cache block
                current_index_i = current_i % cache_size;
                current_index_j = current_j % cache_size;

                // if j changes cache block
                if (last_cache_index_j != current_cache_index_j || 
                    last_cache_index_i != current_cache_index_i){
                    // If j was not in cache A
                    if (last_cache_index_j != current_cache_index_i){
                        write_cache = true;
                        write_cache_B = last_cache_index_j == index_cache_B;
                        write_cache_C = last_cache_index_j == index_cache_C;
                    }
                    
                    // j moves to last block
                    if(current_cache_index_j == number_of_blocks -1){
                        // i moves to a new block next time it moves
                        if(current_index_i == cache_size - 1){
                            copy_cache_A = true;
                        }
                        // i does not move a block
                        else{
                            copy_cache_A = false;
                        }
                        if(cache_A1_or_A2){
                            next_cache_address = index_cache_A1 + 1;
                        }else{
                            next_cache_address = index_cache_A2 + 1;
                        }
                        request_cache = true;
                    }
                    // j wraps around to i
                    else if (current_cache_index_j == current_cache_index_i){
                        // If i moves cache block
                        if(!write_cache_A && last_cache_index_i != current_cache_index_i) {
                            write_cache_A = true;
                            cache_A1_or_A2 = !cache_A1_or_A2;
                        }else if(write_cache_A && last_cache_index_i != current_cache_index_i){
                            Console.WriteLine("Warning - Cache A not written in time");
                        }
                        request_cache = false;
                        write_cache = false;
                    }
                    // j does not move to last block
                    else{
                        next_cache_address = current_cache_index_j + 1;
                        if(next_cache_address <= dirty_blocks){
                            request_cache = true;
                        }
                    }
                    // At the last 3 cache blocks before the end of data
                    // This we already have in cache and therefore we do not
                    // need to write or request the data. 
                    if(current_cache_index_j >= number_of_blocks - 2 && 
                    current_cache_index_i >= number_of_blocks - 3){
                        request_cache = false;
                        write_cache = false;
                        write_cache_A = false;
                        last_cache_blocks = true;
                        // If i moves block
                        if(current_cache_index_i != last_cache_index_i){
                            //  If i was in an A cache
                            if(last_cache_index_i == index_cache_A1 || last_cache_index_i == index_cache_A2){
                                write_cache_A = true;
                            }else{
                                write_cache = true;
                                write_cache_B = last_cache_index_i == index_cache_B;
                                write_cache_C = last_cache_index_i == index_cache_C;
                                output.valid = true;
                                output.val = (uint)acc_ramctrl.Data.Length;
                            }
                        }
                    }

                    if(dirty_blocks < number_of_blocks){
                        dirty_blocks++;
                    }
                }



                /*  NOTE: A request cache will never happen without a write cache
                    since we never want to request something if we havent written
                    anything yet. Also everytime we request something, we have to
                    replace it with something already in the cache, and therefore
                    we must write this to RAM before we can override the data. 
                */
                if(request_cache){
                    acc_ramctrl.Enabled = true;
                    acc_ramctrl.Address = next_cache_address;
                    for(int i = 0; i < acc_ramctrl.Data.Length; i++)
                        acc_ramctrl.Data[i] = 0;
                    acc_ramctrl.IsWriting = false;

                    received_data_address = next_cache_address;
                    request_cache = false;
                    write_cache = true;
                    receive_cache = true;
                } 
                else if (write_cache){
                    if (write_cache_B){
                        for (int j = 0; j < acc_ramctrl.Data.Length; j++)
                            acc_ramctrl.Data[j] = Funcs.FromFloat(cache_B[j]);
                
                        acc_ramctrl.Enabled = true;
                        acc_ramctrl.Address = index_cache_B;
                        acc_ramctrl.IsWriting = true;
                        
                        if(next_cache_address > dirty_blocks){
                            index_cache_B = next_cache_address;
                        }
                    }
                    else if (write_cache_C){
                        for (int j = 0; j < acc_ramctrl.Data.Length; j++)
                            acc_ramctrl.Data[j] = Funcs.FromFloat(cache_C[j]);
                
                        acc_ramctrl.Enabled = true;
                        acc_ramctrl.Address = index_cache_C;
                        acc_ramctrl.IsWriting = true;

                        if(next_cache_address > dirty_blocks){
                            index_cache_C = next_cache_address;
                        }
                    }
                    write_cache = false;
                } else if(receive_cache){
                    if(write_cache_B){
                        for (int j = 0; j < acc_ramresult.Data.Length; j++){
                            if(copy_cache_A && !cache_A1_or_A2){
                                cache_A1[j] = Funcs.FromUint(acc_ramresult.Data[j]);
                                index_cache_A1 = received_data_address;
                            }else if (copy_cache_A && cache_A1_or_A2){
                                cache_A2[j] = Funcs.FromUint(acc_ramresult.Data[j]);
                                index_cache_A2 = received_data_address;
                            }else{
                                cache_B[j] = Funcs.FromUint(acc_ramresult.Data[j]);
                                index_cache_B = received_data_address;
                            }
                        }
                        copy_cache_A = false;
                        write_cache_B = false;
                    }else if(write_cache_C){
                        for (int j = 0; j < acc_ramresult.Data.Length; j++){
                            if(copy_cache_A && !cache_A1_or_A2){
                                cache_A1[j] = Funcs.FromUint(acc_ramresult.Data[j]);
                                index_cache_A1 = received_data_address;
                            }else if (copy_cache_A && cache_A1_or_A2){
                                cache_A2[j] = Funcs.FromUint(acc_ramresult.Data[j]);
                                index_cache_A2 = received_data_address;
                            }else{
                                cache_C[j] = Funcs.FromUint(acc_ramresult.Data[j]);
                                index_cache_C = received_data_address;
                            }
                        }
                        copy_cache_A = false;
                        write_cache_C = false;
                    }
                    receive_cache = false;
                } else if (write_cache_A){
                    if(cache_A1_or_A2){
                        for (int j = 0; j < acc_ramctrl.Data.Length; j++)
                            acc_ramctrl.Data[j] = Funcs.FromFloat(cache_A2[j]);
                
                        acc_ramctrl.Enabled = true;
                        acc_ramctrl.Address = index_cache_A2;
                        acc_ramctrl.IsWriting = true;
                    }else{
                        for (int j = 0; j < acc_ramctrl.Data.Length; j++)
                            acc_ramctrl.Data[j] = Funcs.FromFloat(cache_A1[j]);
                
                        acc_ramctrl.Enabled = true;
                        acc_ramctrl.Address = index_cache_A1;
                        acc_ramctrl.IsWriting = true;
                    }
                    output.valid = true;
                    output.val = (uint)acc_ramctrl.Data.Length;
                    write_cache_A = false;
                }

                // Writing j to cache block
                if(current_cache_index_j >= dirty_blocks){
                    if (current_cache_index_j == index_cache_A1){
                        cache_A1[current_index_j] = - Funcs.FromUint(acceleration_input.val);
                    }
                    else if(current_cache_index_j == index_cache_A2){
                        cache_A2[current_index_j] = - Funcs.FromUint(acceleration_input.val);
                    }
                    else if (current_cache_index_j == index_cache_B){
                        cache_B[current_index_j] = - Funcs.FromUint(acceleration_input.val);
                    }
                    else if (current_cache_index_j == index_cache_C){
                        cache_C[current_index_j] = - Funcs.FromUint(acceleration_input.val);
                    } 
                }else{
                    // If A, B or C cache is dirty and have been written too
                    // it is necessary to accumulate the results.
                    if (current_cache_index_j == index_cache_A1){
                        cache_A1[current_index_j] += - Funcs.FromUint(acceleration_input.val);
                    }
                    else if (current_cache_index_j == index_cache_A2){
                        cache_A2[current_index_j] += - Funcs.FromUint(acceleration_input.val);
                    } 
                    else if (current_cache_index_j == index_cache_B){
                        cache_B[current_index_j] += - Funcs.FromUint(acceleration_input.val);
                    }
                    else if (current_cache_index_j == index_cache_C){
                        cache_C[current_index_j] += - Funcs.FromUint(acceleration_input.val);
                    }
                }
                // i will always write in a field that j have already written to
                // unless it is the first initial cycle with input data and so
                // nothing has been written yet. 
                // Therefore at the initial clock cycle we simply write to the cache
                // and thereafter we always accumulate the data 
                if(initial_clock_cycle){
                    cache_A1[current_index_i] = Funcs.FromUint(acceleration_input.val);
                    initial_clock_cycle = false;
                } else if (last_cache_blocks){
                    if (current_cache_index_i == index_cache_A1){
                        cache_A1[current_index_i] += Funcs.FromUint(acceleration_input.val);
                    }
                    else if (current_cache_index_i == index_cache_A2){
                        cache_A2[current_index_i] += Funcs.FromUint(acceleration_input.val);
                    } 
                    else if (current_cache_index_i == index_cache_B){
                        cache_B[current_index_i] += Funcs.FromUint(acceleration_input.val);
                    }
                    else if (current_cache_index_i == index_cache_C){
                        cache_C[current_index_i] += Funcs.FromUint(acceleration_input.val);
                    }
                }
                else{
                    if(cache_A1_or_A2){
                        cache_A1[current_index_i] += Funcs.FromUint(acceleration_input.val);
                    }else{
                        cache_A2[current_index_i] += Funcs.FromUint(acceleration_input.val);
                    }
                }
            }else if (running){
                // When at the last elements of the data, it is necessary to 
                // write to RAM since the loop will not continue after the last
                // elements and therefore the data will not be written unless
                // it is done here
                if (current_i == amount_of_data - 2){
                    Console.WriteLine("Finished acceleration accumulation");
                    
                    write_cache_B = current_cache_index_i == index_cache_B;
                    write_cache_C = current_cache_index_i == index_cache_C;
                    output.valid = true;
                    output.val = (uint)acc_ramctrl.Data.Length;
                    if (write_cache_B){
                        for (int j = 0; j < acc_ramctrl.Data.Length; j++)
                            acc_ramctrl.Data[j] = Funcs.FromFloat(cache_B[j]);
                
                        acc_ramctrl.Enabled = true;
                        acc_ramctrl.Address = index_cache_B;
                        acc_ramctrl.IsWriting = true;
                    }
                    else if (write_cache_C){
                        for (int j = 0; j < acc_ramctrl.Data.Length; j++)
                            acc_ramctrl.Data[j] = Funcs.FromFloat(cache_C[j]);
                
                        acc_ramctrl.Enabled = true;
                        acc_ramctrl.Address = index_cache_C;
                        acc_ramctrl.IsWriting = true;

                    }
                    write_cache = false;
                }
                running = false;
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



    ////// Test process //////
    [ClockedProcess]
    public class Test : SimpleProcess
    {
        [OutputBus]
        public ValBus output = Scope.CreateBus<ValBus>();

        float count;

        public Test(){
            count = 0.0f;
        }

        protected override void OnTick()
        {
            
            output.val = Funcs.FromFloat(count);
            output.valid = true;
            count++;
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