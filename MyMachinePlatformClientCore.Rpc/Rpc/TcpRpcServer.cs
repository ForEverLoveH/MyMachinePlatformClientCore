using System.Collections.Concurrent;
using MyMachinePlatformClientCore.Rpc.Common;
using MyMachinePlatformClientCore.Rpc.Configurator;
using MyMachinePlatformClientCore.Rpc.Tcp;

namespace MyMachinePlatformClientCore.Rpc.Rpc;

 public interface ITcpRpcServer : IRpcServer
 {
     void AddLocalService<T>(T instance);
 }
 public interface IRpcServer
 {
     //void Start();
     //void Stop();
     //void AddLocalService<TInterface>(TInterface instance);
     //void AddLocalService<TInterface>(Func<TInterface> constructor);

     // object GetRemoteServiceProxy(Type serviceType);
     TService GetFirstClientServiceProxy<TService>();
     TService GetRemoteServiceProxy<TService>(long clientId);
     void Start(int port);
     void Stop();
 }
 /// <summary>
 /// Represents the server in tcp RPC communication.
 /// </summary>
 public class TcpRpcServer : TcpServer, ITcpRpcServer
 {
     #region Fields

     IInvocationContextConverter _converter;
     private IRpcServerExecutor _rpcServerExecutor;

     #endregion
     class ServerRpcClient : IRpcTransfer
     {
         public TcpRpcServer _tcpServer;
         private ServiceProxyGenerator _generator;
         private readonly IInvocationContextConverter _converter;
         public ServerRpcClient(TcpRpcServer tcpServer, long clientId, IInvocationContextConverter converter)
         {
             _converter = converter;
             _tcpServer = tcpServer;
             ClientID = clientId;
             _generator = new ServiceProxyGenerator(this);
             foreach (Type clientServiceType in _tcpServer.ClientServiceTypes)
             {
                 _generator.RegisterServiceProxy(clientServiceType, null);
             }
         }

         public event EventHandler<RpcReturnDataEventArgs> RpcReturnDataReceived;
         public long ClientID { get; }
         public ServiceProxyGenerator ProxyGenerator => _generator;

         public void SendInvocation(SendInvocationContext context)
         {
             _tcpServer.SendMessage(ClientID, TcpRpcCommon.MakeRpcPacket(context.InvocationBytes, TcpRpcCommon.RpcRequest));
         }
         public void SetReturnData(byte[] data)
         {
             RpcReturnDataReceived?.Invoke(this, new RpcReturnDataEventArgs(data));
         }
     }
     #region Constructor


     /// <summary>
     /// Create an instance of TcpRpcServer() class.
     /// </summary>
     public TcpRpcServer(IRpcServerExecutor rpcServerExecutor, IInvocationContextConverter invocationContextConverter) : base(RpcOption.Instance.PacketSpliter)
     {
         _converter = invocationContextConverter;
         _rpcServerExecutor = rpcServerExecutor;
     }

     #endregion

     #region Private members

     //The service executor for RPC server.
     private HashSet<Type> _clientServiceTypes = new HashSet<Type>();
     private ConcurrentDictionary<long, ServerRpcClient> _rpcClients = new ConcurrentDictionary<long, ServerRpcClient>();

     #endregion

     #region Properties

     /// <summary>
     /// Gets client service types.
     /// </summary>
     public IReadOnlyCollection<Type> ClientServiceTypes => _clientServiceTypes;

     #endregion

     #region Private methods

     /// <summary>
     /// Override the function to pass the request data to rpc call handler.
     /// </summary>
     /// <param name="tcpRawMessageArgs">A request message from RPC client.</param>
     /// <returns>If true, message delivery to continue. Otherwise false.</returns>
     protected override bool ReceivedMessageFilter(MessageReceivedEventArgs tcpRawMessageArgs)
     {
         int flag = TcpRpcCommon.GetFlag(tcpRawMessageArgs.MessageRawData);
         byte[] content = TcpRpcCommon.GetRpcContent(tcpRawMessageArgs.MessageRawData);
         long clientId = tcpRawMessageArgs.ClientID;
         if (flag == TcpRpcCommon.RpcResponse)
         {
             if (_rpcClients.TryGetValue(clientId, out ServerRpcClient rpcClient))
             {
                 rpcClient.SetReturnData(content);
             }
             return true;
         }
         else if (flag == TcpRpcCommon.RpcRequest)
         {
             try
             {
                 var invocationData = _converter.GetInvocationData(content);
                 _rpcServerExecutor.ExecuteAsync(invocationData).ContinueWith(task =>
                 {
                     var data = _converter.Serialize(task.Result);
                     SendMessage(clientId, TcpRpcCommon.MakeRpcPacket(data, TcpRpcCommon.RpcResponse));
                 });
             }
             catch
             {

             }
             return true;
         }

         return false;
     }

     /// <summary>
     /// Override the function to add client call proxy.
     /// </summary>
     /// <param name="isInThread">whether the call is in thread or not.</param>
     /// <param name="args">The client status changed arguments.</param>
     /// <param name="clientGroups">The client group.</param>
     protected override void OnClientStatusChanged(bool isInThread, ClientStatusChangedEventArgs args, HashSet<string> clientGroups)
     {
         base.OnClientStatusChanged(isInThread, args, clientGroups);
         if (args.Status == ClientStatus.Connected)
         {
             _rpcClients.TryAdd(args.ClientID, new ServerRpcClient(this, args.ClientID, _converter));
         }
         else if (args.Status == ClientStatus.Connecting
               || args.Status == ClientStatus.Closed)
         {
             _rpcClients.TryRemove(args.ClientID, out _);
         }
     }

     #endregion

     #region Public methods

     /// <summary>
     /// Add service to RPC server container.
     /// </summary>
     /// <typeparam name="TInterface">The interface of service.</typeparam>
     /// <param name="instance">The instance of the service that implement the TInterface.</param>
     public void AddLocalService<TInterface>(TInterface instance)
     {
         _rpcServerExecutor.AddService<TInterface>(instance);
     }

     ///// <summary>
     ///// Add service to RPC server container.
     ///// </summary>
     ///// <typeparam name="TInterface">The interface of service.</typeparam>
     ///// <param name="constructor">The func delegate used to create service instance.</param>
     //public void AddLocalService<TInterface>(Func<TInterface> constructor)
     //{
     //    _rpcServerExecutor.AddService<TInterface>(constructor);
     //}

     /// <summary>
     /// Register the service proxy type for client side.
     /// </summary>
     /// <param name="serviceType">The service proxy type that will be called by user.</param>
     public void RegisterRemoteServiceProxy(Type serviceType)
     {
         _clientServiceTypes.Add(serviceType);
     }

     /// <summary>
     /// UnRegister the service proxy type for client side.
     /// </summary>
     /// <param name="serviceType">The service proxy type that will be called by user.</param>
     public void UnRegisterRemoteServiceProxy(Type serviceType)
     {
         _clientServiceTypes.Remove(serviceType);
     }

     /// <summary>
     /// Register the service proxy type for client side.
     /// </summary>
     /// <typeparam name="TService">The service proxy type that will be called by user.</typeparam>
     public void RegisterRemoteServiceProxy<TService>()
     {
         _clientServiceTypes.Add(typeof(TService));
     }

     /// <summary>
     /// UnRegister the service proxy type for client side.
     /// </summary>
     /// <typeparam name="TService">The service proxy type that will be called by user.</typeparam>
     public void UnRegisterRemoteServiceProxy<TService>()
     {
         _clientServiceTypes.Remove(typeof(TService));
     }

     /// <summary>
     /// Get the service proxy for client side.The user can use the service proxy to call RPC service.
     /// </summary>
     /// <typeparam name="TService">The service proxy type that will be called by user.</typeparam>
     /// <param name="clientId">The id of the connected client.</param>
     /// <returns>The instance of the service proxy.</returns>
     public TService GetRemoteServiceProxy<TService>(long clientId)
     {
         if (!_rpcClients.TryGetValue(clientId, out var serverRpcClient))
         {
             throw new Exception("The client is disconnected.");
         }
         return serverRpcClient.ProxyGenerator.GetServiceProxy<TService>();
     }

     /// <summary>
     /// Get the service proxy for client side.The user can use the service proxy to call RPC service.
     /// </summary>
     /// <param name="serviceType">The service proxy type that will be called by user.</param>
     /// <param name="clientId">The id of the connected client.</param>
     /// <returns>The instance of the service proxy.</returns>
     public object GetRemoteServiceProxy(long clientId, Type serviceType)
     {
         if (!_rpcClients.TryGetValue(clientId, out var serverRpcClient))
         {
             throw new Exception("The client is disconnected.");
         }
         return serverRpcClient.ProxyGenerator.GetServiceProxy(serviceType);
     }

     /// <summary>
     /// Get the all service proxy for clients side.
     /// </summary>
     /// <typeparam name="TService">The service proxy type that will be called by user.</typeparam>
     /// <returns>The instances of the service proxy.</returns>
     public List<TService> GetAllRemoteServiceProxy<TService>()
     {
         return _rpcClients.Values.Select(p => p.ProxyGenerator.GetServiceProxy<TService>()).ToList();
     }

     /// <summary>
     /// Get the service proxy for client side.The user can use the service proxy to call RPC service.
     /// </summary>
     /// <param name="serviceType">The service proxy type that will be called by user.</param>
     /// <returns>The instances of the service proxy.</returns>
     public List<object> GetAllRemoteServiceProxy(Type serviceType)
     {
         return _rpcClients.Values.Select(p => p.ProxyGenerator.GetServiceProxy(serviceType)).ToList();
     }

     /// <summary>
     /// Get the service proxy from the first client.The user can use the service proxy to call RPC service.
     /// </summary>
     /// <typeparam name="TService">The service proxy type that will be called by user.</typeparam>
     /// <returns>The instance of the service proxy.</returns>
     public TService GetFirstClientServiceProxy<TService>()
     {
         if (!_rpcClients.Any())
         {
             throw new Exception("The client is disconnected.");
         }
         return _rpcClients.First().Value.ProxyGenerator.GetServiceProxy<TService>();
     }

     /// <summary>
     /// Get the service proxy from the client.The user can use the service proxy to call RPC service.
     /// </summary>
     /// <param name="serviceType">The service proxy type that will be called by user.</param>
     /// <returns>The instance of the service proxy.</returns>
     public object GetFirstClientServiceProxy(Type serviceType)
     {
         if (!_rpcClients.Any())
         {
             throw new Exception("The client is disconnected.");
         }
         return _rpcClients.First().Value.ProxyGenerator.GetServiceProxy(serviceType);
     }


     #endregion

 }