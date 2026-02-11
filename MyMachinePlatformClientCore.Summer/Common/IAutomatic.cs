using MyMachinePlatformClientCore.Summer.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Common
{
    public interface IAutomatic : IObject, IRecipeObject
    {
        /// <summary>说明</summary>
        string Desc { get; set; }
    }
}
