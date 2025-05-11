using System.Net;
using System.Security.Cryptography.X509Certificates;
using MQTTnet;
using MQTTnet.Server;
using MyMachinePlatformClientCore.IService.IMqttService.Server;
using MyMachinePlatformClientCore.Log.MyLogs;

namespace MyMachinePlatformClientCore.Service.MQTTService.Server;
/// <summary>
/// 
/// </summary>
public class MqttServerService : IMqttServerService
{
    // 按照命名规则修改私有字段名
    private string _ipaddress;
    private int _port;
    private MqttServer _mqttServer;
    private Action<LogMessage> _logMessageCallBack;
    
    MqttServerOptionsBuilder options;
    /// <summary>
    /// 
    /// </summary>
    private Action<string> _ReceiveMessageCallBack;
    /// <summary>
    /// 自定义的ssl 证书
    /// </summary>
    private X509Certificate2 _certificate;
    /// <summary>
    /// 已经验证成功的客户端id列表
    /// </summary>
    private Dictionary<string,string>sucessClientIdList= new Dictionary<string, string>();
    /// <summary>
    /// 客户端订阅的主题列表
    /// </summary>
    private Dictionary<string,string> _clientTopicDictionary= new Dictionary<string, string>();


    /// <summary>
    /// 
    /// </summary>
    /// <param name="ipaddress">服务器 IP 地址</param>
    /// <param name="port">服务器端口号</param>
    /// <param name="_certificate">自定义 SSL 证书</param>
    /// <param name="MqttServer_ApplicationMessageEnqueuedOrDroppedAsync"></param>
    /// <param name="logMessageCallBack">日志消息回调方法</param>
    public MqttServerService(string ipaddress, int port, X509Certificate2 _certificate,
        
        Action<LogMessage> logMessageCallBack = null,Action<string> ReceiveMessageCallBack=null)
    {
        this._ipaddress = ipaddress;
        this._port = port;
        this._certificate = _certificate;
        this._logMessageCallBack = logMessageCallBack;
        this._ReceiveMessageCallBack = ReceiveMessageCallBack;
        this.options = new MqttServerOptionsBuilder()
            .WithDefaultEndpointBoundIPAddress(IPAddress.Parse(ipaddress))
            // 启用加密端点
            .WithEncryptedEndpoint()
            .WithEncryptedEndpointPort(port)
            // 设置加密证书
            .WithEncryptionCertificate(_certificate.Export(X509ContentType.Pfx))
            .WithEncryptionSslProtocol(System.Security.Authentication.SslProtocols.Tls12)
            .WithDefaultCommunicationTimeout(TimeSpan.FromSeconds(500));




    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task StartService()
    {
        if (options != null)
        {
            
            _mqttServer = new MqttServerFactory().CreateMqttServer(options.Build());
            _mqttServer.ValidatingConnectionAsync += MqttServer_ValidatingConnectionAsync;
            _mqttServer.ClientConnectedAsync += MqttServer_ClientConnectedAsync;
            _mqttServer.ClientDisconnectedAsync += MqttServer_ClientDisconnectedAsync;
            _mqttServer.ClientSubscribedTopicAsync += MqttServer_ClientSubscribedTopicAsync;
            _mqttServer.ClientUnsubscribedTopicAsync += MqttServer_ClientUnsubscribedTopicAsync;
            _mqttServer.ApplicationMessageEnqueuedOrDroppedAsync += MqttServer_ApplicationMessageEnqueuedOrDroppedAsync;
            _mqttServer.InterceptingPublishAsync += MqttServer_InterceptingPublishAsync;
             await  _mqttServer.StartAsync();
        }
        
    }
    /// <summary>
    /// 
    /// </summary>
    public async Task StopService()
    {
        if (_mqttServer != null)
        {
            await _mqttServer.StopAsync();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private async Task MqttServer_InterceptingPublishAsync(InterceptingPublishEventArgs arg)
    {
        string client = arg.ClientId; 
        string topic =arg.ApplicationMessage.Topic;
        string contents = arg.ApplicationMessage.ConvertPayloadToString();
        string mess = $"接收到消息：Client：【{client}】 Topic：【{topic}】 Mesage：【{contents}】";
        _logMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.Info, mess));
        _ReceiveMessageCallBack?.Invoke(contents);
          
    }

    private async Task MqttServer_ApplicationMessageEnqueuedOrDroppedAsync(ApplicationMessageEnqueuedEventArgs arg)
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// 客户端退订主题事件
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private async Task MqttServer_ClientUnsubscribedTopicAsync(ClientUnsubscribedTopicEventArgs arg)
    {
         string clientId = arg.ClientId;
         string topic = arg.TopicFilter;
         
    }

    /// <summary>
    /// 客户端订阅主题事件
    /// </summary>
    /// <param name="arg"></param>
    private async Task MqttServer_ClientSubscribedTopicAsync(ClientSubscribedTopicEventArgs arg)
    {
        string clientId = arg.ClientId;
        string topic = arg.TopicFilter.Topic;
        _logMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.Success,
            $"客户端{clientId}已订阅主题{topic}"));
        AddTopicToDictionary(clientId,topic);
        
        await Task.CompletedTask;
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private async  Task MqttServer_ClientDisconnectedAsync(ClientDisconnectedEventArgs arg)
    {
        string clientID = arg.ClientId;
        if (sucessClientIdList.Keys.Contains(clientID))
        {
            _logMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.Success,
                $"客户端{clientID}已断开连接"));
            sucessClientIdList.Remove(clientID);
        }
        
        await  Task.CompletedTask;
    }

    /// <summary>
    /// 客户端连接事件
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private  async Task MqttServer_ClientConnectedAsync(ClientConnectedEventArgs arg)
    {
       
        string clientId = arg.ClientId;
        if (sucessClientIdList.Keys.Contains(clientId))
        {
            string ipaddress = arg.Endpoint;
            _logMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.Success,
                $"客户端{clientId}已连接到服务器,ip地址为{ipaddress}"));
            sucessClientIdList[clientId] = ipaddress;

        }
        await Task.CompletedTask;  
       
        
    }

    /// <summary>
    /// 验证客户端连接信息
    /// </summary>
    /// <param name="arg"></param>
    private async Task MqttServer_ValidatingConnectionAsync(ValidatingConnectionEventArgs arg)
    {
       String clientId =  arg.ClientId;
       string userName = arg.UserName;
       string password = arg.Password;
       var clientCertificate = arg.ClientCertificate;
       //验证客户端的证书与客户端的用户名 与密码
       
       
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="clientId"></param>
    /// <param name="topic"></param>
    private void AddTopicToDictionary(string clientId, string topic)
    {
        lock (_clientTopicDictionary)
        {
            if (!_clientTopicDictionary.ContainsKey(clientId))
            {
                _clientTopicDictionary.Add(clientId, topic);
            }
            else
            
                _clientTopicDictionary[clientId] = topic;
        }
    }
}