using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Options
{
    public class ModbusRtuOption : SerialPortOption
    {
        private byte _station = 1;
        public byte Station { get=> _station; set => _station = value; }
        private bool _AddressStartWithZero = false;
        public bool AddressStartWithZero { get=>_AddressStartWithZero;  set => _AddressStartWithZero = value; }  
    }
}
