using HslCommunication;
using  HslCommunication.Profinet.Inovance;
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
    [DisplayName("汇川AMTcp协议")]
    public class InovanceAMTcpPLC : PLCBase, IHasOption<ModbusTcpOption>, ISuportInitialization
    {
        private ModbusTcpOption _option;

        private InovanceAMTcp inovanceAMTcp = new InovanceAMTcp();

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

        public override Action<string> LogMessageCallback { get; set; }

        //
        // 异常:
        //   T:System.InvalidOperationException:
        public override void Connect()
        {
            if (!base.IsConnected)
            {
                inovanceAMTcp.IpAddress = Option.IpAddress;
                inovanceAMTcp.Port = Option.Port;
                inovanceAMTcp.Station = Option.Station;
                inovanceAMTcp.AddressStartWithZero = Option.AddressStartWithZero;
                inovanceAMTcp.IsStringReverse = Option.IsStringReverse;
                inovanceAMTcp.ByteTransform.DataFormat = (HslCommunication.Core.DataFormat)Enum.Parse(typeof(HslCommunication.Core.DataFormat), Option.DataFormat.ToString());
                OperateResult operateResult = inovanceAMTcp.ConnectServer();
                if (!operateResult.IsSuccess)
                {
                    throw new InvalidOperationException($"PLC连接失败:{operateResult.Message}[{operateResult.ErrorCode}]");
                }

                if (Option.UseShortConnection)
                {
                    inovanceAMTcp.ConnectClose();
                }

                SetClient(inovanceAMTcp);
                base.IsConnected = true;
            }
        }

        public override void Disconnect()
        {
            inovanceAMTcp.ConnectClose();
            base.IsConnected = false;
        }

        public virtual async Task InitializeAsync()
        {
            
        }
    }
}
