#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
using Microsoft.EntityFrameworkCore;
using NKE.BlazorUI.AppData.Attributes;
using NKE.BlazorUI.AppData.Converters;
using System.Reflection;

namespace NKE.BlazorUI.AppData.Extensions
{
    public static class DbContextEncryptionExtensions
    {
        /// <summary>
        /// Migrate/re-encrypt các trường [Encrypted] từ oldKey sang newKey.
        /// Nếu dữ liệu chưa mã hóa (plaintext), sẽ tự động mã hóa mới.
        /// </summary>
        /// <param name="db">DbContext instance</param>
        /// <param name="oldKey">AES key cũ (có thể null hoặc "" nếu lần đầu mã hóa)</param>
        /// <param name="newKey">AES key mới</param>
        /// <param name="batchSize">Batch size mỗi lần xử lý</param>
        /// <param name="logError">Action log lỗi (nếu muốn)</param>
        public static async Task MigrateEncryptionKeyAsync<TContext>(
            this TContext db,
            string oldKey,
            string newKey,
            int batchSize = 500,
            Action<string> logError = null)
            where TContext : DbContext
        {
            var model = db.Model;
            foreach (var entityType in model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;
                var keyProp = entityType.FindPrimaryKey()?.Properties?.FirstOrDefault();
                if (keyProp == null) continue;

                var keyPropInfo = clrType.GetProperty(keyProp.Name);

                // Các property có [Encrypted]
                var encryptedProps = clrType.GetProperties()
                    .Where(p => p.GetCustomAttribute<EncryptedAttribute>() != null)
                    .ToList();

                if (encryptedProps.Count == 0) continue;

                // Reflection lấy DbSet<T>
                var setMethod = typeof(DbContext).GetMethod("Set", Type.EmptyTypes);
                var dbSet = setMethod.MakeGenericMethod(clrType).Invoke(db, null);

                // Chuẩn bị Queryable để batch
                var queryable = dbSet as IQueryable<object>;
                if (queryable == null)
                    queryable = ((IQueryable)dbSet).Cast<object>();

                int skip = 0;
                List<object> batch;

                while ((batch = queryable.Skip(skip).Take(batchSize).ToList()).Count > 0)
                {
                    foreach (var entity in batch)
                    {
                        db.Attach(entity);

                        foreach (var prop in encryptedProps)
                        {
                            object encryptedValueObj = prop.GetValue(entity);
                            if (encryptedValueObj == null) continue;

                            Type targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                            string encryptedValueStr = encryptedValueObj.ToString();

                            string plainStr = null;
                            object plainValue = null;

                            try
                            {
                                // Thử giải mã với oldKey
                                plainStr = string.IsNullOrEmpty(oldKey)
                                    ? null
                                    : AesEncryptionHelper.Decrypt(encryptedValueStr, oldKey);

                                // Nếu giải mã thành công, plainStr != null
                                if (plainStr == null)
                                    throw new Exception("PlainStr is null after decrypt."); // Force catch
                            }
                            catch
                            {
                                // Nếu giải mã thất bại, assume đây là dữ liệu gốc (plaintext)
                                plainStr = encryptedValueStr;
                                logError?.Invoke(
                                    $"[MigrateEncryptionKey] Entity={clrType.Name} Id={keyPropInfo.GetValue(entity)} Property={prop.Name} was not previously encrypted. Encrypting as new.");
                            }

                            try
                            {
                                // Convert về đúng type
                                if (targetType == typeof(string))
                                    plainValue = plainStr;
                                else if (targetType == typeof(int))
                                    plainValue = int.Parse(plainStr);
                                else if (targetType == typeof(long))
                                    plainValue = long.Parse(plainStr);
                                else if (targetType == typeof(double))
                                    plainValue = double.Parse(plainStr, System.Globalization.CultureInfo.InvariantCulture);
                                else if (targetType == typeof(float))
                                    plainValue = float.Parse(plainStr, System.Globalization.CultureInfo.InvariantCulture);
                                else if (targetType == typeof(decimal))
                                    plainValue = decimal.Parse(plainStr, System.Globalization.CultureInfo.InvariantCulture);
                                else if (targetType == typeof(DateTime))
                                    plainValue = DateTime.Parse(plainStr, null, System.Globalization.DateTimeStyles.RoundtripKind);
                                else if (targetType == typeof(bool))
                                    plainValue = bool.Parse(plainStr);
                                else
                                    throw new NotSupportedException($"Type {targetType.Name} is not supported for encryption migration!");

                                // Mã hóa lại với key mới
                                string newEncryptedStr = AesEncryptionHelper.Encrypt(Convert.ToString(plainValue, System.Globalization.CultureInfo.InvariantCulture), newKey);

                                // Đặt lại lên property
                                prop.SetValue(entity, newEncryptedStr);
                            }
                            catch (Exception ex)
                            {
                                logError?.Invoke(
                                    $"[MigrateEncryptionKey] Entity={clrType.Name} Id={keyPropInfo.GetValue(entity)} Property={prop.Name} VALUE={plainStr} ERROR={ex.Message}");
                                continue;
                            }
                        }
                    }
                    await db.SaveChangesAsync();
                    skip += batch.Count;
                }
            }
        }

