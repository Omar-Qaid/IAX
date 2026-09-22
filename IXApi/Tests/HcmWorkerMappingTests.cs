using IAX.IXApi.Modules.Finance.Entities;
using IAX.IXApi.Modules.Finance.Foundation.Genders;
using IAX.IXApi.Modules.Finance.Foundation.HcmWorkers;
using IAX.IXApi.Modules.Finance.Foundation.Nationalities;
using IAX.IXApi.Modules.Finance.Foundation.Occupations;
using Mapster;
using Xunit;

namespace IXApi.Tests;

public sealed class HcmWorkerMappingTests
{
    [Fact]
    public void Finance_assembly_scan_binds_party_names_to_worker_dto()
    {
        var config = new TypeAdapterConfig();
        config.Scan(typeof(HcmWorkerMapping).Assembly);
        var worker = new HcmWorker
        {
            PersonnelNumber = "C12",
            Party = new DirPartyTable
            {
                Name = "Wafaa Muhammad Yahya Muree",
                NameAlias = "وفاء محمد يحيى مرعي"
            },
            Occupation = new HcmOccupation(),
            Gender = new Gender(),
            Nationality = new HcmNationality()
        };

        var dto = worker.Adapt<HcmWorkerDto>(config);

        Assert.Equal("Wafaa Muhammad Yahya Muree", dto.Name);
        Assert.Equal("وفاء محمد يحيى مرعي", dto.NameAlias);
    }
}
