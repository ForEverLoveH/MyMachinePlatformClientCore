using System.IO.Compression;

namespace MyMachinePlatformClientCore.Log.MyLogs;

public enum MyLogColor
{
    Red, Green, Blue ,Cyan, Magentna, Yellow, None,
}

 #region 日志功能
 public partial class MyLogTool
 {
     #region 设置日志
     internal static bool IsDisableLog = false;
     private static Action<string> _logFunc;
     private static Action<MyLogColor, string> _colorLogFunc;
     private static Action<string> _warnFunc;
     private static Action<string> _errorFunc;

     public static void SetupLogFunc(Action<string> logFunc)
     {
        _logFunc = logFunc;
     }
     public static void SetupColorLogFunc(Action<MyLogColor, string> colorLogFunc)
     {
        _colorLogFunc = colorLogFunc;
     }
     public static void SetupWarnFunc(Action<string> warnFunc)
     {
         _warnFunc = warnFunc;
     }
     public static void SetupErrorFunc(Action<string> errorFunc)
     {
        _errorFunc = errorFunc;
     }
     #endregion

     #region 外界调用打印
     public static void Log(string message, params object[] args)
     {
         if (IsDisableLog) { return; }
         message = string.Format(message, args);
         if (_logFunc != null)
         {
             _logFunc(message);
         }
         else
         {
             ConsoleLog(message, MyLogColor.None);
         }
     }
     public static void ColorLog(MyLogColor color, string message, params object[] args)
     {
         if (IsDisableLog) { return; }
         message = string.Format(message, args);
         if (_colorLogFunc != null)
         {
             _colorLogFunc(color, message);
         }
         else
         {
             ConsoleLog(message, color);
         }
     }
     public static void Warn(string message, params object[] args)
     {
         if (IsDisableLog) { return; }
         message = string.Format(message, args);
         if (_warnFunc != null)
         {
             _warnFunc(message);
         }
         else
         {
             ConsoleLog(message, MyLogColor.Yellow);
         }
     }
     public static void Error(string message, params object[] args)
     {
         if (IsDisableLog) { return; }
         message = string.Format(message, args);
         if (_errorFunc != null)
         {
             _errorFunc(message);
         }
         else
         {
             ConsoleLog(message, MyLogColor.Red);
         }
     }
     private static void ConsoleLog(string message, MyLogColor color)
     {
         if (IsDisableLog) { return; }
         message = string.Format("# YKCPNetThreadID {0} => {1}", Thread.CurrentThread.ManagedThreadId, message);
         switch (color)
         {
             case MyLogColor.Red:
                 Console.ForegroundColor = ConsoleColor.DarkRed;
                 Console.WriteLine(message);
                 Console.ForegroundColor = ConsoleColor.DarkGray;
                 break;
             case MyLogColor.Green:
                 Console.ForegroundColor = ConsoleColor.DarkGreen;
                 Console.WriteLine(message);
                 Console.ForegroundColor = ConsoleColor.DarkGray;
                 break;
             case MyLogColor.Blue:
                 Console.ForegroundColor = ConsoleColor.DarkBlue;
                 Console.WriteLine(message);
                 Console.ForegroundColor = ConsoleColor.DarkGray;
                 break;
             case MyLogColor.Cyan:
                 Console.ForegroundColor = ConsoleColor.DarkCyan;
                 Console.WriteLine(message);
                 Console.ForegroundColor = ConsoleColor.DarkGray;
                 break;
             case MyLogColor.Magentna:
                 Console.ForegroundColor = ConsoleColor.DarkMagenta;
                 Console.WriteLine(message);
                 Console.ForegroundColor = ConsoleColor.DarkGray;
                 break;
             case MyLogColor.Yellow:
                 Console.ForegroundColor = ConsoleColor.DarkYellow;
                 Console.WriteLine(message);
                 Console.ForegroundColor = ConsoleColor.DarkGray;
                 break;
             case MyLogColor.None:
             default:
                 Console.ForegroundColor = ConsoleColor.DarkGray;
                 Console.WriteLine(message);
                 break;
         }
     }
     #endregion
 }
 #endregion
 #region 二进制的压缩与解压缩
 public partial class MyLogTool
 {
     public static byte[] Compress(byte[] input)
     {
         using (MemoryStream outMS = new MemoryStream())
         {
             using (GZipStream gzs = new GZipStream(outMS, CompressionMode.Compress, true))
             {
                 gzs.Write(input, 0, input.Length);
                 gzs.Close();
                 return outMS.ToArray();
             }
         }
     }
     public static byte[] DeCompress(byte[] input)
     {
         using (MemoryStream inputMS = new MemoryStream(input))
         {
             using (MemoryStream outMs = new MemoryStream())
             {
                 using (GZipStream gzs = new GZipStream(inputMS, CompressionMode.Decompress))
                 {
                     byte[] bytes = new byte[1024];
                     int len = 0;
                     while ((len = gzs.Read(bytes, 0, bytes.Length)) > 0)
                     {
                         outMs.Write(bytes, 0, len);
                     }
                     gzs.Close();
                     return outMs.ToArray();
                 }
             }
         }
     }
 }
 #endregion
 #region UTC时间
 public partial class MyLogTool
 {
     internal static readonly DateTime utcStart = new DateTime(1970, 1, 1);
     public static ulong GetUTCStartMilliseconds()
     {
         TimeSpan ts = DateTime.UtcNow - utcStart;
         return (ulong)ts.TotalMilliseconds;
     }
 }
 #endregion
 #region 字节打包(加入长度信息)及从字节流中分离
 public partial class MyLogTool
 {
     /// <summary>
     /// 将逻辑数据从字节流中分离出来
     /// </summary>
     /// <param name="bytesLst"></param>
     /// <returns></returns>
     public static byte[] SplitLogicBytes(ref List<byte> bytesLst)
     {
         byte[] buff = null;
         if (bytesLst.Count > 4)
         {
             byte[] data = bytesLst.ToArray();
             int len = BitConverter.ToInt32(data, 0);
             if (bytesLst.Count >= len + 4)
             {
                 buff = new byte[len];
                 Buffer.BlockCopy(data, 4, buff, 0, len);
                 bytesLst.RemoveRange(0, len + 4);
             }
         }
         return buff;
     }

     /// <summary>
     /// 打包数据字节，在头部加入长度信息
     /// </summary>
     /// <param name="body"></param>
     /// <returns></returns>
     public static byte[] PackDataInfo(byte[] body)
     {
         int len = body.Length;
         byte[] pkg = new byte[len + 4];
         byte[] head = BitConverter.GetBytes(len);
         head.CopyTo(pkg, 0);
         body.CopyTo(pkg, 4);
         return pkg;
     }
 }
 #endregion