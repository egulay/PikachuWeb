using System;
using System.Linq;
using System.Reflection;

namespace Pikachu.Data.Framework.Repository.Helpers
{
    public class DoNotIncludeAttribute : Attribute { }

    public static class TypeExtensions
    {
        public static PropertyInfo[] GetFilteredProperties(this Type type)
        {
            return type.GetProperties().Where(pi => !Attribute.IsDefined(pi, typeof(DoNotIncludeAttribute))).ToArray();
        }
    }
}
