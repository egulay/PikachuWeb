using System.ComponentModel.DataAnnotations.Schema;
using Pikachu.Data.Framework.Repository.Helpers;
using Pikachu.Data.Framework.Repository.Interfaces;

namespace Pikachu.Data.Framework.Repository
{
    public abstract class EntityBase : IObjectState
    {
        [NotMapped]
        public ObjectState State { get; set; }
    }
}