 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Common.Options
{
    public interface IObject 
    {
        string Name { get; set; }    
        
        string ID { get; set; }

        string Description { get; set; }
        
    }
    

}
