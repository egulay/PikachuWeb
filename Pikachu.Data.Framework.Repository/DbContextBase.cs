using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Pikachu.Data.Framework.Repository.Helpers;
using Pikachu.Data.Framework.Repository.Interfaces;

namespace Pikachu.Data.Framework.Repository
{
    public class DbContextBase : DbContext, IDbContext
    {
        public DbContextBase(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            InstanceId = Guid.NewGuid();
        }

        public Guid InstanceId { get; }

        public void ApplyStateChanges()
        {
            foreach (DbEntityEntry dbEntityEntry in ChangeTracker.Entries())
            {
                var entityState = dbEntityEntry.Entity as IObjectState;
                if (entityState == null)
                    throw new InvalidCastException("All entites must implement the IObjectState interface, " +
                                                   "this interface must be implemented so each entites state can explicitely determined when updating graphs.");
  
                dbEntityEntry.State = StateHelper.ConvertState(entityState.State);
            }
        }

        public void ExecuteSqlCommand(string command, object[] parameterArray)
        {
            var parameters = parameterArray ?? new object[] { };
            Database.ExecuteSqlCommand(command, parameters);
        }

        public Task<int> ExecuteSqlCommandAsync(string command, object[] parameterArray)
        {
            var parameters = parameterArray ?? new object[] { };
            return Database.ExecuteSqlCommandAsync(command, parameters);
        }

        public DataTable ExecuteStoredProcedure(string storedProcedureName, SqlParameter[] parameterArray)
        {
            try
            {
                var dataTable = new DataTable();

                using (DbCommand cmd = new SqlCommand(storedProcedureName))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = Database.Connection;
                    cmd.CommandTimeout = int.MaxValue;

                    if (parameterArray != null)
                        cmd.Parameters.AddRange(parameterArray);

                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();

                    using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        if (reader.HasRows)
                            dataTable.Load(reader);
                    }
                }

                return dataTable;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<T> ExecuteStoredProcedure<T>(string storedProcedureName, SqlParameter[] parameterArray)
            where T : class
        {
            try
            {
                var list = new List<T>();
                using (DbCommand cmd = new SqlCommand(storedProcedureName))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = Database.Connection;
                    cmd.CommandTimeout = int.MaxValue;

                    if (parameterArray != null)
                        cmd.Parameters.AddRange(parameterArray);

                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();

                    using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows) return list;
                        while (reader.Read())
                        {
                            var obj = Activator.CreateInstance<T>();
                            foreach (var prop in
                                obj.GetType().GetFilteredProperties().Where(prop => !Equals(reader[prop.Name], DBNull.Value)))
                            {
                                prop.SetValue(obj, reader[prop.Name], null);
                            }

                            list.Add(obj);
                        }
                        reader.Close();
                    }
                }
                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<T> ExecuteStoredProcedure<T>(string storedProcedureName, string connectionstringKey, SqlParameter[] parameterArray)
            where T : class
        {
            try
            {
                var list = new List<T>();
                var cnnStr = ConfigurationManager.
                    ConnectionStrings[connectionstringKey].ConnectionString;
                using (var cnn = new SqlConnection(cnnStr))
                using (var cmd = new SqlCommand(storedProcedureName, cnn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = int.MaxValue;

                    if (parameterArray != null)
                        cmd.Parameters.AddRange(parameterArray);

                    if (cnn.State != ConnectionState.Open)
                        cnn.Open();

                    using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows) return list;
                        while (reader.Read())
                        {
                            var obj = Activator.CreateInstance<T>();
                            foreach (var prop in
                                obj.GetType()
                                    .GetFilteredProperties()
                                    .Where(prop => !Equals(reader[prop.Name], DBNull.Value)))
                            {
                                prop.SetValue(obj, reader[prop.Name], null);
                            }

                            list.Add(obj);
                        }
                        reader.Close();
                    }
                }

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int ExecuteStoredProcedureNonQuery(string storedProcedureName, SqlParameter[] parameterArray)
        {
            try
            {
                using (DbCommand cmd = new SqlCommand(storedProcedureName))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = Database.Connection;
                    cmd.CommandTimeout = int.MaxValue;

                    if (parameterArray != null)
                        cmd.Parameters.AddRange(parameterArray);

                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();

                    var result = cmd.ExecuteNonQuery();

                    cmd.Connection.Close();
                    cmd.Dispose();

                    return result;
                }
            }
            catch (Exception)
            {
                return -1;
            }
        }

