using System.Net;
using MyMachinePlatformClientCore.Common;
using MyMachinePlatformClientCore.Common.Integration;
using MyMachinePlatformClientCore.Common.Options;
using MyMachinePlatformClientCore.Summer.Options;

namespace MyMachinePlatformClientCore.Summer
{

    public class TcpClientMaster : TcpClient, IHasOption<TcpOption>, IDevice
    {

        public TcpClientMaster(string address, int port) : base(address, port)
        {
            this.Option = new TcpOption()
            {
                IpAddress = address,
                Port = port
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="option"></param>
        public TcpClientMaster(TcpOption option) : base(option.IpAddress, option.Port)
        {
            this.Option = option;
        }

        public TcpOption Option { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public event Action<IDevice>? IsConnectedChanged;
        /// <summary>
        /// 
        /// </summary>
        public virtual void Connect()
        {
            if (!IsConnected)
            {
                if (base.Connect())
                {
                    IsConnectedChanged?.Invoke(this);
                }

            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void OnConnected()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        protected override void OnReceived(byte[] buffer, long offset, long size)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        public void Disconnect()
        {
            if (IsConnected)
            {
                if (base.Disconnect())
                {
                    IsConnectedChanged?.Invoke(this);
                }
            }
        }
    }
}