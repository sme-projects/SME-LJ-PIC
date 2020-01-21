using System;
using SME;
using SME.Components;
using Deflib;

namespace Velocity
{

    [ClockedProcess]
    public class Manager : SimpleProcess
    {

        [InputBus]
        public FlagBus sim_ready;

        [InputBus]
        public FlagBus acc_ready;

        [InputBus]
        public ValBus updated_velocity;

        [InputBus]
        public TrueDualPortMemory<uint>.IReadResultA acc_ramresult;

        [InputBus]
        public TrueDualPortMemory<uint>.IReadResultA velocity_ramresult;

        [OutputBus]
        public ValBus prev_velocity = Scope.CreateBus<ValBus>();
        
        [OutputBus]
        public ValBus acceleration = Scope.CreateBus<ValBus>();
        
        [OutputBus]
        public FlagBus finished = Scope.CreateBus<FlagBus>();
        
        [OutputBus]
        public TrueDualPortMemory<uint>.IControlA acc_ramctrl;

        [OutputBus]
        public TrueDualPortMemory<uint>.IControlA velocity_ramctrl;

        [OutputBus]
        public TrueDualPortMemory<uint>.IControlB updated_velocity_ramctrl;

        uint data_size;
        float delta_timestep;

        int index = 0;
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
                acc_ramctrl.Enabled = false;
                velocity_ramctrl.Enabled = false;
            }
            if(acc_ready.valid){
                running = true;
            }
            if(running){
                acc_ramctrl.Enabled = index < data_size;
                acc_ramctrl.Address = index;
                acc_ramctrl.Data = 0;
                acc_ramctrl.IsWriting = false;

                velocity_ramctrl.Enabled = index < data_size;
                velocity_ramctrl.Address = index;
                velocity_ramctrl.Data = 0;
                velocity_ramctrl.IsWriting = false;

                if(index >= 2 && index <= data_size + 1){
                    acceleration.val = acc_ramresult.Data;
                    prev_velocity.val = velocity_ramresult.Data;
                    acceleration.valid = true;
                    prev_velocity.valid = true;
                }else{
                    acceleration.valid = false;
                    prev_velocity.valid = false;
                }
                if(result_index >= data_size){
                    finished.valid = true;
                    running = false;
                }
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
                index++;
            }
        }
    }
}