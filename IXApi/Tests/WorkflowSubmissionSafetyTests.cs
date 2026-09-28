using System.Text.Json;
using IAX.IXApi.Modules.Workflow.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using IAX.IXApi.Modules.Organization.DocumentManagement.Storage;
using Xunit;

namespace IAX.IXApi.Tests;

public sealed class WorkflowSubmissionSafetyTests
{
    [Theory]
    [InlineData("Employee")]
    [InlineData("Showroom")]
    public void Empty_reference_options_reject_fabricated_selection(string reference)
    {
        var control = new DynamicRequestControlDto { ReferenceType = reference, ControlType = "select" };
        Assert.False(WfRequestService.SelectionIsValid(control, "123"));
        Assert.True(WfRequestService.SelectionIsValid(control, ""));
        control.Options.Add(new() { Value = "456" });
        Assert.False(WfRequestService.SelectionIsValid(control, "123"));
        Assert.True(WfRequestService.SelectionIsValid(control, "456"));
    }

    [Fact]
    public void Table_column_options_do_not_restrict_row_json()
    {
        var control = new DynamicRequestControlDto { ControlType = "table", Options = [new() { Value = "Amount" }] };
        Assert.True(WfRequestService.SelectionIsValid(control, "[{\"Amount\":123}]"));
    }

    [Fact]
    public void Single_selection_rejects_multiple_or_malformed_answers()
    {
        var control = new DynamicRequestControlDto { ControlType = "select", Options = [new() { Value = "a" }, new() { Value = "b" }] };
        Assert.False(WfRequestService.SelectionIsValid(control, "[\"a\",\"b\"]"));
        Assert.False(WfRequestService.SelectionIsValid(control, "[invalid"));
    }

    [Fact]
    public void Mandatory_process_documents_require_actual_files()
    {
        Assert.Throws<ArgumentException>(() => WfRequestService.NormalizeUploadedFiles(
            new DynamicRequestFormDto { MandatoryDocuments = true }, new SubmitDynamicRequestDto()));
    }

    [Fact]
    public void File_metadata_without_file_content_is_rejected()
    {
        var form = new DynamicRequestFormDto { Controls = [new() { RequestControlId = 1, ControlType = "file" }] };
        var submission = new SubmitDynamicRequestDto { Values = [new() { RequestControlId = 1, Value = "[{\"n\":\"fake.pdf\",\"s\":42}]" }] };
        Assert.Throws<ArgumentException>(() => WfRequestService.NormalizeUploadedFiles(form, submission));
        submission.Values.Clear();
        submission.OptionFeatureValues.Add(new() { OptionId = 9, FileValue = "[{\"n\":\"fake.pdf\",\"s\":42}]" });
        Assert.Throws<ArgumentException>(() => WfRequestService.NormalizeUploadedFiles(form, submission));
    }

    [Fact]
    public void Uploaded_file_metadata_comes_from_received_files()
    {
        using var stream = new MemoryStream([1, 2, 3]);
        var file = new FormFile(stream, 0, 3, "files", "actual.pdf") { Headers = new HeaderDictionary(), ContentType = "application/pdf" };
        var form = new DynamicRequestFormDto { Controls = [new() { RequestControlId = 1, ControlType = "file" }] };
        var submission = new SubmitDynamicRequestDto
        {
            Values = [new() { RequestControlId = 1, Value = "[{\"n\":\"fake.txt\",\"s\":1}]" }],
            Files = [file], UploadTargets = [new() { RequestControlId = 1 }]
        };
        WfRequestService.NormalizeUploadedFiles(form, submission);
        using var metadata = JsonDocument.Parse(submission.Values.Single().Value!);
        Assert.Equal("actual.pdf", metadata.RootElement[0].GetProperty("n").GetString());
        Assert.Equal(3, metadata.RootElement[0].GetProperty("s").GetInt64());
        submission.UploadTargets.Clear();
        Assert.Throws<ArgumentException>(() => WfRequestService.NormalizeUploadedFiles(form, submission));
    }

    [Theory]
    [InlineData("123-AB", "000-AA", true)]
    [InlineData("12x-AB", "000-AA", false)]
    [InlineData("123-ABx", "000-AA", false)]
    [InlineData("A9", "\\A0", true)]
    [InlineData("a7", "**", true)]
    [InlineData("", "", false)]
    public void Input_masks_enforce_complete_values(string value, string mask, bool valid) =>
        Assert.Equal(valid, InputMaskRule.IsValid(value, mask));

    [Fact]
    public void Unknown_rules_fail_even_with_empty_values_or_warning_severity()
    {
        var control = new DynamicRequestControlDto { RequestControlId = 1, Validations = [new() { Type = "misspelled", Severity = "Warning" }] };
        var error = Assert.Single(WfRequestService.ValidateRules(control, "", [control], new Dictionary<long, string>()));
        Assert.Equal("Error", error.Severity);
        Assert.Contains("Unsupported", error.ErrorMessage);
    }

