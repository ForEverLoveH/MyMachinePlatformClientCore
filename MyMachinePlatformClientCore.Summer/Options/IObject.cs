using MyMachinePlatformClientCore.Summer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Options
{
    public interface IObject:IRecipeObject
    {
        string Name { get; set; }    
        
        string ID { get; set; }

        string Description { get; set; }
        IPart Owner { get; set; }
        IList<StatusAlarm> StatusAlarms { get; }
    }
    public interface IRecipeObject
    {
        /// <summary>配方</summary>
        IRecipeProvider Recipe { get; }
    }

    public interface IRecipeProvider
    {
        /// <summary>获取指定的配方配置</summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        T Get<T>(string name);
    }

}
