using System;
using SME;
using SME.Components;
using Deflib;

namespace Acceleration {

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
}