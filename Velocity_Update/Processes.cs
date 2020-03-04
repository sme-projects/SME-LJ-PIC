using System;
using SME;
using SME.Components;
using Deflib;

namespace Velocity_Update
{

    // Velocity manager is fetching data from a specialised acceleration ram.
    [ClockedProcess]
    public class Manager : SimpleProcess
    {

        [InputBus]
        public FlagBus sim_ready;

        [InputBus]
        public FlagBus data_ready;

        [InputBus]
        public ValBus updated_data_point;

        [InputBus]
        public RamResultUint external_data_point_ramresult;
        
        [OutputBus]
        public RamCtrlUint external_data_point_ramctrl;

        [InputBus]
        public TrueDualPortMemory<uint>.IReadResultA data_point_ramresult;

        [OutputBus]
        public ValBus prev_data_point = Scope.CreateBus<ValBus>();
        
        [OutputBus]
        public ValBus external_data_point = Scope.CreateBus<ValBus>();
        
        [OutputBus]
        public FlagBus finished = Scope.CreateBus<FlagBus>();
        
        [OutputBus]
        public TrueDualPortMemory<uint>.IControlA data_point_ramctrl;

        [OutputBus]
        public TrueDualPortMemory<uint>.IControlB updated_data_point_ramctrl;

        uint data_size;
        float delta_timestep;

        uint index = 0;
        int result_index = 0;
        bool running = false;

        public Manager(uint size, float timestep){
            data_size = size;
            delta_timestep = timestep;
        }

        protected override void OnTick() {
            // Reset index and result_index before a new simulation loop
            if(sim_ready.valid){
                index = 0;
                result_index = 0;
                external_data_point_ramctrl.Enabled = false;
                data_point_ramctrl.Enabled = false;
            }
            if(data_ready.valid){
                running = true;
            }
            if(running){
                // TODO: Must read cache size at a time
                external_data_point_ramctrl.Enabled = index < data_size;
                external_data_point_ramctrl.Address = index;
                external_data_point_ramctrl.Data = 0;
                external_data_point_ramctrl.IsWriting = false;

                data_point_ramctrl.Enabled = index < data_size;
                data_point_ramctrl.Address = (int)index;
                data_point_ramctrl.Data = 0;
                data_point_ramctrl.IsWriting = false;

                if(index >= 2 && index <= data_size + 1){
                    external_data_point.val = external_data_point_ramresult.Data;
                    prev_data_point.val = data_point_ramresult.Data;
                    external_data_point.valid = true;
                    prev_data_point.valid = true;
                }else{
                    external_data_point.valid = false;
                    prev_data_point.valid = false;
                }
                if(result_index >= data_size){
                    finished.valid = true;
                    running = false;
                }
                if(updated_data_point.valid){
                    updated_data_point_ramctrl.Enabled = result_index < data_size;
                    updated_data_point_ramctrl.Address = result_index;
                    updated_data_point_ramctrl.Data = updated_data_point.val;
                    updated_data_point_ramctrl.IsWriting = true;
                    result_index++;
                }else{
                    updated_data_point_ramctrl.Enabled = false;
                    updated_data_point_ramctrl.Address = 0;
                    updated_data_point_ramctrl.Data = 0;
                    updated_data_point_ramctrl.IsWriting = false;
                }
                index++;
            }
        }
    }
}