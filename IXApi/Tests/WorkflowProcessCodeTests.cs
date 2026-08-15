using IAX.IXApi.Modules.Workflow.Processes;
using IAX.IXApi.Shared.Domain.Entities;
using Xunit;
using IAX.IXApi.Modules.Administration.NumberSequences;

namespace IAX.IXApi.Tests;

public class WorkflowProcessCodeTests
{
    [Fact]
    public void Workflow_process_implements_the_auto_code_contract()
    {
        var process = new WfProcess();

        Assert.IsAssignableFrom<ICode>(process);

        ((ICode)process).Code = "PROC-000001";
        Assert.Equal("PROC-000001", process.Code);
    }

    [Fact]
    public void Sequence_formatter_resolves_prefix_and_configured_padding()
    {
        var sequence = new SysNumberSequence
        {
            NumberSequence = "WfProcess",
            Txt = "Workflow process",
            Format = "PROC-######",
            AnnotatedFormat = "{PREFIX}-{SEQ}"
        };

        var code = SysNumberSequenceService.FormatCode(sequence, 42);

        Assert.Equal("PROC-000042", code);
        Assert.DoesNotContain("{", code);
    }
}
