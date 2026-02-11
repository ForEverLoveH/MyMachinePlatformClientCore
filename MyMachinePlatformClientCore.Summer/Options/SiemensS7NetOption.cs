using MyMachinePlatformClientCore.Summer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Options
{
    public class SiemensS7NetOption : TcpOption
    {
        private SiemensS7Type _s7Type = SiemensS7Type.S1200;
        public SiemensS7Type SiemensS7Type
        {
            get=> _s7Type;
            set => _s7Type = value;
        }

        private byte _slot;
        private byte Slot
        {
            get => _slot;
            set => _slot = value;
        }

        private byte _rack;
        private byte Rrack
        {
            get => _rack;
            set => _rack = value;
        }
        private bool _useShortConnection = false;
        public bool UseShortConnection
        {
            get=>_useShortConnection;
            set => _useShortConnection = value;
        }
    }
}
