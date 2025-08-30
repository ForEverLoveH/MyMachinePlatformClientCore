using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Log.MyLogs
{
    /// <summary>
    /// 输出日志模板配置说明
    ///%m(message):输出的日志消息；
    ///%n(newline):换行；
    ///%d(datetime):输出当前语句运行的时刻；
    ///%r(runtime):输出程序从运行到执行到当前语句时消耗的毫秒数；
    ///%t(threadid):当前语句所在的线程ID ；
    ///%p(priority): 日志的当前日志级别；
    ///%c(class):当前日志对象的名称；
    ///%L：输出语句所在的行号；
    ///%F：输出语句所在的文件名； 
    ///%-10：表示最小长度为10，如果不够，则用空格填充
    /// </summary>
    public class LayoutConversionPattern
    {
        /// <summary>
        /// 调试的信息输出模板
        /// </summary>
        public static string Debug_Pattern = "记录时间：%date %n线程ID:[%thread]%n事件级别:%level %n相关类名:%c%n程序文件:%F 第%L行%n调试描述：%message%newline-----------------------------------------%n";
        /// <summary>
        /// 运行的信息输出模板
        /// </summary>
        public static string Info_Pattern = "记录时间：%date 线程ID:[%thread] 记录类名:[%logger] 详细信息：%message%n";
        /// <summary>
        /// 警告的信息输出模板
        /// </summary>
        public static string Warn_Pattern = "记录时间：%date %n线程ID:[%thread]%n事件级别:%level %n相关类名:%c%n程序文件:%F 第%L行%n调试描述：%message%newline-----------------------------------------%n";
        /// <summary>
        /// 错误信息输出模板
        /// </summary>
        public static string Error_Pattern = "记录时间：%date %n线程ID:[%thread]%n事件级别:%level %n相关类名:%c%n程序文件:%F 第%L行%n调试描述：%message%newline-----------------------------------------%n";
        /// <summary>
        /// 严重的信息输出模板
        /// </summary>
        public static string Fatal_Pattern = "记录时间：%date %n线程ID:[%thread]%n事件级别:%level %n相关类名:%c%n程序文件:%F 第%L行%n调试描述：%message%newline-----------------------------------------%n";
    }
}
