 
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using MQTTnet;
 
using MyMachinePlatformClientCore.IService.IMqttService;
using MyMachinePlatformClientCore.Log.MyLogs;
using MyMachinePlatformClientCore.Service.message_router;

namespace MyMachinePlatformClientCore.Service.MQTTService;
/// <summary>
/// 
/// </summary>
public class MqttClientService:IMqttClientService
{
    /// <summary>
    /// 
    /// </summary>
    private string clientID;
    /// <summary>
    /// 
    /// </summary>
    private string userName;
    /// <summary>
    /// 
    /// </summary>
    private string password;
    /// <summary>
    /// 
    /// </summary>
    private string serverIP;
    /// <summary>
    /// 
    /// </summary>
    private int port;
    /// <summary>
    /// 
    /// </summary>
    private int maxReconnectCount;
    /// <summary>
    /// 
    /// </summary>
    private int currentReconnectCout;
    /// <summary>
    /// 
    /// </summary>
    private IMqttClient _mqttClient;
    /// <summary>
    /// 
    /// </summary>
    private Action<string> RecieveMessageCallBack;
    /// <summary>
    /// 
    /// </summary>
    private Action<LogMessage> LogMessageCallBack;
    /// <summary>
    /// 
    /// </summary>
    private MqttClientOptionsBuilder _optionsBuilder;
    /// <summary>
    /// 客户端雨服务端的SSL证书
    /// </summary>

    private X509Certificate2 _certificate;
    /// <summary>
    /// 
    /// </summary>

    private string topicName;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="topicName"></param>

    public void SetTopicName(string topicName)
    {
        this.topicName = topicName;
    }
    
