namespace MrDHelper.AppData.Converters
{
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    public class AesValueConverter<T> : ValueConverter<T, string>
    {
        public AesValueConverter(string key)
            : base(
                v => AesEncryptionHelper.Encrypt(Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture)!, key),
                v => (T)Convert.ChangeType(AesEncryptionHelper.Decrypt(v, key), typeof(T), System.Globalization.CultureInfo.InvariantCulture))
        { }
    }

}
