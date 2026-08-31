using System.Reflection;
using System.Text.Json;
using IAX.IXApi.Modules.Identity.Roles;
using IAX.IXApi.Modules.Identity.Users;
using IAX.IXApi.Modules.Workflow.Activities;
using IAX.IXApi.Modules.Workflow.Categories;
using IAX.IXApi.Modules.Workflow.Controls;
using IAX.IXApi.Modules.Workflow.Operators;
using IAX.IXApi.Modules.Workflow.Performers;
using IAX.IXApi.Modules.Workflow.Priorities;
using IAX.IXApi.Modules.Workflow.Processes;
using IAX.IXApi.Modules.Workflow.ProcessTypes;
using IAX.IXApi.Modules.Workflow.Requests;
using IAX.IXApi.Modules.Workflow.Steps;
using IAX.IXApi.Modules.Workflow.Transitions;
using IAX.IXApi.Modules.Workflow.Variables;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Shared.Domain.Entities;

namespace IAX.IXApi.Infrastructure.Persistence.Seeding.Chunks;

/// <summary>Imports a compact synthetic workflow definition used by local demonstrations and tests.</summary>
public sealed class LegacyWorkflowMasterDataSeeder : ISeeder
{
    private const string ResourceSuffix = "Persistence.Seeding.Data.LegacyWorkflowMasterData.json";

