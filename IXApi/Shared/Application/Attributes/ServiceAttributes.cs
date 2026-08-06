namespace IAX.IXApi.Shared.Application.Attributes
{
    /// <summary>
    /// A template for custom attributes, which represent services that can be dependency injected.
    /// </summary>
    public interface IServiceAttribute
    {
        ServiceLifetime ServiceLifetime { get; }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ScopedServiceAttribute : Attribute, IServiceAttribute
    {
        public ServiceLifetime ServiceLifetime => ServiceLifetime.Scoped;
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SingletonServiceAttribute : Attribute, IServiceAttribute
    {
        public ServiceLifetime ServiceLifetime => ServiceLifetime.Singleton;
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class TransientServiceAttribute : Attribute, IServiceAttribute
    {
        public ServiceLifetime ServiceLifetime => ServiceLifetime.Transient;
    }
}
