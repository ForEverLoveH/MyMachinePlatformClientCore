namespace MyMachinePlatformClientCore.IService.IRedisService;

public interface IRedisService
{
     void StartService();
     bool SetString(string key, string value, TimeSpan? timeSpan = null);
     string GetString(string key);

     long ListRightPush(string key, string value);
     long ListLeftPush(string key, string value);
     string ListLeftPop(string key);
     string ListRightPop(string key);
}