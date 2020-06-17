using System;
using System.Collections.Generic;
using SME;
using SME.Components;
using Deflib;

using System.Linq;

namespace Acceleration{


    public class Testing_Simulation : SimulationProcess
    {
        [InputBus]
        public ValBus acceleration_ready_signal;
        
        [InputBus]
        public ValBus testing_result_input;

        [OutputBus]
        public TrueDualPortMemory<ulong>.IControlA position_ramctrl;

        [OutputBus]
        public ValBus ready_signal = Scope.CreateBus<ValBus>();

        // Bus only in use when testing Acceleration module seperately 
        [OutputBus]
        public FlagBus mag_sim_finished = Scope.CreateBus<FlagBus>();

        public Testing_Simulation(ulong data_size){
            this.data_size = data_size;
        }

        private ulong data_size;

        public override async System.Threading.Tasks.Task Run()
        {
            await ClockAsync();

            Random rnd = new Random();
            // Testing data
            // Positions array is kept double so that the ulong bitstream can be
            // generated correctly
            double[] positions = new double[data_size];
            for(ulong k = 0; k < data_size; k++){
            //     long random_number = rnd.Next(10,1000);
            //     double double_rnd_number = random_number;
            //     if(!positions.Contains((double) random_number))
            //         positions[k] = (double) random_number;
            //     else
            //         k--;
                // Non-random data:
                positions[k] = (double) k + 1;
            }
            
            // Write data to ram
            for(long k = 0; k < positions.Length; k++){
                position_ramctrl.Address = (int)k;
                position_ramctrl.Data = Funcs.FromDouble(positions[k]);
                position_ramctrl.IsWriting = true;
                position_ramctrl.Enabled = true;
                await ClockAsync();
            }
            bool running = true;
            position_ramctrl.Enabled = false;
            ready_signal.val = data_size;
            ready_signal.valid = true;
            await ClockAsync();
            ready_signal.valid = false;

            ulong i = 0;
            ulong j = 1;
            Queue<double> calculated_result_queue = new Queue<double>();
            while(running){
                mag_sim_finished.valid = false;

                if(testing_result_input.valid){
                    // TODO: Rename the control result to something with "control result"
                    double calculated_result = Sim_Funcs.Acceleration_Calc(positions[i], positions[j]);
                    calculated_result_queue.Enqueue(calculated_result);
                    if(i <= (uint)data_size -2 && j <= (uint)data_size -1){
                        double calc_result = calculated_result_queue.Dequeue();
                        double input_result = Funcs.FromUlong(testing_result_input.val);

                        // TODO: Figure out what the expected and accepted difference can be
                        if(Math.Abs(calc_result - input_result) > 1/(Math.Pow(10,7))){
                            Console.WriteLine("pos {0}: {1}, pos {2}: {3}", i, positions[i], j, positions[j]);
                            Console.WriteLine("Acceleration test sim - Got {0}, Expected {1}", input_result, calc_result);
                        }
                        if(i >= data_size - 2){
                            running = false;
                            Console.WriteLine("Acceleration test successfully completed");
                        }
                        if(j >= data_size -1 ){
                            i++;
                            j = i + 1;
                        }else{
                            j++;
                        }
                    }
                }
                await ClockAsync();
            }
            mag_sim_finished.valid = true;
        }
    }

    public class Testing_Magnitude : SimulationProcess
    {
        [InputBus]
        public ValBus x_coord;

        [InputBus]
        public FlagBus finished;

        [OutputBus]
        public ValBus output = Scope.CreateBus<ValBus>();



        public Testing_Magnitude(ulong data_size){
            this.data_size = data_size;
        }

        private ulong data_size;

        public override async System.Threading.Tasks.Task Run(){

            // await ClockAsync();
            bool running = true;
            int depth_size = (int)Deflib.Magnitude_depth.n;
            double[] val_queue = new double[depth_size - 1];
            bool[] valid_queue = new bool[depth_size - 1];

            int last_index = depth_size - 2;

            while(running){
                output.valid = valid_queue[last_index]; 
                output.val = Funcs.FromDouble(val_queue[last_index]);
                for (int i = last_index; i > 0; i--){
                    val_queue[i] = val_queue[i-1];
                    valid_queue[i] = valid_queue[i-1];
                }
                double val = Funcs.FromUlong(x_coord.val);
                val = Math.Abs(val);
                val_queue[0] = val;
                valid_queue[0] = x_coord.valid;
                if(finished.valid)
                    running = false;
                await ClockAsync();
            }
        }
    }
}