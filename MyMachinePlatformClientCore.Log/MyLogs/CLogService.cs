
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Log.MyLogs
{
    public class CLogService
    {
        static CLogService()
        {
           // SetConsoleOutput(false);
        }
        public static void SetConsoleOutput(bool flag)
        {
            LogHelper.EnableConsoleOutput(flag);
        }
        
        #region Info类型输出
        /// <summary>
        /// log根目录下记录Info类型日志
        /// 输出格式：log\\$yyyy-MM-dd$.txt
        /// </summary>
        /// <returns></returns>
        public static ILog Info()
        {
            return LogHelper.GetLog(null, null, LogType.INFO);
        }
        /// <summary>
        /// 指定目录下生成Info类型日志
        /// 输出格式：log\\$category$\\2017-11-22.txt
        /// </summary>
        /// <param name="category">文件路径</param>
        /// <returns></returns>
        public static ILog Info(string category)
        {
            return LogHelper.GetLog(null, category, LogType.INFO);
        }
        /// <summary>
        /// LogType下生成Info类型日志
        /// </summary>
        /// <param name="categoryLogType">文件夹名称枚举</param>
        /// <returns></returns>
        public static ILog Info(LogType categoryLogType)
        {
            return LogHelper.GetLog(null, categoryLogType.ToString(), LogType.INFO);
        }
        /// <summary>
        /// 指定目录下生成指定Info类型文件日志
        /// 输出格式：log\\$category$\\$fileName$.txt
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="category">文件路径</param>
        /// <returns></returns>
        public static ILog Info(string fileName, string category)
        {
            return LogHelper.GetLog(fileName, category, LogType.INFO);
        }
        /// <summary>
        /// 使用泛型类型名称作为logger名称记录Info类型日志
        /// </summary>
        /// <typeparam name="T">用于标识日志来源的类型</typeparam>
        /// <returns></returns>
        public static ILog Info<T>()
        {
            return LogHelper.GetLog(typeof(T).FullName, null, LogType.INFO);
        }
        /// <summary>
        /// 在指定目录下使用泛型类型名称记录Info类型日志
        /// </summary>
        /// <typeparam name="T">用于标识日志来源的类型</typeparam>
        /// <param name="category">文件路径</param>
        /// <returns></returns>
        public static ILog Info<T>(string category)
        {
            return LogHelper.GetLog(typeof(T).FullName, category, LogType.INFO);
        }
        #endregion
        #region Debug类型输出
        /// <summary>
        /// log根目录下记录Debug类型日志
        /// 输出格式：log\\$yyyy-MM-dd$.txt
        /// </summary>
        /// <returns></returns>
        public static ILog Debug()
        {
            return LogHelper.GetLog(null, null, LogType.DEBUG);
        }

        /// <summary>
        /// LogType下生成Debug类型日志
        /// </summary>
        /// <param name="categoryLogType">文件夹名称枚举</param>
        /// <returns></returns>
        public static ILog Debug(LogType categoryLogType)
        {
            return LogHelper.GetLog(null, categoryLogType.ToString(), LogType.DEBUG);
        }
        /// <summary>
        /// 指定目录下生成Debug类型日志
        /// 输出格式：log\\$category$\\2017-11-22.txt
        /// </summary>
        /// <param name="category">文件路径</param>
        /// <returns></returns>
        public static ILog Debug(string category)
        {
            return LogHelper.GetLog(null, category, LogType.DEBUG);
        }
        /// <summary>
        /// 指定目录下生成指定Debug类型文件日志
        /// 输出格式：log\\$category$\\$fileName$.txt
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="category">文件路径</param>
        /// <returns></returns>
        public static ILog Debug(string fileName, string category)
        {
            return LogHelper.GetLog(fileName, category, LogType.DEBUG);
        }
        /// <summary>
        /// 使用泛型类型名称作为logger名称记录Debug类型日志
        /// </summary>
        /// <typeparam name="T">用于标识日志来源的类型</typeparam>
        /// <returns></returns>
        public static ILog Debug<T>()
        {
            return LogHelper.GetLog(typeof(T).FullName, null, LogType.DEBUG);
        }
        /// <summary>
        /// 在指定目录下使用泛型类型名称记录Debug类型日志
        /// </summary>
        /// <typeparam name="T">用于标识日志来源的类型</typeparam>
        /// <param name="category">文件路径</param>
        /// <returns></returns>
        public static ILog Debug<T>(string category)
        {
            return LogHelper.GetLog(typeof(T).FullName, category, LogType.DEBUG);
        }
        #endregion
        #region Warn类型输出
        /// <summary>
        /// log根目录下记录Warn类型日志
        /// 输出格式：log\\$yyyy-MM-dd$.txt
        /// </summary>
        /// <returns></returns>
        public static ILog Warn()
        {
            return LogHelper.GetLog(null, null, LogType.WARN);
        }
        /// <summary>
        /// 指定目录下生成Warn类型日志
        /// 输出格式：log\\$category$\\2017-11-22.txt
        /// </summary>
        /// <param name="category">文件路径</param>
        /// <returns></returns>
        public static ILog Warn(string category)
        {
            return LogHelper.GetLog(null, category, LogType.WARN);
        }
        /// <summary>
        /// LogType下生成Warn类型日志
        /// </summary>
        /// <param name="categoryLogType">文件夹名称枚举</param>
        /// <returns></returns>
        public static ILog Warn(LogType categoryLogType)
        {
            return LogHelper.GetLog(null, categoryLogType.ToString(), LogType.WARN);
        }
        /// <summary>
        /// 指定目录下生成指定Warn类型文件日志
        /// 输出格式：log\\$category$\\$fileName$.txt
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="category">文件路径</param>
        /// <returns></returns>
        public static ILog Warn(string fileName, string category)
        {
            return LogHelper.GetLog(fileName, category, LogType.WARN);
        }
        /// <summary>
        /// 使用泛型类型名称作为logger名称记录Warn类型日志
        /// </summary>
        /// <typeparam name="T">用于标识日志来源的类型</typeparam>
        /// <returns></returns>
        public static ILog Warn<T>()
        {
            return LogHelper.GetLog(typeof(T).FullName, null, LogType.WARN);
        }
        /// <summary>
        /// 在指定目录下使用泛型类型名称记录Warn类型日志
        /// </summary>
        /// <typeparam name="T">用于标识日志来源的类型</typeparam>
        /// <param name="category">文件路径</param>
        /// <returns></returns>
        public static ILog Warn<T>(string category)
        {
            return LogHelper.GetLog(typeof(T).FullName, category, LogType.WARN);
        }
        #endregion
        #region Error类型输出
        /// <summary>
        /// log根目录下记录Error类型日志
        /// 输出格式：log\\$yyyy-MM-dd$.txt
        /// </summary>
        /// <returns></returns>
        public static ILog Error()
        {
            return LogHelper.GetLog(null, null, LogType.ERROR);
        }
        /// <summary>
        /// 指定目录下生成Error类型日志
        /// 输出格式：log\\$category$\\2017-11-22.txt
        /// </summary>
        /// <param name="category">文件路径</param>
        /// <returns></returns>
        public static ILog Error(string category)
        {
            return LogHelper.GetLog(null, category, LogType.ERROR);
        }
        /// <summary>
        /// LogType下生成Error类型日志
        /// </summary>
        /// <param name="categoryLogType">文件夹名称枚举</param>
        /// <returns></returns>
        public static ILog Error(LogType categoryLogType)
        {
            return LogHelper.GetLog(null, categoryLogType.ToString(), LogType.ERROR);
        }
        /// <summary>
        /// 指定目录下生成指定Error类型文件日志
        /// 输出格式：log\\$category$\\$fileName$.txt
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="category">文件路径</param>
        /// <returns></returns>
        public static ILog Error(string fileName, string category)
        {
            return LogHelper.GetLog(fileName, category, LogType.ERROR);
        }
        /// <summary>
        /// 使用泛型类型名称作为logger名称记录Error类型日志
        /// </summary>
        /// <typeparam name="T">用于标识日志来源的类型</typeparam>
        /// <returns></returns>
        public static ILog Error<T>()
        {
            return LogHelper.GetLog(typeof(T).FullName, null, LogType.ERROR);
        }
        /// <summary>
        /// 在指定目录下使用泛型类型名称记录Error类型日志
        /// </summary>
        /// <typeparam name="T">用于标识日志来源的类型</typeparam>
        /// <param name="category">文件路径</param>
        /// <returns></returns>
        public static ILog Error<T>(string category)
        {
            return LogHelper.GetLog(typeof(T).FullName, category, LogType.ERROR);
        }
        #endregion
        #region Fatal类型输出
        /// <summary>
        /// log根目录下记录Fatal类型日志
        /// 输出格式：log\\$yyyy-MM-dd$.txt
        /// </summary>
        /// <returns></returns>
        public static ILog Fatal()
        {
            return LogHelper.GetLog(null, null, LogType.FATAL);
        }
        /// <summary>
        /// 指定目录下生成Fatal类型日志
        /// 输出格式：log\\$category$\\2017-11-22.txt
        /// </summary>
        /// <param name="category">文件路径</param>
        /// <returns></returns>
        public static ILog Fatal(string category)
        {
            return LogHelper.GetLog(null, category, LogType.FATAL);
        }
        /// <summary>
        /// LogType下生成Fatal类型日志
        /// </summary>
        /// <param name="categoryLogType">文件夹名称枚举</param>
        /// <returns></returns>
        public static ILog Fatal(LogType categoryLogType)
        {
            return LogHelper.GetLog(null, categoryLogType.ToString(), LogType.FATAL);
        }
        /// <summary>
        /// 指定目录下生成指定Fatal类型文件日志
        /// 输出格式：log\\$category$\\$fileName$.txt
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="category">文件路径</param>
        /// <returns></returns>
        public static ILog Fatal(string fileName, string category)
        {
            return LogHelper.GetLog(fileName, category, LogType.FATAL);
        }
        /// <summary>
        /// 使用泛型类型名称作为logger名称记录Fatal类型日志
        /// </summary>
        /// <typeparam name="T">用于标识日志来源的类型</typeparam>
        /// <returns></returns>
        public static ILog Fatal<T>()
        {
            return LogHelper.GetLog(typeof(T).FullName, null, LogType.FATAL);
        }
        /// <summary>
        /// 在指定目录下使用泛型类型名称记录Fatal类型日志
        /// </summary>
        /// <typeparam name="T">用于标识日志来源的类型</typeparam>
        /// <param name="category">文件路径</param>
        /// <returns></returns>
        public static ILog Fatal<T>(string category)
        {
            return LogHelper.GetLog(typeof(T).FullName, category, LogType.FATAL);
        }
        #endregion
        #region 通用类型输出
        /// <summary>
        /// log根目录下记录Fatal类型日志
        /// 输出格式：log\\$yyyy-MM-dd$.txt
        /// </summary>
        /// <returns></returns>
        public static ILog Get()
        {
            return LogHelper.GetLog(null, null, LogType.FATAL);
        }
        /// <summary>
        /// 指定目录下生成Fatal类型日志
        /// 输出格式：log\\$category$\\2017-11-22.txt
        /// </summary>
        /// <param name="category">文件路径</param>
        /// <returns></returns>
        public static ILog Get(string category)
        {
            return LogHelper.GetLog(null, category, LogType.FATAL);
        }
        /// <summary>
        /// LogType下生成Fatal类型日志
        /// </summary>
        /// <param name="categoryLogType">文件夹名称枚举</param>
        /// <returns></returns>
        public static ILog Get(LogType categoryLogType)
        {
            return LogHelper.GetLog(null, categoryLogType.ToString(), LogType.FATAL);
        }
        /// <summary>
        /// 指定目录下生成指定Fatal类型文件日志
        /// 输出格式：log\\$category$\\$fileName$.txt
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="category">文件路径</param>
        /// <returns></returns>
        public static ILog Get(string fileName, string category)
        {
            return LogHelper.GetLog(fileName, category, LogType.FATAL);
        }
        /// <summary>
        /// 使用泛型类型名称作为logger名称记录Fatal类型日志
        /// </summary>
        /// <typeparam name="T">用于标识日志来源的类型</typeparam>
        /// <returns></returns>
        public static ILog Get<T>()
        {
            return LogHelper.GetLog(typeof(T).FullName, null, LogType.FATAL);
        }
        /// <summary>
        /// 在指定目录下使用泛型类型名称记录Fatal类型日志
        /// </summary>
        /// <typeparam name="T">用于标识日志来源的类型</typeparam>
        /// <param name="category">文件路径</param>
        /// <returns></returns>
        public static ILog Get<T>(string category)
        {
            return LogHelper.GetLog(typeof(T).FullName, category, LogType.FATAL);
        }
        #endregion
    }
}
