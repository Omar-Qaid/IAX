namespace IAX.IXApi.Shared.Application.Contracts
{
    public class MasterEntityDto<T> : EntityDto<T>
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}
