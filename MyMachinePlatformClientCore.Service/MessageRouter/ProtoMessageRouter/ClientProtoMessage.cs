using Google.Protobuf;
using MyMachinePlatformClientCore.Common.TcpService.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Service.MessageRouter.ProtoMessage
{
    public class ClientProtoMessage
    {
        public IMessage message { get; set; }

        public TcpClient tcpClient { get; set; }
    }
}