    private bool isConnected;
    /// <summary>
    /// 
    /// </summary>
    public bool IsConnected
    {
        get { return isConnected; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <param name="serverIp"></param>
    /// <param name="port"></param>
    /// <param name="maxReconnectCount"></param>
    /// <param name="topicName"></param>
    /// <param name="_certificate">证书</param>
    /// <param name="_recieveMessageCallBack"></param>
    /// <param name="_logMessageCallBack"></param>
    // 修改参数命名，将可为 null 的引用类型参数标记为可空类型
    public MqttClientService(string clientId, string userName, string password, string serverIp, int port, int maxReconnectCount, string topicName, X509Certificate2 certificate = null,
        Action<string> recieveMessageCallBack = null, Action<LogMessage> logMessageCallBack = null)
    {
        this.clientID = clientId;
        this.userName = userName;
        this.password = password;
        this.serverIP = serverIp;
        this.port = port;
        this.topicName = topicName;
        this._certificate = certificate;
        this.maxReconnectCount = Math.Min(maxReconnectCount, 5);
        this.RecieveMessageCallBack = recieveMessageCallBack;
        this.LogMessageCallBack = logMessageCallBack;
        _optionsBuilder = new MqttClientOptionsBuilder().WithClientId(clientId).WithTcpServer(serverIP, port)
            .WithCredentials(userName, password).WithTimeout(new TimeSpan(0, 0, 1000));

        if (certificate != null)
        {
            _optionsBuilder.WithTlsOptions(new MqttClientTlsOptions()
            {
                UseTls = true, // 启用 TLS
                AllowUntrustedCertificates = false, // 不允许不信任的证书
                IgnoreCertificateChainErrors = false, // 不忽略证书链错误
                SslProtocol = SslProtocols.Tls12, // 使用 TLS 1.2 协议
                ClientCertificatesProvider = new DefaultMqttCertificatesProvider(new X509Certificate2Collection(certificate)),
                CertificateValidationHandler =e=>VerficationCurrentServerCertificate(e),
                
            });
        }
        _mqttClient = new MqttClientFactory().CreateMqttClient();
        _mqttClient.ConnectedAsync += MqttClient_ConnectedAsync;
        _mqttClient.DisconnectedAsync += MqttClient_DisconnectedAsync;
        _mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;
        
    }
    /// <summary>
    /// 验证服务端sll 证书
    /// </summary>
    /// <param name="certificate"></param>
    /// <returns></returns>
    private bool VerficationCurrentServerCertificate(MqttClientCertificateValidationEventArgs certificate)
    {
        //证书
        var serverCertificate = certificate.Certificate;
        
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    private void InitService()
    {
         _mqttClient=new MqttClientFactory().CreateMqttClient();
         
         
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="arg"></param>
    private async Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        string message = "MQTT 客户端已连接";
        LogMessageCallBack?.Invoke( new LogMessage()
        {
            _LogType = LogType.Info,
            message = message,
        });
        // 订阅主题
        var subscribeResult = await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(this.topicName).Build());
        var po = subscribeResult.Items.FirstOrDefault();
        if (po != null)
        {
            var resCode=po.ResultCode;
            if (resCode == MqttClientSubscribeResultCode.GrantedQoS0)
            {
                message = "订阅主题：" + this.topicName + " 成功  ";
                LogMessageCallBack?.Invoke( new LogMessage()
                {
                    message = message,
                    _LogType = LogType.Success,
                } );
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="arg"></param>
    private async Task MqttClient_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
    {
        string message = "MQTT 客户端已断开连接";
        LogMessageCallBack?.Invoke(new LogMessage()
        {
            message = message,
            _LogType = LogType.Warm,
        });
        if (currentReconnectCout < maxReconnectCount)
        {
            currentReconnectCout++;
            message = $"尝试第 {currentReconnectCout} 次重连...";
            LogMessageCallBack?.Invoke(new LogMessage()
            {
                message = message,
                _LogType = LogType.Info,
            });
            await Task.Delay(2000); // 等待 2 秒后尝试重连
            await StartService();
        }
        else
        {
            LogMessageCallBack?.Invoke(   new LogMessage()
            {
                message = "达到最大重连次数，停止重连",
                _LogType = LogType.Warm,
            });
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="arg"></param>
    private async Task MqttClient_ApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
         string topic = arg.ApplicationMessage.Topic;
         string message = System.Text.Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
         RecieveMessageCallBack?.Invoke(message);
         LogMessageCallBack?.Invoke(  new LogMessage()
         {
             _LogType = LogType.Info,
             message = $"收到消息：主题 {topic}，内容 {message}"
         });
    }

    /// <summary>
    /// 
    /// </summary>
    public  async  Task<bool>  StartService()
    {
         var options = _optionsBuilder.Build();
          var  result  =await _mqttClient.ConnectAsync(options);
          if (result.ResultCode == MqttClientConnectResultCode.Success)
          {
              isConnected = true;
              string message=$"服务端:{serverIP}_{port} 连接成功";
              LogMessageCallBack?.Invoke(new LogMessage()
              {
                 message = message,
                 _LogType = LogType.Success
              });
              return true;  
          }return false;
         
    }
    /// <summary>
    /// 
    /// </summary>
    public  async Task StopService()
    {
        if (_mqttClient.IsConnected)
        {
            await _mqttClient.DisconnectAsync();
            LogMessageCallBack?.Invoke(new LogMessage()
            {
                message = "MQTT 客户端已停止",
                _LogType = LogType.Warm
            });
            if(IsConnected)isConnected = false;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public async   Task SendMessage(string message,string topicName = "")
    {
        if (_mqttClient.IsConnected)
        {
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(string.IsNullOrEmpty(topicName) ? this.topicName : topicName)
                .WithPayload(message)
                .Build();

            var publishResult = await _mqttClient.PublishAsync(applicationMessage);
            if (publishResult.ReasonCode == MqttClientPublishReasonCode.Success)
            {
                LogMessageCallBack?.Invoke(new LogMessage()
                {
                    message = $"消息发送成功: {message}",
                    _LogType = LogType.Success,
                });
            }
            else
            {
                LogMessageCallBack?.Invoke(new LogMessage()
                {
                    message = $"消息发送失败: {publishResult.ReasonCode}",
                    _LogType = LogType.Warm
                });
            }
        }
    }
}