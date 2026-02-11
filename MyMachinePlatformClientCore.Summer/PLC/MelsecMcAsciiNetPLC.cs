using HslCommunication;
using HslCommunication.Profinet.Melsec;
using Microsoft.Extensions.DependencyInjection;
using MyMachinePlatformClientCore.Summer.Common;
using MyMachinePlatformClientCore.Summer.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.PLC 
{ 
 
    [DisplayName("三菱PLC-ASCII")]
    public class MelsecMcAsciiNetPLC : PLCBase, IHasOption<TcpOption>, ISuportInitialization
    {
        private TcpOption _option;

        private MelsecMcAsciiNet _melsecMcNet;

        public TcpOption Option
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
                if (_melsecMcNet == null)
                {
                    _melsecMcNet = new MelsecMcAsciiNet();
                }

                _melsecMcNet.IpAddress = Option.IpAddress;
                _melsecMcNet.Port = Option.Port;
                _melsecMcNet.ConnectClose();
                OperateResult operateResult = _melsecMcNet.ConnectServer();
                if (!operateResult.IsSuccess)
                {
                    throw new InvalidOperationException($"PLC连接失败:{operateResult.Message}[{operateResult.ErrorCode}]");
                }

                SetClient(_melsecMcNet);
                base.IsConnected = true;
            }
        }

        public override void Disconnect()
        {
            _melsecMcNet?.ConnectClose();
            base.IsConnected = false;
        }

        public virtual async Task InitializeAsync()
        {
           
        }
    }
}
