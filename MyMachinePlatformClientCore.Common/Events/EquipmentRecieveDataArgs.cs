using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Common.Events
{
    public class EquipmentRecieveDataArgs:EventArgs 
    {
        public string EquipmentName { get; set; }   
        public string Message { get; set; }
        public EquipmentRecieveDataArgs(string equipmentName, string message)
        {
            this.  EquipmentName = equipmentName;
            this.  Message = message;
        }
    }
}
