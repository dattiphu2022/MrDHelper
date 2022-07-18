using System.Collections.Generic;

namespace MrDHelper
{
    public class Cell
    {
        public object? this[string propertyName]
        {
            get { return Datas[propertyName]; }
            set { Datas[propertyName] = value; }
        }
        public int ColSpan { get; set; } = 1;
        public int RowSpan { get; set; } = 1;
        public string Title { get; set; } = string.Empty;
        public Dictionary<string, object?> Datas { get; set; } = new Dictionary<string, object?>();

    }
}