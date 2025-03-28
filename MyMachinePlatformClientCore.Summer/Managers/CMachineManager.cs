using System.Net;
using Google.Protobuf;
using MyMachinePlatformClientCore.Common.TcpService.Client;
using MyMachinePlatformClientCore.Service.message_router;
using MyMachinePlatformClientCore.Service.ModbusService;
using MyMachinePlatformClientCore.Service.OMRonService;
using static MyMachinePlatformClientCore.Service.MessageRouter.CProtoMessageRouter;

namespace MyMachinePlatformClientCore.Summer.Managers;

public class CMachineManager
{
    #region  服务管理类
    /// <summary>
    /// 
    /// </summary>
    private CMqttServiceManager _mqttServiceManager=new CMqttServiceManager();
    /// <summary>
    /// 
    /// </summary>
    
    private CFtpServiceManager _ftpServiceManager=new CFtpServiceManager();
    /// <summary>
    /// modbus TCP 链接
    /// </summary>
    private  CMyModbusServiceManagers _modbusTcpServiceManagers=new CMyModbusServiceManagers();
    /// <summary>
    /// modbus RTU 链接
    /// </summary>
    private  CMyModbusServiceManagers _modbusRTUServiceManagers=new CMyModbusServiceManagers();
    /// <summary>
    /// /
    /// </summary>

    private CHttpClientServiceManager _httpClientServiceManager=  new CHttpClientServiceManager();
    /// <summary>
    /// 
    /// </summary>
    private CRunManager _runManager=new CRunManager();
    /// <summary>
    /// 
    /// </summary>

    private CTcpClientServiceManager _tcpClientServiceManager;
    /// <summary>
    /// 
    /// </summary>
    private   MessageRouterManager _messageRouterManager;
    
    #endregion

    #region  其他类
    /// <summary>
    /// 
    /// </summary>
    public OMRonEIPService _eipService; 
    

    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_mqttServiceManager"></param>
    public CMachineManager()
    {
        
    }
    
    #region mqtt相关
    /// <summary>
    /// 
    /// </summary>
    public void StartMqttClientService() => _mqttServiceManager.StartMqttClientService();
    /// <summary>
    /// 
    /// </summary>
    public void StopMqttClientService() => _mqttServiceManager.StopMqttClientService();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="topicName"></param>
    public void SendMessage(string message, string topicName = "") => _mqttServiceManager.SendMessage(message, topicName);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="topicName"></param>
    public void SetTopicName(string topicName) => _mqttServiceManager.TopicName = topicName;
    
    #endregion

