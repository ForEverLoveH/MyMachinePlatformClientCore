
using MyMachinePlatformClientCore.Common.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore .Common.Integration
{
    public class Automatic:Device,IAutomatic,IObject
    {
        protected Automatic()
        {
            
           
        }
        

       
        

         

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
