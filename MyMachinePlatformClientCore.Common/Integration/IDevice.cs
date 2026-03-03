using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Common.Integration
{
    public interface IDevice
    {

        bool IsConnected { get; }

        event Action<IDevice> IsConnectedChanged;
 
        void Connect();

        void Disconnect();
    }
    public class Device : IDevice
    {
        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                IsConnectedChanged?.Invoke(this);
            }
        }

        public event Action<IDevice> IsConnectedChanged;

        public virtual void Connect()
        {
             
        }

        public virtual void Disconnect()
        {
             
        }
    }
}
