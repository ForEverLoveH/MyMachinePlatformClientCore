using log4net.Core;
using MyMachinePlatformClientCore.Summer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.CustomAttribute
{
    /// <summary>表示可引发报警的属性。</summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public sealed class AlarmAttribute : Attribute
    {
        public AlarmAttribute(object value, AlarmLevel level, string description = null, Logical logical = Logical.Equal)
        {
            this.Value = value;
            this.Level = level;
            this.Description = description;
            this.Logical = logical;
        }

        /// <summary>参考值。</summary>
        public object Value { get; set; }

        /// <summary>是否严重。</summary>
        public AlarmLevel Level { get; set; }

        /// <summary>报警说明。</summary>
        public string Description { get; set; }

        /// <summary>判断逻辑。</summary>
        public Logical Logical { get; }
    }
}
