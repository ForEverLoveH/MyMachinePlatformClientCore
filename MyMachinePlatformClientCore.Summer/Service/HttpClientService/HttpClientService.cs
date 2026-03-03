using MyMachinePlatformClientCore.Log.MyLogs;
 
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Service 
{
    public class HttpClientService : IHttpClientService
    {
        private string url;
        private Action<LogMessage> LogMessageHandleCallBack;
        public string Url
        {
            set => url = value;
            get => url;
        }

        public HttpClientService(string url, Action<LogMessage> logDataHandleCallBack = null)
        {
            this.url = url;
            this.LogMessageHandleCallBack = logDataHandleCallBack;
        }



        /// <summary>
        /// 发送Post请求
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="postName"></param>
        /// <param name="timeOut"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="contentType"></param>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <returns></returns>
        public async Task<TOut> SendPostRequestMessageToServerAsync<TIn, TOut>(TIn obj, string postName, int timeOut = 30,
          CookieContainer cookieContainer = null, string contentType = "application/json",
       string headers = "jsonData=", CancellationToken cancellationToken = default)
         where TIn : class where TOut : class
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url) || obj == null) return default;

                url += postName;
                string json = JsonConvert.SerializeObject(obj);
                LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage( LogType.INFO, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} 请求服务器:{url}，请求内容为：{json}"));
                if (string.IsNullOrWhiteSpace(json))
                {
                    LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage( LogType.ERROR, $"发送post请求服务器:{url}出现错误,错误信息为：请求内容为空"));
                    return default;
                }

                json = headers + json;
                // 配置 HttpClientHandler
                using var handler = new HttpClientHandler
                {
                    CookieContainer = cookieContainer ?? new CookieContainer(),
                    SslProtocols = System.Security.Authentication.SslProtocols.Tls12
                };
                // 配置 HttpClient
                using var client = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(timeOut)
                };

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
                var content = new StringContent(json, Encoding.UTF8, contentType);
                // 重试机制
                const int maxRetries = 3;
                int retryCount = 0;

                while (retryCount < maxRetries)
                {
                    if (!NetworkInterface.GetIsNetworkAvailable())
                    {
                        await Task.Delay(1000, cancellationToken);
                        continue;
                    }

                    try
                    {
                        using var response = await client.PostAsync(url, content, cancellationToken);

                        if (response.IsSuccessStatusCode)
                        {
                            var result = await response.Content.ReadAsStringAsync();

                            if (!string.IsNullOrEmpty(result))
                            {
                                LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(  LogType.INFO, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} 服务器返回内容为：{result}"));
                                return JsonConvert.DeserializeObject<TOut>(result);
                            }
                            else
                            {
                                LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(  LogType.ERROR,  $"服务器返回内容为空，请求地址为：{url}"));
                                return default;
                            }
                        }
                        else
                        {
                            retryCount++;
                            LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(
                                LogType.WARN,
                                $"发送post请求服务器:{url}出现错误,错误状态码信息为：{response.StatusCode}"));

                            if (retryCount < maxRetries)
                            {
                                await Task.Delay(1000 * retryCount, cancellationToken);
                                continue;
                            }
                            return default;
                        }
                    }
                    catch (Exception e) when (e is not TaskCanceledException)
                    {
                        retryCount++;
                        LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(LogType.WARN, $"发送post请求服务器:{url}出现异常,异常信息为：{e.Message}"));
                        if (retryCount < maxRetries)
                        {
                            await Task.Delay(1000 * retryCount, cancellationToken);
                            continue;
                        }
                        return default;
                    }
                }
                LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage( LogType.ERROR,  $"发送post请求服务器:{url}失败，已达到最大重试次数"));
                return default;
            }
            catch (Exception e) when (e is not TaskCanceledException)
            {
                LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,  $"请求服务器:{url}出现异常,异常信息为：{e.Message}"));
                return default;
            }
        }


        /// <summary>
        /// 发送Get请求
        /// </summary>
        /// <param name="postName"></param>
        /// <param name="timeOut"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="contentType"></param>
        /// <typeparam name="TOut"></typeparam>
        /// <returns></returns>
        public async Task<TOut> SendGetRequestMessageToServer<TOut>(string postName, int timeOut = 30,
            CookieContainer cookieContainer = null, string contentType = "application/json",CancellationToken cancellation= default) where TOut : class
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url)) return default;
                url = url + postName;
                // 创建 HTTP 请求
                using (HttpClientHandler httpHandler = new HttpClientHandler())
                {
                    httpHandler.CookieContainer = cookieContainer;
                    httpHandler.AllowAutoRedirect = true;
                    httpHandler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                    using (HttpClient client = new HttpClient(httpHandler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(timeOut);
                        using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                        {
                            request.Headers.Accept.Clear();
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType)); ;
                            using (HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellation))
                            {
                                if (response.IsSuccessStatusCode)
                                {
                                    string result = await response.Content.ReadAsStringAsync();
                                    LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO, string.Format(DateTime.Now.ToString("yyyy-MM-dd HH:ss:pp") + "服务器返回内容为：{0}", result)));

                                    return JsonConvert.DeserializeObject<TOut>(result);
                                }
                                else
                                {
                                    LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(LogType.WARN, string.Format("发送get请求服务器:{0}出现错误,错误状态码信息为：{1}", url, response.StatusCode)));
                                    
                                    return default;
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception e)
            {
                LogMessageHandleCallBack?.Invoke(new LogMessage()
                {
                    _LogType = LogType.ERROR,
                    message = string.Format("请求服务器:{0}出现异常,异常信息为：{1}", url, e.Message)
                });

                return default;
            }
        }
        

        

        /// <summary>
        /// 发送Put请求
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
        public async Task<TOut> SendPutRequestMessageToServer<TIn, TOut>(TIn obj, string postName, int timeOut = 30,
           CookieContainer cookieContainer = null, string contentType = "application/json", string headerCode = "jsonData=", CancellationToken cancellationToken = default)
            where TIn : class where TOut : class
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url)) return default;
                url = url + postName;
                string json = JsonConvert.SerializeObject(obj);
                LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO, string.Format(DateTime.Now.ToString("yyyy-MM-dd HH:ss:pp") + "请求服务器:{0}，请求内容为：{1}", url, json)));
                if (string.IsNullOrWhiteSpace(json)) return default;
                using(var handler = new HttpClientHandler())
                {
                    handler.CookieContainer = cookieContainer;
                    handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                    handler.AllowAutoRedirect = true;
                    using (HttpClient client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(timeOut);
                        StringContent context= new StringContent(json, Encoding.UTF8, contentType);
                        using (var request = new HttpRequestMessage(HttpMethod.Put, url))
                        {
                            request.Content = context;
                            request.Headers.Accept.Clear();
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
                            using (HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                            {
                                if (response.IsSuccessStatusCode)
                                {
                                    string result = await response.Content.ReadAsStringAsync();
                                    LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                                        string.Format(DateTime.Now.ToString("yyyy-MM-dd HH:ss:pp") + "服务器返回内容为：{0}", result)));
                                    return JsonConvert.DeserializeObject<TOut>(result);
                                }
                                else
                                {
                                    LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(LogType.WARN,
                                        string.Format("发送put请求服务器:{0}出现错误,错误状态码信息为：{1}", url, response.StatusCode)));
                                    return default;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                    string.Format("请求服务器:{0}出现异常,异常信息为：{1}", url, e.Message)));
                  
                return default;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postName"></param>
        /// <param name="timeOut"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="contentType"></param>
        /// <typeparam name="TOut"></typeparam>
        /// <returns></returns>
        public async Task<TOut> SendDeleteRequestMessageToServer<TOut>(string postName, int timeOut = 30,
           CookieContainer cookieContainer = null, string contentType = "application/json", CancellationToken cancellationToken = default)
            where TOut : class
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url)) return default;
                url = url + postName;
                using (var handler = new HttpClientHandler())
                {
                    handler.CookieContainer = cookieContainer;
                    handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                    handler.AllowAutoRedirect = true;
                    using (HttpClient client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(timeOut);
                        using (var request = new HttpRequestMessage(HttpMethod.Delete, url))
                        {
                            request.Headers.Accept.Clear();
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
                            using (HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                            {
                                if (response.IsSuccessStatusCode)
                                {
                                    string result = await response.Content.ReadAsStringAsync();
                                    LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                                        string.Format(DateTime.Now.ToString("yyyy-MM-dd HH:ss:pp") + "服务器返回内容为：{0}", result)));
                                    return JsonConvert.DeserializeObject<TOut>(result);
                                }
                                else
                                {
                                    LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(LogType.WARN,
                                        string.Format("发送delete请求服务器:{0}出现错误,错误状态码信息为：{1}", url, response.StatusCode)));
                                    return default;
                                }
                            }
                        }

                    }


                }
            }
            catch (Exception e)
            {
                LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                    string.Format(string.Format("请求服务器:{0}出现异常,异常信息为：{1}", url, e.Message))));

                return null;
            }

        }

        /// <summary>
        /// 发送下载文件请求
        /// </summary>
        /// <param name="postName"></param>
        /// <param name="savePath"></param>
        /// <param name="timeOut"></param>
        /// <param name="cookieContainer"></param>
        /// <returns></returns>
        public async Task<bool> SendDownloadFileRequest(string postName, string savePath, int timeOut = 30,
           CookieContainer cookieContainer = null, string contentType = "application/json", CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url)) return false;
                url += postName;
                using(var handler = new HttpClientHandler())
                {
                    handler.CookieContainer = cookieContainer;
                    handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                    handler.AllowAutoRedirect = true;
                    using(HttpClient client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(timeOut);
                        using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                        {
                            request.Headers.Accept.Clear();
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
                            using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                            {
                                if(response.IsSuccessStatusCode)
                                {
                                    int bufferSize = 4096;
                                    byte[] buffer = new byte[bufferSize];
                                    using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                                    {
                                        using (var responseStream = await response.Content.ReadAsStreamAsync())
                                        {
                                            int bytesRead;
                                            while ((bytesRead = await responseStream.ReadAsync(buffer, 0, bufferSize, cancellationToken)) > 0)
                                            {
                                                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                                            }
                                        }
                                        LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} 文件下载成功，保存路径为：{savePath}"));
             
                                        return true;
                                    }
                                }else
                                {
                                    LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(LogType.WARN, string.Format("发送下载文件请求服务器:{0}出现错误,错误状态码信息为：{1}", url, response.StatusCode))); 
                                    return false;
                                }
                            }
                        }
                    }
                }
                 
                
            }
            catch (Exception e)
            {
                LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,string.Format("请求服务器:{0}出现异常,异常信息为：{1}", url, e.Message)));
                return default;
            }
        }

        /// <summary>
        /// 发送上传文件请求
        /// </summary>
        /// <param name="postName"></param>
        /// <param name="filePath">本地文件路径</param>
        /// <param name="timeOut">超时</param>
        /// <param name="cookieContainer"></param>
        /// <param name="contentType"></param>
        /// <PARAM name="fileMaxSize">文件最大大小</PARAM>
        /// <returns></returns>
        public async Task<bool> SendUploadFileRequest(string postName, string filePath, long fileMaxSize,
            int chunkSize = 1024 * 1024, int timeOut = 30,
           CookieContainer cookieContainer = null, string contentType = "application/octet-stream", CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(url) || !File.Exists(filePath)) return false;
            url += postName;
            try
            {
                FileInfo info = new FileInfo(filePath);
                if (info.Length > fileMaxSize)
                {
                    return await UploadFileInChunks(filePath, chunkSize, timeOut, cookieContainer, contentType,cancellationToken);
                }
                else
                {
                    // 文件小于最大大小，直接上传
                    return await UploadWholeFile(filePath, timeOut, cookieContainer, contentType,cancellationToken);
                }
            }
            catch (Exception e)
            {
                LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, string.Format("请求服务器:{0}出现异常, 异常信息为：{1}", url, e.Message)));
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="timeOut"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        private async Task<bool> UploadWholeFile(string filePath, int timeOut, CookieContainer cookieContainer,
            string contentType, CancellationToken cancellationToken = default)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            return await SendFileChunk(url, fileBytes, 0, 1, timeOut, cookieContainer, contentType,cancellationToken);
        }

        /// <summary>
        /// 分块上传
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="chunkSize"></param>
        /// <param name="timeOut"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        private async Task<bool> UploadFileInChunks(string filePath, int chunkSize, int timeOut,
           CookieContainer cookieContainer, string contentType, CancellationToken cancellationToken = default)
        {
            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    long fileLength = fileStream.Length;
                    long totalChunks = (fileLength + chunkSize - 1) / chunkSize;
                    long currentChunk = 0;
                    byte[] buffer = new byte[chunkSize];
                    while (currentChunk < totalChunks)
                    {
                        while (!IsNetworkAvailable())
                        {
                            await Task.Delay(1000);
                        }

                        int bytesRead = await fileStream.ReadAsync(buffer, 0, chunkSize);
                        if (bytesRead > 0)
                        {
                            try
                            {
                                bool success = await SendFileChunk(url, buffer, currentChunk, totalChunks, timeOut,
                                    cookieContainer, contentType);
                                if (success) currentChunk++;
                                else break; // 上传失败终止所有上传
                            }
                            catch (Exception e)
                            {
                               LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(LogType.WARN, string.Format("上传块 {0} 时出现异常, 异常信息为：{1}", currentChunk, e.Message)));

                                return false;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }

                }

                LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                    string.Format(DateTime.Now.ToString("yyyy-MM-dd HH:ss:pp") + "文件上传成功，文件路径为：{0}", filePath)));

                return true;
            }
            catch (Exception e)
            {
                LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, string.Format("请求服务器:{0}出现异常, 异常信息为：{1}", url, e.Message)));

                return false;
            }
        }

        /// <summary>
        /// 发送单个文件块
        /// </summary>
        /// <param name="url">请求的URL</param>
        /// <param name="buffer">文件块数据</param>
        /// <param name="currentChunk">当前块的索引</param>
        /// <param name="totalChunks">总块数</param>
        /// <param name="timeOut">超时时间</param>
        /// <param name="cookieContainer">Cookie集合</param>
        /// <param name="contentType">内容类型</param>
        /// <returns>是否发送成功</returns>
        private async Task<bool> SendFileChunk(string url, byte[] buffer, long currentChunk, long totalChunks, int timeOut,
           CookieContainer cookieContainer, string contentType,CancellationToken cancellationToken = default)
        {
            try
            {
                using(var handler  = new HttpClientHandler())
                {
                    handler.CookieContainer = cookieContainer;
                    handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                    handler.AllowAutoRedirect = true;
                    using (HttpClient client = new HttpClient(handler))
                    {
                        client.Timeout = TimeSpan.FromSeconds(timeOut);
                        using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                        {
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            request.Headers.Add("X-Chunk-Index", currentChunk.ToString());
                            request.Headers.Add("X-Total-Chunks", totalChunks.ToString());

                            request.Content = new ByteArrayContent(buffer);
                            request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                            request.Content.Headers.ContentLength = buffer.Length;
                            using (HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                            {
                                if (response.IsSuccessStatusCode)
                                {
                                    return true;
                                }
                                else
                                {
                                    LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(LogType.WARN, string.Format("发送文件块 {0} 请求服务器:{1}出现错误,错误状态码信息为：{2}", currentChunk, url, response.StatusCode)));
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessageHandleCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, string.Format("发送文件块 {0} 请求服务器:{1}出现异常,异常信息为：{2}", currentChunk, url, ex.Message)));
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool IsNetworkAvailable()
        {
            return NetworkInterface.GetIsNetworkAvailable();
        }
    }
}
