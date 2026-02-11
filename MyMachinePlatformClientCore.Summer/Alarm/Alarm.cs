using MyMachinePlatformClientCore.Summer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer
{
    public class Alarm
    {
        public Alarm(AlarmLevel level, string description)
        {
            this.Level = level;
            this.Description = description;
        }

        /// <summary>是否严重。</summary>
        public AlarmLevel Level { get; }

        /// <summary>是否使能。</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>报警描述。</summary>
        public virtual string Description { get; }

        public override string ToString() => $"[{this.Level}]:{this.Description}";
    }
}
