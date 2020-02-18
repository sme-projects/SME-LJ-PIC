using SME;
using System;
using Deflib;


namespace Cache{

         ////// CACHE //////
    [ClockedProcess]
    public class AccelerationCache : SimpleProcess
    {
        [InputBus]
        public ValBus ready;

        [InputBus]
        public ValBus acceleration_input;

        [InputBus]
        public RamResultArray acc_ramresult;

        [OutputBus]
        public RamCtrlArray acc_ramctrl;

        [OutputBus]
        public FlagBus output = Scope.CreateBus<FlagBus>();

        float[] cache_A1;
        float[] cache_A2;
        float[] cache_B;
        float[] cache_C;
        
        uint cache_size;


        public AccelerationCache(uint size){
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
        int dirty_blocks = 0;
        
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
            if(ready.valid){
                
                amount_of_data = ready.val;
                number_of_blocks = amount_of_data / cache_size;
                
                // Output is not ready
                output.valid = false;
                
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
            if(!running){
                output.valid = false;
            }

            if(acceleration_input.valid && running){

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
                        request_cache = false;
                        write_cache = false;
                        // If i moves cache block
                        if(!write_cache_A && last_cache_index_i != current_cache_index_i) {
                            request_cache = true;
                            write_cache = true;
                            next_cache_address = current_cache_index_j + 1;
                            write_cache_A = true;
                            cache_A1_or_A2 = !cache_A1_or_A2;
                        }else if(write_cache_A && last_cache_index_i != current_cache_index_i){
                            Console.WriteLine("Warning - Cache A not written in time");
                        }
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
                    if(current_cache_index_i >= (int)number_of_blocks - 3){
                        request_cache = false;
                        write_cache = false;
                        write_cache_A = false;
                        last_cache_blocks = true;
                        // If i moves block
                        if(current_cache_index_i != last_cache_index_i){
                            //  If i was in an A cache
                            if(last_cache_index_i == index_cache_A1 || last_cache_index_i == index_cache_A2){
                                write_cache = true;
                                write_cache_A = true;
                                write_cache_B = false;
                                write_cache_C = false;
                                // If only two caches it is necessary to change
                                // cache here, so the right cache is written to 
                                // ram
                                if(amount_of_data <= cache_size * 2){
                                    cache_A1_or_A2 = !cache_A1_or_A2;
                                }
                            }else{
                                write_cache = true;
                                write_cache_B = last_cache_index_i == index_cache_B;
                                write_cache_C = last_cache_index_i == index_cache_C;
                                output.valid = true;
                            }
                        }
                    }

                    if(dirty_blocks < number_of_blocks){
                        dirty_blocks++;
                    }
                }
                // TODO: Should be rewritten for better solution
                if(amount_of_data <= cache_size){
                    if(current_j == amount_of_data - 1 && current_i == 0){
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
                    for(int i = 0; i < cache_size; i++)
                        acc_ramctrl.Data[i] = 0;
                    acc_ramctrl.IsWriting = false;

                    received_data_address = next_cache_address;
                    request_cache = false;
                    write_cache = true;
                    receive_cache = true;
                } 
                else if (write_cache){
                    if (write_cache_B){
                        for (int j = 0; j < cache_size; j++)
                            acc_ramctrl.Data[j] = Funcs.FromFloat(cache_B[j]);
                
                        acc_ramctrl.Enabled = true;
                        acc_ramctrl.Address = index_cache_B;
                        acc_ramctrl.IsWriting = true;
                        
                        if(next_cache_address > dirty_blocks){
                            index_cache_B = next_cache_address;
                        }
                    }
                    else if (write_cache_C){
                        for (int j = 0; j < cache_size; j++)
                            acc_ramctrl.Data[j] = Funcs.FromFloat(cache_C[j]);
                
                        acc_ramctrl.Enabled = true;
                        acc_ramctrl.Address = index_cache_C;
                        acc_ramctrl.IsWriting = true;

                        if(next_cache_address > dirty_blocks){
                            index_cache_C = next_cache_address;
                        }
                    }else if (write_cache_A){
                        if(cache_A1_or_A2){
                            for (int j = 0; j < cache_size; j++)
                                acc_ramctrl.Data[j] = Funcs.FromFloat(cache_A2[j]);
                    
                            acc_ramctrl.Enabled = true;
                            acc_ramctrl.Address = index_cache_A2;
                            acc_ramctrl.IsWriting = true;
                        }else{
                            for (int j = 0; j < cache_size; j++)
                                acc_ramctrl.Data[j] = Funcs.FromFloat(cache_A1[j]);
                    
                            acc_ramctrl.Enabled = true;
                            acc_ramctrl.Address = index_cache_A1;
                            acc_ramctrl.IsWriting = true;
                        }
                        output.valid = true;
                    }
                    write_cache = false;
                } else if(receive_cache){
                    if(write_cache_B){
                        for (int j = 0; j < cache_size; j++){
                            if(copy_cache_A && !cache_A1_or_A2){
                                cache_A1[j] = Funcs.FromUint(acc_ramresult.Data[j]);
                                index_cache_A1 = received_data_address;
                                if(received_data_address < number_of_blocks - 3)
                                    index_cache_B = received_data_address;
                            }else if (copy_cache_A && cache_A1_or_A2){
                                cache_A2[j] = Funcs.FromUint(acc_ramresult.Data[j]);
                                index_cache_A2 = received_data_address;
                                if(received_data_address < number_of_blocks - 3)
                                    index_cache_B = received_data_address;
                            }else{
                                cache_B[j] = Funcs.FromUint(acc_ramresult.Data[j]);
                                index_cache_B = received_data_address;
                            }
                        }
                        copy_cache_A = false;
                        write_cache_B = false;
                    }else if(write_cache_C){
                        for (int j = 0; j < cache_size; j++){
                            if(copy_cache_A && !cache_A1_or_A2){
                                cache_A1[j] = Funcs.FromUint(acc_ramresult.Data[j]);
                                index_cache_A1 = received_data_address;
                                if(received_data_address < number_of_blocks - 3)
                                    index_cache_C = received_data_address;
                            }else if (copy_cache_A && cache_A1_or_A2){
                                cache_A2[j] = Funcs.FromUint(acc_ramresult.Data[j]);
                                index_cache_A2 = received_data_address;
                                if(received_data_address < number_of_blocks - 3)
                                    index_cache_C = received_data_address;
                            }else{
                                cache_C[j] = Funcs.FromUint(acc_ramresult.Data[j]);
                                index_cache_C = received_data_address;
                            }
                        }
                        copy_cache_A = false;
                        write_cache_C = false;
                    } else if (write_cache_A){
                        for (int j = 0; j < cache_size; j++){
                            if(!cache_A1_or_A2){
                                //  Written cache A1
                                if(index_cache_B == index_cache_A2){
                                    cache_B[j] = Funcs.FromUint(acc_ramresult.Data[j]);
                                } else if(index_cache_C == index_cache_A2){
                                    cache_C[j] = Funcs.FromUint(acc_ramresult.Data[j]);
                                }
                            }else{
                                // Written cache A2
                                if(index_cache_B == index_cache_A1){
                                    cache_B[j] = Funcs.FromUint(acc_ramresult.Data[j]);
                                } else if(index_cache_C == index_cache_A1){
                                    cache_C[j] = Funcs.FromUint(acc_ramresult.Data[j]);
                                }
                            }
                        }
                        // Update cache index
                        if(!cache_A1_or_A2){
                            if(index_cache_B == index_cache_A2)
                                index_cache_B = received_data_address;
                            else if(index_cache_C == index_cache_A2)
                                index_cache_C = received_data_address;
                        }else{
                            if(index_cache_B == index_cache_A1)
                                index_cache_B = received_data_address;
                            else if(index_cache_C == index_cache_A1)
                                index_cache_C = received_data_address;
                        }
                        write_cache_A = false;
                    }
                    receive_cache = false;
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
                        cache_A1[current_index_j] = cache_A1[current_index_j] - Funcs.FromUint(acceleration_input.val);
                    }
                    else if (current_cache_index_j == index_cache_A2){
                        cache_A2[current_index_j] = cache_A2[current_index_j] - Funcs.FromUint(acceleration_input.val);
                    } 
                    else if (current_cache_index_j == index_cache_B){
                        cache_B[current_index_j] = cache_B[current_index_j] - Funcs.FromUint(acceleration_input.val);
                    }
                    else if (current_cache_index_j == index_cache_C){
                        cache_C[current_index_j] = cache_C[current_index_j] - Funcs.FromUint(acceleration_input.val);
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
                        cache_A1[current_index_i] = cache_A1[current_index_i] + Funcs.FromUint(acceleration_input.val);
                    }
                    else if (current_cache_index_i == index_cache_A2){
                        cache_A2[current_index_i] = cache_A2[current_index_i] + Funcs.FromUint(acceleration_input.val);
                    } 
                    else if (current_cache_index_i == index_cache_B){
                        cache_B[current_index_i] = cache_B[current_index_i] + Funcs.FromUint(acceleration_input.val);
                    }
                    else if (current_cache_index_i == index_cache_C){
                        cache_C[current_index_i] = cache_C[current_index_i] + Funcs.FromUint(acceleration_input.val);
                    }
                }
                else{
                    if(cache_A1_or_A2){
                        cache_A1[current_index_i] = cache_A1[current_index_i] + Funcs.FromUint(acceleration_input.val);
                    }else{
                        cache_A2[current_index_i] = cache_A2[current_index_i] + Funcs.FromUint(acceleration_input.val);
                    }
                }
            }
            else if (running){
                // When at the last elements of the data, it is necessary to 
                // write to RAM since the loop will not continue after the last
                // elements
                if (current_i == amount_of_data - 2){
                    // Console.WriteLine("Finished acceleration accumulation");
                    
                    write_cache_B = current_cache_index_i == index_cache_B;
                    write_cache_C = current_cache_index_i == index_cache_C;
                    write_cache_A = current_cache_index_i == index_cache_A1 || current_cache_index_i == index_cache_A2;
                    output.valid = true;
                    if (write_cache_B){
                        for (int j = 0; j < cache_size; j++)
                            acc_ramctrl.Data[j] = Funcs.FromFloat(cache_B[j]);
                
                        acc_ramctrl.Enabled = true;
                        acc_ramctrl.Address = index_cache_B;
                        acc_ramctrl.IsWriting = true;
                    }
                    else if (write_cache_C){
                        for (int j = 0; j < cache_size; j++)
                            acc_ramctrl.Data[j] = Funcs.FromFloat(cache_C[j]);
                
                        acc_ramctrl.Enabled = true;
                        acc_ramctrl.Address = index_cache_C;
                        acc_ramctrl.IsWriting = true;

                    }
                    else if (write_cache_A && cache_A1_or_A2){
                        for (int j = 0; j < cache_size; j++)
                            acc_ramctrl.Data[j] = Funcs.FromFloat(cache_A1[j]);
                
                        acc_ramctrl.Enabled = true;
                        acc_ramctrl.Address = index_cache_A1;
                        acc_ramctrl.IsWriting = true;
                    }
                    else if (write_cache_A && !cache_A1_or_A2){
                        for (int j = 0; j < cache_size; j++)
                            acc_ramctrl.Data[j] = Funcs.FromFloat(cache_A2[j]);
                
                        acc_ramctrl.Enabled = true;
                        acc_ramctrl.Address = index_cache_A2;
                        acc_ramctrl.IsWriting = true;
                    }
                    write_cache = false;
                }
                running = false;
            }
            
            // Console.WriteLine("Clock cycle count is: {0}", count);
            count++;
        }
    }
}