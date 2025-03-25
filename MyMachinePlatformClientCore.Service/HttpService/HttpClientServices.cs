using System.Diagnostics;
using System.Net;
using System.Text;
using MyMachinePlatformClientCore.Log.MyLogs;
using Newtonsoft.Json;

namespace MyMachinePlatformClientCore.Service.HttpService;

public class HttpClientServices
{
    private string url;

    public HttpClientServices(string url)
    {
        this.url = url;
    }

     
    /// <summary>
    /// 发送post请求
    /// </summary>
    /// <param name="tin"></param>
    /// <param name="postName"></param>
    /// <param name="timeOut"></param>
    /// <param name="cookieContainer"></param>
    /// <param name="contentType"></param>
    /// <param name="headers"></param>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    /// <returns></returns>
    public async Task<TOut> SendPostRequestToServer<TIn, TOut>(TIn tin, string postName, int timeOut = 30,
        CookieCollection cookieContainer = null, string contentType = "application/json",string headers="jsonData=")
        where TIn : class where TOut : class
    {
        url+=postName;
        string json = JsonConvert.SerializeObject(tin);
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                string result="";
                Stopwatch watch = new Stopwatch();
                watch.Start();
                MyLogTool.Log(DateTime.Now.ToString("yyyy-MM-DD HH:pp:ss")+$"正在通过{postName}接口往服务端发送{json}数据");
                using (HttpClient client = new HttpClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls |
                                                           SecurityProtocolType.Tls11 | SecurityProtocolType.Tls13;
                    client.Timeout = new TimeSpan(timeOut * 1000);
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
                    );
                    json=headers+json;
                   
                    if (cookieContainer != null)
                    {
                        foreach (Cookie cookie in cookieContainer)
                        {
                          client.DefaultRequestHeaders.TryAddWithoutValidation(cookie.Name, cookie.Value);
                        }
                        
                    }  
                    var content = new StringContent(json, Encoding.UTF8, contentType);
                    var response = await client.PostAsync(url, content);
                    if (response.IsSuccessStatusCode)
                        result = await response.Content.ReadAsStringAsync();
                }

                if (!string.IsNullOrEmpty(result))
                {
                    watch.Stop();
                    MyLogTool.Log(DateTime.Now.ToString("yyyy-MM-DD HH:pp:ss")+$"调用{postName}接口往服务端发送{json}数据成功,用时{watch.ElapsedMilliseconds}毫秒");
                    return JsonConvert.DeserializeObject<TOut>(result);
                    
                }
                else
                {
                    MyLogTool.ColorLog(MyLogColor.Red,$"调用{postName}接口往服务端发送{json}数据失败！！");
                    watch.Stop();
                    return default(TOut);
                }
            }
            catch (Exception e)
            {
                 MyLogTool.ColorLog(MyLogColor.Red,$"调用{postName}接口往服务端发送{json}数据出现异常！！");
                 return default(TOut);
            }
            
        }else return default(TOut);
    }
    
}