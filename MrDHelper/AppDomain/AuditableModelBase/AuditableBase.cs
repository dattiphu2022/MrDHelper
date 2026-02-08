using MrDHelper.AppDomain.EfSqliteFts5;
using System.ComponentModel.DataAnnotations;

namespace MrDHelper.AppDomain.AuditableModelBase
{
    /// <summary>
    /// Default ID has GUID type.
    /// </summary>
    public abstract class AuditableBase : IAuditable, IHasGuidId
    {
        [Key]
        public virtual Guid Id { get; set; } = Guid.NewGuid();
        public string? CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;
        public string? EditedBy { get; set; }
        public DateTimeOffset? EditedDate { get; set; }
        public bool IsDeleted { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
    public abstract class AuditableBase<TIdType> : IAuditable
    {
        [Key]
        public virtual TIdType Id { get; set; } = default!;
        public string? CreatedBy { get; set; }
        public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;
        public string? EditedBy { get; set; }
        public DateTimeOffset? EditedDate { get; set; }
        public bool IsDeleted { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}

