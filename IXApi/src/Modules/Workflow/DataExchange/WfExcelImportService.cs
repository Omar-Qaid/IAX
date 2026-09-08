using ClosedXML.Excel;
using IAX.IXApi.Infrastructure.Persistence.Repositories;
using IAX.IXApi.Infrastructure.Identity;


using IAX.IXApi.Modules.Workflow.Processes;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Shared.Application.Attributes;
using System.Reflection;

namespace IAX.IXApi.Modules.Workflow.DataExchange
{
    public class WfExcelImportService : IWfExcelImportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WfExcelImportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task ImportProcessesAsync(Stream fileStream)
        {
            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed()?.RowsUsed().Skip(1) ?? []; // Skip header

            var processes = new List<WfProcess>();

            foreach (var row in rows)
            {
                var process = new WfProcess
                {
                    Code = row.Cell(1).GetValue<string>(),
                    Name = row.Cell(3).GetValue<string>(),
                    Description = row.Cell(5).GetValue<string>(),
                    CategoryId = row.Cell(6).GetValue<short>(),
                    Score = row.Cell(7).GetValue<decimal>(),
                    IsRepeatable = row.Cell(8).GetValue<bool>(),
                    MandatoryDocuments = row.Cell(9).GetValue<bool>(),
                    PriorityId = row.Cell(10).GetValue<byte>(),
                    ProcessTypeId = row.Cell(11).GetValue<byte>(),
                    IsSystemDefined  = row.Cell(12).GetValue<bool>(),
                    SortOrder = row.Cell(13).GetValue<byte>(),
                    RepeatIntervalHours = row.Cell(14).IsEmpty() ? (byte)0 : row.Cell(14).GetValue<byte>(),
                    IsActive = true,
                    IsDeleted = false
                    // Set other default or required properties if needed
                };
                 processes.Add(process);
            }

            if (processes.Any())
            {
                await _unitOfWork.Repository<WfProcess>().AddRangeAsync(processes);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task ImportActivitiesAsync(Stream fileStream)
        {
            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed()?.RowsUsed().Skip(1) ?? []; // Skip header

            var activities = new List<WfActivity>();

            foreach (var row in rows)
            {
                var activity = new WfActivity
                {
                    Code = row.Cell(1).GetValue<string>(),
                    Name = row.Cell(3).GetValue<string>(),
                    // Assuming similar order for base Entity properties
                    ActivityTypeId = row.Cell(4).GetValue<byte>(),
                    StepId = row.Cell(5).GetValue<long>(),
                    PerformerId = row.Cell(6).GetValue<long>(),
                    Score = row.Cell(7).GetValue<decimal>(),
                    IsSystemNotificationEnabled = row.Cell(8).GetValue<bool>(),
                    IsEmailNotificationEnabled = row.Cell(9).GetValue<bool>(),
                    IsSmsNotificationEnabled = row.Cell(10).GetValue<bool>(),
                    CanViewPreviousSteps = row.Cell(11).GetValue<bool>(),
                    CanViewPreviousDocuments = row.Cell(12).GetValue<bool>(),
                    MandatoryDocuments   = row.Cell(13).GetValue<bool>(),
                    AutoPassAfterHours = row.Cell(14).GetValue<byte>(),
                    SysNotificationTemplateId = row.Cell(15).IsEmpty() ? null : row.Cell(15).GetValue<int?>(),
                    IsWhatsAppNotificationEnabled = !row.Cell(16).IsEmpty() && row.Cell(16).GetValue<bool>(),
                    IsAutoPassEnabled = !row.Cell(17).IsEmpty() && row.Cell(17).GetValue<bool>(),
                    IsActive = true,
                    IsDeleted = false
                };
                activities.Add(activity);
            }

            if (activities.Any())
            {
                await _unitOfWork.Repository<WfActivity>().AddRangeAsync(activities);
                await _unitOfWork.CompleteAsync();
            }
        }

        public Task<byte[]> GenerateProcessTemplateAsync()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Processes");

            // Headers
            worksheet.Cell(1, 1).Value = "Code";
            worksheet.Cell(1, 2).Value = "NameAR";
            worksheet.Cell(1, 3).Value = "Name";
            worksheet.Cell(1, 4).Value = "DescriptionAR";
            worksheet.Cell(1, 5).Value = "Description";
            worksheet.Cell(1, 6).Value = "CategoryId";
            worksheet.Cell(1, 7).Value = "Score";
            worksheet.Cell(1, 8).Value = "IsRepeatable";
            worksheet.Cell(1, 9).Value = "MandatoryDocuments";
            worksheet.Cell(1, 10).Value = "PriorityId";
            worksheet.Cell(1, 11).Value = "ProcessTypeId";
            worksheet.Cell(1, 12).Value = "IsSystemDefined";
            worksheet.Cell(1, 13).Value = "SortOrder";
            worksheet.Cell(1, 14).Value = "RepeatIntervalHours";
            
            // Example Row
            worksheet.Cell(2, 1).Value = "P01";
            worksheet.Cell(2, 2).Value = "Example Process";
            worksheet.Cell(2, 3).Value = "Example Process";
            worksheet.Cell(2, 4).Value = "Description";
            worksheet.Cell(2, 5).Value = "Description";
            worksheet.Cell(2, 6).Value = 1;
            worksheet.Cell(2, 7).Value = 10;
            worksheet.Cell(2, 8).Value = true;
            worksheet.Cell(2, 9).Value = false;
            worksheet.Cell(2, 10).Value = 1;
            worksheet.Cell(2, 11).Value = 1;
            worksheet.Cell(2, 12).Value = false;
            worksheet.Cell(2, 13).Value = 1;


            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
        }

