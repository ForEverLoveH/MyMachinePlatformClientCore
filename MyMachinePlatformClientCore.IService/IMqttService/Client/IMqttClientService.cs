namespace MyMachinePlatformClientCore.IService.IMqttService;

public interface IMqttClientService
{
    void SetTopicName(string topicName);
    Task<bool> StartService();
    Task StopService();
    Task SendMessage(string message, string topicName = "");
}