using IAX.IXApi.Infrastructure.Persistence;
using IAX.IXApi.Modules.Workflow.Performers;
using Microsoft.EntityFrameworkCore;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

public sealed partial class WfProcessSeedData
{
    private static async Task SeedPerformerUsersAsync(
        ApplicationDbContext db,
        string owner,
        CancellationToken ct)
    {
        var existingIds = await db.WfPerformerUsers.IgnoreQueryFilters()
            .Select(x => x.RecId)
            .ToListAsync(ct);

        var toAdd = new List<WfPerformerUsers>();

        var userPerformers = new[]
        {
            new { PerformerId = 17L, UserID = 155312L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20010L },
            new { PerformerId = 18L, UserID = 25996L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20011L },
            new { PerformerId = 23L, UserID = 155436L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20013L },
            new { PerformerId = 25L, UserID = 155419L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20015L },
            new { PerformerId = 27L, UserID = 155395L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20017L },
            new { PerformerId = 31L, UserID = 155563L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20019L },
            new { PerformerId = 32L, UserID = 155347L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20020L },
            new { PerformerId = 38L, UserID = 26012L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20022L },
            new { PerformerId = 39L, UserID = 155416L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20023L },
            new { PerformerId = 40L, UserID = 155472L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20024L },
            new { PerformerId = 41L, UserID = 155363L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20025L },
            new { PerformerId = 43L, UserID = 155375L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20027L },
            new { PerformerId = 44L, UserID = 155465L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20028L },
            new { PerformerId = 45L, UserID = 155420L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20029L },
            new { PerformerId = 46L, UserID = 155707L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20030L },
            new { PerformerId = 50L, UserID = 155353L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20034L },
            new { PerformerId = 59L, UserID = 155911L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20037L },
            new { PerformerId = 62L, UserID = 155484L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20040L },
            new { PerformerId = 65L, UserID = 155362L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20041L },
            new { PerformerId = 68L, UserID = 155568L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20043L },
            new { PerformerId = 70L, UserID = 155800L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20046L },
            new { PerformerId = 71L, UserID = 156754L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20047L },
            new { PerformerId = 73L, UserID = 155501L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20049L },
            new { PerformerId = 74L, UserID = 26015L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20050L },
            new { PerformerId = 75L, UserID = 155509L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20051L },
            new { PerformerId = 76L, UserID = 155685L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20052L },
            new { PerformerId = 77L, UserID = 155362L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20053L },
            new { PerformerId = 78L, UserID = 155553L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20054L },
            new { PerformerId = 26L, UserID = 155644L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20067L },
            new { PerformerId = 26L, UserID = 155501L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20068L },
            new { PerformerId = 26L, UserID = 26015L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20069L },
            new { PerformerId = 26L, UserID = 155707L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20070L },
            new { PerformerId = 81L, UserID = 155708L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20073L },
            new { PerformerId = 84L, UserID = 155349L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20075L },
            new { PerformerId = 85L, UserID = 155806L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20076L },
            new { PerformerId = 86L, UserID = 156647L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20077L },
            new { PerformerId = 87L, UserID = 155635L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20078L },
            new { PerformerId = 88L, UserID = 155529L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20079L },
            new { PerformerId = 89L, UserID = 155394L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20080L },
            new { PerformerId = 90L, UserID = 156834L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20081L },
            new { PerformerId = 91L, UserID = 155329L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20082L },
            new { PerformerId = 92L, UserID = 156152L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20083L },
            new { PerformerId = 93L, UserID = 156835L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20084L },
            new { PerformerId = 97L, UserID = 156895L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20089L },
            new { PerformerId = 100L, UserID = 156858L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20093L },
            new { PerformerId = 100L, UserID = 26018L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20094L },
            new { PerformerId = 101L, UserID = 156869L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20095L },
            new { PerformerId = 106L, UserID = 155725L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20099L },
            new { PerformerId = 99L, UserID = 155398L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20100L },
            new { PerformerId = 109L, UserID = 155907L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20101L },
            new { PerformerId = 112L, UserID = 155802L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20106L },
            new { PerformerId = 116L, UserID = 157002L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20109L },
            new { PerformerId = 117L, UserID = 155437L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20110L },
            new { PerformerId = 119L, UserID = 156869L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20112L },
            new { PerformerId = 120L, UserID = 156790L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20113L },
            new { PerformerId = 98L, UserID = 156675L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20116L },
            new { PerformerId = 121L, UserID = 155769L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20117L },
            new { PerformerId = 122L, UserID = 156676L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20118L },
            new { PerformerId = 128L, UserID = 155553L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20119L },
            new { PerformerId = 129L, UserID = 156815L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20120L },
            new { PerformerId = 130L, UserID = 156807L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20121L },
            new { PerformerId = 131L, UserID = 156962L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20124L },
            new { PerformerId = 134L, UserID = 156761L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20128L },
            new { PerformerId = 142L, UserID = 157077L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20131L },
            new { PerformerId = 145L, UserID = 157134L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20132L },
            new { PerformerId = 29L, UserID = 155335L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20134L },
            new { PerformerId = 148L, UserID = 157120L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20136L },
            new { PerformerId = 149L, UserID = 157170L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20138L },
            new { PerformerId = 152L, UserID = 155700L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20141L },
            new { PerformerId = 154L, UserID = 157181L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20142L },
            new { PerformerId = 155L, UserID = 155542L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20144L },
            new { PerformerId = 95L, UserID = 157290L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20148L },
            new { PerformerId = 24L, UserID = 157002L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20149L },
            new { PerformerId = 132L, UserID = 156815L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20150L },
            new { PerformerId = 37L, UserID = 155544L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20152L },
            new { PerformerId = 49L, UserID = 155568L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20153L },
            new { PerformerId = 141L, UserID = 155800L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20154L },
            new { PerformerId = 54L, UserID = 155364L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20157L },
            new { PerformerId = 166L, UserID = 155631L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20158L },
            new { PerformerId = 169L, UserID = 155351L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20160L },
            new { PerformerId = 156L, UserID = 157289L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20161L },
            new { PerformerId = 171L, UserID = 155667L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20162L },
            new { PerformerId = 172L, UserID = 156869L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20166L },
            new { PerformerId = 173L, UserID = 157362L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20167L },
            new { PerformerId = 80L, UserID = 157362L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20169L },
            new { PerformerId = 146L, UserID = 157362L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20170L },
            new { PerformerId = 179L, UserID = 157371L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20173L },
            new { PerformerId = 180L, UserID = 157372L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20174L },
            new { PerformerId = 181L, UserID = 155343L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20175L },
            new { PerformerId = 94L, UserID = 155667L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20179L },
            new { PerformerId = 157L, UserID = 156895L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20180L },
            new { PerformerId = 183L, UserID = 156895L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20181L },
            new { PerformerId = 178L, UserID = 157324L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20182L },
            new { PerformerId = 103L, UserID = 155455L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20183L },
            new { PerformerId = 184L, UserID = 157312L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20184L },
            new { PerformerId = 192L, UserID = 157513L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20185L },
            new { PerformerId = 193L, UserID = 157527L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20186L },
            new { PerformerId = 185L, UserID = 155800L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20187L },
            new { PerformerId = 197L, UserID = 157536L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20188L },
            new { PerformerId = 202L, UserID = 157615L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20189L },
            new { PerformerId = 47L, UserID = 157652L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20196L },
            new { PerformerId = 53L, UserID = 155644L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20197L },
            new { PerformerId = 205L, UserID = 157662L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20198L },
            new { PerformerId = 207L, UserID = 155912L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20199L },
            new { PerformerId = 208L, UserID = 157713L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20200L },
            new { PerformerId = 158L, UserID = 157453L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20202L },
            new { PerformerId = 215L, UserID = 157746L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20204L },
            new { PerformerId = 216L, UserID = 157746L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20205L },
            new { PerformerId = 213L, UserID = 157349L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20206L },
            new { PerformerId = 133L, UserID = 157662L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20207L },
            new { PerformerId = 79L, UserID = 157662L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20208L },
            new { PerformerId = 42L, UserID = 155644L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20209L },
            new { PerformerId = 72L, UserID = 155465L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20211L },
            new { PerformerId = 104L, UserID = 155335L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20212L },
            new { PerformerId = 19L, UserID = 155335L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20215L },
            new { PerformerId = 218L, UserID = 155658L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20218L },
            new { PerformerId = 118L, UserID = 155553L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20219L },
            new { PerformerId = 48L, UserID = 157837L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20221L },
            new { PerformerId = 209L, UserID = 157837L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20222L },
            new { PerformerId = 64L, UserID = 155430L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20225L },
            new { PerformerId = 203L, UserID = 157925L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20226L },
            new { PerformerId = 227L, UserID = 157954L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20228L },
            new { PerformerId = 228L, UserID = 157930L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20229L },
            new { PerformerId = 230L, UserID = 157970L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20230L },
            new { PerformerId = 219L, UserID = 158032L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20231L },
            new { PerformerId = 217L, UserID = 155556L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20232L },
            new { PerformerId = 234L, UserID = 155556L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20233L },
            new { PerformerId = 235L, UserID = 156815L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20236L },
            new { PerformerId = 236L, UserID = 157879L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20237L },
            new { PerformerId = 223L, UserID = 156790L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20240L },
            new { PerformerId = 83L, UserID = 155395L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20243L },
            new { PerformerId = 61L, UserID = 157662L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20244L },
            new { PerformerId = 60L, UserID = 157120L, ExtendedProperties = (string)null, RelatedField = 0L, RecId = 20245L },
        };

        foreach (var item in userPerformers)
        {
            if (!existingIds.Contains(item.RecId))
            {
                toAdd.Add(new WfPerformerUsers
                {
                    RecId = item.RecId,
                    PerformerId = item.PerformerId,
                    UserID = item.UserID,
                    ExtendedProperties = item.ExtendedProperties,
                    RelatedField = item.RelatedField,
                    CreatedBy = owner,
                    OwnerAccountId = owner,
                    IsActive = true
                });
            }
        }

        if (toAdd.Any())
        {
            await db.WfPerformerUsers.AddRangeAsync(toAdd, ct);
            
            await db.Database.OpenConnectionAsync(ct);
            try
            {
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT WfUsersPerformers ON", ct);
                await db.SaveChangesAsync(ct);
                await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT WfUsersPerformers OFF", ct);
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }
        }
    }
}
