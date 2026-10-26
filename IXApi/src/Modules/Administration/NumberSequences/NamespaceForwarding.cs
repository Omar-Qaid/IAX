// Compatibility forwarding for sub-namespaces under IAX.IXApi.Modules.Administration.NumberSequences
namespace IAX.IXApi.Modules.Administration.NumberSequences.Entities
{
    public class SysNumberSequenceMarker {}
}

namespace IAX.IXApi.Modules.Administration.NumberSequences.Interfaces
{
    public interface ISysNumberSequenceService : NumberSequences.ISysNumberSequenceService {}
}

namespace IAX.IXApi.Modules.Administration.NumberSequences.Dtos
{
    public class SysNumberSequenceDto : NumberSequences.SysNumberSequenceDto {}
    public class NextSequenceRequestDto : NumberSequences.NextSequenceRequestDto {}
    public class NextSequenceResultDto : NumberSequences.NextSequenceResultDto {}
    public class ResetSequenceRequestDto : NumberSequences.ResetSequenceRequestDto {}
}

namespace IAX.IXApi.Modules.Administration.NumberSequences.Services
{
    public class SysNumberSequenceServiceMarker {}
}

namespace IAX.IXApi.Modules.Administration.NumberSequences.Controllers
{
    public class SysNumberSequenceControllerMarker {}
}

namespace IAX.IXApi.Modules.Administration.NumberSequences.Validation
{
    public class SysNumberSequenceDtoValidatorMarker {}
}

namespace IAX.IXApi.Modules.Administration.NumberSequences.Configuration
{
    public class SysNumberSequenceConfigurationMarker {}
}
