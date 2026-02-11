using HslCommunication;
using HslCommunication.Core;
using HslCommunication.Profinet.Omron;
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
    /// 欧姆龙fins PLC
    /// </summary>
    public class OmronFinsNetPLC : PLCBase, IHasOption<OmronFinsNetOption>, ISuportInitialization
    {
        public override Action<string> LogMessageCallback { get  ; set  ; }
        /// <summary>
        /// 
        /// </summary>
        private OmronFinsNetOption option;
        public OmronFinsNetOption Option { get =>option; set => option = value; } 
        /// <summary>
        /// 
        /// </summary>
        private OmronFinsNet omronFinsNet = new OmronFinsNet();
        public override void Connect()
        {
            if (this.IsConnected)
                return;
            this.omronFinsNet.IpAddress = this.Option.IpAddress;
            this.omronFinsNet.Port = (int)this.Option.Port;
            this.omronFinsNet.SA1 = this.Option.SA1;
            this.omronFinsNet.DA2 = this.Option.DA2;
            this.omronFinsNet.ByteTransform.IsStringReverseByteWord = this.Option.IsStringReverseByteWord;
            this.omronFinsNet.ByteTransform.DataFormat = (HslCommunication.Core.DataFormat)Enum.Parse(typeof(HslCommunication.Core.DataFormat), this.Option.DataFormat.ToString());
            OperateResult operateResult = this.omronFinsNet.ConnectServer();
            if (!operateResult.IsSuccess)
                throw new InvalidOperationException($"PLC连接失败:{operateResult.Message}[{operateResult.ErrorCode}]");
            if (this.Option.UseShortConnection)
                this.omronFinsNet.ConnectClose();
            this.SetClient((IReadWriteNet)this.omronFinsNet);
            this.IsConnected = true;
        }

        public override void Disconnect()
        {
            this.omronFinsNet.ConnectClose();
            this.IsConnected = false;
        }

        public virtual  async Task InitializeAsync()
        {
             
        }
    }
}
