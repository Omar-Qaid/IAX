using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Shared.Application.Querying;
using Mapster;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;

namespace IAX.IXApi.Bootstrap.Extensions
{
    public static class QueryableExtensions
    {

        #region Data Grid
        public static IQueryable<T> WhereDataGrid<T>(this IQueryable<T> query, QueryFilterDto queryFilter)
        {
            // Free-text search across all string properties of T (e.g. lookup popup).
            if (!string.IsNullOrWhiteSpace(queryFilter.SearchTerm))
            {
                var searchExpr = BuildSearchExpression<T>(queryFilter.SearchTerm.Trim());
                if (searchExpr != null) query = query.Where(searchExpr);
            }

            var filterExpression = DataGrid.DataGridFilterExpression<T>(queryFilter.Filters ?? new());
            if (filterExpression != null)
            {
                query = query.Where(filterExpression);
            }
            var sortExpr = DataGrid.DataGridSortExpression<T>(queryFilter.SortField ?? string.Empty);
            if (sortExpr == null) return query;

            return queryFilter.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(sortExpr)
                : query.OrderBy(sortExpr);
        }

        /// <summary>
        /// Builds an `x => x.Prop1.Contains(term) || x.Prop2.Contains(term) || ...`
        /// expression over every readable string property on T. Used by SearchTerm.
        /// </summary>
        private static Expression<Func<T, bool>>? BuildSearchExpression<T>(string term)
        {
            var stringProps = typeof(T)
                .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(string) && p.CanRead)
                .ToArray();
            if (stringProps.Length == 0) return null;

            var param = Expression.Parameter(typeof(T), "x");
            var termConst = Expression.Constant(term, typeof(string));
            var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;

            Expression? body = null;
            foreach (var prop in stringProps)
            {
                var propAccess = Expression.Property(param, prop);
                // Guard nulls: prop != null && prop.Contains(term)
                var notNull = Expression.NotEqual(propAccess, Expression.Constant(null, typeof(string)));
                var contains = Expression.Call(propAccess, containsMethod, termConst);
                var clause = Expression.AndAlso(notNull, contains);
                body = body == null ? clause : Expression.OrElse(body, clause);
            }
            return body == null ? null : Expression.Lambda<Func<T, bool>>(body, param);
        }
        #endregion
    }
}
