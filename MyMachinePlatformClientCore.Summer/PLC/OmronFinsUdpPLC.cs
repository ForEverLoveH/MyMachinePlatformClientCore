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
    public class OmronFinsUdpPLC : PLCBase, IHasOption<OmronFinsNetOption>, ISuportInitialization
    {
        private OmronFinsNetOption _option;

        private OmronFinsUdp omronFinsUdp = new OmronFinsUdp();

        public OmronFinsNetOption Option
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
                omronFinsUdp.IpAddress = Option.IpAddress;
                omronFinsUdp.Port = Option.Port;
                omronFinsUdp.SA1 = Option.SA1;
                omronFinsUdp.DA2 = Option.DA2;
                omronFinsUdp.ByteTransform.IsStringReverseByteWord = Option.IsStringReverseByteWord;
                omronFinsUdp.ByteTransform.DataFormat = (HslCommunication.Core.DataFormat)Enum.Parse(typeof(HslCommunication.Core.DataFormat), Option.DataFormat.ToString());
                SetClient(omronFinsUdp);
                base.IsConnected = true;
            }
        }

        public override void Disconnect()
        {
            base.IsConnected = false;
        }

        public virtual async Task InitializeAsync()
        {
             
        }
    }
}
