using MrDHelper.Models;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace MrDHelper.GenericHelper;

public static class DisplayExtensions
{
    private static readonly ConcurrentDictionary<(Type Type, string MemberName), string> _displayNameCache = new();

    // =====================================================
    // 1) ENUM
    // =====================================================

    /// Gets the Display(Name) value for an enum member.
    public static string GetDisplayName(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var displayAttr = field?.GetCustomAttribute<DisplayAttribute>();
        return displayAttr?.GetName() ?? value.ToString();
    }

    /// Gets the Display(Order) value for an enum member.
    public static int GetDisplayOrder(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var displayAttr = field?.GetCustomAttribute<DisplayAttribute>();
        return displayAttr?.Order ?? int.MaxValue;
    }

    /// Gets enum members with Display/Order metadata, sorted by order.
    public static IEnumerable<DisplayEnumModel<TEnum>>
        GetDisplayOrderedProperties<TEnum>()
        where TEnum : struct, Enum
    {
        var enumType = typeof(TEnum);

        return enumType
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral) // Enum values are literals.
            .Select(f =>
            {
                var displayAttr = f.GetCustomAttribute<DisplayAttribute>();
                var name = displayAttr?.GetName() ?? f.Name;
                var order = displayAttr?.Order ?? int.MaxValue;
                var value = (TEnum)Enum.Parse(enumType, f.Name);

                return new DisplayEnumModel<TEnum>(value, name, order, f);
            })
            .OrderBy(x => x.Order)
            .ThenBy(x => x.DisplayName, StringComparer.CurrentCulture)
            .ToList();
    }

    /// Returns a compact `(Value, DisplayName)` list for enum dropdowns.
    public static List<(TEnum Value, string DisplayName)>
        GetValueLabelListForEnum<TEnum>()
        where TEnum : struct, Enum
        => GetDisplayOrderedProperties<TEnum>()
            .Select(x => (x.Value, x.DisplayName))
            .ToList();

    // =====================================================
    // 2) CLASS / RECORD: PROPERTIES
    // =====================================================

    /// Gets the Display(Name) value for a property in `T` (label/header).
    public static string GetDisplayName<T>(this T obj, string propertyName)
        where T : class
    {
        var prop = typeof(T).GetProperty(propertyName);
        var displayAttr = prop?.GetCustomAttribute<DisplayAttribute>();
        return displayAttr?.GetName() ?? propertyName;
    }

    /// Gets the Display(Name) value from `PropertyInfo`, using a cache to avoid repeated reflection.
    public static string GetDisplayName(this PropertyInfo property)
    {
        if (property == null) return string.Empty;

        var type = property.DeclaringType ?? property.ReflectedType;
        if (type == null) return property.Name;

        return _displayNameCache.GetOrAdd((type, property.Name), _ =>
        {
            var displayAttr = property.GetCustomAttribute<DisplayAttribute>();
            return displayAttr?.GetName() ?? property.Name;
        });
    }

    /// Gets the Display(Name) value by type and property name without requiring an instance.
    public static string GetDisplayName(this Type targetType, string propertyName)
    {
        if (targetType == null) return string.Empty;
        if (string.IsNullOrWhiteSpace(propertyName)) return propertyName ?? string.Empty;

        return _displayNameCache.GetOrAdd((targetType, propertyName), _ =>
        {
            var prop = targetType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null) return propertyName;

            var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
            return displayAttr?.GetName() ?? prop.Name;
        });
    }

    /// Gets the Display(Name) value from an expression such as `x => x.Property`.
    /// Also supports `x => (object)x.ValueTypeProperty`.
    public static string GetDisplayName<TModel>(Expression<Func<TModel, object?>> selector)
        where TModel : class
    {
        if (selector == null) return string.Empty;

        MemberExpression? memberExpr = selector.Body switch
        {
            MemberExpression m => m,
            UnaryExpression { NodeType: ExpressionType.Convert, Operand: MemberExpression m } => m,
            _ => null
        };

        if (memberExpr?.Member is PropertyInfo prop)
            return prop.GetDisplayName();

        return selector.ToString();
    }

    /// Gets the Display(Order) value for a property in `T`.
    public static int GetDisplayOrder<T>(this T obj, string propertyName)
        where T : class
    {
        var prop = typeof(T).GetProperty(propertyName);
        var displayAttr = prop?.GetCustomAttribute<DisplayAttribute>();
        return displayAttr?.Order ?? int.MaxValue;
    }

    /// Gets public instance properties, excluding `[ScaffoldColumn(false)]`, sorted by Display(Order).
    /// Call this on an instance so `T` can be inferred correctly.
    public static IEnumerable<DisplayClassModel>
        GetDisplayOrderedProperties<T>(this T obj)
        where T : class
    {
        return typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p =>
            {
                var scaffoldAttr = p.GetCustomAttribute<ScaffoldColumnAttribute>();
                return scaffoldAttr == null || scaffoldAttr.Scaffold;
            })
            .Select(p =>
            {
                var displayAttr = p.GetCustomAttribute<DisplayAttribute>();
                var name = displayAttr?.GetName() ?? p.Name;
                var order = displayAttr?.Order ?? int.MaxValue;

                return new DisplayClassModel(p.Name, name, order, p.PropertyType, p);
            })
            .OrderBy(x => x.Order)
            .ThenBy(x => x.DisplayName, StringComparer.CurrentCulture)
            .ToList();
    }

    /// Convenience overload that works from a `Type` without an instance.
    public static IEnumerable<DisplayClassModel>
        GetDisplayOrderedProperties(Type targetType)
    {
        return targetType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p =>
            {
                var scaffoldAttr = p.GetCustomAttribute<ScaffoldColumnAttribute>();
                return scaffoldAttr == null || scaffoldAttr.Scaffold;
            })
            .Select(p =>
            {
                var displayAttr = p.GetCustomAttribute<DisplayAttribute>();
                var name = displayAttr?.GetName() ?? p.Name;
                var order = displayAttr?.Order ?? int.MaxValue;

                return new DisplayClassModel(p.Name, name, order, p.PropertyType, p);
            })
            .OrderBy(x => x.Order)
            .ThenBy(x => x.DisplayName, StringComparer.CurrentCulture)
            .ToList();
    }

    /// Overload generic: GetDisplayOrderedProperties<TModel>()
    public static IEnumerable<DisplayClassModel>
        GetDisplayOrderedPropertiesForModel<TModel>()
        where TModel : class
        => GetDisplayOrderedProperties(typeof(TModel));

    // =====================================================
    // 3) STATIC CLASS / CLASS WITH CONST MEMBERS: CONSTANT FIELDS
    // =====================================================

    /// Gets public static `const string` members with `[Display]`, sorted by Display(Order).
    public static List<DisplayConstantModel>
        GetDisplayOrderedConstants(Type staticOrConstHolderType)
    {
        // Supports both static classes and regular classes because `const` members are still static.
        return staticOrConstHolderType
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string)) // Only const strings.
            .Select(f =>
            {
                var display = f.GetCustomAttribute<DisplayAttribute>();
                var order = display?.Order ?? int.MaxValue;
                var label = display?.GetName() ?? f.Name;  // GetName() supports ResourceType-based localization.
                var value = (string)f.GetRawConstantValue()!;
                return new DisplayConstantModel(value, label, order, f);
            })
            .OrderBy(x => x.Order)
            .ThenBy(x => x.DisplayName, StringComparer.CurrentCulture)
            .ToList();
    }

    /// Generic convenience wrapper for `GetDisplayOrderedConstants<TStatic>()`.
    public static List<DisplayConstantModel>
        GetDisplayOrderedConstants<TStatic>()
        => GetDisplayOrderedConstants(typeof(TStatic));

    /// Returns a compact `(Value, DisplayName)` list for constant-based dropdowns.
    public static List<(string Value, string DisplayName)>
        GetValueLabelListForConstants<TStatic>()
        => GetDisplayOrderedConstants<TStatic>()
            .Select(x => (x.Value, x.DisplayName))
            .ToList();
}
