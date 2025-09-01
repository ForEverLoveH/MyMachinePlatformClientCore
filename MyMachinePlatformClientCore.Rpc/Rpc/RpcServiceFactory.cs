using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyMachinePlatformClientCore.Rpc.Rpc;

public interface IRpcServiceFactory : IServiceProvider
{

}
internal class RpcServiceFactory : IRpcServiceFactory
{
    IServiceProvider _service;
    public RpcServiceFactory(IServiceProvider service)
    {
        _service = service;
    }
    public object GetService(Type serviceType)
    {
        return _service.GetService(serviceType);
    }
}
public interface IInvocationContextConverter
{
    InvocationData GetInvocationData(string str);
    InvocationData GetInvocationData(byte[] data);
    //ReturnData GetReturnData(string str);
    //ReturnData GetReturnData(byte[] data);
    byte[] Serialize(ReturnData data);
}
internal class InvocationContextConverter:IInvocationContextConverter
{
    public InvocationContextConverter()
    {

    }
    public InvocationData GetInvocationData(byte[] data)
    {
       var str = Encoding.UTF8.GetString(data);
        return GetInvocationData(str);
    }
    public InvocationData GetInvocationData(string str)
    {
        InvocationData invocationData = JsonConvert.DeserializeObject<InvocationData>(str, RpcInvocationSerializerSettings.Default);
        DeserializeInvocationArgments(invocationData, str);
        return invocationData;
    }
    //public ReturnData GetReturnData(byte[] data)
    //{
    //    string str = Encoding.UTF8.GetString(data);
    //    return GetReturnData(str);
    //}
    //public ReturnData GetReturnData(string str)
    //{
    //    return JsonConvert.DeserializeObject<ReturnData>(str, RpcInvocationSerializerSettings.Default);
    //}

    public byte[] Serialize(ReturnData data)
    {
        string s = JsonConvert.SerializeObject(data, RpcInvocationSerializerSettings.Default);
        return Encoding.UTF8.GetBytes(s);
    }

    private void DeserializeInvocationArgments(InvocationData invocationData, string requestJson)
    {
        List<int> list = new List<int>();
        for (int i = 0; i < invocationData.Arguments.Length; i++)
        {
            if (invocationData.Arguments[i] != null && invocationData.Arguments[i].GetType() != invocationData.ArgumentTypes[i])
            {
                if (invocationData.Arguments[i].GetType().IsSubclassOf(typeof(JToken)))
                {
                    JToken jToken = (JToken)invocationData.Arguments[i];
                    invocationData.Arguments[i] = jToken.ToObject(invocationData.ArgumentTypes[i]);
                }
                else
                {
                    list.Add(i);
                }
            }
        }

        if (list.Count == 0)
        {
            return;
        }

        JArray jArray = (JArray)JObject.Parse(requestJson)["Arguments"];
        foreach (int item in list)
        {
            invocationData.Arguments[item] = jArray[item].ToObject(invocationData.ArgumentTypes[item]);
        }
    }
}