using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Common
{
    public interface IDevice
    {

        bool IsConnected { get; }

        event Action<IDevice> IsConnectedChanged;
 
        void Connect();

        void Disconnect();
    }
}