        public Task<byte[]> GenerateActivityTemplateAsync()
        {
             using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Activities");

            // Headers
            worksheet.Cell(1, 1).Value = "Code";
            worksheet.Cell(1, 2).Value = "NameAR";
            worksheet.Cell(1, 3).Value = "Name";
            worksheet.Cell(1, 4).Value = "ActivityTypeId";
            worksheet.Cell(1, 5).Value = "StepId";
            worksheet.Cell(1, 6).Value = "PerformerId";
            worksheet.Cell(1, 7).Value = "Score";
            worksheet.Cell(1, 8).Value = "IsSystemNotificationEnabled";
            worksheet.Cell(1, 9).Value = "IsEmailNotificationEnabled";
            worksheet.Cell(1, 10).Value = "IsSmsNotificationEnabled";
            worksheet.Cell(1, 11).Value = "CanViewPreviousSteps";
            worksheet.Cell(1, 12).Value = "CanViewPreviousDocuments";
            worksheet.Cell(1, 13).Value = "MandatoryDocuments";
            worksheet.Cell(1, 14).Value = "AutoPassAfterHours";
            worksheet.Cell(1, 15).Value = "SysNotificationTemplateId";
            worksheet.Cell(1, 16).Value = "IsWhatsAppNotificationEnabled";
            worksheet.Cell(1, 17).Value = "IsAutoPassEnabled";

             // Example Row
            worksheet.Cell(2, 1).Value = "A01";
            worksheet.Cell(2, 2).Value = "Activity Name";
            worksheet.Cell(2, 3).Value = "Activity Name";
            worksheet.Cell(2, 4).Value = 1;
            worksheet.Cell(2, 5).Value = 1;
            worksheet.Cell(2, 6).Value = 1;
            worksheet.Cell(2, 7).Value = 5;
            worksheet.Cell(2, 8).Value = true;
            worksheet.Cell(2, 9).Value = true;
            worksheet.Cell(2, 10).Value = false;
            worksheet.Cell(2, 11).Value = false;
            worksheet.Cell(2, 12).Value = true;
            worksheet.Cell(2, 13).Value = false;
            worksheet.Cell(2, 14).Value = 24;
            worksheet.Cell(2, 15).Value = 1;
            worksheet.Cell(2, 16).Value = false;
            worksheet.Cell(2, 17).Value = true;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
        }
    }
}

