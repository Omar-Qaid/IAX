using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Shared.Domain.Entities;
using Mapster;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Shared.Domain.Entities
{

    /*
    Suggested Base Fields
Almost every table can include:

RecId
Partition
DataAreaId

CreatedBy
CreatedDateTime

ModifiedBy
ModifiedDateTime

RecVersion
    */
    public abstract class Entity<T> : BaseEntity<T>, IBaseEntity ,IMultiCompany
    {
        object IBaseEntity.RecId => RecId!;
    }
    public abstract class SharedEntity<T> : BaseEntity<T>, IBaseEntity
    {
        object IBaseEntity.RecId => RecId!;
    }

    public abstract class LookupEntity<T> : BaseEntity<T>, IBaseEntity, IMultiCompany , ICode
    {
        object IBaseEntity.RecId => RecId!;
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? NameAR { get; set; }
    }

    public abstract class MasterEntity<T> : LookupEntity<T>
    {
        public string? Description { get; set; }
        public string? DescriptionAR { get; set; }
    }
}



