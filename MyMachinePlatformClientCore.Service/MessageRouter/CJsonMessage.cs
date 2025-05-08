using MyMachinePlatformClientCore.Service.MessageRouter.JsonMessageRouter;
using System;
using System.Reflection;

namespace MyMachinePlatformClientCore.Service.message_router;

public class CJsonMessage
{
    public  JsonMessage message { get; set; }
    /// <summary>
    /// 消息数据类型
    /// </summary>
    public  CMessageType messageType { get; set; }

}
/// <summary>
/// 
/// </summary>
public class JsonMessage
{
    public Req_Login req_Login { get; set; }

    public Req_Register req_Register { get; set; }

    public Req_HeartBeat req_HeartBeat { get; set; }
    /// <summary>
    /// 消息数据的回复
    /// </summary>
    public Rsp_Login rsp_Login { get; set; }

    public Rsp_Register rsp_Register { get; set; }

    public Rsp_HeartBeat rsp_HeartBeat { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static JsonMessage ConvertToJsonMessage<T>(T request) where T : class
    {
        JsonMessage jsonMessage = new JsonMessage();
        Type type = typeof(T);
        string name = type.Name;
        Type types = typeof(JsonMessage);
        var properties = type.GetProperties().ToList();
        var property = properties.Find(a=>a.PropertyType==type);
        if (property == null) return null;
        property.SetValue(jsonMessage, request);
        return jsonMessage;
        
        
    }
    
    
}
#region 请求
public class Req_HeartBeat
{
}

public class Req_Register
{
}

public class Req_Login
{
}
#endregion

#region  回复
    
public class Rsp_HeartBeat
{
}
/// <summary>
/// 注册的回复
/// </summary>
public class Rsp_Register
{
}
/// <summary>
/// 登录的回复
/// </summary>
public class Rsp_Login
{
}
#endregion