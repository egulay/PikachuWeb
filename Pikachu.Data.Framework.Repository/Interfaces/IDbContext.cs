using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace Pikachu.Data.Framework.Repository.Interfaces
{
    public interface IDbContext
    {
        DbContextConfiguration Configuration { get; }
        Guid InstanceId { get; }
        DbSet<T> Set<T>() where T : class;
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        Task<int> SaveChangesAsync();
        void Refresh();
        void Dispose();
        IEnumerable<DbEntityValidationResult> GetValidationErrors();
        void ApplyStateChanges();
        void ExecuteSqlCommand(string command, object[] parameterArray);
        Task<int> ExecuteSqlCommandAsync(string command, object[] parameterArray);
        DataTable ExecuteStoredProcedure(string storedProcedureName, SqlParameter[] parameterArray);
        List<TEntity> ExecuteStoredProcedure<TEntity>(string storedProcedureName, SqlParameter[] parameterArray) where TEntity : class;
        List<TEntity> ExecuteStoredProcedure<TEntity>(string storedProcedureName, string connectionStringKey,
            SqlParameter[] parameterArray) where TEntity : class;
        int ExecuteStoredProcedureNonQuery(string storedProcedureName,
            SqlParameter[] parameterArray);
        object ExecuteStoredProcedureScalar(string storedProcedureName,
            SqlParameter[] parameterArray);
        object ExecuteSqlCommandScalar(string command, object[] parameterArray);
        void BulkInsert<TEntity>(IEnumerable<TEntity> entities, SqlConnection connection = null);
        void BulkInsert<TEntity>(IEnumerable<TEntity> entities, string tableName, SqlConnection connection = null);
        List<TEntity> ExecuteSqlCommand<TEntity>(string command, SqlParameter[] parameterArray) where TEntity : class;
    }
}