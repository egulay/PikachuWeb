using System.ComponentModel.DataAnnotations.Schema;
using Pikachu.Data.Framework.Repository.Helpers;

namespace Pikachu.Data.Framework.Repository.Interfaces
{
    public interface IObjectState
    {
        [NotMapped]
        ObjectState State { get; set; }
    }
}