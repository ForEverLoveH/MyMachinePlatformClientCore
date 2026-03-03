using HslCommunication;
using HslCommunication.Profinet.Keyence;
using MyMachinePlatformClientCore.Common.Integration;
using MyMachinePlatformClientCore.Common.Options;
 
using MyMachinePlatformClientCore.Summer.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.PLC
{
    public class KeyenceMcAsciiNetPLC : PLCBase, IHasOption<TcpOption>, ISuportInitialization
    {
        private KeyenceMcNet _keyenceMcAsciiNet;
        public KeyenceMcAsciiNetPLC(TcpOption Option)
        {
            this.Option = Option;
        }
        private TcpOption _option;
        public TcpOption Option { get=>_option  ; set => _option = value; }

        public override void Connect()
        {
            if (!base.IsConnected)
            {
                if (_keyenceMcAsciiNet == null)
                {
                    _keyenceMcAsciiNet = new HslCommunication.Profinet.Keyence.KeyenceMcNet();
                    // _keyenceMcAsciiNet.ByteTransform.DataFormat = HslCommunication.Core.DataFormat.BADC;
                    _keyenceMcAsciiNet.ByteTransform.IsStringReverseByteWord = true;
                }

                _keyenceMcAsciiNet.IpAddress = Option.IpAddress;
                _keyenceMcAsciiNet.Port = Option.Port;
                _keyenceMcAsciiNet.ConnectClose();
                OperateResult operateResult = _keyenceMcAsciiNet.ConnectServer();
                if (!operateResult.IsSuccess)
                {
                    throw new InvalidOperationException($"PLC连接失败:{operateResult.Message}[{operateResult.ErrorCode}]");
                }

                SetClient(_keyenceMcAsciiNet);
                base.IsConnected = true;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Disconnect()
        {
            _keyenceMcAsciiNet?.ConnectClose();
            base.IsConnected = false;
        }
    }
}
