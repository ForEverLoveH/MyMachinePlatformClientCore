using System.Diagnostics;
namespace MLog.Test
{
    public class LogPerformanceTest
    {
        public static void RunTest(int count = 100000)
        {
            Console.WriteLine($"开始测试写入 {count} 条日志...");
            Console.WriteLine($"控制台输出状态: {(LogHelper.enableConsoleOutput ? "启用" : "禁用")}");

            string testMessage = "这是一条测试日志消息，包含中文和English混合内容123";

            var sw = Stopwatch.StartNew();

            for (int i = 0; i < count; i++)
            {
                Log.Info().Info($"Test log {i}: {testMessage}");
            }

            sw.Stop();

            Console.WriteLine($"写入 {count} 条日志总耗时: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine($"平均每条日志耗时: {(double)sw.ElapsedMilliseconds / count:F3}ms");
            Console.WriteLine($"每秒写入日志数: {count * 1000 / sw.ElapsedMilliseconds}");
        }
    }
}