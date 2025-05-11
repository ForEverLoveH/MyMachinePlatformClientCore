using MyMachinePlatformClientCore.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMachinePlatformClientCore.Log.MyLogs;
using MyMachinePlatformClientCore.Service.Managers;

namespace MyMachinePlatformClientCore.Service 
{
    /// <summary>
    /// 登录服务
    /// </summary>
    public class LoginService : ILoginService
    {
        /// <summary>
        /// net 网络类型 0 tcp 1 http
        /// </summary>
        private int type = 0;
        /// <summary>
        /// 
        /// </summary>
        private CTcpClientServiceManager _tcpService;
        /// <summary>
        /// 
        /// </summary>

        private Action<LogMessage> _logDataCallBack;
        /// <summary>
        /// 
        /// </summary>
        private IHttpClientService _httpClientService;
        /// <summary>
        /// 
        /// </summary>
        private bool isJson = false;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tcpService"></param>
        /// <param name="httpClientService"></param>
        /// <param name="logDataCallBack"></param>
        public LoginService(int type,CTcpClientServiceManager tcpService=null,IHttpClientService httpClientService=null, Action<LogMessage> logDataCallBack= null)
        {
            this.type = type;
            this. _tcpService = tcpService;
            this._httpClientService = httpClientService;
            this._logDataCallBack =_logDataCallBack;
            if (type==0)
            {
                isJson = tcpService.IsJson;
            }
        }
         
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async  Task<bool> LoginAsync(string userName, string password)
        {
            if (type==0)
            {
                if (!isJson)
                {
                    return await SendCurrentLoginByProtoBuf(userName, password);
                }
                else
                {
                    return await SendCurrentLoginByJson(userName, password);
                }
            }
            else
            {
                return await SendCurrentLoginByHttp(userName, password);
            }
            

            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private async Task<bool> SendCurrentLoginByHttp(string userName, string password)
        {
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private async Task<bool> SendCurrentLoginByJson(string userName, string password)
        {
           return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        // 定义一个私有异步方法，用于通过ProtoBuf发送当前登录信息
        private async Task<bool> SendCurrentLoginByProtoBuf(string userName, string password)
        {
            // 创建一个UserLoginRequest对象，并设置用户名和密码
            UserLoginRequest loginRequest = new UserLoginRequest()
            {
                Password = password,  // 设置登录请求的密码
                Username = userName, // 设置登录请求的用户名
            };
            // 创建一个新的Task，用于发送ProtoBuf数据
            Task task = new Task(() =>
            {
                // 调用_tcpService的CSendProtobufData方法发送登录请求
                _tcpService.CSendProtobufData(loginRequest);
            });
            await task;
            // 异步等待任务完成，并返回任务是否完成的结果
            return task.IsCompleted;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <param name="phone"></param>
        /// <returns></returns>
        public async Task<bool> RegisterAsync(string userName, string password, string email, string phone)
        {

            return true;
        }
    }
}
