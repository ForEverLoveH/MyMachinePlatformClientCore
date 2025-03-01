using Newtonsoft.Json;

namespace MyMachinePlatformClientCore.Service.JsonService;

public class CJsonService
{

    public static  string SerializeObject<T>(T obj)  
    {
        return obj == null ? String.Empty : JsonConvert.SerializeObject(obj);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="json"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T DeserializeObject<T>(string json)  
    {
        return string.IsNullOrEmpty(json)? default : JsonConvert.DeserializeObject<T>(json);
    }

    public static T ReadJsonFileToObject  <T>(string filePath)  
    {
        if (!File.Exists(filePath))
        {
            return default;
        }
        string json = File.ReadAllText(filePath);
        return DeserializeObject<T>(json);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="filePath"></param>
    /// <typeparam name="T"></typeparam>
    public static void WriteObjectToJsonFile<T>(T obj, string filePath) where T : class
    {
        string json = SerializeObject(obj);
        File.WriteAllText(filePath, json);
    }

}