    public async Task SeedAsync(ApplicationDbContext db, RoleManager<AspNetRole> roles, UserManager<AspNetUser> users, CancellationToken ct)
    {
        _ = roles;
        var owner = (await users.FindByNameAsync("sys"))?.Id ?? "sys";
        var data = await ReadDataAsync(ct);

        // These lookup tables do not exist in the legacy export as standalone masters,
        // but their keys are referenced by WfProcesses and WfPerformers. Seed them here
        // so this import remains valid even when the generic WorkflowSeeder was skipped,
        // partially committed, or previously failed.
        await AddMissingAsync(db, db.WfPriorities, new[]
        {
            new WfPriority { RecId=1,Code="LOW",Name="Low",Description="Low Priority",SortOrder=1,IsActive=true,CreatedBy=owner,OwnerAccountId=owner },
            new WfPriority { RecId=2,Code="MED",Name="Medium",Description="Medium Priority",SortOrder=2,IsActive=true,CreatedBy=owner,OwnerAccountId=owner },
            new WfPriority { RecId=3,Code="HIGH",Name="High",Description="High Priority",SortOrder=3,IsActive=true,CreatedBy=owner,OwnerAccountId=owner }
        }, "WfPriorities", ct);
        await AddMissingAsync(db, db.WfProcessTypes, new[]
        {
            new WfProcessType { RecId=1,Code="STD",Name="Standard",IsActive=true,CreatedBy=owner,OwnerAccountId=owner },
            new WfProcessType { RecId=2,Code="REV",Name="Review",IsActive=true,CreatedBy=owner,OwnerAccountId=owner },
            new WfProcessType { RecId=3,Code="APP",Name="Approval",IsActive=true,CreatedBy=owner,OwnerAccountId=owner }
        }, "WfProcessTypes", ct);
        await AddMissingAsync(db, db.WfPerformerTypes, new[]
        {
            new WfPerformerType { RecId=1,Code="RELATIONAL",Name="Relational",SortOrder=1,IsActive=true,CreatedBy=owner,OwnerAccountId=owner }
        }, "WfPerformerType", ct);

        await AddMissingAsync(db, db.WfDataTypes, data.DataTypes.Select(x => new WfDataType { RecId=x.Id,Code=$"DT{x.Id}",Name=Text(x.Name,255),Description=Text(x.Description,1000),SortOrder=x.Id,IsActive=true,CreatedBy=owner,OwnerAccountId=owner }), "WfDataTypes", ct);
        await AddMissingAsync(db, db.WfControls, data.Controls.Select(x => new WfControl { RecId=x.Id,Code=Text(x.Code,50)??$"CTRL{x.Id}",Name=Text(x.Name,255),Description=Text(x.Description,1000),ControlType=Text(x.ControlType,255)??"TextBox",SortOrder=x.Id,IsActive=true,CreatedBy=owner,OwnerAccountId=owner }), "WfControls", ct);
        await AddMissingAsync(db, db.WfActivityTypes, data.ActivityTypes.GroupBy(x=>x.Id).Select(g=>g.First()).Select(x => new WfActivityType { RecId=x.Id,Code=$"AT{x.Id}",Name=Text(x.Name,255),Description=Text(x.Description,1000),SortOrder=x.Id,IsActive=true,CreatedBy=owner,OwnerAccountId=owner }), "WfActivityTypes", ct);
        await AddMissingAsync(db, db.WfOperators, data.Operators.Select(x => new WfOperator { RecId=x.Id,Code=$"OP{x.Id}",Name=Text(x.Name,255),Description=Text(x.Description,1000),SortOrder=x.Id,IsActive=x.Active,CreatedBy=owner,OwnerAccountId=owner }), "WfOperators", ct);
        await AddMissingAsync(db, db.WfCategories, data.Categories.Select(x => new WfCategory { RecId=x.Id,Code=$"CAT{x.Id}",Name=Text(x.Name,255),Description=Text(x.Description,1000),SortOrder=(byte)Math.Min(x.Id,byte.MaxValue),SysField=x.SysField,IsActive=x.Active,CreatedBy=owner,OwnerAccountId=owner }), "WfCategories", ct);

        await AddMissingAsync(db, db.WfPerformers, data.Performers.Select(x => new WfPerformer { RecId=x.Id,Code=$"PERF{x.Id}",Name=Text(x.Name,255),PerformerTypeId=1,RelatedField=x.RelatedField,IsEmployee=x.IsEmployee,IsManager1=x.IsManager1,IsManager2=x.IsManager2,IsManager3=x.IsManager3,IsManager4=x.IsManager4,IsActive=x.Active,CreatedBy=owner,OwnerAccountId=owner }), "WfPerformers", ct);
        //await AddMissingAsync(db, db.WfProcesses, data.Processes.Select(x => new WfProcess { RecId=x.Id,Code=$"PROC{x.Id}",Name=Text(x.Name,255),Description=Text(x.Description,1000),CategoryId=x.CategoryId,PriorityId=x.PriorityId,ProcessTypeId=x.ProcessTypeId,CanRepeat=x.CanRepeat,SysField=x.SysField,Score=x.Score,IsActive=x.Active,CreatedBy=owner,OwnerAccountId=owner }), "WfProcesses", ct);
        //await AddMissingAsync(db, db.WfSteps, data.Steps.Select(x => new WfStep { RecId=x.Id,ProcessId=x.ProcessId,Code=$"STEP{x.Id}",Name=Text(x.Name,255),Description=Text(x.Description,1000),SortOrder=x.SortOrder,AutoPassingHrs=x.AutoPassingHrs,AllMandatory=x.AllMandatory,Score=x.Score,SysField=x.SysField,IsActive=x.Active,CreatedBy=owner,OwnerAccountId=owner }), "WfSteps", ct);
        //await AddMissingAsync(db, db.WfVariables, data.Variables.Select(x => new WfVariable { RecId=x.Id,ProcessId=x.ProcessId,DataTypeId=x.DataTypeId,Code=$"VAR{x.Id}",Name=Text(x.Name,255),Description=Text(x.Description,1000),SortOrder=x.SortOrder,IsActive=x.Active,CreatedBy=owner,OwnerAccountId=owner }), "WfVariables", ct);
        //await AddMissingAsync(db, db.WfRequestControls, data.RequestControls.Select(x => new WfRequestControl { RecId=x.Id,ProcessId=x.ProcessId,ControlId=x.ControlId,Code=$"RC{x.Id}",Name=Text(x.Name,255),Description=Text(x.Description,1000),SortOrder=x.SortOrder??0,Score=x.Score,ValidationRules=x.Mandatory?"<Validation><Required>true</Required></Validation>":null,ExtendedProperties=x.ExtendedProperties,IsActive=x.Active,CreatedBy=owner,OwnerAccountId=owner }), "WfRequestControls", ct);
        //await AddMissingAsync(db, db.WfActivities, data.Activities.Select(x => new WfActivity { RecId=x.Id,ActivityTypeId=2,StepId=x.StepId,PerformerId=x.PerformerId,Code=$"ACT{x.Id}",Name=Text(x.Name,255),Description=Text(x.Description,1000),Score=x.Score,MandatoryDocs=x.MandatoryDocs,ShowPreviousDocs=x.ShowPreviousDocs,ShowPreviousSteps=x.ShowPreviousSteps,AlertingByEmail=x.AlertingByEmail,AlertingBySms=x.AlertingBySms,AlertingBySystem=x.AlertingBySystem,AutoPassEnabled=x.AutoPassEnabled,AutoPassingHrs=x.AutoPassingHrs,ExtendedProperties=x.ExtendedProperties,IsActive=x.Active,CreatedBy=owner,OwnerAccountId=owner }), "WfActivities", ct);
        //await AddMissingAsync(db, db.WfActivityControls, data.ActivityControls.Select(x => new WfActivityControl { RecId=x.Id,ActivityId=x.ActivityId,ProcessId=x.ProcessId,ControlId=x.ControlId,Code=$"AC{x.Id}",Name=Text(x.Name,255),Description=Text(x.Description,1000),SortOrder=x.SortOrder??0,Score=x.Score,ValidationRules=x.Mandatory?"<Validation><Required>true</Required></Validation>":null,ExtendedProperties=x.ExtendedProperties,IsActive=x.Active,CreatedBy=owner,OwnerAccountId=owner }), "WfActivityControls", ct);
        //var activityControlIds=data.ActivityControls.Select(x=>x.Id).ToHashSet();
        //var requestControlIds=data.RequestControls.Select(x=>x.Id).ToHashSet();
        //var variableIds=data.Variables.Select(x=>x.Id).ToHashSet();
        //await AddMissingAsync(db, db.WfActivityMappingVariables, data.ActivityMappings.Where(x=>x.Active&&activityControlIds.Contains(x.ActivityControlId)&&variableIds.Contains(x.VariableId)).Select(x => new WfActivityMappingVariable { RecId=x.Id,ActivityControlId=x.ActivityControlId,VariableId=x.VariableId,VariableOrder=x.SortOrder,CreatedBy=owner,OwnerAccountId=owner }), "WfActivityMappingVariables", ct);
        //await AddMissingAsync(db, db.WfRequestMappingVariables, data.RequestMappings.Where(x=>x.Active&&requestControlIds.Contains(x.RequestControlId)&&variableIds.Contains(x.VariableId)).Select(x => new WfRequestMappingVariable { RecId=x.Id,RequestControlId=x.RequestControlId,VariableId=x.VariableId,SortOrder=x.SortOrder,CreatedBy=owner,OwnerAccountId=owner }), "WfRequestMappingVariables", ct);
        //var processIds=data.Processes.Select(x=>x.Id).ToHashSet();
        //var activityIds=data.Activities.Select(x=>x.Id).ToHashSet();
        //var operatorIds=data.Operators.Select(x=>x.Id).ToHashSet();
        //await AddMissingAsync(db, db.WfTransitions, data.Transitions.Where(x=>x.Active&&processIds.Contains(x.ProcessId)&&variableIds.Contains(x.VariableId)&&operatorIds.Contains(x.OperatorId)&&(!x.ActivityId.HasValue||activityIds.Contains(x.ActivityId.Value))&&(!x.RequestControlId.HasValue||requestControlIds.Contains(x.RequestControlId.Value))).Select(x => new WfTransition { RecId=x.Id,ProcessId=x.ProcessId,ActivityId=x.ActivityId,RequestControlId=x.RequestControlId,VariableId=x.VariableId,OperatorId=x.OperatorId,Value=Text(x.Value,255)??"",StepId=x.StepId,SortOrder=x.SortOrder,CreatedBy=owner,OwnerAccountId=owner }), "WfTransitions", ct);

        //var performerIds=data.Performers.Select(x=>x.Id).ToHashSet();
        //await AddMissingAsync(db, db.WfPerformerUsers, data.PerformerUsers.Where(x=>performerIds.Contains(x.PerformerId)).Select(x => new WfPerformerUsers { RecId=x.Id,PerformerId=x.PerformerId,UserID=x.UserID,RelatedField=x.RelatedField,ExtendedProperties=x.ExtendedProperties,CreatedBy=owner,OwnerAccountId=owner }), "WfUsersPerformers", ct);
        //var departmentIds=(await db.Departments.IgnoreQueryFilters().AsNoTracking().Select(x=>x.RecId).ToListAsync(ct)).ToHashSet();
        //var occupationIds=(await db.Occupations.IgnoreQueryFilters().AsNoTracking().Select(x=>x.RecId).ToListAsync(ct)).ToHashSet();
        //var employeeIds=(await db.HcmWorkers.IgnoreQueryFilters().AsNoTracking().Select(x=>x.RecId).ToListAsync(ct)).ToHashSet();
        //await AddMissingAsync(db, db.WfUsersProcesses, data.UsersProcesses
        //    .Where(x=>processIds.Contains(x.ProcessId)
        //        &&(!x.DepartmentId.HasValue||departmentIds.Contains(x.DepartmentId.Value))
        //        &&(!x.OccupationId.HasValue||occupationIds.Contains(x.OccupationId.Value))
        //        &&(!x.EmployeeId.HasValue||employeeIds.Contains(x.EmployeeId.Value)))
        //    .Select(x => new WfUsersProcess { RecId=x.Id,ProcessId=x.ProcessId,DepartmentId=x.DepartmentId,OccupationId=x.OccupationId,EmployeeId=x.EmployeeId,CreatedBy=owner,OwnerAccountId=owner }), "WfUsersProcesses", ct);
    }

