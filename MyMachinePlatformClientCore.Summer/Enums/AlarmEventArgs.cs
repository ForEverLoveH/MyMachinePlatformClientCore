using MyMachinePlatformClientCore.Summer.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Enums
{
    /// <summary>报警事件参数。</summary>
    public class AlarmEventArgs : EventArgs
    {
        /// <summary>报警对象。</summary>
        public IObject Owner { get; set; }

        /// <summary>报警。</summary>
        public Alarm Alarm { get; set; }

        /// <summary>是否激活。</summary>
        public bool IsFired { get; set; }
    }
}
