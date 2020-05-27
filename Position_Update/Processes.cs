using System;
using SME;
using SME.Components;
using Deflib;

namespace Position_Update
{

    [ClockedProcess]
    public class Manager : SimpleProcess
    {

        [InputBus]
        public FlagBus reset;

        [InputBus]
        public FlagBus data_ready;

        [InputBus]
        public ValBus updated_position;

        [InputBus]
        public TrueDualPortMemory<ulong>.IReadResultB velocity_data_point_ramresult;

        [InputBus]
        public TrueDualPortMemory<ulong>.IReadResultA position_ramresult;

        [OutputBus]
        public ValBus prev_position = Scope.CreateBus<ValBus>();
        
        [OutputBus]
        public ValBus velocity_data_point = Scope.CreateBus<ValBus>();
        
        [OutputBus]
        public FlagBus finished = Scope.CreateBus<FlagBus>();
        
        [OutputBus]
        public TrueDualPortMemory<ulong>.IControlB velocity_data_point_ramctrl;

        [OutputBus]
        public TrueDualPortMemory<ulong>.IControlA position_ramctrl;

        [OutputBus]
        public TrueDualPortMemory<ulong>.IControlB updated_position_ramctrl;

        uint data_size;
        double delta_timestep;

        int index = 0;
        int result_index = 0;
        bool running = false;

        public Manager(ulong size, double timestep){
            data_size = (uint)size;
            delta_timestep = timestep;
        }

        protected override void OnTick() {
            // Reset index and result_index before a new simulation loop
            finished.valid = false;
            if(reset.valid){
                index = 0;
                result_index = 0;
                velocity_data_point_ramctrl.Enabled = false;
                position_ramctrl.Enabled = false;
            }
            if(data_ready.valid){
                running = true;
            }
            if(running){
                velocity_data_point_ramctrl.Enabled = index < data_size;
                velocity_data_point_ramctrl.Address = index;
                velocity_data_point_ramctrl.Data = 0;
                velocity_data_point_ramctrl.IsWriting = false;

                position_ramctrl.Enabled = index < data_size;
                position_ramctrl.Address = index;
                position_ramctrl.Data = 0;
                position_ramctrl.IsWriting = false;

                if(index >= 2 && index <= data_size + 1){
                    velocity_data_point.val = velocity_data_point_ramresult.Data;
                    prev_position.val = position_ramresult.Data;
                    velocity_data_point.valid = true;
                    prev_position.valid = true;
                }else{
                    velocity_data_point.valid = false;
                    prev_position.valid = false;
                }
                if(result_index >= data_size){
                    finished.valid = true;
                    running = false;
                }
                if(updated_position.valid){
                    updated_position_ramctrl.Enabled = result_index < data_size;
                    updated_position_ramctrl.Address = result_index;
                    updated_position_ramctrl.Data = updated_position.val;
                    updated_position_ramctrl.IsWriting = true;
                    result_index++;
                }else{
                    updated_position_ramctrl.Enabled = false;
                    updated_position_ramctrl.Address = 0;
                    updated_position_ramctrl.Data = 0;
                    updated_position_ramctrl.IsWriting = false;
                }
                index++;
            }
        }
    }
}