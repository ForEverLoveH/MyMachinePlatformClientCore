using System.Collections.Concurrent;
using System.Net;
using System.Text;
using MyMachinePlatformClientCore.Rpc.Exceptions;
using Newtonsoft.Json;

namespace MyMachinePlatformClientCore.Rpc.Rpc;

public interface IRpcServerExecutor: IDisposable
{
    Task<ReturnData> ExecuteAsync(InvocationData invocationData);
    void AddService<TInterface>(TInterface instance);
}
//
// 摘要:
//     The RPC service container that provides method to execute RPC calls.
public class RpcServerExecutor : IRpcServerExecutor
{
    private enum ServiceRegisterType
    {
        Instance,
        Func
    }

    private class ServiceRegisterInfo
    {
        public object Service { get; }

        public ServiceRegisterType Type { get; }

        public ServiceRegisterInfo(object service, ServiceRegisterType type)
        {
            Service = service;
            Type = type;
        }
    }

    private ConcurrentDictionary<Type, ServiceRegisterInfo> _serviceDict = new ConcurrentDictionary<Type, ServiceRegisterInfo>();

    //
    // 摘要:
    //     Gets or sets a service provider to create RPC call services.
    public IServiceProvider ServiceProvider { get; set; }



    //
    // 摘要:
    //     Add service to RPC server container.
    //
    // 参数:
    //   instance:
    //     The instance of the service that implement the TInterface.
    //
    // 类型参数:
    //   TInterface:
    //     The interface of service.
    public void AddService<TInterface>(TInterface instance)
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));
        Type typeFromHandle = typeof(TInterface);
        _serviceDict[typeFromHandle] = new ServiceRegisterInfo(instance, ServiceRegisterType.Instance);
    }

    //
    // 摘要:
    //     Add service to RPC server container.
    //
    // 参数:
    //   constructor:
    //     The func delegate used to create service instance.
    //
    // 类型参数:
    //   TInterface:
    //     The interface of service.
    public void AddService<TInterface>(Func<TInterface> constructor)
    {
        Type typeFromHandle = typeof(TInterface);
        _serviceDict[typeFromHandle] = new ServiceRegisterInfo(constructor, ServiceRegisterType.Func);
    }

    //
    // 摘要:
    //     When a transport layer error occurs, the user can call this method to directly
    //     create an error message.
    //
    // 参数:
    //   id:
    //     The id of the invocation.
    //
    //   statusCode:
    //     The status code in http codes.
    //
    //   message:
    //     A message describing the error.
    public static byte[] CreateErrorResponse(Guid id, HttpStatusCode statusCode, string message)
    {
        string s = JsonConvert.SerializeObject(new ReturnData<object>
        {
            Id = id,
            HttpStatusCode = (int)statusCode,
            ExceptionMessage = message
        }, RpcInvocationSerializerSettings.Default);
        return Encoding.UTF8.GetBytes(s);
    }

    //
    // 摘要:
    //     Execute an RPC call with serialized data.LZL
    //
    // 参数:
    //   data:
    public Task<ReturnData> ExecuteAsync(InvocationData invocationData)
    {
        if (invocationData==null)
        {
            return null;
        }

        return Task.Run(delegate
        {
            ReturnData returnData = new ReturnData<object>();
            Type type = typeof(ReturnData<>).MakeGenericType((invocationData.ReturnType == typeof(void)) ? typeof(object) : invocationData.ReturnType);
            returnData = (ReturnData)Activator.CreateInstance(type);
            returnData.Id = invocationData.Id;
            returnData.HttpStatusCode = 200;
            object obj2 = null;
            if (!_serviceDict.TryGetValue(invocationData.MethodDeclaringType, out var value))
            {
                if (ServiceProvider != null)
                {
                    try
                    {
                        obj2 = ServiceProvider.GetService(invocationData.MethodDeclaringType);
                    }
                    catch
                    {
                    }
                }

                if (obj2 == null)
                {
                    returnData.HttpStatusCode = 404;
                    returnData.ExceptionMessage = "The service is not found.";
                    goto IL_020f;
                }
            }

            try
            {
                if (obj2 == null)
                {
                    if (value.Type == ServiceRegisterType.Instance)
                    {
                        obj2 = value.Service;
                    }
                    else if (value.Type == ServiceRegisterType.Func)
                    {
                        obj2 = value.Service.GetType().GetMethod("Invoke").Invoke(value.Service, null);
                    }
                }

                object value2 = InvocationExecutor.Execute(obj2, invocationData);
                type.GetProperty("ReturnObject").SetValue(returnData, value2);
            }
            catch (ArgumentException ex)
            {
                returnData.HttpStatusCode = 400;
                returnData.ExceptionMessage = ex.Message;
            }
            catch (InvalidOperationException ex2)
            {
                returnData.HttpStatusCode = 400;
                returnData.ExceptionMessage = ex2.Message;
            }
            catch (RpcException ex3)
            {
                returnData.HttpStatusCode = ex3.Code;
                returnData.ExceptionMessage = ex3.Message;
            }
            catch (Exception ex4)
            {
                returnData.HttpStatusCode = 500;
                returnData.ExceptionMessage = ex4.Message;
            }

            goto IL_020f;
        IL_020f:
            return returnData;
        });
    }

    //
    // 摘要:
    //     Release all resources.
    public void Dispose()
    {
        _serviceDict.Clear();
    }
}