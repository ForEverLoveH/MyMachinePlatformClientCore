using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Log.MyLogs
{
    public class LogMessage
    {
        public LogType _LogType { get; set; }
        public string message { get; set; }

        public static LogMessage SetMessage(LogType logType, string message)
        {
            return new LogMessage()
            {
                _LogType = logType,
                message = message
            };
        }
    }
}
