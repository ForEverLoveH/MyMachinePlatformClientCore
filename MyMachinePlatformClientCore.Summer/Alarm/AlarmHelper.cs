using log4net.Core;
using MyMachinePlatformClientCore.Summer.Common;
using MyMachinePlatformClientCore.Summer.Enums;
using MyMachinePlatformClientCore.Summer.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MyMachinePlatformClientCore.Summer;
using MyMachinePlatformClientCore.Summer.CustomAttribute;

namespace MyMachinePlatformClientCore.Summer 
{
    public static class AlarmHelper
    {
        public static void InitalizeAlarms(this IObject obj)
        {
            foreach (PropertyInfo runtimeProperty in obj.GetType().GetRuntimeProperties())
            {
                AlarmAttribute[] array = runtimeProperty.GetCustomAttributes<AlarmAttribute>(true).ToArray<AlarmAttribute>();
                if (((IEnumerable<AlarmAttribute>)array).Any<AlarmAttribute>())
                {
                    foreach (AlarmAttribute alarmAttribute in array)
                    {
                        AlarmAttribute attribute = alarmAttribute;
                        Predicate<object> predicate;
                        switch (attribute.Logical)
                        {
                            case Logical.Equal:
                                predicate = (Predicate<object>)(value => value.Equals(attribute.Value));
                                break;
                            case Logical.Not:
                                predicate = (Predicate<object>)(value => !value.Equals(attribute.Value));
                                break;
                            case Logical.And:
                                predicate = (Predicate<object>)(value =>
                                {
                                    ulong uint64 = Convert.ToUInt64(attribute.Value);
                                    return ((long)Convert.ToUInt64(value) & (long)uint64) == (long)uint64;
                                });
                                break;
                            case Logical.Xor:
                                predicate = (Predicate<object>)(value =>
                                {
                                    ulong uint64 = Convert.ToUInt64(attribute.Value);
                                    return ((long)Convert.ToUInt64(value) ^ (long)uint64) == (long)uint64;
                                });
                                break;
                            default:
                                predicate = (Predicate<object>)(o => false);
                                break;
                        }
                        if (!obj.StatusAlarms.Any<StatusAlarm>((Func<StatusAlarm, bool>)(x => x.Description == attribute.Description && x.Level == attribute.Level)))
                        {
                            StatusAlarm statusAlarm = new StatusAlarm(obj, runtimeProperty, attribute.Level, attribute.Description, predicate);
                            obj.StatusAlarms.Add(statusAlarm);
                        }
                    }
                }
            }
        }

        public static void NotifyAlarm<T>(this IObject obj, ref T prop, T value, [CallerMemberName] string callerName = null)
        {
            if (EqualityComparer<T>.Default.Equals(prop, value))
                return;
            prop = value;
            foreach (StatusAlarm statusAlarm in obj.StatusAlarms.Where<StatusAlarm>((Func<StatusAlarm, bool>)(o => o.PropertyInfo.Name == callerName && o.IsEnabled)))
            {
                if (statusAlarm != null)
                {
                    statusAlarm.IsFired = statusAlarm.Predicate((object)prop);
                    if (!(obj is IPart part))
                        part = obj.Owner;
                    AlarmEventArgs e = new AlarmEventArgs()
                    {
                        Owner = obj,
                        Alarm = (Alarm)statusAlarm,
                        IsFired = statusAlarm.IsFired
                    };
                    part?.OnAlarmChanged(e);
                }
            }
        }
    }
}
