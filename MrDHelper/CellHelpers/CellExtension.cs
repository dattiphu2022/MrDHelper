using MrDHelper.CollectionHelpers.IEnummerableHelper;
using MrDHelper.GenericHelper;
using MrDHelper.Models;
using System.Collections.Generic;

namespace MrDHelper.CellHelpers
{
    public static class CellExtension
    {
        public static T? ConvertTo<T>(this Cell source) where T : class, new()
        {
            if (source.IsNull())
            {
                return null;
            }
            var ouput = new T();
            var properties = ouput?.GetType().GetProperties();
            if (properties.NotNull())
            {
                properties.ForEach(x =>
                {
                    x.SetValue(ouput, source[x.Name]);
                });
            }
            return ouput;
        }
        public static Cell? ConvertToCell<T>(this T? source) where T : class
        {
            if (source.IsNull())
            {
                return null;
            }
            Cell? output = new Cell();

            output.Datas = source.GetProperties();

            return output;
        }
        public static Dictionary<string, object?> GetProperties<T>(this T source)
        {
            Dictionary<string, object?> output = new Dictionary<string, object?>();

            if (source.IsNull())
            {
                return output;
            }
            var properties = source?.GetType().GetProperties();
            if (properties.NotNull())
            {
                properties.ForEach(x =>
                {
                    output.Add(x.Name, x.GetValue(source));
                });
            }
            return output;
        }
    }
}