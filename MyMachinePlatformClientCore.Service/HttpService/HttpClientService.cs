using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using MyMachinePlatformClientCore.IService;
using MyMachinePlatformClientCore.Log.MyLogs;
using MyMachinePlatformClientCore.Service.LogService;
namespace MyMachinePlatformClientCore.Service.HttpService;

/// <summary>
/// HTTP客户端服务
/// </summary>
public class HttpClientService : IHttpClientService
{
    private string url;
    private    Action<LogMessage >LogMessageHandleCallBack;
    public string Url
    {
        set => url = value;
        get => url;
    }

    public HttpClientService(string url,Action<LogMessage> logDataHandleCallBack=null)
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
        CookieCollection cookieContainer = null, string contentType = "application/json",string headers = "jsonData=")
        where TIn : class where TOut : class
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url)) return default;
            url += postName;
            string json = JsonService.CJsonService.SerializeObject<TIn>(obj);
            LogMessageHandleCallBack?.Invoke(new LogMessage()
            {
                message =
                    string.Format(DateTime.Now.ToString("yyyy-MM-dd HH:ss:pp") + "请求服务器:{0}，请求内容为：{1}", url, json),
                _LogType = LogType.INFO
            });
            
            string result = "";
            if (!string.IsNullOrWhiteSpace(json))
            {
                json = headers + json;
                byte[] byteArray = Encoding.UTF8.GetBytes(json);
                int byteLength = byteArray.Length;
                int sendBytes = 0;
                while (sendBytes < byteLength)
                {
                    while (!IsNetworkAvailable())
                    {
                        await Task.Delay(1000);
                    }

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls |
                                                           SecurityProtocolType.Tls11 | SecurityProtocolType.Tls13;
                    request.Method = "POST";
                   
                    request.ContentType = contentType;
                    request.Timeout = timeOut * 1000;
                    request.Accept = "application/json";
                    if (cookieContainer != null)
                    {
                        request.CookieContainer = new CookieContainer();
                        request.CookieContainer.Add(cookieContainer);
                    }
                    // 设置需要发送的数据长度
                    request.ContentLength = byteLength - sendBytes;
                    using (Stream requestStream = await request.GetRequestStreamAsync())
                    {
                        await requestStream.WriteAsync(byteArray, sendBytes, byteArray.Length - sendBytes);
                    }

                    using (HttpWebResponse response = (HttpWebResponse) await  request.GetResponseAsync())
                    {
                        if (response != null && response.StatusCode == HttpStatusCode.OK)
                        {
                            using (Stream responseStream = response.GetResponseStream())
                            {

                                using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8))
                                {
                                    result = await reader.ReadToEndAsync();
                                    LogMessageHandleCallBack?.Invoke(new LogMessage()
                                    {
                                        _LogType = LogType.INFO,
                                        message =string.Format(DateTime.Now.ToString("yyyy-MM-dd HH:ss:pp") + "服务器返回内容为：{0}",
                                        result),
                                    });
                                }
                            }
                        }
                        else
                        {
                            LogMessageHandleCallBack?.Invoke(new LogMessage()
                            {
                                _LogType = LogType.WARN,
                                message =string.Format("发送post请求服务器:{0}出现错误,错误状态码信息为：{1}", url, response.StatusCode)
                            });
                             
                            return default;
                        }

                    }

                }
            }

            if (!string.IsNullOrEmpty(result))
            {
                return JsonService.CJsonService.DeserializeObject<TOut>(result);
            }else { return default; }

            
        }
        catch (Exception e)
        {
            LogMessageHandleCallBack?.Invoke(new LogMessage()
            {
                _LogType = LogType.ERROR,
                message =string.Format("请求服务器:{0}出现异常,异常信息为：{1}", url, e.Message)
            });
            
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
        CookieCollection cookieContainer = null, string contentType = "application/json") where TOut : class
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url)) return default;
            url = url + postName;
            // 创建 HTTP 请求
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls |
                                                   SecurityProtocolType.Tls11 | SecurityProtocolType.Tls13;
            // 设置请求方法为 GET
            request.Method = "GET";
            
             
            // 设置请求的内容类型
            request.ContentType = contentType;
            // 设置请求超时时间
            request.Timeout = timeOut * 1000;
            // 设置接受的响应类型
            request.Accept = "application/json";
            // 如果有 Cookie 集合，添加到请求中
            if (cookieContainer != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookieContainer);
            }
            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            string result = await reader.ReadToEndAsync();
                             LogMessageHandleCallBack?.Invoke(new LogMessage()
                             {
                                 _LogType = LogType.INFO,
                                 message = string.Format(
                                     DateTime.Now.ToString("yyyy-MM-dd HH:ss:pp") + $"往服务器{url}发送get请求" + "返回内容为：{0}",
                                     result)
                             });
                          
                            return JsonService.CJsonService.DeserializeObject<TOut>(result);
                        }
                        else
                        {
                            LogMessageHandleCallBack?.Invoke(new LogMessage()
                            {
                                _LogType = LogType.WARN,
                                message = string.Format("发送get请求服务器:{0}出现错误,错误状态码信息为：{1}", url, response.StatusCode)
                            });
                           
                            return default;
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
    /// 
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
        CookieCollection cookieContainer = null, string contentType = "application/json") where TIn : class where TOut : class
    {
         return  await SendPutRequestMessageToServer<TIn,TOut>(obj, postName, timeOut, cookieContainer, contentType,"");
    }


    private void SetCurrentLogMessage(string message, LogType logType)
    {
        LogMessage messages = new LogMessage()
        {
            message = message,
            _LogType = logType
        };
        LogMessageHandleCallBack?.Invoke(messages);
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
        CookieCollection cookieContainer = null, string contentType = "application/json",string headerCode="jsonData=")
        where TIn : class where TOut : class
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url)) return default;
            url = url + postName;
            string json = JsonService.CJsonService.SerializeObject<TIn>(obj);
            SetCurrentLogMessage(
                string.Format(DateTime.Now.ToString("yyyy-MM-dd HH:ss:pp") + "请求服务器:{0}，请求内容为：{1}", url, json),
                LogType.INFO);
            if(string.IsNullOrWhiteSpace(json))return default;
            
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls |
                                                   SecurityProtocolType.Tls11 | SecurityProtocolType.Tls13;
            request.Method = "PUT";
             
            request.ContentType = contentType;
            request.Timeout = timeOut * 1000;
            request.Accept = "application/json";
            if (cookieContainer != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookieContainer);
            }
            json= headerCode + json;
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            request.ContentLength = bytes.Length;
            using (Stream requestStream = await request.GetRequestStreamAsync())
            {
                await requestStream.WriteAsync(bytes, 0, bytes.Length);
            }
            String result = "";
            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            result = await reader.ReadToEndAsync();
                            SetCurrentLogMessage(
                                string.Format(DateTime.Now.ToString("yyyy-MM-dd HH:ss:pp") + "服务器返回内容为：{0}", result),
                                LogType.INFO);


                        }
                        else
                        {
                            SetCurrentLogMessage(
                                string.Format("发送put请求服务器:{0}出现错误,错误状态码信息为：{1}", url, response.StatusCode),
                                LogType.WARN);
                            
                            return default;
                        }
                    }
                }

            }
            if (!String.IsNullOrEmpty(result))
            {
                return JsonService.CJsonService.DeserializeObject<TOut>(result);
            }
            else
            {
                return default(TOut);
            }
        }
        catch (Exception e)
        {
            SetCurrentLogMessage(
                string.Format("请求服务器:{0}出现异常,异常信息为：{1}", url, e.Message),
                LogType.ERROR);
             
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
        CookieCollection cookieContainer = null, string contentType = "application/json")
        where TOut : class
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url)) return default;
            url = url + postName;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls |
                                                   SecurityProtocolType.Tls11 | SecurityProtocolType.Tls13;
            request.Method = "DELETE";
            
            request.ContentType = contentType;
            request.Timeout = timeOut * 1000;
            request.Accept = "application/json";
            if (cookieContainer != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookieContainer);
            }

            using (HttpWebResponse webResponse = (HttpWebResponse)await request.GetResponseAsync())
            {
                using (StreamReader reader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
                {
                    if (webResponse.StatusCode == HttpStatusCode.OK)
                    {
                        string result = await reader.ReadToEndAsync();
                        SetCurrentLogMessage(
                            string.Format(DateTime.Now.ToString("yyyy-MM-dd HH:ss:pp") + "服务器返回内容为：{0}", result),
                            LogType.INFO);
                       
                        return JsonService.CJsonService.DeserializeObject<TOut>(result);
                    }
                    else
                    {
                        SetCurrentLogMessage(
                            string.Format("发送delete请求服务器:{0}出现错误,错误状态码信息为：{1}", url, webResponse.StatusCode),
                            LogType.WARN);

                        
                        return default;
                    }
                }
            }
        }
        catch (Exception e)
        {
            SetCurrentLogMessage(
                string.Format(string.Format("请求服务器:{0}出现异常,异常信息为：{1}", url, e.Message)),
                LogType.ERROR);
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
        CookieCollection cookieContainer = null, string contentType = "application/json")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            url += postName;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls |
                                                   SecurityProtocolType.Tls11 | SecurityProtocolType.Tls13;
            request.Method = "GET";
            
            request.Timeout = timeOut * 1000;
            request.Accept = "*/*";
            request.ContentType = contentType;
            if (cookieContainer != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookieContainer);
            }

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    long contentLength = response.ContentLength;
                    const int bufferSize = 4096;
                    byte[] buffer = new byte[bufferSize];

                    using (FileStream fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                    {
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            int bytesRead;
                            // 循环读取响应流并写入文件流
                            while ((bytesRead = await responseStream.ReadAsync(buffer, 0, bufferSize)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                            }
                            
                        }
                        //组包
                        SetCurrentLogMessage(string.Format(DateTime.Now.ToString("yyyy-MM-dd HH:ss:pp") + "文件下载成功，保存路径为：{0}", savePath),LogType.INFO);
                        return true;
                    }
                }
                else
                {
                    SetCurrentLogMessage(string.Format(
                        DateTime.Now.ToString("yyyy-MM-dd HH:ss:pp") + "文件失败" + "，错误状态码信息为：{0}",
                        response.StatusCode.ToString()), LogType.ERROR);
                    
                    return false;

                }

            }
        }
        catch (Exception e)
        {
            SetCurrentLogMessage(string.Format("请求服务器:{0}出现异常,异常信息为：{1}", url, e.Message), LogType.ERROR);
             
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
        CookieCollection cookieContainer = null, string contentType = "application/octet-stream")
    {
        if (!string.IsNullOrWhiteSpace(url) || !File.Exists(filePath)) return false;
        url += postName;
        try
        {
            FileInfo info = new FileInfo(filePath);
            if (info.Length > fileMaxSize)
            {
                return await UploadFileInChunks(filePath, chunkSize, timeOut, cookieContainer, contentType);
            }
            else
            {
                // 文件小于最大大小，直接上传
                return await UploadWholeFile(filePath, timeOut, cookieContainer, contentType);
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
    private async Task<bool> UploadWholeFile(string filePath, int timeOut, CookieCollection cookieContainer,
        string contentType)
    {
        byte[] fileBytes = File.ReadAllBytes(filePath);
        return await SendFileChunk(url, fileBytes, 0, 1, timeOut, cookieContainer, contentType);
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
        CookieCollection cookieContainer, string contentType)
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
                            SetCurrentLogMessage(string.Format("上传块 {0} 时出现异常, 异常信息为：{1}", currentChunk, e.Message),LogType.WARN);
                            
                            return false;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

            }

            SetCurrentLogMessage(
                string.Format(DateTime.Now.ToString("yyyy-MM-dd HH:ss:pp") + "文件上传成功，文件路径为：{0}", filePath),
                LogType.INFO);
            
            return true;
        }
        catch (Exception e)
        {
            SetCurrentLogMessage(string.Format("请求服务器:{0}出现异常, 异常信息为：{1}", url, e.Message), LogType.ERROR);
            
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
        CookieCollection cookieContainer, string contentType)
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls |
                                               SecurityProtocolType.Tls11 | SecurityProtocolType.Tls13;
        request.Method = "POST";
        if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            request.ProtocolVersion = HttpVersion.Version10;
        request.ContentType = contentType;
        request.Timeout = timeOut * 1000;
        request.Accept = "application/json";
        if (cookieContainer != null)
        {
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(cookieContainer);
        }
        // 添加自定义头部信息，用于标识当前块和总块数
        request.Headers.Add("X-Chunk-Index", currentChunk.ToString());
        request.Headers.Add("X-Total-Chunks", totalChunks.ToString());
        request.ContentLength = buffer.Length;
        using (Stream requestStream = await request.GetRequestStreamAsync())
        {
            await requestStream.WriteAsync(buffer, 0, buffer.Length);
        }
        using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
        {
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                SetCurrentLogMessage(string.Format("发送文件块 {0} 到服务器:{1} 出现错误, 错误状态码信息为：{2}", currentChunk, url, response.StatusCode),LogType.WARN);
               
                return false;
            }
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
        
            
        
        
