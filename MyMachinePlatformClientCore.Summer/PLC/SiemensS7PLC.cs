using HslCommunication;
using HslCommunication.Profinet.Siemens;
using Microsoft.Extensions.DependencyInjection;
using MyMachinePlatformClientCore.Summer.Common;
using MyMachinePlatformClientCore.Summer.Enums;
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
    public class SiemensS7PLC : PLCBase, IHasOption<SiemensS7NetOption>, ISuportInitialization
    {
        private SiemensS7NetOption _option;

        private SiemensS7Net _siemensS7Client;

        public SiemensS7NetOption Option
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

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public override void Connect()
        {
            if (!base.IsConnected)
            {
                if (!Enum.TryParse(Option.SiemensS7Type.ToString(), out SiemensPLCS siemens))
                {
                    throw new InvalidOperationException($"无法识别的PLC类型: {siemens}");
                }

                if (_siemensS7Client == null)
                {
                    _siemensS7Client = new SiemensS7Net(siemens);
                }

                _siemensS7Client.IpAddress = _option.IpAddress;
                _siemensS7Client.Port = _option.Port;
                OperateResult operateResult = _siemensS7Client.ConnectServer();
                if (!operateResult.IsSuccess)
                {
                    throw new InvalidOperationException($"PLC连接失败:{operateResult.Message}[{operateResult.ErrorCode}]");
                }

                SetClient(_siemensS7Client);
                base.IsConnected = true;
            }
        }

        public override void Disconnect()
        {
            _siemensS7Client?.ConnectClose();
            base.IsConnected = false;
        }

        public virtual async Task InitializeAsync()
        {
            
        }
    }
}
