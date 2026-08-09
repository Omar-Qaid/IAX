using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IAX.IXApi.Shared.Application.Attributes;
using IAX.IXApi.Infrastructure.Persistence.Services;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;
using Microsoft.AspNetCore.Hosting;

namespace IAX.IXApi.Modules.Organization.Attachments
{
    public class OrgAttachmentService : BaseService<OrgAttachment>, IOrgAttachmentService
    {
        private readonly IWebHostEnvironment _env;

        public OrgAttachmentService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, IWebHostEnvironment env) 
            : base(unitOfWork, currentUser)
        {
            _env = env;
        }

        public async Task<OrgAttachmentDetail> SaveFileAsync(
            long attachmentId, 
            string fileName, 
            string contentType, 
            long fileSize, 
            Stream fileStream, 
            CancellationToken cancellationToken = default)
        {
            // Determine uploads directory
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var uploadsDir = Path.Combine(webRoot, "uploads");

            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            // Create unique file name to avoid overwrite
            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
            var physicalPath = Path.Combine(uploadsDir, uniqueFileName);
            var virtualPath = $"/uploads/{uniqueFileName}";

            // Physically copy the file
            using (var destinationStream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.CopyToAsync(destinationStream, cancellationToken);
            }

            // Create and save database record
            var detail = new OrgAttachmentDetail
            {
                AttachmentId = attachmentId,
                FileName = fileName,
                FileType = contentType,
                FilePath = virtualPath,
                FileSize = fileSize,
                Description = $"Uploaded on {DateTime.UtcNow}"
            };

            var detailRepo = _unitOfWork.Repository<OrgAttachmentDetail>();
            var savedDetail = await detailRepo.AddAsync(detail, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);

            return savedDetail;
        }

        public async Task DeleteFileAsync(long fileId, CancellationToken cancellationToken = default)
        {
            var detailRepo = _unitOfWork.Repository<OrgAttachmentDetail>();
            var detail = await detailRepo.GetByIdAsync(fileId, cancellationToken);
            if (detail == null) return;

            // Delete physical file
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var physicalPath = Path.Combine(webRoot, detail.FilePath.TrimStart('/'));

            if (File.Exists(physicalPath))
            {
                try
                {
                    File.Delete(physicalPath);
                }
                catch
                {
                    // Log or handle file delete failure silently or log
                }
            }

            // Remove database record
            await detailRepo.RemoveAsync(detail);
            await _unitOfWork.CompleteAsync(cancellationToken);
        }
    }
}