    private static async Task AddMissingAsync<TEntity>(ApplicationDbContext db, DbSet<TEntity> set, IEnumerable<TEntity> source, string _, CancellationToken ct) where TEntity:class,IBaseEntity
    {
        // ID 0 rows are workflow sentinels, not real master records. The target model
        // intentionally excludes them (the base WorkflowSeeder also removes them).
        var rows=source.Where(x=>!IsZeroKey(x.RecId)).ToList(); if(rows.Count==0)return;
        var existing=(await set.IgnoreQueryFilters().AsNoTracking().ToListAsync(ct)).Select(x=>x.RecId).ToHashSet();
        var missing=rows.Where(x=>!existing.Contains(x.RecId)).ToList(); if(missing.Count==0)return;
        var entityType=db.Model.FindEntityType(typeof(TEntity))??throw new InvalidOperationException($"Missing EF metadata for {typeof(TEntity).Name}.");
        var tableName=entityType.GetTableName()??throw new InvalidOperationException($"Missing table mapping for {typeof(TEntity).Name}.");
        var schema=entityType.GetSchema();
        var table=string.IsNullOrWhiteSpace(schema)?SqlIdentifier(tableName):$"{SqlIdentifier(schema)}.{SqlIdentifier(tableName)}";
        await set.AddRangeAsync(missing,ct); await SaveIdentityAsync(db,table,ct);
    }

