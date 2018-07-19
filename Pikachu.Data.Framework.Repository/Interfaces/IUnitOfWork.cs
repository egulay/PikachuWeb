using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Pikachu.Data.Framework.Repository.Interfaces
{
    public interface IUnitOfWork : IUnitOfWorkForService
    {
        void Refresh();
        void Save();
        Task<int> SaveAsync();
        Task<int> SaveAsync(CancellationToken cancellationToken);
        void Dispose(bool disposing);
        void Dispose();
        void ExecuteSqlCommand(string command, object[] parameterArray);
        Task<int> ExecuteSqlCommandAsync(string command, object[] parameterArray);
        DataTable ExecuteStoredProcedure(string storedProcedureName, SqlParameter[] parameterArray);
        List<T> ExecuteStoredProcedure<T>(string storedProcedureName, SqlParameter[] parameterArray) where T : class;

        List<T> ExecuteStoredProcedure<T>(string storedProcedureName, string connectionStringKey,
            SqlParameter[] parameterArray) where T : class;

        int ExecuteStoredProcedureNonQuery(string storedProcedureName,
            SqlParameter[] parameterArray);
        object ExecuteStoredProcedureScalar(string storedProcedureName,
            SqlParameter[] parameterArray);
        object ExecuteSqlCommandScalar(string command,  object[] parameterArray);
        List<T> ExecuteSqlCommand<T>(string command, SqlParameter[] parameterArray) where T : class;
        void BulkInsert<TEntity>(IEnumerable<TEntity> entities, SqlConnection connection = null);
        void BulkInsert<TEntity>(IEnumerable<TEntity> entities, string tableName, SqlConnection connection = null);
    }

    public interface IUnitOfWorkForService
    {
        IRepository<TEntity> Repository<TEntity>() where TEntity : class, new();
    }
}