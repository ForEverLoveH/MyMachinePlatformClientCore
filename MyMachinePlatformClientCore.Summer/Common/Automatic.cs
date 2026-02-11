using Microsoft.Extensions.Logging;
using MyMachinePlatformClientCore.Summer.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Common
{
    public class Automatic : IAutomatic, IObject, IRecipeObject
    {
        protected Automatic()
        {
            this.StatusAlarms = (IList<StatusAlarm>)new List<StatusAlarm>();
            this.InitalizeAlarms();
        }
        /// <summary>日志记录器。</summary>
        public ILogger Logger { get; set; }

       
        public IServiceProvider Service { get; set; }

        /// <summary>配方</summary>
        public IRecipeProvider Recipe => this.Owner?.Recipe;

        /// <inheritdoc />
        public string Desc { get; set; }

        /// <summary>抛出报警信息。</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prop"></param>
        /// <param name="value"></param>
        /// <param name="callerName"></param>
        protected void NotifyAlarm<T>(ref T prop, T value, [CallerMemberName] string callerName = null)
        {
            this.NotifyAlarm<T>(ref prop, value, callerName);
        }

        /// <inheritdoc />
        public string Id { get; set; }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public IPart Owner { get; set; }

        /// <inheritdoc />
        public IList<StatusAlarm> StatusAlarms { get; }
        public string ID { get  ; set  ; }
        public string Description { get ; set ; }
    }
}