    #region  HttpClient相关
    /// <summary>
    /// 
    /// </summary>
    public string HttpUrl
    {
        get => _httpClientServiceManager.Url;
        set => _httpClientServiceManager.Url = value;
    }
    /// <summary>
    /// post请求
    /// </summary>
    /// <param name="tin"></param>
    /// <param name="postName">方法名</param>
    /// <param name="timeOut"></param>
    /// <param name="cookie"></param>
    /// <param name="contentType"></param>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <returns></returns>
    public  async  Task<TOut> SendPostRequestMessageToServe<TIn,TOut>(TIn tin,string postName ,int timeOut=30,CookieCollection cookie=null, string contentType = "application/json") where TIn : class where TOut : class 
    =>await _httpClientServiceManager?.SendPostRequestMessageToServe<TIn, TOut>(tin, postName, timeOut, cookie, contentType);
    /// <summary>
    /// get请求
    /// </summary>
    /// <param name="postName"></param>
    /// <param name="timeOut"></param>
    /// <param name="cookie"></param>
    /// <param name="contentType"></param>
    /// <typeparam name="TOut"></typeparam>
    /// <returns></returns>
    public async Task<TOut> SendGetRequestMessageToServer<TOut>(string postName, int timeOut = 30, CookieCollection cookie = null, string contentType = "application/json") where TOut : class
    =>await _httpClientServiceManager?.SendGetRequestMessageToServer<TOut>(postName, timeOut, cookie, contentType);
   /// <summary>
   /// delete 请求
   /// </summary>
   /// <param name="postName"></param>
   /// <param name="timeOut"></param>
   /// <param name="cookie"></param>
   /// <param name="contentType"></param>
   /// <typeparam name="TOut"></typeparam>
   /// <returns></returns>
    public async Task<TOut> SendDeleteRequestMessageToServer<TOut>(string postName, int timeOut = 30, CookieCollection cookie = null, string contentType = "application/json") where TOut : class
    =>await _httpClientServiceManager?.SendDeleteRequestMessageToServer<TOut>(postName, timeOut, cookie, contentType);
    /// <summary>
    /// put 请求
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="postName"></param>
    /// <param name="timeOut"></param>
    /// <param name="cookieContainer"></param>
    /// <param name="contentType"></param>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <returns></returns>
    public async Task<TOut> SendPutRequestMessageToServer<TIn, TOut>(TIn obj, string postName, int timeOut = 30,
        CookieCollection cookieContainer = null, string contentType = "application/json")
        where TIn : class where TOut : class
        => await _httpClientServiceManager?.SendPutRequestMessageToServer<TIn, TOut>(obj, postName, timeOut, cookieContainer, contentType);
    /// <summary>
    /// 发送下载文件请求
    /// </summary>
    /// <param name="localFilePath"></param>
    /// <param name="remoteFilePath"></param>
    /// <returns></returns>
    public async Task<bool> SendDownloadFileRequest(string localFilePath, string remoteFilePath)
        => await _httpClientServiceManager?.SendDownloadFileRequest(localFilePath, remoteFilePath);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="postName"></param>
    /// <param name="filePath"></param>
    /// <param name="fileMaxSize"></param>
    /// <param name="chunkSize"></param>
    /// <param name="timeOut"></param>
    /// <param name="cookieContainer"></param>
    /// <param name="contentType"></param>
    /// <returns></returns>
    public  async Task<bool> SendUploadFileRequest(string postName, string filePath, long fileMaxSize, int chunkSize = 1024 * 1024,
        int timeOut = 30,
        CookieCollection cookieContainer = null, string contentType = "application/octet-stream")=>await _httpClientServiceManager?.SendUploadFileRequest(postName, filePath, fileMaxSize, chunkSize, timeOut, cookieContainer, contentType);

    #endregion

    #region TcpClient相关

    private string _ipAddress;
    /// <summary>
    /// 
    /// </summary>
    private int _port;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <param name="port"></param>
    public void StartTcpClientService(string ipAddress, int port,bool isJson= false)
    {
        _ipAddress = ipAddress;
        _port = port;
        _tcpClientServiceManager = new CTcpClientServiceManager(ipAddress, port,isJson);
        _tcpClientServiceManager.StartService();
    }
    /// <summary>
    /// 
    /// </summary>
    public void StopTcpClientService() => _tcpClientServiceManager.StopService();
    /// <summary>
    /// 
    /// </summary>
    public void Reconnect() => _tcpClientServiceManager.Reconnect();
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    public void CSendJsonData<T>(T message) where T:class => _tcpClientServiceManager.CSendJsonData(message);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public void CSendJsonData(string message) => _tcpClientServiceManager.CSendJsonData(message);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public void CSendJsonData(byte[] message) => _tcpClientServiceManager.CSendJsonData(message);
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    public void CSendProtobufData<T>(T message) where T:IMessage<T> => _tcpClientServiceManager.CSendProtobufData(message);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    public void  CSendProtobufData(byte[] data) => _tcpClientServiceManager.CSendProtobufData(data);




    #endregion

    #region  FTPClient相关
    /// <summary>
    /// ftp 上传文件
    /// </summary>
    /// <param name="localFilePath"></param>
    /// <param name="remoteFilePath"></param>
    /// <param name="sizeThreshold">文件的最大阈值</param>
    /// <returns></returns>

