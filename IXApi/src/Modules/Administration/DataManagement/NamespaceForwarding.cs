// Compatibility forwarding for callers using IAX.IXApi.Modules.Administration.DataManagement
global using SysDataManagementController = IAX.IXApi.Modules.Administration.DataManagement.Controllers.SysDataManagementController;
global using SysImportError = IAX.IXApi.Modules.Administration.DataManagement.Contracts.SysImportError;
global using SysImportResult = IAX.IXApi.Modules.Administration.DataManagement.Contracts.SysImportResult;
global using SysExportColumnDto = IAX.IXApi.Modules.Administration.DataManagement.Dtos.SysExportColumnDto;
global using SysExportRequest = IAX.IXApi.Modules.Administration.DataManagement.Dtos.SysExportRequest;

global using ISysDataManagementEndpointService = IAX.IXApi.Modules.Administration.DataManagement.Interfaces.ISysDataManagementEndpointService;
global using ISysDataManagementEntityProvider = IAX.IXApi.Modules.Administration.DataManagement.Interfaces.ISysDataManagementEntityProvider;
global using ISysDataManagementService = IAX.IXApi.Modules.Administration.DataManagement.Interfaces.ISysDataManagementService;
global using ISysExcelService = IAX.IXApi.Modules.Administration.DataManagement.Interfaces.ISysExcelService;

global using SysDataManagementEndpointService = IAX.IXApi.Modules.Administration.DataManagement.Services.SysDataManagementEndpointService;
global using SysDataManagementService = IAX.IXApi.Modules.Administration.DataManagement.Services.SysDataManagementService;
global using SysExcelService = IAX.IXApi.Modules.Administration.DataManagement.Services.SysExcelService;
global using SysDataManagementEntityProvider = IAX.IXApi.Modules.Administration.DataManagement.Services.SysDataManagementEntityProvider;

namespace IAX.IXApi.Modules.Administration.DataManagement.Dtos
{
    // Alias import for Dtos namespace
    public interface ISysDataManagementDtoMarker { }
}

namespace IAX.IXApi.Modules.Administration.DataManagement.Providers
{
    public class SysDataManagementEntityProvider : IAX.IXApi.Modules.Administration.DataManagement.Services.SysDataManagementEntityProvider { }
}

namespace IAX.IXApi.Modules.Administration.DataManagement.Services
{
    public interface ISysDataManagementService : Interfaces.ISysDataManagementService { }
    public interface ISysExcelService : Interfaces.ISysExcelService { }
    public interface ISysDataManagementEndpointService : Interfaces.ISysDataManagementEndpointService { }
    public interface ISysDataManagementEntityProvider : Interfaces.ISysDataManagementEntityProvider { }
}
