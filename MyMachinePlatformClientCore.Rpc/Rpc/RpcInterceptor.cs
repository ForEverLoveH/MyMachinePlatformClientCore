using System.Collections.Concurrent;
using System.Net;
using System.Reflection;
using System.Text;
using Castle.DynamicProxy;
using MyMachinePlatformClientCore.Rpc.Exceptions;
using Newtonsoft.Json;

namespace MyMachinePlatformClientCore.Rpc.Rpc;

internal class RpcInterceptor : IInterceptor, IDisposable
{
    private int _rpcTimeout;

    private IRpcTransfer _rpcTransfer;

    private object _serviceToken;

    private ConcurrentDictionary<Guid, RequestResponseTask> _requestTaskDict = new ConcurrentDictionary<Guid, RequestResponseTask>();

    internal RpcInterceptor(IRpcTransfer rpcTransfer, int rpcTimeout, object serviceToken)
    {
        _serviceToken = serviceToken;
        _rpcTimeout = rpcTimeout;
        _rpcTransfer = rpcTransfer;
        _rpcTransfer.RpcReturnDataReceived += _rpcTransfer_RpcReturnDataReceived;
    }

    private void _rpcTransfer_RpcReturnDataReceived(object sender, RpcReturnDataEventArgs e)
    {
        if (e.Data == null)
        {
            return;
        }
        var str = Encoding.UTF8.GetString(e.Data);
        ReturnData returnData = JsonConvert.DeserializeObject<ReturnData>(str);
        Guid id = returnData.Id;
        if ((returnData.Id == Guid.Empty)||!_requestTaskDict.TryGetValue(id, out var value))
        {
            return;
        }

        Type type = null;
        if (returnData.HttpStatusCode == 200)
        {
            type = typeof(ReturnData<>).MakeGenericType((value.ReturnType == typeof(void)) ? typeof(object) : value.ReturnType);
            returnData = (ReturnData)JsonConvert.DeserializeObject(str, type, RpcInvocationSerializerSettings.Default);
        }

        try
        {
            HttpStatusCode httpStatusCode2 = (value.StatusCode = (HttpStatusCode)returnData.HttpStatusCode);
            switch (httpStatusCode2)
            {
                case HttpStatusCode.BadRequest:
                case HttpStatusCode.NotFound:
                case HttpStatusCode.InternalServerError:
                    value.ExceptionMessage = returnData.ExceptionMessage;
                    break;
                case HttpStatusCode.OK:
                    {
                        PropertyInfo property = type.GetProperty("ReturnObject");
                        value.Result = property.GetValue(returnData);
                        break;
                    }
                default:
                    value.ExceptionMessage = httpStatusCode2.ToString();
                    break;
            }
        }
        catch (Exception ex)
        {
            value.ExceptionMessage = ex.Message;
        }

        value.WaitEvent.Set();
    }

    private InvocationData Convert(IInvocation invocation)
    {
        return new InvocationData
        {
            Id = Guid.NewGuid(),
            ReturnType = invocation.Method.ReturnType,
            MethodDeclaringType = invocation.Method.DeclaringType,
            MethodName = invocation.Method.Name,
            GenericArguments = (invocation.GenericArguments ?? new Type[0]),
            Arguments = invocation.Arguments,
            ArgumentTypes = (from p in invocation.Method.GetParameters()
                             select p.ParameterType).ToArray()
        };
    }

    public void Intercept(IInvocation invocation)
    {
        InvocationData invocationData = Convert(invocation);
        Guid id = invocationData.Id;
        string s = JsonConvert.SerializeObject(invocationData, RpcInvocationSerializerSettings.Default);
        RequestResponseTask requestResponseTask = new RequestResponseTask(id, invocation.Method.ReturnType);
        _requestTaskDict[id] = requestResponseTask;
        byte[] bytes = Encoding.UTF8.GetBytes(s);
        _rpcTransfer.SendInvocation(new SendInvocationContext(id, bytes, _serviceToken));
        RequestResponseTask value;
        if (!requestResponseTask.WaitEvent.WaitOne(_rpcTimeout))
        {
            _requestTaskDict.TryRemove(id, out value);
            requestResponseTask.Dispose();
            throw new RpcTimeoutException();
        }

        _requestTaskDict.TryRemove(id, out value);
        if (requestResponseTask.StatusCode != HttpStatusCode.OK)
        {
            throw new RpcException((int)requestResponseTask.StatusCode, requestResponseTask.ExceptionMessage);
        }

        invocation.ReturnValue = requestResponseTask.Result;
        requestResponseTask.Dispose();
    }

    public void Dispose()
    {
        _rpcTransfer.RpcReturnDataReceived -= _rpcTransfer_RpcReturnDataReceived;
    }
}