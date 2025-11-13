using System.ComponentModel.DataAnnotations;

namespace MrDHelper.AppDomain.AuditableModelBase
{
    public interface IAuditable
    {
        string? CreatedBy { get; set; }
        DateTimeOffset CreatedDate { get; set; }
        string? EditedBy { get; set; }
        DateTimeOffset? EditedDate { get; set; }
        bool IsDeleted { get; set; }
        public byte[]? RowVersion { get; set; }
    }

}