    public async Task<bool> UploadFile(string localFilePath, string remoteFilePath,long sizeThreshold = 10 * 1024 * 1024)=>await _ftpServiceManager?.UploadFile(localFilePath, remoteFilePath, sizeThreshold);
    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="localFilePath"></param>
    /// <param name="remoteFilePath"></param>
    /// <returns></returns>
    public async Task<bool> DownloadFile(string localFilePath, string remoteFilePath)=>await _ftpServiceManager?.DownloadFile(localFilePath, remoteFilePath);
    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="remoteFilePath"></param>
    /// <returns></returns>
    public  async Task<bool> DeleteFile(string remoteFilePath)=>await _ftpServiceManager?.DeleteFile(remoteFilePath);
    #endregion

    #region  myModbus相关

        #region  myModbusRTU相关

        /// <summary>
        /// 
        /// </summary>
        /// <param name="PortName"></param>
        /// <param name="BaudRate"></param>
        /// <param name="DataBits"></param>
        /// <param name="StopBits"></param>
        /// <param name="Parity"></param>
        /// <param name="WriteTimeout"></param>
        /// <param name="ReadTimeout"></param>
        public void StartMyModbusRTUService(string PortName, int BaudRate, int DataBits,
            System.IO.Ports.StopBits StopBits, System.IO.Ports.Parity Parity, int WriteTimeout = 200,
            int ReadTimeout = 200)
           => _modbusRTUServiceManagers.StartMyModbusRTUService(PortName, BaudRate, DataBits, StopBits, Parity,WriteTimeout, ReadTimeout);
        /// <summary>
        /// 
        /// </summary>
        public void StopMyModbusRTUService() => _modbusRTUServiceManagers.StopService();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsMyModbusRTUServiceConnected() => _modbusRTUServiceManagers.IsConnection;
        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="functionCode">功能码类型</param>
        /// <param name="slaveAddress">站地址</param>
        /// <param name="startAddress">位置,从0开始</param>
        /// <param name="numberOfPoints">长度</param>
        /// <returns></returns>
        public async Task<(bool[]?, ushort[]?)> MyModbusRtuReadData(FunctionCode functionCode, byte slaveAddress,
            ushort startAddress, ushort numberOfPoints)=> await   _modbusRTUServiceManagers.ReadData(functionCode, slaveAddress, startAddress, numberOfPoints);
        
           /// <summary>
           /// 
           /// </summary>
           /// <param name="functionCode">功能码</param>
           /// <param name="slaveAddress">从站地址</param>
           /// <param name="startAddress">起始地址</param>
           /// <param name="coilsBuffer">线圈数据</param>
           /// <param name="registerBuffer">寄存器数据</param>
        public async Task MyModbusRtuWriteData(FunctionCode functionCode, byte slaveAddress, ushort startAddress, bool[]? coilsBuffer = null, ushort[]? registerBuffer = null)
         => await _modbusRTUServiceManagers.WriteData(functionCode, slaveAddress, startAddress, coilsBuffer, registerBuffer);
        


        #endregion

        #region MyModbusTCP相关

        /// <summary>
        ///
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        public void StartMyModbusTCPService(string ipAddress, int port)=> _modbusTcpServiceManagers.StartMyModbusTcpService(ipAddress, port);
        /// <summary>
        /// 
        /// </summary>
        public void StopMyModbusTCPService() => _modbusTcpServiceManagers.StopService();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsMyModbusTcpServiceConnected()=>_modbusTcpServiceManagers.IsConnection;
        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="functionCode">功能码类型</param>
        /// <param name="slaveAddress">站地址</param>
        /// <param name="startAddress">位置,从0开始</param>
        /// <param name="numberOfPoints">长度</param>
        /// <returns></returns>
        public async Task<(bool[]?, ushort[]?)> MyModbusTcpReadData(FunctionCode functionCode, byte slaveAddress,
            ushort startAddress, ushort numberOfPoints)=>await _modbusTcpServiceManagers.ReadData(functionCode, slaveAddress, startAddress, numberOfPoints);
        
        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="functionCode">功能码类型</param>
        /// <param name="slaveAddress">站地址</param>
        /// <param name="startAddress">位置,从0开始</param>
        /// <param name="coilsBuffer">线圈数据</param>
        /// <param name="registerBuffer">寄存器数据</param>
        public async Task MyModbusTcpWriteData(FunctionCode functionCode, byte slaveAddress, ushort startAddress, bool[]? coilsBuffer = null, ushort[]? registerBuffer = null)
            =>await _modbusTcpServiceManagers.WriteData(functionCode, slaveAddress, startAddress, coilsBuffer, registerBuffer);



