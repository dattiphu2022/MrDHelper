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
                    var datas = Datas ?? throw new NullReferenceException();
                    return datas[propertyName];
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
                    var datas = Datas ?? throw new NullReferenceException();
                    datas[propertyName] = value;
                }
                catch (Exception)
                {
                    var datas = Datas;
                    if (datas is null)
                    {
                        throw;
                    }
                    datas.Add(propertyName, value);
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
            GC.SuppressFinalize(this);
        }
    }
}
