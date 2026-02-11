using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Options
{
    public class ModbusTcpOption : TcpOption
    {
        private byte _station;
        public byte Station 
        {  get=>_station; 
            set => _station = value; 
        }
        private bool _addressStartWithZero = true;
        public bool AddressStartWithZero
        {
            get => _addressStartWithZero;
            set => _addressStartWithZero = value;
        }
        private DataFormat _dataFormat;
        public DataFormat DataFormat
        {
            get => _dataFormat;
            set => _dataFormat = value;
        }
        private bool _isStringReverse;
        public bool IsStringReverse
        {
            get=> _isStringReverse;
            set => _isStringReverse = value;
        }
        private bool _UseShortConnection;
        public bool UseShortConnection
        {
            get=> _UseShortConnection;
            set => _UseShortConnection = value;
        }
    }
}
