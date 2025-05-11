namespace MyMachinePlatformClientCore.IService.IMqttService.Server;

public interface IMqttServerService
{
    Task StartService();
    Task StopService();
}