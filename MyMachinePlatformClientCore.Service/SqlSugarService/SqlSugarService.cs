using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MyMachinePlatformClientCore.IService.ISqlSugarService;

namespace MyMachinePlatformClientCore.Service.SqlSugarService
{
    public class SqlSugarService:ISqlSugarService
    {
        private static string connectionSql = "Data Source=localhost;port=3306;user ID = root;pwd=root;database=MMoCore";
        /// <summary>
        /// 
        /// </summary>
        static SqlSugarService()
        {
            //connectionSql= AppSettingService.AppSettingService.GetConnectionString("DefaultConnection");
        }
        private static  object _lock = new object();
        /// <summary>
        /// 
        /// </summary>
        protected  static ISqlSugarClient _db;
        /// <summary>
        /// 
        /// </summary>
        protected   static ISqlSugarClient db
        {
            get
            {
                if(_db == null)
                {
                    lock (_lock)
                    {
                        if (_db == null)
                        {
                            _db = new SqlSugarClient(new ConnectionConfig()
                            {
                                ConnectionString = connectionSql,
                                DbType = DbType.MySql,
                                IsAutoCloseConnection = true,
                                InitKeyType = InitKeyType.Attribute,
                            });
                        }
                    }
                }
                return _db;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async  Task<T> Find<T>(Expression<Func<T, bool>> whereLambda) where T : class 
        {
            return await db.Queryable<T>().Where(whereLambda).FirstAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<List<T>> FindList<T>(Expression<Func<T, bool>> whereLambda) where T : class 
        {
            return await db.Queryable<T>().Where(whereLambda).ToListAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<List<T>> FindAll<T>() where T : class
        {
            return await db.Queryable<T>().ToListAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<int> Insert<T>(T entity) where T : class,new()
        {
            return await db.Insertable(entity).ExecuteCommandAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<int> Insert<T>(List<T>entities) where T : class
        {
            return await db.Insertable(entities).ExecuteCommandAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async  Task<T> InsertReturn<T>(T entity) where T : class,new()
        {
            return await db.Insertable(entity).ExecuteReturnEntityAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="updateColumsExpression"></param>
        /// <param name="where"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<int> Update<T>(Expression<Func<T,T>>updateColumsExpression,Expression<Func<T,bool>>where) where T : class, new()
        {
            return await db.Updateable<T>().SetColumns(updateColumsExpression).Where(where).ExecuteCommandAsync();
        } 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<int> Update<T>(T entity) where T : class, new()
        {
            return await db.Updateable(entity).ExecuteCommandAsync();
            
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<int> Delete<T>(T entity) where T : class,new()
        {
            return await db.Deleteable(entity).ExecuteCommandAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>

        public async Task<int> Delete<T>(Expression<Func<T, bool>> whereLambda) where T : class, new()
        {

            return await db.Deleteable<T>().Where(whereLambda).ExecuteCommandAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<int> Delete<T>(List<T> entities) where T : class, new()
        {
            return await db.Deleteable(entities).ExecuteCommandAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<int> ExcuteSql(string sql, object parameters)
        {
             return await db.Ado.ExecuteCommandAsync(sql, parameters);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<int> ExcuteSql(string sql, params object[] parameters)
        {
            return await db.Ado.ExecuteCommandAsync(sql, parameters); 
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T> ExecuteSql<T>(string sql, object parameters) where T : class, new()
        {
            return await db.Ado.SqlQuerySingleAsync<T>(sql, parameters);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<List<T>> ExecuteSql<T>(string sql, params object[] parameters) where T:class,new()
        {
            return await db.Ado.SqlQueryAsync<T>(sql, parameters);
        }
        
        
       
    }
}
