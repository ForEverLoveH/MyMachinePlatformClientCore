using System.Linq.Expressions;

namespace MyMachinePlatformClientCore.IService.ISqlSugarService;
/// <summary>
/// sqlsugar仓储服务接口
/// </summary>
public interface ISqlSugarRepositoryService<T> where T : class, new()
{
   
    Task<T> Find<T>(Expression<Func<T, bool>> whereLambda) where T : class;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="whereLambda"></param>
    /// <returns></returns>
    Task<List<T>> FindList<T>(Expression<Func<T, bool>> whereLambda) where T : class ;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="whereLambda"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<List<T>> FindPageList<T>(Expression<Func<T, bool>> whereLambda, int pageIndex, int pageSize) where T : class;
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<List<T>> FindAll<T>() where T : class;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<int> Insert<T>(T entity) where T : class, new();
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entities"></param>
    /// <returns></returns>
    Task<int> Insert<T>(List<T>entities) where T : class;
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entity"></param>
    /// <returns></returns>

    Task<T> InsertReturn<T>(T entity) where T : class, new();
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="updateColumsExpression"></param>
    /// <param name="where"></param>
    /// <returns></returns>
    Task<int> Update<T>(Expression<Func<T, T>> updateColumsExpression, Expression<Func<T, bool>> where)
        where T : class, new();
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<int> Update<T>(T entity) where T : class, new();
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<int> Delete<T>(T entity) where T : class, new();
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="whereLambda"></param>
    /// <returns></returns>
    Task<int> Delete<T>(Expression<Func<T, bool>> whereLambda) where T : class, new();

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entities"></param>
    /// <returns></returns>
    Task<int> Delete<T>(List<T> entities) where T : class, new();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    Task<int> ExcuteSql(string sql, object parameters);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    Task<int> ExcuteSql(string sql, params object[] parameters);
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    Task<T> ExecuteSql<T>(string sql, object parameters) where T : class, new();
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    Task<List<T>> ExecuteSql<T>(string sql, params object[] parameters) where T : class, new();

    

}