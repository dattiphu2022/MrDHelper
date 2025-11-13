using System;
using System.Reflection;

namespace MrDHelper.Models
{
    public sealed class DisplayClassModel
    {
        public DisplayClassModel(string propertyName, string displayName, int order, Type propertyType, PropertyInfo propInfo)
        {
            PropertyName = propertyName;
            DisplayName = displayName;
            Order = order;
            PropertyType = propertyType;
            PropInfo = propInfo;
        }

        public string PropertyName {get;set;}
        public string DisplayName {get;set;}
        public int Order {get;set;}
        public Type PropertyType {get;set;}
        public PropertyInfo PropInfo { get; set; }
    }
}
