using log4net.Appender;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Log.MyLogs
{
    public class ColoredMessageConsoleAppender : AppenderSkeleton
    {
        private static readonly Dictionary<Level, ConsoleColor> colorMapping = new Dictionary<Level, ConsoleColor>
        {
            { Level.Debug, ConsoleColor.White },
            { Level.Info, ConsoleColor.Green },
            { Level.Warn, ConsoleColor.Yellow },
            { Level.Error, ConsoleColor.Red },
            { Level.Fatal, ConsoleColor.Magenta }
        };

        private static readonly object consoleLock = new object();

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (loggingEvent == null) return;

            try
            {
                lock (consoleLock)
                {
                    // 确保控制台使用UTF-8编码
                    Console.OutputEncoding = Encoding.UTF8;

                    Console.Write($"{DateTime.Now:HH:mm:ss.fff} [");

                    if (colorMapping.TryGetValue(loggingEvent.Level, out var color))
                    {
                        Console.ForegroundColor = color;
                        Console.Write(loggingEvent.Level.DisplayName);
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write(loggingEvent.Level.DisplayName);
                    }

                    Console.WriteLine($"] - {loggingEvent.RenderedMessage}");
                }
            }
            catch
            {
                // 即使发生异常也不抛出，确保不影响主程序运行
            }
        }
    }
}
