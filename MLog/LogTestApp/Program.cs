// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");
// 注册编码提供程序
using System.Text;
//Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

//基本使用
MLog.Log.Debug().Debug($"{"Hello, World!"}");
MLog.Log.Info().Info("Hello, World!");
MLog.Log.Warn().Warn("Hello, World!");
MLog.Log.Error().Error("Hello, World!");
MLog.Log.Info().Fatal("Hello, World!");

//写入到log/123132文件夹下
//MLog.Log.Info("123132").Info("Hello, World!");


// 测试不同日志级别和格式
//using MLog;
//string value1 = "测试数据1";
//int value2 = 42;
//// Debug - 白色占位符
//MLog.Log.Debug("屌炸天\\Debug").Debug($"调试信息: {value1}, {value2}");
//// Info - 绿色占位符
//MLog.Log.Info("屌炸天\\Info").Info(string.Format("普通信息: {0}, {1}", value1, value2));
//// Warn - 黄色占位符
//MLog.Log.Warn("屌炸天\\Warn").Warn($"警告信息: {value1}, {value2}");
//// Error - 洋红色占位符
//MLog.Log.Error("屌炸天\\Error").Error($"错误信息: {value1}, {value2}");
//// Fatal - 红色占位符
//MLog.Log.Fatal("屌炸天\\Fatal").Fatal($"致命错误: {value1}, {value2}");


//// 设置日志级别为 Info
//MLog.LogHelper.SetLogLevel(LogType.INFO);
//// 禁用控制台输出
//MLog.LogHelper.EnableConsoleOutput(false);
//// 测试日志
//string value = "测试数据";
//// 不会显示，因为级别低于 Info
//MLog.Log.Debug().Debug($"调试信息: {value}");
//// 会显示，因为级别大于等于 Info
//MLog.Log.Info().Info($"普通信息: {value}");
//MLog.Log.Warn().Warn($"警告信息: {value}");
//MLog.Log.Error().Error($"错误信息: {value}");
//MLog.Log.Fatal().Fatal($"致命错误: {value}");

//try
//{
//    Convert.ToInt16("asd");
//}
//catch (Exception ex)
//{
//    // Fatal - 红色占位符
//    MLog.Log.Error().Error(ex);
//}

//性能测试
//LogPerformanceTest.RunTest();


//using LogTestApp;

//new LoginService().Start();
//new GameService().Start();
