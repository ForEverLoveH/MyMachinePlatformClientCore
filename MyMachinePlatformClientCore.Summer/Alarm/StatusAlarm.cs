using MyMachinePlatformClientCore.Summer.Enums;
using MyMachinePlatformClientCore.Summer.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer
{
    public sealed class StatusAlarm : Alarm
    {
        public StatusAlarm(
          IObject owner,
          PropertyInfo propertyInfo,
          AlarmLevel level,
          string description,
          System.Predicate<object> predicate)
          : base(level, description)
        {
            this.Owner = owner;
            this.PropertyInfo = propertyInfo;
            this.Predicate = predicate;
        }

        /// <summary>报警监控对象。</summary>
        public IObject Owner { get; }

        /// <summary>报警属性信息。</summary>
        public PropertyInfo PropertyInfo { get; }

        /// <summary>判断逻辑。</summary>
        internal System.Predicate<object> Predicate { get; }

        /// <summary>当前报警状态是否激活</summary>
        public bool IsFired { get; internal set; }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.Owner.GetHashCode() + this.PropertyInfo.Name.GetHashCode();
        }

        public override string ToString()
        {
            if ( string.IsNullOrEmpty(this.Owner.Name))
                return $"[{this.Level}]:{this.Description}";
            return $"[{this.Owner.Name}-{this.Level}]:{this.Description}";
        }
    }
}