    #endregion

    #endregion

    #region CRunManager相关
    public void Init() => _runManager?.Init();

    #endregion

    #region  消息路由相关
    /// <summary>
    /// 
    /// </summary>
    private int threadCount=10;
    /// <summary>
    /// 默认发送proto数据
    /// </summary>
    private int type=1;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="threadCount"></param>
    /// <param name="type"></param>
    public void StartMessageRouter()
    {
        _messageRouterManager = new MessageRouterManager(threadCount,type);
        _messageRouterManager.StartService();
    }
    /// <summary>
    /// 
    /// </summary>
    public void StopMessageRouter() =>  _messageRouterManager.StopService();

    #region Json 数据相关
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tcpClient"></param>
    /// <param name="json"></param>
    public void AddMessageDataToQueue(TcpClient tcpClient, string json)
    {
        if (type == 0) _messageRouterManager.AddMessageDataToQueue(tcpClient, json);
        else return;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    public void AddNewMessageHandlersToDictionary<T>(Action<TcpClient, object> action) where T : class 
    { 
        if(type == 0) _messageRouterManager.AddNewMessageHandlersToDictionary<T>(action);
       
    }


    #endregion

    #region proto数据
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tcpClient"></param>
    /// <param name="message"></param>
    public void AddMessageDataToQueue<T>(TcpClient tcpClient, T message) where T : IMessage
    {
        if (type == 1) 
        { 
              _messageRouterManager.AddMessageDataToQueue(tcpClient, message); 
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="handler"></param>
    public void OnMessage<T>(MessageHandler<T> handler) where T : IMessage
    {
        if (type == 1)
        {
            _messageRouterManager.OnMessage(handler);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="handler"></param>
    public void OffMessage<T>(MessageHandler<T> handler) where T : IMessage
    {
        if (type == 1)
        {
            _messageRouterManager.OffMessage(handler);
        }
    }

    #endregion
    #endregion

    #region plc相关
    
    #region EIP 协议相关
    
    /// <summary>
    /// 
    /// </summary>
    private string _eipIpAddress;
    /// <summary>
    /// 
    /// </summary>
    private int _eipPort;
    /// <summary>
    /// 
    /// </summary>
    private int _blockPort;

    /// <summary>
    /// eip 读取地址
    /// </summary>
    private Dictionary<string, OMRonEIPDataType> eipReadAddress;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <param name="dataType"></param>
    private void AddEipReadAddressToDic(string address, OMRonEIPDataType dataType)
    {
        if (!eipReadAddress.Keys.Contains(address))
        {
            eipReadAddress.Add(address, dataType);
        }
        else
        {
            return;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public void StartEIPService()
    {
        _eipService = new OMRonEIPService( );
        _eipService.SetTCPParams(IPAddress.Parse(_eipIpAddress), _eipPort, _blockPort);
        _eipService.Connect();
        // 添加读取地址
        AddEipServiceReadAddress();
    }
    /// <summary>
    /// 添加eip 读写地址
    /// </summary>
    private void AddEipServiceReadAddress()
    {
        if (eipReadAddress != null)
        {
            foreach (var item in eipReadAddress.Keys)
            {
                 eipReadAddress.TryGetValue(item, out var dataType);
                 _eipService.AddReadArea(item, dataType);
            }
        }
    }

    #endregion
    #endregion
}