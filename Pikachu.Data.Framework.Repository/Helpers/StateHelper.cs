using System.Data.Entity;

namespace Pikachu.Data.Framework.Repository.Helpers
{
    public class StateHelper
    {
        public static EntityState ConvertState(ObjectState state)
        {
            switch (state)
            {
                case ObjectState.Added:
                    return EntityState.Added;
                case ObjectState.Modified:
                    return EntityState.Modified;
                case ObjectState.Deleted:
                    return EntityState.Deleted;
                case ObjectState.Detached:
                    return EntityState.Detached;
                case ObjectState.Unchanged:
                    return EntityState.Unchanged;
                default:
                    return EntityState.Unchanged;
            }
        }
    }
}