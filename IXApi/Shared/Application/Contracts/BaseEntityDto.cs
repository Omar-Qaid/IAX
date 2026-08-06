namespace IAX.IXApi.Shared.Application.Contracts
{
    public abstract class BaseEntityDto<T>
    {
        // Id must round-trip in the body so /range updates can target the right rows.
        // Create still ignores the client-supplied id via [AdaptIgnore] on the entity key.
        public T Id { get; set; } = default(T)!;

    }
}
