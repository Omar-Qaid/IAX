using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Infrastructure.Persistence.Services;

namespace IAX.IXApi.Modules.Organization.Attachments
{
    public interface IOrgAttachmentService : IBaseService<OrgAttachment>
    {
        Task<OrgAttachmentDetail> SaveFileAsync(long attachmentId, string fileName, string contentType, long fileSize, Stream fileStream, CancellationToken cancellationToken = default);
        Task DeleteFileAsync(long fileId, CancellationToken cancellationToken = default);
    }
}
