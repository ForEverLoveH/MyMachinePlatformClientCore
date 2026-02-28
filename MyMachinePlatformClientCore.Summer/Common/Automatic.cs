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
    public class Automatic : IAutomatic, IObject 
    {
        protected Automatic()
        {
            
           
        }
        /// <summary>日志记录器。</summary>
        public ILogger Logger { get; set; }

       
        public IServiceProvider Service { get; set; }

         

        /// <inheritdoc />
        public string Desc { get; set; }

        

        /// <inheritdoc />
        public string Id { get; set; }

        /// <inheritdoc />
        public string Name { get; set; }

       
        public string ID { get  ; set  ; }
        public string Description { get ; set ; }

        public async Task InitializeAsync()
        {
             
        }
    }
}