        public object ExecuteStoredProcedureScalar(string storedProcedureName,
            SqlParameter[] parameterArray)
        {
            try
            {
                using (DbCommand cmd = new SqlCommand(storedProcedureName))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = Database.Connection;
                    cmd.CommandTimeout = int.MaxValue;

                    if (parameterArray != null)
                        cmd.Parameters.AddRange(parameterArray);

                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();

                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public object ExecuteSqlCommandScalar(string command, object[] parameterArray)
        {
            try
            {
                using (DbCommand cmd = new SqlCommand(command))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = Database.Connection;
                    cmd.CommandTimeout = int.MaxValue;

                    if (parameterArray != null)
                        cmd.Parameters.AddRange(parameterArray);

                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();

                    return cmd.ExecuteScalar();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<TEntity> ExecuteSqlCommand<TEntity>(string command, SqlParameter[] parameterArray) where TEntity : class
        {
            try
            {
                var list = new List<TEntity>();
                using (DbCommand cmd = new SqlCommand(command))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = Database.Connection;
                    cmd.CommandTimeout = int.MaxValue;

                    if (parameterArray != null)
                        cmd.Parameters.AddRange(parameterArray);

                    if (cmd.Connection.State != ConnectionState.Open)
                        cmd.Connection.Open();

                    using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        if (!reader.HasRows) return list;
                        while (reader.Read())
                        {
                            var obj = Activator.CreateInstance<TEntity>();
                            foreach (var prop in
                                obj.GetType().GetFilteredProperties().Where(prop => !Equals(reader[prop.Name], DBNull.Value)))
                            {
                                prop.SetValue(obj, reader[prop.Name], null);
                            }

                            list.Add(obj);
                        }
                        reader.Close();
                    }
                }
                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public new IDbSet<T> Set<T>() where T : class
        {
            return base.Set<T>();
        }

        public override int SaveChanges()
        {
            ApplyStateChanges();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync()
        {
            ApplyStateChanges();
            return base.SaveChangesAsync();
        }

        public void Refresh()
        {
            var context = ((IObjectContextAdapter)this).ObjectContext;
            var refreshableObjects = ChangeTracker.Entries().Select(c => c.Entity).ToList();
            context.Refresh(RefreshMode.StoreWins, refreshableObjects);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            ApplyStateChanges();
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(DbModelBuilder builder)
        {
            builder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(builder);
        }

        public void BulkInsert<TEntity>(IEnumerable<TEntity> entities, SqlConnection connection = null)
        {
            var enumerable = entities as TEntity[] ?? entities.ToArray();
            if (!enumerable.Any())
            {
                return;
            }
            entities = enumerable.ToArray();

            connection = connection ?? new SqlConnection(Database.Connection.ConnectionString);

            if (connection.State != ConnectionState.Open)
                connection.Open();

            var t = entities.First().GetType();
            var tableAttribute = (TableAttribute) t.GetCustomAttributes(
                    typeof(TableAttribute), false)
                .Single();

            var bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = string.Concat("[dbo].[", tableAttribute.Name, "]")
            };

            var propertyInfos = t.GetProperties();
            var properties = propertyInfos.Where(EntityTypeFilter).ToArray();

            // prepare in-memory table
            var table = new DataTable();
            foreach (var property in properties)
            {
                var propertyType = property.PropertyType;
                if (propertyType.IsGenericType &&
                    propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    propertyType = Nullable.GetUnderlyingType(propertyType);
                }

                table.Columns.Add(new DataColumn(property.Name, propertyType));
            }

            // detect child collections and prepare lists for their entities
            var childCollectionProperties = propertyInfos.Where(p => p.PropertyType.Name.Contains("ICollection"))
                .ToArray();
            var children = childCollectionProperties.ToDictionary(childProperty => childProperty.Name,
                childProperty => new List<EntityBase>());

            // fill the data and child collections
            foreach (var entity in entities)
            {
                table.Rows.Add(properties.Select(
                        property => GetPropertyValue(
                            property.GetValue(entity, null)))
                    .ToArray());

                foreach (var childProperty in childCollectionProperties)
                {
                    var childEntities = (IEnumerable)childProperty.GetValue(entity);
                    foreach (var o in childEntities)
                    {
                        children[childProperty.Name].Add((EntityBase)o);
                    }
                }
            }

            // write the table to SQL server
            bulkCopy.WriteToServer(table);

            // recursively write child entries
            foreach (var childList in children.Values)
            {
                BulkInsert(childList, connection);
            }

            connection.Close();
        }

        private static bool EntityTypeFilter(PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType.Name.Contains("ICollection") ||
                propertyInfo.PropertyType.Name.Contains("ObjectState") ||
                propertyInfo.PropertyType.UnderlyingSystemType.IsSubclassOf(typeof(EntityBase)))
                return false;

            var attribute = Attribute.GetCustomAttribute(propertyInfo,
                typeof(AssociationAttribute)) as AssociationAttribute;

            if (attribute == null) return true;
            return attribute.IsForeignKey == false;
        }

        private static object GetPropertyValue(object obj)
        {
            return obj ?? DBNull.Value;
        }

        public void BulkInsert<TEntity>(IEnumerable<TEntity> entities, string tableName, SqlConnection connection = null)
        {
            var enumerable = entities as TEntity[] ?? entities.ToArray();
            if (!enumerable.Any())
            {
                return;
            }
            entities = enumerable.ToArray();

            connection = connection ?? new SqlConnection(Database.Connection.ConnectionString);

            if (connection.State != ConnectionState.Open)
                connection.Open();

            var t = entities.First().GetType();
            var bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = tableName
            };

            var propertyInfos = t.GetProperties();
            var properties = propertyInfos.Where(EntityTypeFilter).ToArray();

            // prepare in-memory table
            var table = new DataTable();
            foreach (var property in properties)
            {
                var propertyType = property.PropertyType;
                if (propertyType.IsGenericType &&
                    propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    propertyType = Nullable.GetUnderlyingType(propertyType);
                }

                table.Columns.Add(new DataColumn(property.Name, propertyType));
            }

            // detect child collections and prepare lists for their entities
            var childCollectionProperties = propertyInfos.Where(p => p.PropertyType.Name.Contains("ICollection"))
                .ToArray();
            var children = childCollectionProperties.ToDictionary(childProperty => childProperty.Name,
                childProperty => new List<EntityBase>());

            // fill the data and child collections
            foreach (var entity in entities)
            {
                table.Rows.Add(properties.Select(
                        property => GetPropertyValue(
                            property.GetValue(entity, null)))
                    .ToArray());

                foreach (var childProperty in childCollectionProperties)
                {
                    var childEntities = (IEnumerable)childProperty.GetValue(entity);
                    foreach (var o in childEntities)
                    {
                        children[childProperty.Name].Add((EntityBase)o);
                    }
                }
            }

            // write the table to SQL server
            bulkCopy.WriteToServer(table);

            // recursively write child entries
            foreach (var childList in children.Values)
            {
                BulkInsert(childList, connection);
            }

            connection.Close();
        }
    }
}