        public static async Task MigrateEncryptedFieldsToPlaintextAsync<TContext>(
            this TContext db,
            string oldKey,
            int batchSize = 500,
            Action<string> logError = null)
            where TContext : DbContext
        {
            var model = db.Model;
            foreach (var entityType in model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;
                var keyProp = entityType.FindPrimaryKey()?.Properties?.FirstOrDefault();
                if (keyProp == null) continue;

                var keyPropInfo = clrType.GetProperty(keyProp.Name);

                var encryptedProps = clrType.GetProperties()
                    .Where(p => p.GetCustomAttribute<EncryptedAttribute>() != null)
                    .ToList();

                if (encryptedProps.Count == 0) continue;

                var setMethod = typeof(DbContext).GetMethod("Set", Type.EmptyTypes);
                var dbSet = setMethod.MakeGenericMethod(clrType).Invoke(db, null);

                var queryable = dbSet as IQueryable<object>;
                if (queryable == null)
                    queryable = ((IQueryable)dbSet).Cast<object>();

                int skip = 0;
                List<object> batch;

                while ((batch = queryable.Skip(skip).Take(batchSize).ToList()).Count > 0)
                {
                    foreach (var entity in batch)
                    {
                        db.Attach(entity);

                        foreach (var prop in encryptedProps)
                        {
                            object encryptedValueObj = prop.GetValue(entity);
                            if (encryptedValueObj == null) continue;

                            Type targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                            string encryptedValueStr = encryptedValueObj.ToString();
                            string plainStr = null;

                            try
                            {
                                plainStr = AesEncryptionHelper.Decrypt(encryptedValueStr, oldKey);
                            }
                            catch
                            {
                                plainStr = encryptedValueStr;
                                logError?.Invoke(
                                    $"[MigrateToPlaintext] Entity={clrType.Name} Id={keyPropInfo.GetValue(entity)} Property={prop.Name} is already plaintext.");
                            }

                            try
                            {
                                object plainValue;
                                if (targetType == typeof(string))
                                    plainValue = plainStr;
                                else if (targetType == typeof(int))
                                    plainValue = int.Parse(plainStr);
                                else if (targetType == typeof(long))
                                    plainValue = long.Parse(plainStr);
                                else if (targetType == typeof(double))
                                    plainValue = double.Parse(plainStr, System.Globalization.CultureInfo.InvariantCulture);
                                else if (targetType == typeof(float))
                                    plainValue = float.Parse(plainStr, System.Globalization.CultureInfo.InvariantCulture);
                                else if (targetType == typeof(decimal))
                                    plainValue = decimal.Parse(plainStr, System.Globalization.CultureInfo.InvariantCulture);
                                else if (targetType == typeof(DateTime))
                                    plainValue = DateTime.Parse(plainStr, null, System.Globalization.DateTimeStyles.RoundtripKind);
                                else if (targetType == typeof(bool))
                                    plainValue = bool.Parse(plainStr);
                                else
                                    throw new NotSupportedException($"Type {targetType.Name} is not supported for migration to plaintext!");

                                prop.SetValue(entity, plainValue);
                            }
                            catch (Exception ex)
                            {
                                logError?.Invoke(
                                    $"[MigrateToPlaintext] Entity={clrType.Name} Id={keyPropInfo.GetValue(entity)} Property={prop.Name} VALUE={plainStr} ERROR={ex.Message}");
                                continue;
                            }
                        }
                    }
                    await db.SaveChangesAsync();
                    skip += batch.Count;
                }
            }
        }

    }

}

#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.