    [Fact]
    public void Uniqueness_rules_are_not_treated_as_unsupported_local_rules()
    {
        var control = new DynamicRequestControlDto { Validations = [new() { Type = "uniqueGlobal" }] };
        Assert.Empty(WfRequestService.ValidateRules(control, "123", [control], new Dictionary<long, string>()));
    }

    [Fact]
    public async Task Long_structured_answers_round_trip_without_a_255_character_model_limit()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new AnswerContext(new DbContextOptionsBuilder<AnswerContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var value = "sig:" + string.Join(';', Enumerable.Repeat("123,456", 200));
        var entity = new WfRequestDetail { ControlValue = value, DataAreaId = "dat", RowVersion = [] };
        db.Add(entity);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.Equal(value, (await db.Set<WfRequestDetail>().SingleAsync()).ControlValue);
        Assert.Null(db.Model.FindEntityType(typeof(WfRequestDetail))!.FindProperty(nameof(WfRequestDetail.ControlValue))!.GetMaxLength());
    }

    private sealed class AnswerContext(DbContextOptions<AnswerContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new WfRequestDetailConfiguration());
            builder.Entity<WfRequestDetail>().HasKey(item => item.RecId);
            builder.Entity<WfRequestDetail>().Property(item => item.ControlValue).HasColumnType("TEXT");
            builder.Entity<WfRequestDetail>().Property(item => item.RowVersion).ValueGeneratedNever();
        }
    }

    [Fact]
    public async Task Failed_upload_rolls_back_saved_rows_cleans_files_and_allows_retry()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new AnswerContext(new DbContextOptionsBuilder<AnswerContext>().UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        var storage = new TrackingStorage();
        await Assert.ThrowsAsync<IOException>(() => WorkflowSubmissionTransaction.RunAsync<int>(db, storage, async paths =>
        {
            db.Add(new WfRequestDetail { ControlValue = "answer", DataAreaId = "dat", RowVersion = [] });
            await db.SaveChangesAsync();
            paths.Add("first-upload.pdf");
            throw new IOException("Second upload failed");
        }, CancellationToken.None));
        Assert.Empty(await db.Set<WfRequestDetail>().ToListAsync());
        Assert.Empty(db.ChangeTracker.Entries());
        Assert.Equal(["first-upload.pdf"], storage.Deleted);

        await WorkflowSubmissionTransaction.RunAsync(db, storage, async paths =>
        {
            db.Add(new WfRequestDetail { ControlValue = "answer", DataAreaId = "dat", RowVersion = [] });
            await db.SaveChangesAsync();
            paths.Add("retry.pdf");
            return true;
        }, CancellationToken.None);
        Assert.Single(await db.Set<WfRequestDetail>().ToListAsync());
        Assert.DoesNotContain("retry.pdf", storage.Deleted);
    }

    private sealed class TrackingStorage : IFileStorageProvider
    {
        public string Name => "Test";
        public List<string> Deleted { get; } = [];
        public Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            Assert.False(cancellationToken.IsCancellationRequested);
            Deleted.Add(storageKey);
            return Task.CompletedTask;
        }
        public Task<StoredDocument> SaveAsync(Stream source, string extension, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<Stream> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    [Fact]
    public async Task Lost_commit_confirmation_does_not_delete_committed_attachments_or_retry()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var interceptor = new LostCommitConfirmation();
        await using var db = new AnswerContext(new DbContextOptionsBuilder<AnswerContext>().UseSqlite(connection)
            .AddInterceptors(interceptor).Options);
        await db.Database.EnsureCreatedAsync();
        interceptor.Enabled = true;
        var storage = new TrackingStorage();
        var attempts = 0;
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => WorkflowSubmissionTransaction.RunAsync(db, storage, async paths =>
        {
            attempts++;
            db.Add(new WfRequestDetail { ControlValue = "answer", DataAreaId = "dat", RowVersion = [] });
            await db.SaveChangesAsync();
            paths.Add("committed.pdf");
            return true;
        }, CancellationToken.None));
        Assert.Contains("outcome is unknown", error.Message);
        Assert.Equal(1, attempts);
        Assert.Empty(storage.Deleted);
        Assert.Single(await db.Set<WfRequestDetail>().ToListAsync());
    }

    private sealed class LostCommitConfirmation : DbTransactionInterceptor
    {
        public bool Enabled { get; set; }
        public override Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData,
            CancellationToken cancellationToken = default) => Enabled
                ? Task.FromException(new IOException("Connection lost after commit")) : Task.CompletedTask;
    }
}
