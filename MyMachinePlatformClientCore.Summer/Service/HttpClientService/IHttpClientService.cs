using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Service 
{
    public interface IHttpClientService
    {
        /// <summary>
        /// 
        /// </summary>
        string Url { get; set; }
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
        /// <param name="headers"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>

        Task<TOut> SendPostRequestMessageToServerAsync<TIn, TOut>(TIn obj, string postName, int timeOut = 30,CookieContainer cookieContainer = null, string contentType = "application/json", string headers = "jsonData=", CancellationToken cancellationToken = default) where TIn : class where TOut : class;
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="postName"></param>
        /// <param name="timeOut"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="contentType"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>

        Task<TOut> SendGetRequestMessageToServer<TOut>(string postName, int timeOut = 30, CookieContainer cookieContainer = null, string contentType = "application/json", CancellationToken cancellation = default) where TOut : class;
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
        /// <param name="headerCode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<TOut> SendPutRequestMessageToServer<TIn, TOut>(TIn obj, string postName, int timeOut = 30, CookieContainer cookieContainer = null, string contentType = "application/json", string headerCode = "jsonData=", CancellationToken cancellationToken = default)  where TIn : class where TOut : class;
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="postName"></param>
        /// <param name="timeOut"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="contentType"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>

        Task<TOut> SendDeleteRequestMessageToServer<TOut>(string postName, int timeOut = 30, CookieContainer cookieContainer = null, string contentType = "application/json", CancellationToken cancellationToken = default)   where TOut : class;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="postName"></param>
        /// <param name="savePath"></param>
        /// <param name="timeOut"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="contentType"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> SendDownloadFileRequest(string postName, string savePath, int timeOut = 30, CookieContainer cookieContainer = null, string contentType = "application/json", CancellationToken cancellationToken = default);


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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> SendUploadFileRequest(string postName, string filePath, long fileMaxSize, int chunkSize = 1024 * 1024, int timeOut = 30, CookieContainer cookieContainer = null, string contentType = "application/octet-stream", CancellationToken cancellationToken = default);
    }
}
