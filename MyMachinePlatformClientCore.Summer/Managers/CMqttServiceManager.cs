using MyMachinePlatformClientCore.IService.IMqttService;
using MyMachinePlatformClientCore.Service.JsonService;
using MyMachinePlatformClientCore.Service.MQTTService;

namespace MyMachinePlatformClientCore.Summer.Managers;
/// <summary>
/// 
/// </summary>
public class MqttClientBase
{
    public string TopicName { get; set; }
    public int MaxReconnectCount{get; set; }
    public string ServerIP{get; set; }
    public int Port { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string ClientID { get; set; }
}
/// <summary>
/// 
/// </summary>
public class CMqttServiceManager
{
    private   IMqttClientService _mqttClientService;
    
    
    private  string _topicName;

    public string TopicName
    {
        get=>_topicName;
        set{_topicName=value;this._mqttClientService.SetTopicName(value);}
    }
    /// <summary>
    /// 
    /// </summary>
    private int _maxReconnectCount;
    
    private string _serverIP;
    private int _port;
    private string _userName;
    private string _password;
    private string _clientID;

    private string mqttConfigPath;
    /// <summary>
    /// 
    /// </summary>
    public CMqttServiceManager()
    {
        mqttConfigPath=Path.Combine(AppContext.BaseDirectory,"/Config/mqttClientConfig.json");
        ReadMqttConfigFile();
    }
    /// <summary>
    /// 
    /// </summary>
    private void ReadMqttConfigFile()
    {
         MqttClientBase mqttClientBase =CJsonService.ReadJsonFileToObject<MqttClientBase>(mqttConfigPath);
        if (mqttClientBase != null)
        {
            this._topicName = mqttClientBase.TopicName;
            this._maxReconnectCount = mqttClientBase.MaxReconnectCount;
            this._serverIP = mqttClientBase.ServerIP;
            this._port = mqttClientBase.Port;
            this._userName = mqttClientBase.UserName;
            this._password = mqttClientBase.Password;
            this._clientID = mqttClientBase.ClientID;
        }
         
    }
    /// <summary>
    /// 
    /// </summary>
    public async void StartMqttClientService()
    {
        _mqttClientService = new MqttClientService(this._clientID,this._userName,this._password,this._serverIP,this._port,this._maxReconnectCount,this._topicName,
            message =>
            {
                RecieveDataFromMqttClientService(message);
            }, mess =>
            {
                LogMessageDataFromMqttClientService(mess);
            });
        await _mqttClientService.StartService();
    }
    /// <summary>
    /// 
    /// </summary>
    public async void StopMqttClientService()
    {
        if (_mqttClientService != null)
        {
           await  _mqttClientService.StopService();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="topicNames"></param>
    public async void SendMessage(string message, string topicNames = "")
    {
         if(!string.IsNullOrEmpty(topicNames))this._topicName = topicNames;
         await _mqttClientService.SendMessage(message,_topicName);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    private void LogMessageDataFromMqttClientService(string message)
    {
         
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    private void RecieveDataFromMqttClientService(string message)
    {
         
    }
}