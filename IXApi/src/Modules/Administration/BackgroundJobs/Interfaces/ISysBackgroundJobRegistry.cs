using System;
using System.Collections.Generic;
using IAX.IXApi.Modules.Administration.BackgroundJobs.Services.Handlers;

namespace IAX.IXApi.Modules.Administration.BackgroundJobs.Interfaces
{
    public interface ISysBackgroundJobRegistry
    {
        IReadOnlyCollection<string> RegisteredKeys { get; }
        IReadOnlyCollection<string> SelfScopedKeys => Array.Empty<string>();
        bool SupportsWholeJobRetry(string jobKey) => true;
        bool IsRegistered(string jobKey);
        ISysBackgroundJobHandler? Resolve(string jobKey, IServiceProvider scopedProvider);
    }
}
