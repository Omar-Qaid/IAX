using System.IO;

namespace IAX.IXApi.Modules.Workflow.DataExchange
{
    public interface IWfExcelImportService
    {
        Task ImportProcessesAsync(Stream fileStream);
        Task ImportActivitiesAsync(Stream fileStream);
        Task<byte[]> GenerateProcessTemplateAsync();
        Task<byte[]> GenerateActivityTemplateAsync();
    }
}
