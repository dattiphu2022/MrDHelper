using System.Reflection;

namespace MrDHelper.Models
{
    public sealed class DisplayConstantModel
    {
        public DisplayConstantModel(string value, string displayName, int order, FieldInfo fieldInfo)
        {
            Value = value;
            DisplayName = displayName;
            Order = order;
            FieldInfo = fieldInfo;
        }

        public string Value { get; set; }
        public string DisplayName { get; set; }
        public int Order { get; set; }
        public FieldInfo FieldInfo { get; set; }
    }
}
