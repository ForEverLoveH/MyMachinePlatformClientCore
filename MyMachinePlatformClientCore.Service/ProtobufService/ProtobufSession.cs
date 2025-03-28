using System.Reflection;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using MyMachinePlatformClientCore.Log.MyLogs;

namespace MyMachinePlatformClientCore.Service.ProtobufService;

 public   class ProtobufSession
 {
    /// <summary>
    /// 
    /// </summary>
     private static Dictionary<string, Type> _registry = new Dictionary<string, Type>();
    /// <summary>
    /// 
    /// </summary>
     private static Dictionary<int, Type> mDict1 = new Dictionary<int, Type>();
     private static Dictionary<Type, int> mDict2 = new Dictionary<Type, int>();

     static ProtobufSession()
     {
         List<string> list = new List<string>();
         var q = from t in Assembly.GetExecutingAssembly().GetTypes() select t;
         q.ToList().ForEach(t =>
         {
             if (typeof(IMessage).IsAssignableFrom(t))
             {
                 var desc = t.GetProperty("Descriptor").GetValue(t) as MessageDescriptor;
                 _registry.Add(desc.FullName, t);
                 list.Add(desc.FullName);
             }
         });

         list.Sort((x, y) =>
         {
             //按照字符串长度排序，
             if (x.Length != y.Length)
             {
                 return x.Length - y.Length;
             }
             //如果长度相同
             return string.Compare(x, y, StringComparison.Ordinal);
         });

         for (int i = 0; i < list.Count; i++)
         {
             var fname = list[i];
             var t = _registry[fname];
             MyLogTool.ColorLog(MyLogColor.Yellow,"Proto类型注册：{0} - {1}", i, fname);
             mDict1[i] = t;
             mDict2[t] = i;
         }
     }

     /// <summary>
     /// 序列化protobuf
     /// </summary>
     /// <param name="msg"></param>
     /// <returns></returns>
     public static byte[] Serialize<T>(T msg) where T:IMessage<T>
     {
         using (MemoryStream rawOutput = new MemoryStream())
         {
             msg.WriteTo(rawOutput);
             byte[] result = rawOutput.ToArray();
             return result;
         }
     }

     /// <summary>
     /// 解析
     /// </summary>
     /// <typeparam name="T"></typeparam>
     /// <param name="dataBytes"></param>
     /// <returns></returns>
     public static T Parse<T>(byte[] dataBytes) where T : IMessage, new()
     {
         T msg = new T();
         msg = (T)msg.Descriptor.Parser.ParseFrom(dataBytes);
         return msg;
     }
     /// <summary>
     /// 
     /// </summary>
     /// <param name="type"></param>
     /// <returns></returns>
     public static int SeqCode(Type type)
     {
         return mDict2[type];
     }

     /// <summary>
     /// 
     /// </summary>
     /// <param name="code"></param>
     /// <returns></returns>
     public static Type SeqType(int code)
     {
         return mDict1[code];
     }
     /// <summary>
     /// 
     /// </summary>
     /// <param name="byteArray"></param>
     /// <param name="type"></param>
     /// <returns></returns>
     
     public static IMessage DeserializeByteArray(byte[] byteArray, Type type)
     {
         if (type.IsClass && typeof(IMessage).IsAssignableFrom(type))
         {
             // 创建类型的实例
             IMessage message = (IMessage)Activator.CreateInstance(type);
             // 使用 Protocol Buffers 反序列化
             message.MergeFrom(byteArray);
             return message;
         }
         else
         {
             throw new ArgumentException("提供的类型不是类类型或未实现 IMessage 接口");
         }
     }
     /// <summary>
     /// 根据消息编码进行解析
     /// </summary>
     /// <param name="typeCode"></param>
     /// <param name="data"></param>
     /// <param name="offset"></param>
     /// <param name="len"></param>
     /// <returns></returns>
     public static IMessage ParseFrom(int typeCode, byte[] data, int offset, int len)
     {
         Type t =  SeqType(typeCode);
         MyLogTool.ColorLog(MyLogColor.Blue,"收到消息："+ t.FullName);
         var desc = t.GetProperty("Descriptor").GetValue(t) as MessageDescriptor;
         var msg = desc.Parser.ParseFrom(data, offset, len);
         MyLogTool.ColorLog(MyLogColor.Green,"解析消息：code={0} - {1}", typeCode, msg);
         return msg;
     }
 }