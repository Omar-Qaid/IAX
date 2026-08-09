using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Services
{
    public interface ISysBackgroundJobRegistry
    {
        /// <summary>All registered handler keys (for discovery in the UI).</summary>
        IReadOnlyCollection<string> RegisteredKeys { get; }

        /// <summary>Returns true if a handler is registered for the given key.</summary>
        bool IsRegistered(string jobKey);

        /// <summary>
        /// Resolves a handler instance for the key from the supplied (scoped) provider,
        /// or null if none is registered.
        /// </summary>
        ISysBackgroundJobHandler? Resolve(string jobKey, IServiceProvider scopedProvider);
    }
}