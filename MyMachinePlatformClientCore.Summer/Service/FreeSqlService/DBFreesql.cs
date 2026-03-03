 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Service
{
    public class DBFreesql
    {
        private static string freeSqlConnection = "Data Source=localhost;port=3306;user ID = root;pwd=root;database=MMoCore";
        /// <summary>
        /// 
        /// </summary>
        public DBFreesql()
        {

        }
         
        /// <summary>
        /// 
        /// </summary>
        private static Lazy<IFreeSql> ServersqlLazy = new Lazy<IFreeSql>(() => new FreeSql.FreeSqlBuilder()
            .UseMonitorCommand(cmd => Trace.WriteLine($"Sql：{cmd.CommandText}"))//监听SQL语句,Trace在输出选项卡中查看
            .UseConnectionString(FreeSql.DataType.MySql, freeSqlConnection)
            .UseAutoSyncStructure(true) //自动同步实体结构到数据库，FreeSql不会扫描程序集，只有CRUD时才会生成表。
            .Build());
        public static IFreeSql mysql => ServersqlLazy.Value;
    }
}
