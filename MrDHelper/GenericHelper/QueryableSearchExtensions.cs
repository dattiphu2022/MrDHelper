using Microsoft.EntityFrameworkCore;
using MrDHelper;
using System.Linq.Expressions;

namespace NKE.BlazorUI.Extensions;

public static class QueryableSearchExtensions
{
    /// <summary>
    /// Tìm kiếm theo 1 từ khóa trên nhiều thuộc tính string (client-side).
    /// So sánh dựa trên <see cref="StringSearch.Normalize(string)"/> (ví dụ: bỏ dấu, lower-case).
    /// </summary>
    /// <typeparam name="T">Kiểu phần tử nguồn.</typeparam>
    /// <param name="source">Nguồn truy vấn.</param>
    /// <param name="keyword">Từ khóa cần tìm (một từ/chuỗi).</param>
    /// <param name="stringProperties">Các thuộc tính string cần tìm (có thể là navigation/computed).</param>
    /// <returns>IQueryable đã lọc (đã materialize sang client trước khi lọc).</returns>
    /// <remarks>
    /// Gọi <c>AsEnumerable()</c> để Normalize ⇒ lọc **phía client**:
    /// — EF** không thể** tiếp tục dịch sang SQL.
    /// Vì vậy mọi <c>Include</c>, <c>OrderBy</c>, <c>Skip/Take</c> sau đó đều chạy **client-side**.
    /// Chỉ dùng với tập nhỏ hoặc sau khi đã giới hạn bằng điều kiện server-side trước.
    /// </remarks>
    /// <example>
    /// db.Reports
    ///   .Where(r => r.CreatedAt >= cutoff) // server-side trước
    ///   .ApplySearch("ha noi", r => r.Title, r => r.Summary); // client-side normalize
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

        // pre-compile accessors 1 lần để tối ưu
        var accessors = stringProperties.Select(p => p.Compile()).ToArray();

        return source
            .AsEnumerable() // chuyển sang client filtering (để gọi Normalize)
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
    /// Tìm kiếm với **nhiều từ khóa** (phân tách bởi khoảng trắng, xuống dòng, ',' ';')
    /// theo logic **OR** trên nhiều thuộc tính string (client-side).
    /// </summary>
    /// <remarks>
    /// Dùng <c>AsEnumerable()</c> để Normalize và tách từ ⇒ lọc **phía client** (giống hàm trên).
    /// Hạn chế với tập lớn. Cân nhắc giới hạn số từ khóa/độ dài tổng để tránh degrade hiệu năng.
    /// Hành vi khớp có Normalize (bỏ dấu, lower-case...) nên có thể khác so với server-side LIKE.
    /// </remarks>
    /// <example>
    /// db.Reports.ApplySearchAnyClientSide("ha noi e30", r => r.Title, r => r.Summary);
    /// // match nếu chứa "ha" **hoặc** "noi" **hoặc** "e30"
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
                    // chỉ cần chứa 1 key
                    if (keys.Any(k => normVal.Contains(k)))
                        return true;
                }
                return false;
            })
            .AsQueryable();
    }

    /// <summary>
    /// Tìm kiếm **1 từ khóa** theo logic **OR giữa các thuộc tính**,
    /// tạo biểu thức để EF **dịch sang SQL**: <c>prop LIKE '%keyword%'</c>.
    /// </summary>
    /// <remarks>
    /// Không Normalize (phụ thuộc collation DB):
    /// - Độ nhạy hoa/thường và dấu (diacritics) do collation quyết định.
    /// - Nếu cần hành vi như client-side (bỏ dấu, case-insensitive), cần:
    ///   (1) dùng collation phù hợp, hoặc
    ///   (2) tạo computed column đã-normalize, hoặc
    ///   (3) map đến SQL function custom.
    /// Chú ý: <paramref name="keyword"/> nếu chứa wildcard (<c>%</c>, <c>_</c>) sẽ tham gia pattern của LIKE.
    /// Cân nhắc escape nếu muốn so khớp theo literal.
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
            // Thay thế parameter của propExpr bằng param chung
            var visitor = new ReplaceParameterVisitor(propExpr.Parameters[0], param);
            var body = visitor.Visit(propExpr.Body)!;

            // Điều kiện: prop != null && EF.Functions.Like(prop, pattern)
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
    /// Thay thế tham số cũ bằng tham số mới trong biểu thức,
    /// giúp gom nhiều biểu thức thuộc tính về chung một <see cref="ParameterExpression"/>
    /// để có thể kết hợp (OR/AND) trong cùng một lambda.
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
