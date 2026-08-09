using Mapster;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Shared.Application.Contracts
{
    
    public abstract class EntityDto<T>
    {
        public bool IsActive { get; set; }
        
        // Audit Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public UserDto? CreatedByUser { get; set; }
        public UserDto? LastModifiedByUser { get; set; }
        public UserDto? OwnerAccount { get; set; }
        public string? OwnerAccountId { get; set; }


        public T RecId { get; set; } = default(T)!;

        [Timestamp]
        public byte[]? RowVersion { get; set; }
        public bool IsDeleted { get; set; } = false;
        public int RecVersion { get; set; } = 1;
        public string DataAreaId { get; set; } = "dat";
    }

    public class UserDto
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string Name => UserName;
    }
}
