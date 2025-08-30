
using MyMachinePlatformClientCore.Log.MyLogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio.TwiML.Messaging;

namespace MyMachinePlatformClientCore.Service.LogService
{
    public  interface ILogService
    {
        void LogWarn(string message);
        void LogError(string message);
        void LogInfo(string message);
        void LogDebug(string message);
        void LogFatal(string message);
    }
    public class LogService : ILogService
    {

        public void LogDebug(string message)
        {
           
            CLogService.Debug().Debug(message);
        }

        

        public void LogError(string message)
        {
            

            
            CLogService.Error().Error(message);
        }

        public void LogFatal(string message)
        {
           
            CLogService.Fatal().Fatal(message);
        }

        public void LogInfo(string message)
        {
           
            CLogService.Info().Info(message);
        }

      
        public void LogWarn(string message)
        {
          
            CLogService.Warn().Warn(message);
        }

        
    }
}
