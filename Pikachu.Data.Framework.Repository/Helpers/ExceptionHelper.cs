using System;
using System.Collections.Generic;
using System.Linq;

namespace Pikachu.Data.Framework.Repository.Helpers
{
    public static class ExceptionHelper
    {
        internal static IEnumerable<TSource> FromHierarchy<TSource>(
            this TSource source,
            Func<TSource, TSource> nextItem,
            Func<TSource, bool> canContinue)
        {
            for (var current = source; canContinue(current); current = nextItem(current))
            {
                yield return current;
            }
        }

        internal static IEnumerable<TSource> FromHierarchy<TSource>(
            this TSource source,
            Func<TSource, TSource> nextItem)
            where TSource : class
        {
            return FromHierarchy(source, nextItem, s => s != null);
        }

        public static string GetAllMessages(this Exception exception)
        {
            var messages = exception.FromHierarchy(ex => ex.InnerException)
                .Select(ex => ex.Message);
            return string.Join(Environment.NewLine, messages);
        }

        //public static string NotifyException(string functionName, string context, Exception ex)
        //{
        //    var source = string.Concat(functionName, ": ", context);
        //    source = string.Concat(source, Environment.NewLine, ex.GetAllMessages());
        //    //Debug.WriteLine(source);
        //    return source;
        //}
    }
}
