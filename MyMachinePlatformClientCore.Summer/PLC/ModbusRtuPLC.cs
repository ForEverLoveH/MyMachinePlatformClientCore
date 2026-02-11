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
    public class ModbusRtuPLC :PLCBase, IHasOption<ModbusRtuOption>, ISuportInitialization
    {
        private ModbusRtuOption _option;

        private ModbusRtu _busRtuClient;

        public ModbusRtuOption Option
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

        public override Action<string> LogMessageCallback { get  ; set ; }

        public override void Connect()
        {
            if (!base.IsConnected)
            {
                if (_busRtuClient == null)
                {
                    _busRtuClient = new ModbusRtu();
                }

                _busRtuClient.Station = Option.Station;
                _busRtuClient.AddressStartWithZero = Option.AddressStartWithZero;
                _busRtuClient.SerialPortInni(Option.PortName, Option.BaudRate);
                _busRtuClient.Close();
                if (_busRtuClient.IsOpen())
                {
                    throw new InvalidOperationException("PLC连接失败");
                }

                SetClient(_busRtuClient);
                base.IsConnected = true;
            }
        }

        public override void Disconnect()
        {
            _busRtuClient?.Close();
            base.IsConnected = false;
        }

        public virtual async Task InitializeAsync()
        {
            
        }
    }
}
