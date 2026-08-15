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
    public abstract class Entity<T> : BaseEntity<T>, IBaseEntity, IMultiCompany
    {
        object IBaseEntity.RecId => RecId!;
    }

    public abstract class SharedEntity<T> : BaseEntity<T>, IBaseEntity
    {
        object IBaseEntity.RecId => RecId!;
    }

    public abstract class LookupEntity<T> : BaseEntity<T>, IBaseEntity, IMultiCompany, ICode
    {
        object IBaseEntity.RecId => RecId!;
        public string? Code { get; set; }
        public string? Name { get; set; }
    }

    public abstract class MasterEntity<T> : LookupEntity<T>
    {
        public string? Description { get; set; }
 
    }
}
