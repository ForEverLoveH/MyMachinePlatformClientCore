using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Configuration;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository;
using log4net.Repository.Hierarchy;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace MLog
{
    public class LogHelper
    {
        private static readonly ConcurrentDictionary<string, ILog> loggerContainer = new ConcurrentDictionary<string, ILog>();
        private static readonly Dictionary<string, ReadParamAppender> appenderContainer = new Dictionary<string, ReadParamAppender>();
        private static readonly object lockObj = new object();

        private static Level currentLevel = Level.All;
        internal static bool enableConsoleOutput = true;

        //默认配置
        private const int MAX_SIZE_ROLL_BACKUPS = 20;
        private const string DATE_PATTERN = "yyyyMMdd\".txt\"";
        private const string MAXIMUM_FILE_SIZE = "256MB";
        private const string LEVEL = "ALL";

        static LogHelper()
        {
            // 注册编码提供程序
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        /// <summary>
        /// 设置日志级别
        /// </summary>
        /// <param name="logType">日志级别枚举</param>
        public static void SetLogLevel(LogType logType)
        {
            switch (logType)
            {
                case LogType.DEBUG:
                    currentLevel = Level.Debug;
                    break;
                case LogType.INFO:
                    currentLevel = Level.Info;
                    break;
                case LogType.WARN:
                    currentLevel = Level.Warn;
                    break;
                case LogType.ERROR:
                    currentLevel = Level.Error;
                    break;
                case LogType.FATAL:
                    currentLevel = Level.Fatal;
                    break;
                default:
                    currentLevel = Level.All;
                    break;
            }

            // 更新所有现有logger的级别
            var repository = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();
            foreach (var logger in repository.GetCurrentLoggers())
            {
                ((Logger)logger).Level = currentLevel;
            }
        }
        /// <summary>
        /// 设置是否启用控制台输出
        /// </summary>
        public static void EnableConsoleOutput(bool enable)
        {
            enableConsoleOutput = enable;
        }

        public static ILog GetLog(string? loggerName, string category = null, LogType logType = LogType.INFO, bool additivity = false)
        {

            if (string.IsNullOrEmpty(loggerName))
                loggerName = DateTime.Now.ToString("yyyy-MM-dd");
            else
                loggerName = string.Format("{0}_{1}", DateTime.Now.ToString("yyyy-MM-dd"), loggerName);

            return loggerContainer.GetOrAdd("File~!@#$%^&*()_+" + category + loggerName, delegate (string name)
            {
                // 创建文件appender
                RollingFileAppender fileAppender = null;
                ReadParamAppender appender = null;
                if (appenderContainer.ContainsKey(loggerName))
                {
                    appender = appenderContainer[loggerName];
                    fileAppender = GetNewFileApender(loggerName, string.IsNullOrEmpty(appender.File) ? GetFile(category, loggerName) : appender.File, appender.MaxSizeRollBackups,
                        appender.AppendToFile, true, appender.MaximumFileSize, RollingFileAppender.RollingMode.Size, appender.DatePattern, appender.LayoutPattern);
                }
                else
                {
                    fileAppender = GetNewFileApender(loggerName, GetFile(category, loggerName), MAX_SIZE_ROLL_BACKUPS, true, true, MAXIMUM_FILE_SIZE, RollingFileAppender.RollingMode.Size,
                        DATE_PATTERN, GetLayOut_ConversionPattern(logType));
                }

                log4net.Repository.Hierarchy.Hierarchy repository = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();
                Logger logger = repository.LoggerFactory.CreateLogger(repository, loggerName);
                logger.Hierarchy = repository;
                logger.Parent = repository.Root;
                logger.Level = currentLevel;
                logger.Additivity = additivity;
                logger.AddAppender(fileAppender);

                // 只在启用控制台输出时添加控制台appender
                if (enableConsoleOutput)
                {
                    var consoleAppender = GetColoredConsoleAppender(logType);
                    logger.AddAppender(consoleAppender);
                }

                logger.Repository.Configured = true;
                return new LogImpl(logger);
            });
        }

        private static ColoredMessageConsoleAppender GetColoredConsoleAppender(LogType logType)
        {
            var consoleAppender = new ColoredMessageConsoleAppender();
            var layout = new PatternLayout("%date{HH:mm:ss.fff} [%level] - %message%newline");
            consoleAppender.Layout = layout;

            layout.ActivateOptions();
            consoleAppender.ActivateOptions();

            return consoleAppender;
        }

        //如果没有指定文件路径则在运行路径下建立 Log\{loggerName}.txt  
        private static string GetFile(string category, string loggerName)
        {
            //string baseDir = string.IsNullOrEmpty(category) ? "Log" : $"Log\\{category}";
            //return Path.Combine(baseDir, $"{loggerName}");  // 移除 .txt 扩展名，让 RollingFileAppender 处理
            if (string.IsNullOrEmpty(category))
            {
                return string.Format(@"Log\{0}.txt", loggerName);
            }
            else
            {
                return string.Format(@"Log\{0}\{1}.txt", category, loggerName);
            }
        }

        private static Level GetLoggerLevel(string level)
        {
            if (!string.IsNullOrEmpty(level))
            {
                switch (level.ToLower().Trim())
                {
                    case "debug":
                        return Level.Debug;
                    case "info":
                        return Level.Info;
                    case "warn":
                        return Level.Warn;
                    case "error":
                        return Level.Error;
                    case "fatal":
                        return Level.Fatal;
                }
            }
            return Level.Debug;
        }

        private static RollingFileAppender GetNewFileApender(string appenderName, string file, int maxSizeRollBackups, bool appendToFile = true, bool staticLogFileName = true, string maximumFileSize = "256MB", RollingFileAppender.RollingMode rollingMode = RollingFileAppender.RollingMode.Size, string datePattern = "yyyyMMdd\".txt\"", string layoutPattern = "%d [%t] %-5p %c  - %m%n")
        {
            RollingFileAppender appender = new RollingFileAppender
            {
                LockingModel = new FileAppender.MinimalLock(),
                Name = appenderName,
                File = file,
                AppendToFile = appendToFile,
                MaxSizeRollBackups = maxSizeRollBackups,
                MaximumFileSize = maximumFileSize,
                //StaticLogFileName = staticLogFileName,
                //RollingStyle = rollingMode,
                StaticLogFileName = staticLogFileName,  // 改为 false，允许动态文件名
                RollingStyle = rollingMode,  // 改为 Composite，同时支持日期和大小滚动
                DatePattern = datePattern,
                PreserveLogFileNameExtension = true,
                CountDirection = 1,
                Encoding = Encoding.UTF8
            };

            PatternLayout layout = new PatternLayout(layoutPattern);
            appender.Layout = layout;
            layout.ActivateOptions();
            appender.ActivateOptions();
            return appender;
        }

        private static string GetLayOut_ConversionPattern(LogType logType)
        {
            string layout_pattern = string.Empty;
            switch (logType)
            {
                case LogType.DEBUG:
                    layout_pattern = LayoutConversionPattern.Debug_Pattern;
                    break;
                case LogType.INFO:
                    layout_pattern = LayoutConversionPattern.Info_Pattern;
                    break;
                case LogType.WARN:
                    layout_pattern = LayoutConversionPattern.Warn_Pattern;
                    break;
                case LogType.ERROR:
                    layout_pattern = LayoutConversionPattern.Error_Pattern;
                    break;
                case LogType.FATAL:
                    layout_pattern = LayoutConversionPattern.Fatal_Pattern;
                    break;
                default:
                    layout_pattern = "记录时间：%date %n线程ID:[%thread]%n事件级别:%level %n相关类名:%c%n程序文件:%F 第%L行%n调试描述：%message%newline-----------------------------------------%n";
                    break;
            }
            return layout_pattern;
        }
    }
}