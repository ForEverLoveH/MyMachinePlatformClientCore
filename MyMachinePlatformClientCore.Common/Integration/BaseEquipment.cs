using MyMachinePlatformClientCore.Common.Events;
using MyMachinePlatformClientCore.Common.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Common.Integration
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TOption"></typeparam>
    /// <typeparam name="TArgs"></typeparam>
    public interface IEquipment<TOption,TArgs> :IDevice,IAutomatic,ISuportInitialization,IHasOption<TOption> where TOption: IOption where TArgs : EquipmentRecieveDataArgs
    {

          EventHandler<TArgs> RecieveDataCallBack { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TOption"></typeparam>
    /// <typeparam name="TArgs"></typeparam>
    public abstract class BaseEquipment<TOption, TArgs> : Automatic, IEquipment<TOption, TArgs> where TOption : IOption where TArgs : EquipmentRecieveDataArgs
    {
        public EventHandler<TArgs> RecieveDataCallBack { get ; set  ; }
        public TOption Option { get ; set ; }
    }
}
