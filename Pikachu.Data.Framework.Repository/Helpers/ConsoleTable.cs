﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Pikachu.Data.Framework.Repository.Helpers
{
    public class ConsoleTable
    {
        public IList<object> Columns { get; protected set; }
        public IList<object[]> Rows { get; protected set; }

        public ConsoleTable(params string[] columns)
        {
            Columns = new List<object>(columns);
            Rows = new List<object[]>();
        }

        public ConsoleTable AddColumn(IEnumerable<string> names)
        {
            foreach (var name in names)
                Columns.Add(name);
            return this;
        }

        public ConsoleTable AddRow(params object[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (!Columns.Any())
                throw new Exception("Please set the columns first");

            if (Columns.Count != values.Length)
                throw new Exception(
                    $"The number columns in the row ({Columns.Count}) does not match the values ({values.Length}");

            Rows.Add(values);
            return this;
        }

        public static ConsoleTable From<T>(IEnumerable<T> values)
        {
            var table = new ConsoleTable();

            var columns = GetColumns<T>();

            var enumerable = columns as string[] ?? columns.ToArray();
            table.AddColumn(enumerable);

            foreach (
                var propertyValues in values.Select(value => enumerable.Select(column => GetColumnValue<T>(value, column)))
            )
                table.AddRow(propertyValues.ToArray());

            return table;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            var columnLengths = ColumnLengths();

            var format = Enumerable.Range(0, Columns.Count)
                             .Select(i => " | {" + i + ",-" + columnLengths[i] + "}")
                             .Aggregate((s, a) => s + a) + " |";

            var maxRowLength = Math.Max(0, Rows.Any() ? Rows.Max(row => string.Format(format, row).Length) : 0);
            var columnHeaders = string.Format(format, Columns.ToArray());

            var longestLine = Math.Max(maxRowLength, columnHeaders.Length);

            var results = Rows.Select(row => string.Format(format, row)).ToList();

            var divider = " " + string.Join("", Enumerable.Repeat("-", longestLine - 1)) + " ";

            builder.AppendLine(divider);
            builder.AppendLine(columnHeaders);

            foreach (var row in results)
            {
                builder.AppendLine(divider);
                builder.AppendLine(row);
            }

            builder.AppendLine(divider);
            builder.AppendLine("");
            builder.AppendFormat(" Count: {0}", Rows.Count);

            return builder.ToString();
        }

        public string ToMarkDownString()
        {
            var builder = new StringBuilder();

            var columnLengths = ColumnLengths();

            var format = Format(columnLengths);

            var columnHeaders = string.Format(format, Columns.ToArray());

            var results = Rows.Select(row => string.Format(format, row)).ToList();

            var divider = Regex.Replace(columnHeaders, @"[^|]", "-");

            builder.AppendLine(columnHeaders);
            builder.AppendLine(divider);
            results.ForEach(row => builder.AppendLine(row));

            return builder.ToString();
        }

        public string ToStringAlternative()
        {
            var builder = new StringBuilder();

            var columnLengths = ColumnLengths();

            var format = Format(columnLengths);

            var columnHeaders = string.Format(format, Columns.ToArray());

            var results = Rows.Select(row => string.Format(format, row)).ToList();

            var divider = Regex.Replace(columnHeaders, @"[^|]", "-");
            var dividerPlus = divider.Replace("|", "+");

            builder.AppendLine(dividerPlus);
            builder.AppendLine(columnHeaders);

            foreach (var row in results)
            {
                builder.AppendLine(dividerPlus);
                builder.AppendLine(row);
            }
            builder.AppendLine(dividerPlus);

            return builder.ToString();
        }

        private string Format(IReadOnlyList<int> columnLengths)
        {
            var format = (Enumerable.Range(0, Columns.Count)
                              .Select(i => " | {" + i + ",-" + columnLengths[i] + "}")
                              .Aggregate((s, a) => s + a) + " |").Trim();
            return format;
        }

        private List<int> ColumnLengths()
        {
            var columnLengths = Columns
                .Select((t, i) => Rows.Select(x => x[i])
                    .Union(Columns)
                    .Where(x => x != null)
                    .Select(x => x.ToString().Length).Max())
                .ToList();
            return columnLengths;
        }

        public void Write(Format format = Helpers.Format.Default)
        {
            switch (format)
            {
                case Helpers.Format.Default:
                    Console.WriteLine(ToString());
                    break;
                case Helpers.Format.MarkDown:
                    Console.WriteLine(ToMarkDownString());
                    break;
                case Helpers.Format.Alternative:
                    Console.WriteLine(ToStringAlternative());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        private static IEnumerable<string> GetColumns<T>()
        {
            return typeof(T).GetProperties().Select(x => x.Name).ToArray();
        }

        private static object GetColumnValue<T>(object target, string column)
        {
            return typeof(T).GetProperty(column).GetValue(target, null);
        }
    }

    public enum Format
    {
        Default = 0,
        MarkDown = 1,
        Alternative = 2
    }

}
