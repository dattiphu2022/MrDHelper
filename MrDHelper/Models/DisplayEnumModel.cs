using System;
using System.Reflection;

namespace MrDHelper.Models
{
    public sealed class DisplayEnumModel<TEnum> where TEnum : struct, Enum
    {
        public DisplayEnumModel(TEnum value, string displayName, int order, FieldInfo fieldInfo)
        {
            Value = value;
            DisplayName = displayName;
            Order = order;
            FieldInfo = fieldInfo;
        }

        public TEnum Value {get;set;}
    public string DisplayName {get;set;}
    public int Order {get;set;}
    public FieldInfo FieldInfo { get; set; }
    }
}
