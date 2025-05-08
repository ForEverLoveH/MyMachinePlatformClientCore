using System.Linq.Expressions;
using FreeSql;

namespace MyMachinePlatformClientCore.IService.IFreeSqlService;

public interface IFreesqlService
{
    T Find<T>(Expression<Func<T, bool>> expression = null) where T : class;

    List<TSelect> Find<T, TSelect>(Expression<Func<T, TSelect>> expression) where T : class;

    List<dynamic> Find<T>(Expression<Func<T, dynamic>> expression) where T : class;


    void FindAggregate<T>(Expression<Func<ISelectGroupingAggregate<T>, object>> express, out object key)
        where T : class;

    Task<T> FindAsync<T>(Expression<Func<T, bool>> expression = null) where T : class;

    List<T> FindList<T>(Expression<Func<T, object>> Orderexp = null, Expression<Func<T, bool>> expression = null)
        where T : class;

    Task<List<T>> FindListAsync<T>(Expression<Func<T, object>> expression1 = null,
        Expression<Func<T, bool>> expression = null) where T : class;

    int Insert<T>(T entity) where T : class;
    Task<int> InsertAsync<T>(T entity) where T : class;
    int Insert<T>(List<T> entities) where T : class;

    Task<int> InsertAsync<T>(List<T> entity) where T : class;
    int InsertOrUpdate<T>(T entity) where T : class;
    Task<int> InsertOrUpdateAsync<T>(T entity) where T : class;
    int InsertOrUpdate<T>(List<T> entity) where T : class;
    Task<int> InsertOrUpdateAsync<T>(List<T> entity) where T : class;
    int Update<T>(T entity) where T : class;

    Task<int> UpdateAsync<T>(T entity) where T : class;
    int Update<T>(Expression<Func<T, bool>> expression, Expression<Func<T, bool>> expression1) where T : class;


    Task<int> UpdateAsync<T>(Expression<Func<T, bool>> expression, Expression<Func<T, bool>> expression1)
        where T : class;

    int Delete<T>(T entity) where T : class;
    Task<int> DeleteAsync<T>(T entity) where T : class;

    int Delete<T>(Expression<Func<T, bool>> expression) where T : class;

    Task<int> DeleteAsync<T>(Expression<Func<T, bool>> expression) where T : class;

    List<T> ExcuteSql<T>(string sql, params object[] parameters) where T : class;

    int ExcuteSql(string sql, params object[] parameters);

    Task<List<T>> ExcuteSqlAsync<T>(string sql, params object[] parameters) where T : class;

    Task<int> ExcuteSqlAsync(string sql, object parameters);
}