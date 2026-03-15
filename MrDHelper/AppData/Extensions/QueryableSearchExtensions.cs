using Microsoft.EntityFrameworkCore;
using MrDHelper;
using System.Linq.Expressions;

namespace MrDHelper.AppData.Extensions;

public static class QueryableSearchExtensions
{
    /// <summary>
    /// Searches a single keyword across multiple string properties on the client side.
    /// Matching uses <see cref="StringSearch.Normalize(string)"/> for normalization such as removing diacritics and lowercasing.
    /// </summary>
    /// <typeparam name="T">Source element type.</typeparam>
    /// <param name="source">Query source.</param>
    /// <param name="keyword">Keyword to search for.</param>
    /// <param name="stringProperties">String properties to search, including navigation or computed properties.</param>
    /// <returns>A filtered `IQueryable` after materializing data to the client.</returns>
    /// <remarks>
    /// Calls <c>AsEnumerable()</c> to normalize values, so filtering runs on the client side.
    /// EF can no longer translate subsequent operations to SQL.
    /// As a result, any later <c>Include</c>, <c>OrderBy</c>, or <c>Skip/Take</c> also runs on the client.
    /// Use this only for small result sets or after applying server-side narrowing first.
    /// </remarks>
    /// <example>
    /// db.Reports
    ///   .Where(r => r.CreatedAt >= cutoff) // server-side first
    ///   .ApplySearch("ha noi", r => r.Title, r => r.Summary); // client-side normalization
    /// </example>
    public static IQueryable<T> ApplySearchClientSide<T>(
        this IQueryable<T> source,
        string? keyword,
        params Expression<Func<T, string?>>[] stringProperties)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (string.IsNullOrWhiteSpace(keyword) || stringProperties is null || stringProperties.Length == 0)
            return source;

        var normKey = keyword.Normalize();
        if (normKey.Length == 0) return source;

        // Pre-compile accessors once for better performance.
        var accessors = stringProperties.Select(p => p.Compile()).ToArray();

        return source
            .AsEnumerable() // Switch to client-side filtering so Normalize can be used.
            .Where(item =>
            {
                foreach (var get in accessors)
                {
                    var val = get(item);
                    if (val is null) continue;

                    var normVal = val.Normalize();
                    if (normVal.Contains(normKey))
                        return true;
                }
                return false;
            })
            .AsQueryable();
    }

    /// <summary>
    /// Searches multiple keywords, split by whitespace, new lines, `,`, or `;`,
    /// using OR logic across multiple string properties on the client side.
    /// </summary>
    /// <remarks>
    /// Uses <c>AsEnumerable()</c> to normalize and split tokens, so filtering runs on the client side just like the method above.
    /// Be careful with large datasets. Consider limiting keyword count or total length to avoid performance degradation.
    /// Matching behavior may differ from server-side LIKE because normalization removes diacritics and lowercases text.
    /// </remarks>
    /// <example>
    /// db.Reports.ApplySearchAnyClientSide("ha noi e30", r => r.Title, r => r.Summary);
    /// // matches if it contains "ha", "noi", or "e30"
    /// </example>

    public static IQueryable<T> ApplySearchAnyClientSide<T>(
        this IQueryable<T> source,
        string? keywords,
        params Expression<Func<T, string?>>[] stringProperties)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        if (string.IsNullOrWhiteSpace(keywords) || stringProperties is null || stringProperties.Length == 0)
            return source;

        var keys = keywords
            .Split(new[] { ' ', '\t', '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(StringHelper.Normalize)
            .Where(k => k.Length > 0)
            .Distinct()
            .ToArray();

        if (keys.Length == 0) return source;

        var accessors = stringProperties.Select(p => p.Compile()).ToArray();

        return source
            .AsEnumerable()
            .Where(item =>
            {
                foreach (var get in accessors)
                {
                    var val = get(item);
                    if (val is null) continue;

                    var normVal = val.Normalize();
                    // Only one keyword needs to match.
                    if (keys.Any(k => normVal.Contains(k)))
                        return true;
                }
                return false;
            })
            .AsQueryable();
    }

    /// <summary>
    /// Searches a single keyword with OR logic across properties,
    /// building an expression that EF can translate to SQL as <c>prop LIKE '%keyword%'</c>.
    /// </summary>
    /// <remarks>
    /// Does not normalize values and therefore depends on the database collation.
    /// Case sensitivity and diacritic handling are controlled by the collation.
    /// If you need behavior similar to the client-side version, consider using a suitable collation,
    /// a normalized computed column, or a mapped custom SQL function.
    /// Note that <paramref name="keyword"/> participates directly in the LIKE pattern when it contains wildcards such as <c>%</c> or <c>_</c>.
    /// Escape the keyword if you need literal matching.
    /// </remarks>
    /// <example>
    /// db.Reports.ApplySearchAnyServerSide("ha", r => r.Title, r => r.Summary);
    /// // WHERE (Title LIKE '%ha%' OR Summary LIKE '%ha%')
    /// </example>

    public static IQueryable<T> ApplySearchAnyServerSide<T>(
        this IQueryable<T> source,
        string? keyword,
        params Expression<Func<T, string?>>[] stringProperties)
    {
        if (string.IsNullOrWhiteSpace(keyword) || stringProperties.Length == 0)
            return source;

        keyword = keyword.Trim();
        var param = Expression.Parameter(typeof(T), "x");

        // %term% pattern
        var likePattern = Expression.Constant($"%{keyword}%");

        Expression? combined = null;
        foreach (var propExpr in stringProperties)
        {
            // Replace the property expression parameter with the shared parameter.
            var visitor = new ReplaceParameterVisitor(propExpr.Parameters[0], param);
            var body = visitor.Visit(propExpr.Body)!;

            // Condition: prop != null && EF.Functions.Like(prop, pattern)
            var notNull = Expression.NotEqual(body, Expression.Constant(null, typeof(string)));

            var efFunctions = Expression.Property(null, typeof(EF), nameof(EF.Functions));
            var likeMethod = typeof(DbFunctionsExtensions).GetMethod(
                nameof(DbFunctionsExtensions.Like),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) })!;

            var likeCall = Expression.Call(likeMethod, efFunctions, body, likePattern);
            var andExpr = Expression.AndAlso(notNull, likeCall);

            combined = combined == null ? andExpr : Expression.OrElse(combined, andExpr);
        }

        if (combined == null) return source;
        var lambda = Expression.Lambda<Func<T, bool>>(combined, param);
        return source.Where(lambda);
    }

    /// <summary>
    /// Replaces the original parameter with a new parameter in an expression,
    /// allowing multiple property expressions to share one <see cref="ParameterExpression"/>
    /// so they can be combined with OR or AND inside a single lambda.
    /// </summary>

    private class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParam;
        private readonly ParameterExpression _newParam;
        public ReplaceParameterVisitor(ParameterExpression oldParam, ParameterExpression newParam)
        {
            _oldParam = oldParam;
            _newParam = newParam;
        }
        protected override Expression VisitParameter(ParameterExpression node)
            => node == _oldParam ? _newParam : base.VisitParameter(node);
    }
}
