using HslCommunication;
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
    /// 
    /// </summary>
    public class OmronCipNetPLC : PLCBase, IHasOption<OmronCipNetPLCOption>, ISuportInitialization
    {
        /// <summary>
        /// 
        /// </summary>
        private OmronCipNetPLCOption _option;
        /// <summary>
        /// 
        /// </summary>
        public OmronCipNetPLCOption Option { get =>_option ; set  =>_option=value; }
        /// <summary>
        /// 
        /// </summary>
        private OmronCipNet _omronCipNet= new OmronCipNet();
        /// <summary>
        /// 
        /// </summary>
        public override Action<string> LogMessageCallback { get  ; set ; }
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public override void Connect()
        {
            if (!base.IsConnected)
            {
                if (_omronCipNet == null)
                {
                    _omronCipNet = new OmronCipNet();
                }
                _omronCipNet.IpAddress = Option.IpAddress;
                _omronCipNet.Port = Option.Port;
                _omronCipNet.Slot = Option.Slot;
                OperateResult operateResult = _omronCipNet.ConnectServer();
                if (!operateResult.IsSuccess)
                {
                    throw new InvalidOperationException($"PLC连接失败:{operateResult.Message}[{operateResult.ErrorCode}]");
                }

                SetClient(_omronCipNet);
                base.IsConnected = true;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Disconnect()
        {
            _omronCipNet?.ConnectClose();
            base.IsConnected = false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual async Task InitializeAsync()
        {

        }

    }
}
