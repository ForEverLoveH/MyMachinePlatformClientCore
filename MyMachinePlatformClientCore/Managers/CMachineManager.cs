using MyMachinePlatformClientCore.Service.Managers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MyMachinePlatformClientCore.IService;
using MyMachinePlatformClientCore.Log.MyLogs;
using MyMachinePlatformClientCore.Service;
using MyMachinePlatformClientCore.Service.HttpService;
using MyMachinePlatformClientCore.Service.LogService;
using MyMachinePlatformClientCore.Service.message_router;
using MyMachinePlatformClientCore.Service.MessageRouter.JsonMessageRouter;
using MyMachinePlatformClientCore.Service.ModbusService;
using MyMachinePlatformClientCore.Service.OMRonService;
using MyMachinePlatformClientCore.Service.SqlSugarService;
using System.Windows.Media;
using MyMachinePlatformClientCore.IService.ISqlSugarService;
using MyMachinePlatformClientCore.Models.Models.dbModels;


namespace MyMachinePlatformClientCore.Managers
{
    public class CMachineManager
    {

        #region ftp相关
        /// <summary>
        /// 
        /// </summary>
        private CFtpServiceManager _ftpServiceManager;

        /// <summary>
        /// 上传文件到服务器
        /// </summary>
        /// <param name="localFilePath">本地文件路径</param>
        /// <param name="remoteFilePath">远程服务器文件路径</param>
        /// <returns></returns>
        public async  Task<bool> UpLoadFileToServer(string localFilePath , string remoteFilePath)
        {
            if (!string.IsNullOrEmpty(localFilePath) &&! string.IsNullOrEmpty(remoteFilePath))
            {
                 return await _ftpServiceManager.UploadFile(localFilePath, remoteFilePath);
            }
            return false;
        }
        /// <summary>
        /// 从服务器下载文件
        /// </summary>
        /// <returns></returns>
        public async Task<bool> DownLoadFileFromServer(string localFilePath, string remoteFilePath)
        {
            if (!string.IsNullOrEmpty(localFilePath) && !string.IsNullOrEmpty(remoteFilePath))
            {
                return await _ftpServiceManager.DownloadFile(localFilePath, remoteFilePath);
            }
            return false;
        }
        /// <summary>
        /// 从服务器删除文件
        /// </summary>
        /// <param name="remoteFilePath"></param>
        /// <returns></returns>
        public async Task<bool> DeleteFileFromServer(string remoteFilePath)
        {
            if (!string.IsNullOrEmpty(remoteFilePath))
            {
                return await _ftpServiceManager.DeleteFile(remoteFilePath);
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        private void StartFtpService()
        {
            _ftpServiceManager = new CFtpServiceManager(HandleCurrentLogCallBack);
            
        }
        #endregion


        #region MQTT相关
        /// <summary>
        /// 
        /// </summary>
        private CMqttServiceManager _mqttServiceManager;
        /// <summary>
        /// 
        /// </summary>
        private  async Task<bool> StartMqttService()
        {
            _mqttServiceManager = new CMqttServiceManager(mess =>
            {
                RecieveDataFromMqttClientCallBack(mess);
            }, message =>
            {
                HandleCurrentLogCallBack(message);
            });
           return  await _mqttServiceManager.StartMqttClientService();
        }
         
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mess"></param>
        private void RecieveDataFromMqttClientCallBack(string message)
        {
            
        }

        #endregion


        #region Modbus相关
        /// <summary>
        /// 
        /// </summary>
        private CMyModbusServiceManagers _modbusRTUServiceManager;
        /// <summary>
        /// 
        /// </summary>
        
        private CMyModbusServiceManagers _modbusTCPServiceManager;

        
        #region  modbusRTU相关
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
        public void StartModbusRTUService(string PortName, int BaudRate, int DataBits,
            System.IO.Ports.StopBits StopBits, System.IO.Ports.Parity Parity, int WriteTimeout = 200, int ReadTimeout = 200)
        {
             
                _modbusRTUServiceManager = new CMyModbusServiceManagers(HandleCurrentLogCallBack);
                _modbusRTUServiceManager.StartMyModbusRTUService(PortName, BaudRate, DataBits, StopBits, Parity,
                    WriteTimeout, ReadTimeout);
            
        }
        /// <summary>
        /// 
        /// </summary>
        public bool? IsModbusRTUServiceConnected => _modbusRTUServiceManager?.IsConnection;
        /// <summary>
        /// 
        /// </summary>
        public void StopModbusRTUService()
        {
            if (_modbusRTUServiceManager != null)
            {
                _modbusRTUServiceManager.StopService();
            }
        }
        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="functionCode">功能码类型</param>
        /// <param name="slaveAddress">站地址</param>
        /// <param name="startAddress">位置,从0开始</param>
        /// <param name="coilsBuffer">线圈数据</param>
        /// <param name="registerBuffer">寄存器数据</param> 
        public async Task WriteModbusRTUData(FunctionCode functionCode, byte slaveAddress, ushort startAddress,
            bool[]? coilsBuffer = null, ushort[]? registerBuffer = null) =>await  _modbusRTUServiceManager.WriteData(functionCode,slaveAddress, startAddress, coilsBuffer, registerBuffer); 
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="functionCode"></param>
        /// <param name="slaveAddress"></param>
        /// <param name="startAddress"></param>
        /// <param name="numberOfPoints"></param>
        /// <returns></returns>
        public async Task<(bool[]?, ushort[]?)> ReadModbusRTUData(FunctionCode functionCode, byte slaveAddress,
            ushort startAddress, ushort numberOfPoints) => await  _modbusRTUServiceManager.ReadData(functionCode,slaveAddress, startAddress, numberOfPoints);
        
        #endregion

        #region   modbusTCP相关
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipaddress"></param>
        /// <param name="port"></param>
        public void StartModbusTcpServer(string ipaddress, int port)
        {
            if (_modbusTCPServiceManager == null)
            {
                _modbusTCPServiceManager = new CMyModbusServiceManagers(HandleCurrentLogCallBack
                );
                _modbusTCPServiceManager.StartMyModbusTcpService(ipaddress, port);
            }
        }
        
        public void StopModbusTcpServer()=> _modbusTCPServiceManager.StopService();
        /// <summary>
        /// 
        /// </summary>
        public bool? IsModbusTcpServiceConnected => _modbusTCPServiceManager?.IsConnection;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="functionCode"></param>
        /// <param name="slaveAddress"></param>
        /// <param name="startAddress"></param>
        /// <param name="coilsBuffer"></param>
        /// <param name="registerBuffer"></param>
        public async Task WriteModbusTcpData(FunctionCode functionCode, byte slaveAddress, ushort startAddress,
            bool[]? coilsBuffer = null, ushort[]? registerBuffer = null) =>await  _modbusTCPServiceManager.WriteData(functionCode,slaveAddress, startAddress, coilsBuffer, registerBuffer); 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="functionCode"></param>
        /// <param name="slaveAddress"></param>
        /// <param name="startAddress"></param>
        /// <param name="numberOfPoints"></param>
        /// <returns></returns>
        public async Task<(bool[]?, ushort[]?)> ReadModbusTcpData(FunctionCode functionCode, byte slaveAddress,
            ushort startAddress, ushort numberOfPoints) => await _modbusTCPServiceManager.ReadData(functionCode,slaveAddress, startAddress, numberOfPoints);

        #endregion


        #endregion

        #region  PLC相关
        
        #region  EIP协议相关
        /// <summary>
        /// 
        /// </summary>
        public OMRonEIPService _omRonEipService;
        /// <summary>
        /// 
        /// </summary>
        private string _Eipaddress;
        /// <summary>
        /// 
        /// </summary>
        private int _Eipport;
        /// <summary>
        /// 
        /// </summary>
        private int _EipBindPort;
        
        
        
        /// <summary>
        /// 
        /// </summary>
        private ConcurrentDictionary<string,OMRonEIPDataType>eipAddressDic=new ConcurrentDictionary<string, OMRonEIPDataType>();
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipaddress"></param>
        /// <param name="port"></param>
        /// <param name="bindPort">本地端口 必须大于1023，如：50001,5002,5003</param>
        public void StartEIPService(string ipaddress, int port,int bindPort)
        {
            _omRonEipService = new OMRonEIPService();
            _omRonEipService.SetTCPParams(IPAddress.Parse(ipaddress), port, bindPort);
            _omRonEipService.Connect();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        private void ReadEIPAddressByPath(string path)
        {
            string json = File.ReadAllText(path);
            if (!string.IsNullOrWhiteSpace(json))
            {
                
            }
        }
        

        #endregion

        #region  fins 协议相关
        /// <summary>
        /// 
        /// </summary>
        private string _finsIpAddress;
        /// <summary>
        /// 
        /// </summary>
        private int _finsPort;
        /// <summary>
        /// 
        /// </summary>
        public OMRonTcpFinsService _omRonTcpFinsService;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipaddress"></param>
        /// <param name="port"></param>
        public void StartOMRonTcpFinsService(string ipaddress, int port)
        {
            _omRonTcpFinsService = new OMRonTcpFinsService(HandleCurrentLogCallBack);
            _omRonTcpFinsService.SetTCPParams(IPAddress.Parse(ipaddress), port);
            _omRonTcpFinsService.Connect();
            
        }
            
        

        #endregion
        
        

        #endregion

        #region  tcpClient服务相关
        /// <summary>
        /// 
        /// </summary>
        private string _tcpIpAddress;
        /// <summary>
        /// 
        /// </summary>
        private int _tcpPort;
        /// <summary>
        /// 
        /// </summary>
        private  CTcpClientServiceManager tcpClientServiceManager;
        /// <summary>
        /// 
        /// </summary>
        private bool isjson = false;
        
         
        /// <summary>
        /// 
        /// </summary>
        private void StartTcpService()
        {
            tcpClientServiceManager = new CTcpClientServiceManager(_tcpIpAddress, _tcpPort,isjson,HandleCurrentLogCallBack);
            if (tcpClientServiceManager.IsConnected)
            {
                HandleCurrentLogCallBack(LogMessage.SetMessage(LogType.Success,$"服务端{_tcpIpAddress}:{_tcpPort}连接成功"));
            }
            else
            {
                HandleCurrentLogCallBack(LogMessage.SetMessage(LogType.Error, $"服务端{_tcpIpAddress}:{_tcpPort}连接失败"));
            }

        }

        #endregion
        
        
        #region http 相关
        /// <summary>
        /// 
        /// </summary>
        private IHttpClientService _httpClientService;
        /// <summary>
        /// 
        /// </summary>
        private string httpUrl;
        /// <summary>
        /// 
        /// </summary>
        public void StartHttpClientService()
        {
            _httpClientService = new HttpClientService(httpUrl,HandleCurrentLogCallBack);
            
        }
        
        #endregion

        #region RSA加密相关
        /// <summary>
        /// 公钥
        /// </summary>
        private string _rsaPublicKey;
        /// <summary>
        /// 私钥
        /// </summary>
        private string _rsaPrivateKey;
        /// <summary>
        /// 
        /// </summary>
        private RSAService _RSAService;
        /// <summary>
        /// 
        /// </summary>
        public void StartRSAService()
        {
            _RSAService = new RSAService(_rsaPrivateKey, _rsaPublicKey );
        }

        #endregion


        #region  log日志相关
        /// <summary>
        /// 
        /// </summary>
        private LogService _logService;
        /// <summary>
        /// 
        /// </summary>

        private void StartLogService()
        {
            _logService = new LogService(4, mess =>
            {
                HandleCurrentLogCallBack(mess);
            });
            _logService.StartLogService();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void HandleCurrentLogCallBack(LogMessage message)
        {
            string mess = message.message;
            LogType type = message._LogType;
            switch (type)
            {
                case LogType.Error:
                    MyLogTool.ColorLog(MyLogColor.Red,mess); break;
                case LogType.Warm :
                    MyLogTool.ColorLog(MyLogColor.Blue,mess); break;
                case LogType.Success:
                    MyLogTool.ColorLog(MyLogColor.Green,mess); break;
                case LogType.Info:
                    MyLogTool.ColorLog(MyLogColor.Magentna,mess); break;
                case LogType.None:
                    
                    MyLogTool.ColorLog(MyLogColor.Cyan,mess); break;
            }
        }

        #endregion

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            ReadInitData();
            ServiceInit();
        }
        /// <summary>
        ///  读取初始化项目信息
        /// </summary>
        private void ReadInitData()
        {
            
        }
        /// <summary>
        /// 
        /// </summary>
        private void ServiceInit()
        {
            StartLogService();
            
            //StartEIPService(_Eipaddress, _Eipport, _EipBindPort);

            //StartOMRonTcpFinsService(_finsIpAddress, _finsPort);

            StartFtpService();

            //StartMqttService().Wait(10);

            StartMessageRouterService();

            //StartTcpService();

            StartLoginService();
        }

        #region  消息路由相关(tcp 客户端)

        /// <summary>
        /// 
        /// </summary>
        private MessageRouterManager _messageRouterManager;
        /// <summary>
        /// 
        /// </summary>

        private void StartMessageRouterService()
        {
            _messageRouterManager = new MessageRouterManager(10, 1);
            _messageRouterManager.StartService();
        }
        

        #endregion

        #region  业务流程服务相关

        #region  登录服务相关

        private int netType = 0;
        /// <summary>
        /// 
        /// </summary>
        private ILoginService _LoginService;
        /// <summary>
        /// 
        /// </summary>
        private void StartLoginService()
        {
            _LoginService = new LoginService(netType,tcpClientServiceManager,_httpClientService, HandleCurrentLogCallBack);
        }
        /// <summary>
        /// 
        /// </summary>
        /// 
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<bool> LoginAsync(string userName, string password) =>await _LoginService.LoginAsync(userName, password);
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <param name="phone"></param>
        /// <returns></returns>

        public async Task<bool> RegisterAsync(string userName, string password, string email, string phone)=>await _LoginService.RegisterAsync(userName, password, email, phone);

        #endregion
        #endregion

        
        #region SqlSugarService相关
        /// <summary>
        /// 
        /// </summary>
        private ISqlSugarService _sqlsugarService= new  SqlSugarService();



        

        
        

        #endregion

    }
}
