using HslCommunication;
using HslCommunication.Profinet.Melsec;
using MyMachinePlatformClientCore.Summer.Common;
using MyMachinePlatformClientCore.Summer.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.PLC
{
    public class MelsecMcNetPLC : PLCBase, IHasOption<TcpOption>, ISuportInitialization
    {
        public override Action<string> LogMessageCallback { get  ; set ; }
        private MelsecMcNet _melsecMcNet;
        private TcpOption tcpOption;
        public TcpOption Option { get => tcpOption; set => tcpOption = value; }     

        public override void Connect()
        {
            if (!base.IsConnected)
            {
                if (_melsecMcNet == null)
                {
                    _melsecMcNet = new MelsecMcNet();
                }

                _melsecMcNet.IpAddress = Option.IpAddress;
                _melsecMcNet.Port = Option.Port;
                OperateResult operateResult = _melsecMcNet.ConnectServer();
                if (!operateResult.IsSuccess)
                {
                    throw new InvalidOperationException($"PLC连接失败:{operateResult.Message}[{operateResult.ErrorCode}]");
                }

                SetClient(_melsecMcNet);
                base.IsConnected = true;
            }
        }
        /// <summary>
        /// 
        /// </summary>
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
