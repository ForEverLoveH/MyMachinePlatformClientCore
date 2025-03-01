 
using System.Text;
using MQTTnet;
using MyMachinePlatformClientCore.IService.IMqttService;

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
    private Action<string> LogMessageCallBack;
    /// <summary>
    /// 
    /// </summary>
    private MqttClientOptionsBuilder _optionsBuilder;

    private string topicName;

    public void SetTopicName(string topicName)
    {
        this.topicName = topicName;
    }
    
    private bool isConnected;

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
    /// <param name="_recieveMessageCallBack"></param>
    /// <param name="_logMessageCallBack"></param>
    public MqttClientService(string clientId,string userName,string password,string serverIp,int port,int maxReconnectCount,string topicName,Action<string>_recieveMessageCallBack=null,Action<string>_logMessageCallBack=null)
    {
        this.clientID = clientId;
        this.userName = userName;
        this.password = password;
        this.serverIP = serverIp;
        this.port = port;
        this.topicName = topicName;
        this.maxReconnectCount =Math.Min(maxReconnectCount,5);
        this.RecieveMessageCallBack = _recieveMessageCallBack;
        this.LogMessageCallBack = _logMessageCallBack;
        _optionsBuilder = new MqttClientOptionsBuilder().WithClientId(clientId).WithTcpServer(serverIP,port).WithCredentials(userName,password).WithTimeout(new TimeSpan(0,0,1000));
        InitService();

    }
    /// <summary>
    /// 
    /// </summary>
    private void InitService()
    {
         _mqttClient=new MqttClientFactory().CreateMqttClient();
         _mqttClient.ConnectedAsync += MqttClient_ConnectedAsync;
         _mqttClient.DisconnectedAsync += MqttClient_DisconnectedAsync;
         _mqttClient.ApplicationMessageReceivedAsync += MqttClient_ApplicationMessageReceivedAsync;
         
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="arg"></param>
    private async Task MqttClient_ConnectedAsync(MqttClientConnectedEventArgs arg)
    {
         
        LogMessageCallBack?.Invoke("MQTT 客户端已连接");
        // 订阅主题
        var subscribeResult = await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(this.topicName).Build());
        var po = subscribeResult.Items.FirstOrDefault();
        if (po != null)
        {
            var resCode=po.ResultCode;
            if (resCode == MqttClientSubscribeResultCode.GrantedQoS0)
            {
                LogMessageCallBack?.Invoke("订阅主题：" + this.topicName + " 成功  ");
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="arg"></param>
    private async Task MqttClient_DisconnectedAsync(MqttClientDisconnectedEventArgs arg)
    {
        LogMessageCallBack?.Invoke("MQTT 客户端已断开连接");
        if (currentReconnectCout < maxReconnectCount)
        {
            currentReconnectCout++;
            LogMessageCallBack?.Invoke($"尝试第 {currentReconnectCout} 次重连...");
            await Task.Delay(2000); // 等待 2 秒后尝试重连
            await StartService();
        }
        else
        {
            LogMessageCallBack?.Invoke("达到最大重连次数，停止重连");
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
         LogMessageCallBack?.Invoke($"收到消息：主题 {topic}，内容 {message}");
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
              LogMessageCallBack?.Invoke(message);
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
            LogMessageCallBack?.Invoke("MQTT 客户端已停止");
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
                LogMessageCallBack?.Invoke($"消息发送成功: {message}");
            }
            else
            {
                LogMessageCallBack?.Invoke($"消息发送失败: {publishResult.ReasonCode}");
            }
        }
    }
}