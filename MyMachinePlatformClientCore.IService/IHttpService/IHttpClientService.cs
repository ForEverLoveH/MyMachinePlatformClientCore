using System.Net;

namespace MyMachinePlatformClientCore.IService;

public interface IHttpClientService
{
    string Url { get; set; }

   
    /// <summary>
    /// 发送post请求
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="postName"></param>
    /// <param name="timeOut"></param>
    /// <param name="cookieContainer"></param>
    /// <param name="contentType"></param>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <returns></returns>
    Task<TOut> SendPostRequestMessageToServerAsync<TIn, TOut>(TIn obj, string postName, int timeOut = 30,
        CookieCollection cookieContainer = null, string contentType = "application/json",string header="jsonData=")
        where TIn : class where TOut : class;

    /// <summary>
    /// 发送get请求
    /// </summary>
    /// <param name="postName"></param>
    /// <param name="timeOut"></param>
    /// <param name="cookieContainer"></param>
    /// <param name="contentType"></param>
    /// <typeparam name="TOut"></typeparam>
    /// <returns></returns>
    Task<TOut> SendGetRequestMessageToServer<TOut>(string postName, int timeOut = 30,
        CookieCollection cookieContainer = null, string contentType = "application/json") where TOut : class;
    /// <summary>
    /// 发送put请求
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="postName"></param>
    /// <param name="timeOut"></param>
    /// <param name="cookieContainer"></param>
    /// <param name="contentType"></param>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <returns></returns>
    Task<TOut> SendPutRequestMessageToServer<TIn, TOut>(TIn obj, string postName, int timeOut = 30,
        CookieCollection cookieContainer = null, string contentType = "application/json")
        where TIn : class where TOut : class;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="postName"></param>
    /// <param name="timeOut"></param>
    /// <param name="cookieContainer"></param>
    /// <param name="contentType"></param>
    /// <param name="headerCode"></param>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <returns></returns>
    Task<TOut> SendPutRequestMessageToServer<TIn, TOut>(TIn obj, string postName, int timeOut = 30,
        CookieCollection cookieContainer = null, string contentType = "application/json",
        string headerCode = "jsonData=")
        where TIn : class where TOut : class;
    /// <summary>
    /// 发送delete请求
    /// </summary>
    /// <param name="postName"></param>
    /// <param name="timeOut"></param>
    /// <param name="cookieContainer"></param>
    /// <param name="contentType"></param>
    /// <typeparam name="TOut"></typeparam>
    /// <returns></returns>
    Task<TOut> SendDeleteRequestMessageToServer<TOut>(string postName, int timeOut = 30,
        CookieCollection cookieContainer = null, string contentType = "application/json")
        where TOut : class;

    /// <summary>
    /// 发送下载文件请求
    /// </summary>
    /// <param name="postName"></param>
    /// <param name="savePath"></param>
    /// <param name="timeOut"></param>
    /// <param name="cookieContainer"></param>
    /// <returns></returns>
    Task<bool> SendDownloadFileRequest(string postName, string savePath, int timeOut = 30,
        CookieCollection cookieContainer = null, string contentType = "application/json");
    /// <summary>
    /// 发送上传文件请求
    /// </summary>
    /// <param name="postName">请求名</param>
    /// <param name="filePath">本地文件路径</param>
    /// <param name="fileMaxSize">文件的最大阈值</param>
    /// <param name="chunkSize">分块的阈值</param>
    /// <param name="timeOut"></param>
    /// <param name="cookieContainer"></param>
    /// <param name="contentType"></param>
    /// <returns></returns>
    Task<bool> SendUploadFileRequest(string postName, string filePath, long fileMaxSize, int chunkSize = 1024 * 1024,
        int timeOut = 30,
        CookieCollection cookieContainer = null, string contentType = "application/octet-stream");
}