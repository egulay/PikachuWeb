using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Ext.Net;
using Ext.Net.MVC;
using Pikachu.Data.Framework.Repository.Helpers;
using Pikachu.Data.Framework.Repository.Interfaces;
using RenderTest.Data.Models;
using RenderTest.Web.Models;

namespace RenderTest.Web.Controllers
{
    public class HomeController : RenderTestBaseController
    {
        public HomeController(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ReadTestData(StoreRequestParameters parameters)
        {
            var gridFilters = parameters.GridFilters;
            var filterString = new StringBuilder();
            var limit = parameters.Limit;
            int count;

            IQueryable<TestTable> rangeData;

            if (gridFilters != null)
            {
                Parallel.ForEach(gridFilters.Conditions, (condition) =>
                {
                    var comparison = condition.Comparison;
                    var field = condition.Field;

                    object value;
                    switch (condition.Type)
                    {
                        case FilterType.Boolean:
                            value = condition.Value<bool>();
                            filterString.Append(string.Concat(field, ".Equals(", value.ToString(), ") and "));
                            break;
                        case FilterType.Date:
                            value = condition.Value<DateTime>();
                            switch (comparison)
                            {
                                case Comparison.Eq:
                                    filterString.Append(string.Concat(field, ".Equals(DateTime(",
                                        DateTime.Parse(value.ToString()).Year, ",",
                                        DateTime.Parse(value.ToString()).Month, ",",
                                        DateTime.Parse(value.ToString()).Day, ")) and "));
                                    break;
                                case Comparison.Gt:
                                    filterString.Append(string.Concat(field, " >= DateTime(",
                                        DateTime.Parse(value.ToString()).Year, ",",
                                        DateTime.Parse(value.ToString()).Month, ",",
                                        DateTime.Parse(value.ToString()).Day, ") and "));
                                    break;
                                case Comparison.Lt:
                                    filterString.Append(string.Concat(field, " <= DateTime(",
                                        DateTime.Parse(value.ToString()).Year, ",",
                                        DateTime.Parse(value.ToString()).Month, ",",
                                        DateTime.Parse(value.ToString()).Day, ") and "));
                                    break;
                            }

                            break;
                        case FilterType.Number:
                            value = condition.Value<object>();
                            switch (comparison)
                            {
                                case Comparison.Eq:
                                    filterString.Append(string.Concat(field, ".Equals(", value.ToString(), ") and "));
                                    break;
                                case Comparison.Gt:
                                    filterString.Append(string.Concat(field, " >= ", value.ToString(), " and "));
                                    break;
                                case Comparison.Lt:
                                    filterString.Append(string.Concat(field, " <= ", value.ToString(), " and "));
                                    break;
                            }

                            break;
                        case FilterType.String:
                            value = condition.Value<string>();
                            filterString.Append(string.Concat(field, @".Contains(", "\"", value, "\"", ") and "));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                });

                var filterQuery = filterString.ToString()
                    .Substring(0, filterString.ToString().LastIndexOf("and", StringComparison.Ordinal)).Trim();

                if (parameters.Sort.Length > 0) // Filter with sort
                {
                    var sorter = parameters.Sort[0];
                    var direction = sorter.Direction;

                    rangeData =
                        DataContext.Repository<TestTable>().Query().OrderBy(DataContext.Repository<TestTable>()
                                .Query()
                                .CompileSingleOrderBy(sorter.Property, direction.ToString()))
                            .Get().Where(filterQuery).AsQueryable();

                    count = rangeData.Count();

                    if (sorter.Direction == SortDirection.ASC)
                        rangeData = (parameters.Start < 0 || limit < 0)
                            ? rangeData.OrderBy(sorter.Property).Skip(0).Take(limit)
                            : rangeData.OrderBy(sorter.Property).Skip(parameters.Start).Take(limit);
                    else
                        rangeData = (parameters.Start < 0 || limit < 0)
                            ? rangeData.OrderBy(string.Concat(sorter.Property, " descending")).Skip(0).Take(limit)
                            : rangeData.OrderBy(string.Concat(sorter.Property, " descending")).Skip(parameters.Start)
                                .Take(limit);
                }
                else // Filter without sort
                {
                    rangeData = DataContext.Repository<TestTable>().Query().OrderBy(o => o.OrderBy(i => i.Id))
                        .Get().Where(filterQuery).AsQueryable();

                    count = rangeData.Count();

                    rangeData = (parameters.Start < 0 || limit < 0)
                        ? rangeData.OrderBy(o => o.Id).Skip(0).Take(limit)
                        : rangeData.OrderBy(o => o.Id).Skip(parameters.Start).Take(limit);
                }
            }
            else
            {
                if (parameters.Sort.Length > 0) // Sort without filter
                {
                    var sorter = parameters.Sort[0];
                    var direction = sorter.Direction;

                    rangeData = (parameters.Start < 0 || limit < 0)
                        ? DataContext.Repository<TestTable>().Query().OrderBy(DataContext.Repository<TestTable>()
                                .Query()
                                .CompileSingleOrderBy(sorter.Property, direction.ToString()))
                            .GetPage(1, limit, out count).AsQueryable()
                        : DataContext.Repository<TestTable>().Query().OrderBy(DataContext.Repository<TestTable>()
                                .Query()
                                .CompileSingleOrderBy(sorter.Property, direction.ToString()))
                            .GetPage(parameters.Page, limit, out count).AsQueryable();
                }
                else // Get default data without sort or filter
                {
                    rangeData = (parameters.Start < 0 || limit < 0)
                        ? DataContext.Repository<TestTable>()
                            .Query()
                            .OrderBy(o => o.OrderBy(i => i.Id))
                            .GetPage(1, limit, out count)
                            .AsQueryable()
                        : DataContext.Repository<TestTable>()
                            .Query()
                            .OrderBy(o => o.OrderBy(i => i.Id))
                            .GetPage(parameters.Page, limit, out count)
                            .AsQueryable();
                }
            }

            return this.Store(rangeData.Select(t => new TestTableModel
            {
                Id = t.Id,
                MyStringField = t.MyStringField,
                MyDateField = t.MyDateField,
                MyBoolField = t.MyBoolField,
                MyIntField = t.MyIntField,
                MyMoneyField = t.MyMoneyField
            }).ToList(), count);
        }
    }
}