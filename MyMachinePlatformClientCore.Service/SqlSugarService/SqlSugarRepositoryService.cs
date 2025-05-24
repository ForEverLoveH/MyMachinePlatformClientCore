using System.Linq.Expressions;
using MyMachinePlatformClientCore.IService.ISqlSugarService;
using SqlSugar;

namespace MyMachinePlatformClientCore.Service.SqlSugarService;
/// <summary>
/// sqlsugar仓储服务
/// </summary>
public class SqlSugarRepositoryService <T>:ISqlSugarRepositoryService<T> where T : class, new()
{
    
    private static ISqlSugarClient _db;
    /// <summary>
    /// 
    /// </summary>
    private static object  _lock = new object();
    /// <summary>
    /// 
    /// </summary>
    private static string  _connectionSql ;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="connectionSql"></param>
    public SqlSugarRepositoryService(string connectionSql)
    {
         _connectionSql = connectionSql;
    }
    /// <summary>
    /// 
    /// </summary>
    public static ISqlSugarClient Db
    {
        get
        {
            if (_db == null)
            {
                lock (_lock)
                {
                    if (_db == null)
                    {
                        _db = new SqlSugarClient(new ConnectionConfig()
                        {
                            ConnectionString = _connectionSql,
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
    public async Task<T> Find<T>(Expression<Func<T, bool>> whereLambda) where T : class
    {
        return await Db.Queryable<T>().Where(whereLambda).FirstAsync();
    }

     
    /// <summary>
    /// 
    /// </summary>
    /// <param name="whereLambda"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>

    public async Task<List<T>> FindList<T>(Expression<Func<T, bool>> whereLambda) where T : class
    {
        return await Db.Queryable<T>().Where(whereLambda).ToListAsync();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="whereLambda"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task <List<T>> FindPageList<T>(Expression<Func<T, bool>> whereLambda,int pageIndex,int pageSize)  where T : class
    {
        return await Db.Queryable<T>().Where(whereLambda).ToPageListAsync(pageIndex, pageSize);
            
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<List<T>> FindAll<T>() where T : class
    {
        return await Db.Queryable<T>().ToListAsync();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async Task<int> Insert<T>(T entity) where T : class,new()
    {
        return await Db.Insertable(entity).ExecuteCommandAsync();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="entities"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
     public async Task<int> Insert<T>(List<T>entities) where T : class
        {
            return await Db.Insertable(entities).ExecuteCommandAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async  Task<T> InsertReturn<T>(T entity) where T : class,new()
        {
            return await Db.Insertable(entity).ExecuteReturnEntityAsync();
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
            return await Db.Updateable<T>().SetColumns(updateColumsExpression).Where(where).ExecuteCommandAsync();
        } 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<int> Update<T>(T entity) where T : class, new()
        {
            return await Db.Updateable(entity).ExecuteCommandAsync();
            
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<int> Delete<T>(T entity) where T : class,new()
        {
            return await Db.Deleteable(entity).ExecuteCommandAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>

        public async Task<int> Delete<T>(Expression<Func<T, bool>> whereLambda) where T : class, new()
        {
            return await Db.Deleteable<T>().Where(whereLambda).ExecuteCommandAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entities"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<int> Delete<T>(List<T> entities) where T : class, new()
        {
            return await Db.Deleteable(entities).ExecuteCommandAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<int> ExcuteSql(string sql, object parameters)
        {
             return await Db.Ado.ExecuteCommandAsync(sql, parameters);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<int> ExcuteSql(string sql, params object[] parameters)
        {
            return await Db.Ado.ExecuteCommandAsync(sql, parameters); 
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
            return await Db.Ado.SqlQuerySingleAsync<T>(sql, parameters);
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
            return await Db.Ado.SqlQueryAsync<T>(sql, parameters);
        }
        
        
}