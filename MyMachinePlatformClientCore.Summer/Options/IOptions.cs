using HslCommunication.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer
{
    public class IOption
    {
    }
    public class SerialPortOption : IOption
    {
        private string _portName;

        private int _baudRate = 115200;

        private int _dataBits = 8;

        public string PortName
        {
            get
            {
                return _portName;
            }
            set
            {
                if (_portName != value)
                    _portName = value;
            }
        }

        public int BaudRate
        {
            get
            {
                return _baudRate;
            }
            set
            {
                if (_baudRate != value)
                    _baudRate = value;
                
            }
        }

        public int DataBits
        {
            get
            {
                return _dataBits;
            }
            set
            {
                if (_dataBits != value)
                   _dataBits = value;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class TcpOption : IOption
    {
        public string IpAddress { get; set; }
        public int Port { get; set; }
        
    }
    public class OmronCipNetPLCOption : TcpOption
    {
        private byte slot;
        public byte Slot { get=>slot;   set=> slot = value  ; }
    }
    public class OmronFinsNetOption : TcpOption
    {
        private byte _sa1 = 13;

        public  byte SA1
        {
            get=> _sa1;
            set=> _sa1 = value;

        }
        private byte _da2;
        public byte DA2
        {
            get => _da2;
            set => _da2 = value;
        }
        private bool _isStringReverseByteWord;
        public bool IsStringReverseByteWord
        {
            get => _isStringReverseByteWord;
            set => _isStringReverseByteWord = value;
        }
        private DataFormat _dataFormat = DataFormat.CDAB;
        public DataFormat DataFormat
        {
            get => _dataFormat;
            set => _dataFormat = value;
        }
        private bool _UseShortConnection;
        public bool UseShortConnection
        {
            get=> _UseShortConnection;
            set => _UseShortConnection = value;
        }
    }
}
