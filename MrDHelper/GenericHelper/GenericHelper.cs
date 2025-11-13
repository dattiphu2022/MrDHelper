namespace MrDHelper.GenericHelper;

/// <summary>
/// Extension methods for generic usage.
/// </summary>
public static class ObjectHelper
{
    /// <summary>
    /// Check if the object is null
    /// </summary>
    /// <typeparam name="T">Input</typeparam>
    /// <param name="input">object to check</param>
    /// <returns>True if T is null, and vice versa.</returns>
    public static bool IsNull<T>(this T input)
    {
        return input is null;
    }

    /// <summary>
    /// Check if the object is not null
    /// </summary>
    /// <typeparam name="T">Input</typeparam>
    /// <param name="input">object to check</param>
    /// <returns>False if T is null, and vice versa.</returns>
    public static bool NotNull<T>(this T input)
    {
        return !input.IsNull();
    }
}
