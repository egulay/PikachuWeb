using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Pikachu.Data.Framework.Repository.Interfaces;

namespace Pikachu.Data.Framework.Repository
{
    public class UnitOfWork : IDisposable, IUnitOfWork
    {
        private readonly IDbContext _context;
        private Hashtable _repositories;
        private bool _disposed;

        public UnitOfWork(IDbContext context)
        {
            _context = context;
            _context.Configuration.LazyLoadingEnabled = false;
            InstanceId = Guid.NewGuid();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }

        public Task<int> ExecuteSqlCommandAsync(string command, object[] parameterArray)
        {
            var parameters = parameterArray ?? new object[] { };
            return _context.ExecuteSqlCommandAsync(command, parameters);
        }

        public DataTable ExecuteStoredProcedure(string storedProcedureName, SqlParameter[] parameterArray)
        {
            return _context.ExecuteStoredProcedure(storedProcedureName, parameterArray);
        }

        public List<T> ExecuteStoredProcedure<T>(string storedProcedureName, SqlParameter[] parameterArray)
            where T : class
        {
            return _context.ExecuteStoredProcedure<T>(storedProcedureName, parameterArray);
        }

        public List<T> ExecuteStoredProcedure<T>(string storedProcedureName, string connectionStringKey,
            SqlParameter[] parameterArray) where T : class
        {
            return _context.ExecuteStoredProcedure<T>(storedProcedureName, connectionStringKey, parameterArray);
        }

        public int ExecuteStoredProcedureNonQuery(string storedProcedureName, SqlParameter[] parameterArray)
        {
            return _context.ExecuteStoredProcedureNonQuery(storedProcedureName, parameterArray);
        }

        public object ExecuteStoredProcedureScalar(string storedProcedureName,
            SqlParameter[] parameterArray)
        {
            return _context.ExecuteStoredProcedureScalar(storedProcedureName, parameterArray);
        }

        public object ExecuteSqlCommandScalar(string command, object[] parameterArray)
        {
            return _context.ExecuteSqlCommandScalar(command, parameterArray);
        }

        public List<TEntity> ExecuteSqlCommand<TEntity>(string command, SqlParameter[] parameterArray) where TEntity : class
        {
            return _context.ExecuteSqlCommand<TEntity>(command, parameterArray);
        }

        public void ExecuteSqlCommand(string command, object[] parameterArray)
        {
            var parameters = parameterArray ?? new object[] { };
            _context.ExecuteSqlCommand(command, parameters);
        }

        public Guid InstanceId { get; }

        public void Refresh()
        {
            _context.Refresh();
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public Task<int> SaveAsync()
        {
            return _context.SaveChangesAsync();
        }

        public Task<int> SaveAsync(CancellationToken cancellationToken)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }

        public IRepository<TEntity> Repository<TEntity>() where TEntity : class, new()
        {
            if (_repositories == null)
            {
                _repositories = new Hashtable();
            }

            var type = typeof(TEntity).Name;

            if (_repositories.ContainsKey(type))
            {
                return (IRepository<TEntity>)_repositories[type];
            }

            var repositoryType = typeof(Repository<>);
            _repositories.Add(type, Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _context));

            return (IRepository<TEntity>)_repositories[type];
        }

        public IEnumerable<DbEntityValidationResult> GetValidationErrors()
        {
            return _context.GetValidationErrors();
        }

        public void BulkInsert<TEntity>(IEnumerable<TEntity> entities, SqlConnection connection = null)
        {
            _context.BulkInsert(entities, connection);
        }

        public void BulkInsert<TEntity>(IEnumerable<TEntity> entities, string tableName, SqlConnection connection = null)
        {
            _context.BulkInsert(entities, tableName, connection);
        }
    }
}