    private static async Task<SeedData> ReadDataAsync(CancellationToken ct)
    {
        var assembly=typeof(LegacyWorkflowMasterDataSeeder).Assembly;
        var name=assembly.GetManifestResourceNames().Single(x=>x.EndsWith(ResourceSuffix,StringComparison.Ordinal));
        await using var stream=assembly.GetManifestResourceStream(name)??throw new InvalidOperationException($"Missing resource {name}");
        return await JsonSerializer.DeserializeAsync<SeedData>(stream,new JsonSerializerOptions{PropertyNameCaseInsensitive=true},ct)??throw new InvalidOperationException("Invalid synthetic workflow seed resource.");
    }
    private static string? Text(string? value,int max)=>string.IsNullOrWhiteSpace(value)?null:(value.Length<=max?value:value[..max]);
    private static string SqlIdentifier(string value)=>$"[{value.Replace("]","]]",StringComparison.Ordinal)}]";
    private static bool IsZeroKey(object value)=>value is byte b?b==0:value is short s?s==0:value is int i?i==0:value is long l&&l==0;
    private static async Task SaveIdentityAsync(ApplicationDbContext db,string table,CancellationToken ct)
    {
        await db.Database.OpenConnectionAsync(ct);
        try
        {
            await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT "+table+" ON",ct);
            await db.SaveChangesAsync(ct);
            await db.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT "+table+" OFF",ct);
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }

    private sealed class SeedData { public BasicByte[] DataTypes{get;set;}=[];public Control[] Controls{get;set;}=[];public BasicByte[] ActivityTypes{get;set;}=[];public ActiveByte[] Operators{get;set;}=[];public Category[] Categories{get;set;}=[];public Performer[] Performers{get;set;}=[];public Process[] Processes{get;set;}=[];public Step[] Steps{get;set;}=[];public Variable[] Variables{get;set;}=[];public RequestControl[] RequestControls{get;set;}=[];public Activity[] Activities{get;set;}=[];public ActivityControl[] ActivityControls{get;set;}=[];public Mapping[] ActivityMappings{get;set;}=[];public RequestMapping[] RequestMappings{get;set;}=[];public Transition[] Transitions{get;set;}=[];public PerformerUser[] PerformerUsers{get;set;}=[];public UsersProcess[] UsersProcesses{get;set;}=[]; }
    private class BasicByte {public byte Id{get;set;}public string? Name{get;set;}public string? Description{get;set;}}
    private sealed class Control:BasicByte {public string? Code{get;set;}public string? ControlType{get;set;}}
    private sealed class ActiveByte:BasicByte {public bool Active{get;set;}}
    private sealed class Category {public short Id{get;set;}public string? Name{get;set;}public string? Description{get;set;}public bool Active{get;set;}public bool SysField{get;set;}}
    private sealed class Performer {public long Id{get;set;}public string? Name{get;set;}public string? Description{get;set;}public bool Active{get;set;}public long? RelatedField{get;set;}public bool IsEmployee{get;set;}public bool IsManager1{get;set;}public bool IsManager2{get;set;}public bool IsManager3{get;set;}public bool IsManager4{get;set;}}
    private sealed class Process {public long Id{get;set;}public string? Name{get;set;}public string? Description{get;set;}public short CategoryId{get;set;}public bool Active{get;set;}public bool CanRepeat{get;set;}public bool SysField{get;set;}public byte PriorityId{get;set;}public decimal Score{get;set;}public byte ProcessTypeId{get;set;}}
    private sealed class Step {public long Id{get;set;}public long ProcessId{get;set;}public string? Name{get;set;}public string? Description{get;set;}public byte SortOrder{get;set;}public bool SysField{get;set;}public bool Active{get;set;}public byte AutoPassingHrs{get;set;}public bool AllMandatory{get;set;}public decimal Score{get;set;}}
    private sealed class Variable {public long Id{get;set;}public long ProcessId{get;set;}public string? Name{get;set;}public string? Description{get;set;}public byte DataTypeId{get;set;}public bool Active{get;set;}public byte SortOrder{get;set;}}
    private class RequestControl {public long Id{get;set;}public long ProcessId{get;set;}public byte ControlId{get;set;}public string? Name{get;set;}public string? Description{get;set;}public byte? SortOrder{get;set;}public bool Active{get;set;}public bool Mandatory{get;set;}public string? ExtendedProperties{get;set;}public decimal Score{get;set;}}
    private sealed class Activity {public long Id{get;set;}public long StepId{get;set;}public string? Name{get;set;}public string? Description{get;set;}public long PerformerId{get;set;}public bool Active{get;set;}public bool MandatoryDocs{get;set;}public bool ShowPreviousDocs{get;set;}public bool ShowPreviousSteps{get;set;}public bool AlertingByEmail{get;set;}public bool AlertingBySms{get;set;}public bool AlertingBySystem{get;set;}public bool AutoPassEnabled{get;set;}public byte AutoPassingHrs{get;set;}public decimal Score{get;set;}public string? ExtendedProperties{get;set;}}
    private sealed class ActivityControl:RequestControl {public long ActivityId{get;set;}}
    private class Mapping {public long Id{get;set;}public long ActivityControlId{get;set;}public long VariableId{get;set;}public bool Active{get;set;}public byte SortOrder{get;set;}}
    private sealed class RequestMapping:Mapping {public long RequestControlId{get;set;}}
    private sealed class Transition {public long Id{get;set;}public long ProcessId{get;set;}public long? ActivityId{get;set;}public long VariableId{get;set;}public byte OperatorId{get;set;}public string? Value{get;set;}public long StepId{get;set;}public long? RequestControlId{get;set;}public bool Active{get;set;}public byte SortOrder{get;set;}}
    private sealed class PerformerUser {public long Id{get;set;}public long PerformerId{get;set;}public long UserID{get;set;}public long RelatedField{get;set;}public string? ExtendedProperties{get;set;}}
    private sealed class UsersProcess {public long Id{get;set;}public long ProcessId{get;set;}public short? DepartmentId{get;set;}public short? OccupationId{get;set;}public long? EmployeeId{get;set;}}
}
