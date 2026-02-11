using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Options
{
    public interface IHasOption<T> where T : IOption
    {
        T Option { get; set; }
    }
}
