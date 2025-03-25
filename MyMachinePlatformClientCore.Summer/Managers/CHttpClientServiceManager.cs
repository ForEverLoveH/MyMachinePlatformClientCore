using MyMachinePlatformClientCore.IService;
using MyMachinePlatformClientCore.Service.HttpService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Managers
{
    public class CHttpClientServiceManager
    {
        /// <summary>
        /// 
        /// </summary>
        private IHttpClientService _httpClientService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        public CHttpClientServiceManager(string url="")
        {
            if(!string.IsNullOrEmpty(url))
                 _httpClientService = new HttpClientService(url);
        }
        public string Url{ get=>_httpClientService?.Url;set=>_httpClientService.Url = value;}

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="tin"></param>
        /// <param name="postName"></param>
        /// <param name="timeOut"></param>
        /// <param name="cookie"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public async Task<TOut> SendPostRequestMessageToServe<TIn, TOut>(TIn tin, string postName, int timeOut = 30,
            CookieCollection cookie = null, string contentType = "application/json")
            where TIn : class where TOut : class
        {
            return await _httpClientService.SendPostRequestMessageToServerAsync<TIn, TOut>(tin, postName, timeOut, cookie, contentType);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="postName"></param>
        /// <param name="timeOut"></param>
        /// <param name="cookie"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public async Task<TOut> SendGetRequestMessageToServer<TOut>(string postName, int timeOut = 30, CookieCollection cookie = null, string contentType = "application/json") where TOut : class => await _httpClientService.SendGetRequestMessageToServer<TOut>(postName, timeOut, cookie, contentType);
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="obj"></param>
        /// <param name="postName"></param>
        /// <param name="timeOut"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
       public async  Task<TOut> SendPutRequestMessageToServer<TIn, TOut>(TIn obj, string postName, int timeOut = 30,
       CookieCollection cookieContainer = null, string contentType = "application/json")
       where TIn : class where TOut : class=> await _httpClientService.SendPutRequestMessageToServer<TIn, TOut>(obj, postName, timeOut, cookieContainer, contentType);
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="postName"></param>
        /// <param name="timeOut"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
       public async Task<TOut> SendDeleteRequestMessageToServer<TOut>(string postName, int timeOut = 30, CookieCollection cookieContainer = null, string contentType = "application/json") where TOut : class => await _httpClientService.SendDeleteRequestMessageToServer<TOut>(postName, timeOut, cookieContainer, contentType);
        /// <summary>
        /// 发送下载文件请求
        /// </summary>
        /// <param name="postName"></param>
        /// <param name="savePath"></param>
        /// <param name="timeOut"></param>
        /// <param name="cookieContainer"></param>
        /// <returns></returns>
      public async Task<bool>   SendDownloadFileRequest(string postName, string savePath, int timeOut = 30,
        CookieCollection cookieContainer = null, string contentType = "application/json") 
            => await _httpClientService.SendDownloadFileRequest(postName, savePath, timeOut, cookieContainer,contentType); 
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
            CookieCollection cookieContainer = null, string contentType = "application/octet-stream")=> await _httpClientService.SendUploadFileRequest(postName, filePath, fileMaxSize, chunkSize, timeOut, cookieContainer, contentType);
    }

}
