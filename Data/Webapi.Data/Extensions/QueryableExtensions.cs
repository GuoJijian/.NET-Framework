using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Webapi.Core.Utils;

namespace Webapi.Data
{
    public static class QueryableExtensions
    {

        /// <summary>
        /// 将查询语句转换成Sql, 便于进一步的Sql拼接
        /// <seealso href="https://gist.github.com/rionmonster/2c59f449e67edf8cd6164e9fe66c545a#gistcomment-3109335" />
        /// </summary>
        /// <param name="query"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static string ToSql<TEntity>(this IQueryable<TEntity> query) where TEntity : class
        {
            using var enumerator = query.Provider.Execute<IEnumerable<TEntity>>(query.Expression).GetEnumerator();
            var relationalCommandCache = enumerator.Private("_relationalCommandCache");
            var selectExpression = relationalCommandCache.Private<SelectExpression>("_selectExpression");
            var factory = relationalCommandCache.Private<IQuerySqlGeneratorFactory>("_querySqlGeneratorFactory");

            var sqlGenerator = factory.Create();
            var command = sqlGenerator.GetCommand(selectExpression);

            var sql = command.CommandText;
            return sql;
        }

        /// <summary>
        /// 增加
        /// </summary>
        /// <param name="query"></param>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static (string, IReadOnlyDictionary<string, object>) ToSqlWithParams<TEntity>(this IQueryable<TEntity> query)
        {
            using var enumerator = query.Provider.Execute<IEnumerable<TEntity>>(query.Expression).GetEnumerator();
            var relationalCommandCache = enumerator.Private("_relationalCommandCache");
            var selectExpression = relationalCommandCache.Private<SelectExpression>("_selectExpression");
            var factory = relationalCommandCache.Private<IQuerySqlGeneratorFactory>("_querySqlGeneratorFactory");
            var queryContext = enumerator.Private<RelationalQueryContext>("_relationalQueryContext");

            var sqlGenerator = factory.Create();
            var command = sqlGenerator.GetCommand(selectExpression);

            var parametersDict = queryContext.ParameterValues;
            var sql = command.CommandText;
            return (sql, parametersDict);
        }
    }
}
