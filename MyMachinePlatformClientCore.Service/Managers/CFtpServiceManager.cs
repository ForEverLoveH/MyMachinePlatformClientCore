using MyMachinePlatformClientCore.IService.IFTPService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMachinePlatformClientCore.Log.MyLogs;
using MyMachinePlatformClientCore.Service.FTPService;
using MyMachinePlatformClientCore.Service.JsonService;
using MyMachinePlatformClientCore.Service.LogService;

namespace MyMachinePlatformClientCore.Service.Managers
{
    public class FtpBaseInfo
    {
        public string FtpServerIp { get; set; }
        public int FtpServerPort { get; set; }
        public string FtpUserName { get; set; }
        public string FtpPassword { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class CFtpServiceManager
    {
        private IFTPClientService clientService;

        private   string _ftpServerIp;
        /// <summary>
        /// 
        /// </summary>
        private  int _ftpServerPort;
        /// <summary>
        /// 
        /// </summary>
        private  string _ftpUserName;
        /// <summary>
        /// 
        /// </summary>
        private  string _ftpPassword;
        /// <summary>
        /// 
        /// </summary>
        private Action<LogMessage> _logDataCallBack;
        /// <summary>
        /// 
        /// </summary>
        public CFtpServiceManager(Action<LogMessage>logDataCallBack=null)
        {
            this._logDataCallBack = logDataCallBack;
            ReadFtpServiceConfig();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ReadFtpServiceConfig()
        {
             string ftpServerConfigPath =Path.Combine( AppDomain.CurrentDomain.BaseDirectory , "Config\\FtpServiceConfig.json");
              FtpBaseInfo info=   CJsonService.ReadJsonFileToObject<FtpBaseInfo>( ftpServerConfigPath);
              if (info != null)
              {
                  this._ftpServerIp = info.FtpServerIp;
                  this._ftpServerPort = info.FtpServerPort;
                  this._ftpUserName = info.FtpUserName;
                  this._ftpPassword = info.FtpPassword;
                  clientService = new FTPClientService(_ftpServerIp, _ftpServerPort, _ftpUserName, _ftpPassword, _logDataCallBack);
              }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="localFilePath">本地路径</param>
        /// <param name="remoteFilePath">远端路径</param>
        /// <param name="sizeThreshold">文件最大的阈值</param>
        /// <returns></returns>
        public async Task<bool> UploadFile(string localFilePath, string remoteFilePath,long sizeThreshold = 10 * 1024 * 1024)=>await clientService?.UploadFile(localFilePath, remoteFilePath, sizeThreshold);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="remoteFilePath"></param>
        /// <param name="localFilePath"></param>
        /// <returns></returns>
        public async  Task<bool> DownloadFile(string remoteFilePath, string localFilePath)=>await clientService?.DownloadFile(remoteFilePath, localFilePath);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="remoteFilePath"></param>
        /// <returns></returns>
        public async Task<bool> DeleteFile(string remoteFilePath) => await clientService?.DeleteFile(remoteFilePath);
    }
}
