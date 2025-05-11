using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogTestApp
{
    public class GameService
    {
        ILog logger = MLog.Log.Get<GameService>();
        public void Start()
        {
            //logger.Error("主服务启动成功...");
            try
            {
                Convert.ToInt16("asd");
            }
            catch (Exception ex)
            {
                // Fatal - 红色占位符
                logger.Error($"主服务启动异常：{ex.Message}", ex);
            }
            MLog.Log.Fatal<GameService>("爆炸了\\真的吗").Fatal("服务炸了1");
            MLog.Log.Fatal<GameService>("爆炸了/真的吗").Fatal("服务炸了2");
        }
    }
}
