using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using MyMachinePlatformClientCore.IService.IRedisService;
using MyMachinePlatformClientCore.Log.MyLogs;
using StackExchange.Redis;

namespace MyMachinePlatformClientCore.Service.RedisService;
/// <summary>
/// 
/// </summary>
public class RedisService : IRedisService, IDisposable
{
    /// <summary>
    /// 
    /// </summary>
    private  ConnectionMultiplexer _connection;
    /// <summary>
    /// 
    /// </summary>
    private string ipaddress;
    /// <summary>
    /// 
    /// </summary>
    private int port;
    /// <summary>
    /// 
    /// </summary>
    private string password;
    /// <summary>
    /// 
    /// </summary>
    private bool isUseSsl;
    /// <summary>
    /// 
    /// </summary>
    private Action<LogMessage> _logMessageDataCallBack;
     /// <summary>
     /// 
     /// </summary>
    private ISubscriber _subscriber;


    
    /// <summary>
    /// 
    /// </summary>
    private IDatabase _database;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="host"></param>
    /// <param name="port"></param>
    /// <param name="password"></param>
    /// <param name="isUseSsl"></param>
    /// <param name="logMessageDataCallBack"></param>
    /// <param name="_certificate">自定义的证书</param>
    public RedisService(string host ,int port, string password,bool isUseSsl=true,Action<LogMessage> logMessageDataCallBack=null)
    {
         this.ipaddress = host;
         this.port = port;
         this.password = password;
         this.isUseSsl = isUseSsl;
         this._logMessageDataCallBack = logMessageDataCallBack;
         
         
    }
    /// <summary>
    /// 
    /// </summary>
    public void StartService()
    {
        try
        {
            var config = new ConfigurationOptions
            {
                EndPoints = { $"{ipaddress}:{port}" },
                Password = password,
                Ssl = isUseSsl,
                AbortOnConnectFail = false,
                AllowAdmin = true,
                ConnectTimeout = 5000,
                SslProtocols = SslProtocols.Tls12,
                
            };
            _connection = ConnectionMultiplexer.Connect(config);
            if (_connection.IsConnected)
            {
                var src = _connection.GetSubscriber();
                this._subscriber = src;
                _logMessageDataCallBack?.Invoke(LogMessage.SetMessage(LogType.Success,"启动Redis服务成功"));
                _database = _connection.GetDatabase();
                
            }
            
        }
        catch (Exception e)
        {
            _logMessageDataCallBack?.Invoke(LogMessage.SetMessage(LogType.Error,"启动Redis服务失败失败的异常信息为："+e.Message));
            return;
        }
        
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="timeSpan"></param>
    /// <returns></returns>

    public bool SetString(string key, string value, TimeSpan? timeSpan = null)
    {
        return _database.StringSet(key, value, timeSpan);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetString(string key)
    {
        return _database.StringGet(key);
    }

    #region  哈希操作
    /// <summary>
    /// 设置哈希表中的字段值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="field"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool HashSet(string key, string field, string value)
    {
        return _database.HashSet(key, field, value);
    }
    /// <summary>
    /// 获取哈希表中的字段值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="field"></param>
    /// <returns></returns>

    public string HashGet(string key, string field)
    {
        return _database.HashGet(key, field);
        
    }
    #endregion
    #region 列表操作
    /// <summary>
    /// 从列表左侧插入元素
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public long ListLeftPush(string key, string value)
    {
        return _database.ListLeftPush(key, value);
    }

    /// <summary>
    /// 从列表右侧插入元素
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public long ListRightPush(string key, string value)
    {
        return _database.ListRightPush(key, value);
    }

    /// <summary>
    /// 从列表左侧弹出元素
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string ListLeftPop(string key)
    {
        return _database.ListLeftPop(key);
        
    }

    /// <summary>
    /// 从列表右侧弹出元素
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string ListRightPop(string key)
    {
        return _database.ListRightPop(key);
        
    }
    
    #endregion

    #region  订阅操作
    /// <summary>
    /// 订阅消息
    /// </summary>
    /// <param name="channelName"></param>
    /// <param name="onMessage"></param>
    public void Subscribe(string channelName, Action<RedisChannel, RedisValue> onMessage)
    {
        _subscriber.Subscribe(channelName, onMessage);
    }

    /// <summary>
    /// 发布消息
    /// </summary>
    /// <param name="channelName"></param>
    /// <param name="message"></param>
    public void Publish(string channelName, string message)
    {
        _subscriber.Publish(channelName, message);
    }

    #endregion
    
    public void Dispose()
    {
        _connection?.Close();
    }
}