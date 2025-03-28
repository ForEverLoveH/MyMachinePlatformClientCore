using MyMachinePlatformClientCore.Common.TcpService.Server;
using MyMachinePlatformClientCore.Service.message_router;

namespace MyMachinePlatformClientCore.Service.MessageRouter.JsonMessageRouter.JsonServerMessage;

public class ServerJsonMessage
{ 
    public   CJsonMessage jsonMessage { get; set; }
    
    public TcpSession   tcpSession{ get; set; }
}