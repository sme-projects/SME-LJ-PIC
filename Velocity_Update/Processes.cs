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
        public ValBus updated_velocity;

        [InputBus]
        public RamResultUint acceleration_data_point_ramresult;
        
        [OutputBus]
        public RamCtrlUint acceleration_data_point_ramctrl;

        [InputBus]
        public TrueDualPortMemory<uint>.IReadResultB velocity_ramresult;

        [OutputBus]
        public ValBus prev_velocity = Scope.CreateBus<ValBus>();
        
        [OutputBus]
        public ValBus acceleration_data_point = Scope.CreateBus<ValBus>();
        
        [OutputBus]
        public FlagBus finished = Scope.CreateBus<FlagBus>();
        
        [OutputBus]
        public TrueDualPortMemory<uint>.IControlB velocity_ramctrl;

        [OutputBus]
        public TrueDualPortMemory<uint>.IControlA updated_velocity_ramctrl;

        uint data_size;
        float delta_timestep;

        uint index = 0;
        uint ram_return_index = 0;
        int result_index = 0;
        bool running = false;
        uint ready_to_read = 0;

        public Manager(uint size, float timestep){
            data_size = size;
            delta_timestep = timestep;
        }

        protected override void OnTick() {
            // Reset index and result_index before a new simulation loop
            if(sim_ready.valid){
                index = 0;
                result_index = 0;
                acceleration_data_point_ramctrl.Enabled = false;
                velocity_ramctrl.Enabled = false;
                finished.valid = false;
            }
            if(data_ready.valid){
                running = true;
                ready_to_read += (uint)Cache_size.n;
            }
            if(running){
                acceleration_data_point_ramctrl.Enabled = index < data_size;
                acceleration_data_point_ramctrl.Data = 0;
                acceleration_data_point_ramctrl.IsWriting = false;          

                velocity_ramctrl.Enabled = index < data_size;
                velocity_ramctrl.Data = 0;
                velocity_ramctrl.IsWriting = false;


                // Get data from ram and send it to velocity calculation
                if(index - ram_return_index >= 2 || index >= data_size){
                    acceleration_data_point.val = acceleration_data_point_ramresult.Data;
                    prev_velocity.val = velocity_ramresult.Data;
                    acceleration_data_point.valid = true;
                    prev_velocity.valid = true;
                    ram_return_index++;
                }else{
                    acceleration_data_point.valid = false;
                    prev_velocity.valid = false;
                }

                // Update velocity and acceleration ram addresses
                if(index < ready_to_read){
                    acceleration_data_point_ramctrl.Address = index;
                    velocity_ramctrl.Address = (int)index;
                    index++;
                } else {
                    // to avoid read and write to same address
                    velocity_ramctrl.Enabled = false;
                }


                if(result_index >= data_size){
                    finished.valid = true;
                    running = false;
                }
                // Write updated velocity to velocity ram
                if(updated_velocity.valid){
                    updated_velocity_ramctrl.Enabled = result_index < data_size;
                    updated_velocity_ramctrl.Address = result_index;
                    updated_velocity_ramctrl.Data = updated_velocity.val;
                    updated_velocity_ramctrl.IsWriting = true;
                    result_index++;
                }else{
                    updated_velocity_ramctrl.Enabled = false;
                    updated_velocity_ramctrl.Address = 0;
                    updated_velocity_ramctrl.Data = 0;
                    updated_velocity_ramctrl.IsWriting = false;
                }
            }
        }
    }
}