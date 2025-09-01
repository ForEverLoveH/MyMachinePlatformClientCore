using Microsoft.Extensions.DependencyInjection;
using MyMachinePlatformClientCore.Rpc.Rpc;
using MyMachinePlatformClientCore.Rpc.Spliter;
using MyMachinePlatformClientCore.Rpc.Tcp;

namespace MyMachinePlatformClientCore.Rpc.Configurator;

 public interface IRpcRegistrationConfigurator
 {
     IPacketSpliter PacketSpliter { get; set; }
     bool AutoReconnect { get; set; }
     IRpcRegistrationConfigurator AddService(Type serviceType, Type implementationType);

 }
 internal class RpcOption
 {
     static RpcOption _instance;
     public static RpcOption Instance
     {
         get
         {
             if (_instance == null)
                 _instance = new RpcOption();
             return _instance;
         }
     }
     public IPacketSpliter PacketSpliter { get; set; } = new SimplePacketSpliter();
     public bool AutoReconnect { get; set; } = false;
 }
 public class RpcRegistrationConfigurator : IRpcRegistrationConfigurator
 {
     IServiceCollection _services;
     public RpcRegistrationConfigurator(IServiceCollection services)
     {
         _services = services;
     }

     public IPacketSpliter PacketSpliter
     {
         get => RpcOption.Instance.PacketSpliter;
         set => RpcOption.Instance.PacketSpliter = value;
     }
     public bool AutoReconnect
     {
         get => RpcOption.Instance.AutoReconnect;
         set => RpcOption.Instance.AutoReconnect = value;
     }

     public IRpcRegistrationConfigurator AddService(Type serviceType, Type implementationType)
     {
         return this;
     }
 }
 public interface ITcpRpcConfigurator
 {
     void UseClient(string ip, ushort port, Action<ITcpClientConfig> config = null);
     void UseServer(string ip, ushort port, Action<ITcpServerConfig> config = null);
     void UseLocalServer(ushort port, Action<ITcpServerConfig> config = null);
 }
 public static class RpcExtensions
 {
     public static IRpcRegistrationConfigurator AddService<TService, TImpl>(this IRpcRegistrationConfigurator config)
     {
         return config.AddService(typeof(TService), typeof(TImpl));
     }
     public static IServiceCollection AddVIARpc(this IServiceCollection service, Action<IRpcRegistrationConfigurator> config = null)
     {
         service.AddTransient<ITcpRpcServer, TcpRpcServer>();
         service.AddTransient<ITcpRpcClient, TcpRpcClient>();
         service.AddSingleton<IRpcServerExecutor, RpcServerExecutor>();
         service.AddTransient<IInvocationContextConverter, InvocationContextConverter>();
         return service;
     }
     public static IRpcRegistrationConfigurator UsingTcp(this IRpcRegistrationConfigurator config, Action<ITcpRpcConfigurator> tcpConfig)
     {
         return config;
     }
 }