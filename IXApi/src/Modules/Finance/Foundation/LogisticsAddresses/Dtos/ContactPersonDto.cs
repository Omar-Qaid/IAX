using IAX.IXApi.Shared.Application.Contracts;
using IAX.IXApi.Modules.Finance.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace IAX.IXApi.Modules.Finance.Foundation.LogisticsAddresses
{
    public record ContactNameParts(
        [param: MaxLength(FieldLengths.Name)] string FirstName,
        [param: MaxLength(FieldLengths.Name)] string MiddleName,
        [param: MaxLength(FieldLengths.Name)] string LastNamePrefix,
        [param: MaxLength(FieldLengths.Name)] string LastName);

    public record AccountContactInput(ContactNameParts? NameParts,
        [param: Required, MaxLength(FieldLengths.Name)] string Name,
        [param: MaxLength(FieldLengths.NameAlias)] string? NameAlias,
        [param: MaxLength(FieldLengths.LanguageId)] string LanguageId,
        [param: MaxLength(FieldLengths.Initials)] string? Initials,
        [param: MaxLength(FieldLengths.AddressBookNames)] string? AddressBookNames,
        DateOnly? Birthday, DateOnly? Anniversary, bool Inactive, Gender? Gender, MaritalStatus? MaritalStatus);

    public record AccountContactsDto(string AccountNum, string? Name, List<AccountContactRow> Contacts);

    public record AccountContactRow(long RecId, string ContactPersonId, long Party, string Name, string NameAlias,
        string LanguageId, string AddressBookNames, string? Initials, int? BirthYear, int? BirthMonth, int? Birthday,
        int? AnniversaryYear, int? AnniversaryMonth, int? AnniversaryDay, string? Telephone,
        string? Extension, bool Inactive, bool VendorPortalAccessAllowed, ContactNameParts? NameParts, Gender? Gender, MaritalStatus? MaritalStatus);

    public class ContactPersonDto : EntityDto<long>
    {
        public string ContactPersonId { get; set; } = string.Empty;
        public long Party { get; set; }
        public long ContactForParty { get; set; }
        public string? CustAccount { get; set; }
        public NoYes Inactive { get; set; }
        public NoYes Vip { get; set; }
        public NoYes Imported { get; set; }
        public NoYes IsContactPersonExternallyMaintained { get; set; }
        public ContactSensitivity Sensitivity { get; set; }
        public long? MainResponsibleWorker { get; set; }
        public int TimeAvailableFrom { get; set; }
        public int TimeAvailableTo { get; set; }
        public NoYes DirectMail { get; set; }
        public NoYes McrIsDefaultContact { get; set; }
        public NoYes VendorPortalAccessAllowed { get; set; }
        public NoYes WebRequestAccess { get; set; }
        public VendorContactRole VendRole { get; set; }
        public DateTime LastEditAxDateTime { get; set; }
        public int LastEditAxDateTimeTzId { get; set; }
    }
}

