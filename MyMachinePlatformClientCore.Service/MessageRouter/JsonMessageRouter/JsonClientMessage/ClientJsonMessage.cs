using MyMachinePlatformClientCore.Common.TcpService.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMachinePlatformClientCore.Service.message_router;

namespace MyMachinePlatformClientCore.Service.MessageRouter.JsonMessageRouter
{
    /// <summary>
    /// 
    /// </summary>
    public class ClientJsonMessage
    {
        public   TcpClient tcpClient { get; set; }  

        public  CJsonMessage jsonMessage { get; set; }
    }
   
     
}
