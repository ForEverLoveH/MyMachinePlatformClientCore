
using MyMachinePlatformClientCore.Common.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Common.Integration
{
    public interface IAutomatic : IObject,ISuportInitialization
    {
        /// <summary>说明</summary>
        string Desc { get; set; }
    }
}
