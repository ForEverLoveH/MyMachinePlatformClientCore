using MyMachinePlatformClientCore.Summer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Events
{
    /// <summary>
    /// 
    /// </summary>
    public class PartStateChangedEventArgs : EventArgs
    {
        public RunState PreviousState { get; }

        /// <summary>当前状态</summary>
        public RunState CurrentState { get; }

        public PartStateChangedEventArgs(RunState previousState, RunState currentState)
        {
            this.PreviousState = previousState;
            this.CurrentState = currentState;
        }
    }
}
