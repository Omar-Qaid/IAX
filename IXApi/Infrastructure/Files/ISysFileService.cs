namespace IAX.IXApi.Infrastructure.Files
{
    public interface ISysFileService
    {
        Task<SysImageInfo> SaveImageAsync(IFormFile imageFile);
    }
}
