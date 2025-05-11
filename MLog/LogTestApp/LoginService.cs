using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogTestApp
{
    public class LoginService
    {
        ILog logger = MLog.Log.Get<LoginService>();
        public void Start()
        {
            logger.Debug("登录务启动成功...");
            logger.Info("登录务启动成功...");
            logger.Warn("登录务启动成功...");
            logger.Error("登录务启动成功...");
            logger.Fatal("登录务启动成功...");
        }
    }
}
