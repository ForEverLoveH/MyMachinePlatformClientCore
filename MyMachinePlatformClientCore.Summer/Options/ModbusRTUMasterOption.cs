using MyMachinePlatformClientCore.Common.Options;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Options
{
    public class ModbusRTUMasterOption : SerialPortOption
    {
        public Parity Parity { get; set; }
        public StopBits StopBits { get; set; }
        public int WriteTimeout { get; set; }
        public int ReadTimeout { get; set; }
    }
}
