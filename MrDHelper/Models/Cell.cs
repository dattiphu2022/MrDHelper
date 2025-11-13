namespace MrDHelper.Models
{
    using System;
    using System.Collections.Generic;
    public sealed class Cell : IDisposable
    {
        public object? this[string propertyName]
        {
            get {
                try
                {
                    return Datas[propertyName];
                }
                catch (Exception)
                {
                    return null;
                }
            }
            set
            {
                try
                {
                    Datas[propertyName] = value;
                }
                catch (Exception)
                {
                    Datas.Add(propertyName, value);
                }; 
            }
        }
        public int ColSpan { get; set; } = 1;
        public int RowSpan { get; set; } = 1;
        public string Title { get; set; } = string.Empty;
        public Dictionary<string, object?>? Datas { get; set; } = new Dictionary<string, object?>();
        ~Cell()
        {
            Dispose();
        }
        public void Dispose()
        {
            Datas = null;
        }
    }
}