using HslCommunication;
using HslCommunication.ModBus;
using MyMachinePlatformClientCore.Summer.Common;
using MyMachinePlatformClientCore.Summer.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.PLC
{
    /// <summary>
    /// 
    /// </summary>
    public class ModbusTcpPLC:PLCBase, IHasOption<ModbusTcpOption>, ISuportInitialization
    {
        private ModbusTcpOption _option;

        private ModbusTcpNet _busTcpClient;

        public ModbusTcpOption Option
        {
            get
            {
                return _option;
            }
            set
            {
                _option = value;
            }
        }

        public override void Connect()
        {
            if (!base.IsConnected)
            {
                if (_busTcpClient == null)
                {
                    _busTcpClient = new ModbusTcpNet();
                }

                _busTcpClient.IpAddress = Option.IpAddress;
                _busTcpClient.Port = Option.Port;
                _busTcpClient.Station = Option.Station;
                _busTcpClient.AddressStartWithZero = Option.AddressStartWithZero;
                _busTcpClient.IsStringReverse = Option.IsStringReverse;
                OperateResult operateResult = _busTcpClient.ConnectServer();
                if (!operateResult.IsSuccess)
                {
                    throw new InvalidOperationException($"PLC连接失败:{operateResult.Message}[{operateResult.ErrorCode}]");
                }

                SetClient(_busTcpClient);
                base.IsConnected = true;
            }
        }

        public override void Disconnect()
        {
            _busTcpClient?.ConnectClose();
            base.IsConnected = false;
        }

        public virtual async Task InitializeAsync()
        {
             
        }
    }
}
