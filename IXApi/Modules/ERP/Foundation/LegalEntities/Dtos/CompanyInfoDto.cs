namespace IAX.IXApi.Modules.ERP.Foundation.LegalEntities
{
    public class CompanyInfoDto
    {
        public long RecId { get; set; }
        public long Party { get; set; }
        public string DataArea { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? LanguageId { get; set; }
        public string? CurrencyCode { get; set; }
        public string? TaxLicenseNum { get; set; }
        public string? FederalTaxId { get; set; }
        public string? BankAccount { get; set; }
        public long? Calendar { get; set; }
        public string? TimeZone { get; set; }
        public string? Memo { get; set; }
        public string? ArabicName { get; set; }
        public string? LocalizedRegion { get; set; }
        
        public string? Logo { get; set; }
        public string? ReportLogo { get; set; }
        
        public List<IAX.IXApi.Modules.ERP.Foundation.LogisticsAddresses.AddressInfoDto> Addresses { get; set; } = new();
        public List<IAX.IXApi.Modules.ERP.Foundation.LogisticsAddresses.ContactInfoDto> Contacts { get; set; } = new();
    }


}
