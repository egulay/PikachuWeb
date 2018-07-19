using System.Data.Entity;
using Pikachu.Data.Framework.Repository.Interfaces;
using Unity;

namespace Pikachu.Data.Framework.Repository
{
    public abstract class PikachuRDBDataService
    {
        protected IUnitOfWork DataContext { get; private set; }
        protected void CreateSqlServiceProvider<TContext>() where TContext : DbContext, IDbContext
        {
            var container = new UnityContainer();
            container.RegisterType<IUnitOfWork, UnitOfWork>();
            container.RegisterType<IDbContext, TContext>();

            DataContext = container.Resolve<IUnitOfWork>();
        }
    }
}
