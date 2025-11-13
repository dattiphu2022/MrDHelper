using System.ComponentModel.DataAnnotations;

namespace MrDHelper.AppDomain.AuditHistoryModel
{
    public sealed class AuditHistory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset AuditHistoryCreatedTime { get; set; } = DateTimeOffset.UtcNow;
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string PropertyName { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTimeOffset ChangedAt { get; set; }
        public string? ChangedBy { get; set; }
    }
}
