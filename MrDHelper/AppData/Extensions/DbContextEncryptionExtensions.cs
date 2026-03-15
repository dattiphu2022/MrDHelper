#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
using Microsoft.EntityFrameworkCore;
using MrDHelper.AppData.Attributes;
using MrDHelper.AppData.Converters;
using System.Reflection;

namespace MrDHelper.AppData.Extensions;

public static class DbContextEncryptionExtensions
{
    /// <summary>
    /// Migrates or re-encrypts `[Encrypted]` fields from `oldKey` to `newKey`.
    /// If the stored value is still plaintext, it is encrypted with the new key.
    /// </summary>
    /// <param name="db">DbContext instance</param>
    /// <param name="oldKey">Previous AES key. Can be null or empty for first-time encryption.</param>
    /// <param name="newKey">New AES key.</param>
    /// <param name="batchSize">Batch size for each processing cycle.</param>
    /// <param name="logError">Optional error logging callback.</param>
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

            // Properties marked with [Encrypted].
            var encryptedProps = clrType.GetProperties()
                .Where(p => p.GetCustomAttribute<EncryptedAttribute>() != null)
                .ToList();

            if (encryptedProps.Count == 0) continue;

            // Use reflection to get DbSet<T>.
            var setMethod = typeof(DbContext).GetMethod("Set", Type.EmptyTypes);
            var dbSet = setMethod.MakeGenericMethod(clrType).Invoke(db, null);

            // Prepare the queryable source for batch processing.
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
                            // Try to decrypt with the old key.
                            plainStr = string.IsNullOrEmpty(oldKey)
                                ? null
                                : AesEncryptionHelper.Decrypt(encryptedValueStr, oldKey);

                            // If decryption succeeds, plainStr should not be null.
                            if (plainStr == null)
                                throw new Exception("PlainStr is null after decrypt."); // Force catch
                        }
                        catch
                        {
                            // If decryption fails, assume the value is still plaintext.
                            plainStr = encryptedValueStr;
                            logError?.Invoke(
                                $"[MigrateEncryptionKey] Entity={clrType.Name} Id={keyPropInfo.GetValue(entity)} Property={prop.Name} was not previously encrypted. Encrypting as new.");
                        }

                        try
                        {
                            // Convert back to the target type.
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

                            // Re-encrypt with the new key.
                            string newEncryptedStr = AesEncryptionHelper.Encrypt(Convert.ToString(plainValue, System.Globalization.CultureInfo.InvariantCulture), newKey);

                            // Write the encrypted value back to the property.
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

#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
