using Microsoft.EntityFrameworkCore;
using IAX.IXApi.Shared.Domain.Entities;
using IAX.IXApi.Modules.ERP.Entities;
using IAX.IXApi.Modules.Organization.Employees.Entities;
using IAX.IXApi.Modules.Administration.AuditLogs.Entities;
using IAX.IXApi.Modules.Administration.DataManagement.Contracts;
using IAX.IXApi.Infrastructure.Persistence.Seeding.Entities;
using IAX.IXApi.Modules.ERP.AccountsReceivable;
using IAX.IXApi.Modules.ERP.Inventory;
using IAX.IXApi.Modules.ERP.GeneralLedger;
using IAX.IXApi.Modules.ERP.Shared.Features;
using DocumentFormat.OpenXml.Vml.Office;
using DocumentFormat.OpenXml.Bibliography;
using System.ComponentModel.DataAnnotations.Schema;

namespace IAX.IXApi.Infrastructure.Persistence.ModelBuilding
{
    public static class ErpModelBuilderExtensions
    {
        /*
        Accounts CustGroup CustTable Currency DlvTerm DlvMode PaymTerm PaymSched PaymSchedLine Accounts Receivable SalesTable SalesQuotationTable SalesQuotationLine CustInvoiceJour SalesLine CustInvoiceTrans SpecTrans CustTrans CustSettlement LedgerJournalTable LedgerJournalTrans CustPackingSlipJour CustPackingSlipTrans SalesPool ContactPerson CustTransOpen CustConfirmJour CustConfirmTrans CustQuotationJour CustQuotationTrans CustInvoiceTable CustInvoiceLine CustLedger CustLedgerAccounts CustPaymModeTable Inventory Management InventTable InventItemGroup InventItemBarcode InventItemPrice InventTrans UnitOfMeasure InventTransOrigin InventSum InventDim InventSettlement InventClosing InventJournalTable InventJournalTrans InventSite InventLocation InventBatch InventCountJour InventJournalName InventItemLocation InventTableModule General Ledger MainAccount GeneralJournalEntry GeneralJournalAccountEntry FiscalCalendar FiscalCalendarYear FiscalCalendarPeriod TaxGroupData InventPosting MarkupTable MarkupTrans Ledger BankGroup BankAccountTable LedgerChartOfAccounts LedgerJournalName TaxData TaxTable TaxGroupHeading TaxOnItem TaxJournalTrans TaxTrans
        */
        public static ModelBuilder ApplyMissingERPMappings(this ModelBuilder modelBuilder)
        {
            // ==========================================
            // 1. Configure Alternate Keys (Unique business keys)
            // ==========================================

            /*
            modelBuilder.Entity<Currency>().HasAlternateKey(c => c.CurrencyCode);
            modelBuilder.Entity<CustGroup>().HasAlternateKey(cg => cg.CustGroupId);
            modelBuilder.Entity<DlvTerm>().HasAlternateKey(dt => dt.Code);
            modelBuilder.Entity<DlvMode>().HasAlternateKey(dm => dm.Code);
            modelBuilder.Entity<PaymTerm>().HasAlternateKey(pt => pt.PaymTermId);
            modelBuilder.Entity<PaymSched>().HasAlternateKey(ps => ps.Name);
            modelBuilder.Entity<CustPaymModeTable>().HasAlternateKey(cp => cp.PaymMode);
            modelBuilder.Entity<DirPartyTable>().HasAlternateKey(dp => dp.PartyNumber);
            modelBuilder.Entity<TaxGroupHeading>().HasAlternateKey(tg => tg.TaxGroup);
            modelBuilder.Entity<TaxItemGroupHeading>().HasAlternateKey(tig => tig.TaxItemGroup);
            modelBuilder.Entity<SalesPool>().HasAlternateKey(sp => sp.SalesPoolId);
            modelBuilder.Entity<InventSite>().HasAlternateKey(isite => isite.SiteId);
            modelBuilder.Entity<InventLocation>().HasAlternateKey(il => il.InventLocationId);
            modelBuilder.Entity<InventDim>().HasAlternateKey(id => id.InventDimId);
            modelBuilder.Entity<InventTable>().HasAlternateKey(it => it.ItemId);
            modelBuilder.Entity<InventItemGroup>().HasAlternateKey(iig => iig.ItemGroupId);
            modelBuilder.Entity<InventTransOrigin>().HasAlternateKey(ito => ito.InventTransId);
            modelBuilder.Entity<MainAccount>().HasAlternateKey(ma => ma.MainAccountId);
            modelBuilder.Entity<LedgerJournalName>().HasAlternateKey(ljn => ljn.JournalName);
            modelBuilder.Entity<InventJournalName>().HasAlternateKey(ijn => ijn.JournalNameId);
            modelBuilder.Entity<InventJournalTable>().HasAlternateKey(ijt => ijt.JournalId);

            modelBuilder.Entity<CustTable>().HasAlternateKey(c => c.AccountNum);
            modelBuilder.Entity<SalesTable>().HasAlternateKey(s => s.SalesId);
            modelBuilder.Entity<SalesQuotationTable>().HasAlternateKey(sq => sq.QuotationId);
            modelBuilder.Entity<CustConfirmJour>().HasAlternateKey(ccj => ccj.ConfirmId);
            modelBuilder.Entity<CustInvoiceJour>().HasAlternateKey(cij => cij.InvoiceId);
            modelBuilder.Entity<CustPackingSlipJour>().HasAlternateKey(cpj => cpj.PackingSlipId);
            modelBuilder.Entity<CustQuotationJour>().HasAlternateKey(cqj => cqj.QuotationId);
            modelBuilder.Entity<CustInvoiceTable>().HasAlternateKey(cit => cit.InvoiceId);
            modelBuilder.Entity<LogisticsAddressCountryRegion>().HasAlternateKey(cr => cr.CountryRegionId);
            modelBuilder.Entity<LogisticsAddressState>().HasAlternateKey(s => s.StateId);
            modelBuilder.Entity<InventBatch>().HasAlternateKey(ib => ib.InventBatchId);
            modelBuilder.Entity<InventSerial>().HasAlternateKey(iserial => iserial.InventSerialId);
            modelBuilder.Entity<InventCountGroup>().HasAlternateKey(icg => icg.CountGroupId);
            modelBuilder.Entity<UnitOfMeasure>().HasAlternateKey(u => u.Symbol);

            modelBuilder.Entity<BankGroup>(entity =>
            {
                entity.HasAlternateKey(bg => bg.BankGroupId);
                entity.HasOne(bg => bg.Currency)
                    .WithMany()
                    .HasForeignKey(bg => bg.CurrencyCode)
                    .HasPrincipalKey(c => c.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });
            modelBuilder.Entity<BankTransType>().HasAlternateKey(btt => btt.BankTransactionType);
            modelBuilder.Entity<BankAccountTable>().HasAlternateKey(ba => ba.AccountId);
            modelBuilder.Entity<MarkupTable>().HasAlternateKey(mt => mt.MarkupCode);
            modelBuilder.Entity<TaxTable>().HasAlternateKey(tt => tt.TaxCode);
            modelBuilder.Entity<LedgerJournalTable>().HasAlternateKey(ljt => ljt.JournalNum);

            // ==========================================
            // 2. Configure Relationships (Fluent API)
            // ==========================================

            // --- CustTable ---
            modelBuilder.Entity<CustTable>(entity =>
            {
                entity.HasOne(c => c.InvoiceAccount_CustTable)
                    .WithMany()
                    .HasForeignKey(c => c.InvoiceAccount)
                    .HasPrincipalKey(c => c.AccountNum)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(c => c.InventLocation)
                    .WithMany()
                    .HasForeignKey(c => c.InventLocationId)
                    .HasPrincipalKey(il => il.InventLocationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(c => c.InventSite)
                    .WithMany()
                    .HasForeignKey(c => c.InventSiteId)
                    .HasPrincipalKey(isite => isite.SiteId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(c => c.CustGroup)
                    .WithMany(cg => cg.CustTables)
                    .HasForeignKey(c => c.CustGroupId)
                    .HasPrincipalKey(cg => cg.CustGroupId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(c => c.DirPartyTable)
                    .WithMany()
                    .HasForeignKey(c => c.Party)
                    .HasPrincipalKey(dp => dp.RecId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(c => c.Currency)
                    .WithMany()
                    .HasForeignKey(c => c.CurrencyCode)
                    .HasPrincipalKey(curr => curr.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(c => c.TaxGroupHeading)
                    .WithMany()
                    .HasForeignKey(c => c.TaxGroupId)
                    .HasPrincipalKey(tg => tg.TaxGroup)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(c => c.PaymTerm)
                    .WithMany()
                    .HasForeignKey(c => c.PaymTermId)
                    .HasPrincipalKey(pt => pt.PaymTermId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(c => c.CustPaymModeTable)
                    .WithMany()
                    .HasForeignKey(c => c.PaymModeId)
                    .HasPrincipalKey(cp => cp.PaymMode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(c => c.DlvMode)
                    .WithMany()
                    .HasForeignKey(c => c.DlvModeId)
                    .HasPrincipalKey(dm => dm.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(c => c.SalesPool)
                    .WithMany()
                    .HasForeignKey(c => c.SalesPoolId)
                    .HasPrincipalKey(sp => sp.SalesPoolId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(c => c.DimensionAttributeValueSet)
                    .WithMany()
                    .HasForeignKey(c => c.DefaultDimension)
                    .HasPrincipalKey(dav => dav.RecId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(c => c.MainContactWorkerNavigation)
                    .WithMany()
                    .HasForeignKey(c => c.MainContactWorker)
                    .HasPrincipalKey(emp => emp.RecId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(c => c.LogisticsAddressCountryRegion)
                    .WithMany()
                    .HasForeignKey(c => c.CountryRegionId)
                    .HasPrincipalKey(cr => cr.CountryRegionId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(c => c.LogisticsAddressState)
                    .WithMany()
                    .HasForeignKey(c => c.StateId)
                    .HasPrincipalKey(s => s.StateId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- SalesTable ---
            modelBuilder.Entity<SalesTable>(entity =>
            {
                entity.HasOne(s => s.CustAccount_CustTable)
                    .WithMany(c => c.SalesTable)
                    .HasForeignKey(s => s.CustAccount)
                    .HasPrincipalKey(c => c.AccountNum)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(s => s.InvoiceAccount_CustTable)
                    .WithMany()
                    .HasForeignKey(s => s.InvoiceAccount)
                    .HasPrincipalKey(c => c.AccountNum)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(s => s.CustGroupTable)
                    .WithMany()
                    .HasForeignKey(s => s.CustGroup)
                    .HasPrincipalKey(cg => cg.CustGroupId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(s => s.DlvModeTable)
                    .WithMany()
                    .HasForeignKey(s => s.DlvMode)
                    .HasPrincipalKey(dm => dm.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(s => s.DlvTermTable)
                    .WithMany()
                    .HasForeignKey(s => s.DlvTerm)
                    .HasPrincipalKey(dt => dt.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(s => s.InventLocation)
                    .WithMany()
                    .HasForeignKey(s => s.InventLocationId)
                    .HasPrincipalKey(il => il.InventLocationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(s => s.InventSite)
                    .WithMany()
                    .HasForeignKey(s => s.InventSiteId)
                    .HasPrincipalKey(isite => isite.SiteId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(s => s.TaxGroupHeading)
                    .WithMany()
                    .HasForeignKey(s => s.TaxGroupId)
                    .HasPrincipalKey(tg => tg.TaxGroup)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(s => s.CustPaymModeTable)
                    .WithMany()
                    .HasForeignKey(s => s.PaymMode)
                    .HasPrincipalKey(cp => cp.PaymMode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(s => s.CustLedger)
                    .WithMany()
                    .HasForeignKey(s => s.PostingProfile)
                    .HasPrincipalKey(cl => cl.PostingProfile)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(s => s.SalesPool)
                    .WithMany()
                    .HasForeignKey(s => s.SalesPoolId)
                    .HasPrincipalKey(sp => sp.SalesPoolId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(s => s.Currency)
                    .WithMany()
                    .HasForeignKey(s => s.CurrencyCode)
                    .HasPrincipalKey(curr => curr.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(s => s.PaymTermTable)
                    .WithMany()
                    .HasForeignKey(s => s.PaymTerm)
                    .HasPrincipalKey(pt => pt.PaymTermId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(s => s.PaymentSchedule)
                    .WithMany()
                    .HasForeignKey(s => s.PaymentSched)
                    .HasPrincipalKey(ps => ps.Name)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(s => s.SalesResponsibleEmployee)
                    .WithMany()
                    .HasForeignKey(s => s.WorkerSalesResponsible)
                    .HasPrincipalKey(emp => emp.RecId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(s => s.SalesTakerEmployee)
                    .WithMany()
                    .HasForeignKey(s => s.WorkerSalesTaker)
                    .HasPrincipalKey(emp => emp.RecId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(s => s.DeliveryAddress)
                    .WithMany()
                    .HasForeignKey(s => s.DeliveryPostalAddress)
                    .HasPrincipalKey(addr => addr.RecId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- SalesLine ---
            modelBuilder.Entity<SalesLine>(entity =>
            {
                entity.HasOne(sl => sl.SalesTable)
                    .WithMany(s => s.Lines)
                    .HasForeignKey(sl => sl.SalesId)
                    .HasPrincipalKey(s => s.SalesId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sl => sl.InventTable)
                    .WithMany()
                    .HasForeignKey(sl => sl.ItemId)
                    .HasPrincipalKey(it => it.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sl => sl.InventDim)
                    .WithMany()
                    .HasForeignKey(sl => sl.InventDimId)
                    .HasPrincipalKey(id => id.InventDimId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sl => sl.Currency)
                    .WithMany()
                    .HasForeignKey(sl => sl.CurrencyCode)
                    .HasPrincipalKey(c => c.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sl => sl.CustAccount_CustTable)
                    .WithMany()
                    .HasForeignKey(sl => sl.CustAccount)
                    .HasPrincipalKey(c => c.AccountNum)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sl => sl.CustGroup)
                    .WithMany()
                    .HasForeignKey(sl => sl.CustGroupId)
                    .HasPrincipalKey(cg => cg.CustGroupId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sl => sl.DlvModeTable)
                    .WithMany()
                    .HasForeignKey(sl => sl.DlvMode)
                    .HasPrincipalKey(dm => dm.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sl => sl.DlvTermTable)
                    .WithMany()
                    .HasForeignKey(sl => sl.DlvTerm)
                    .HasPrincipalKey(dt => dt.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sl => sl.TaxGroupHeading)
                    .WithMany()
                    .HasForeignKey(sl => sl.TaxGroup)
                    .HasPrincipalKey(tg => tg.TaxGroup)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sl => sl.TaxItemGroupHeading)
                    .WithMany()
                    .HasForeignKey(sl => sl.TaxItemGroup)
                    .HasPrincipalKey(tig => tig.TaxItemGroup)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- SalesQuotationTable ---
            modelBuilder.Entity<SalesQuotationTable>(entity =>
            {
                entity.HasOne(sq => sq.CustAccount_CustTable)
                    .WithMany()
                    .HasForeignKey(sq => sq.CustAccount)
                    .HasPrincipalKey(c => c.AccountNum)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sq => sq.InvoiceAccount_CustTable)
                    .WithMany()
                    .HasForeignKey(sq => sq.InvoiceAccount)
                    .HasPrincipalKey(c => c.AccountNum)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sq => sq.Currency)
                    .WithMany()
                    .HasForeignKey(sq => sq.CurrencyCode)
                    .HasPrincipalKey(curr => curr.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sq => sq.DlvModeTable)
                    .WithMany()
                    .HasForeignKey(sq => sq.DlvMode)
                    .HasPrincipalKey(dm => dm.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sq => sq.DlvTermTable)
                    .WithMany()
                    .HasForeignKey(sq => sq.DlvTerm)
                    .HasPrincipalKey(dt => dt.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sq => sq.InventLocation)
                    .WithMany()
                    .HasForeignKey(sq => sq.InventLocationId)
                    .HasPrincipalKey(il => il.InventLocationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sq => sq.InventSite)
                    .WithMany()
                    .HasForeignKey(sq => sq.InventSiteId)
                    .HasPrincipalKey(isite => isite.SiteId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sq => sq.TaxGroupHeading)
                    .WithMany()
                    .HasForeignKey(sq => sq.TaxGroupId)
                    .HasPrincipalKey(tg => tg.TaxGroup)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sq => sq.CustPaymModeTable)
                    .WithMany()
                    .HasForeignKey(sq => sq.PaymMode)
                    .HasPrincipalKey(cp => cp.PaymMode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sq => sq.PaymTermTable)
                    .WithMany()
                    .HasForeignKey(sq => sq.Payment)
                    .HasPrincipalKey(pt => pt.PaymTermId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sq => sq.CustLedger)
                    .WithMany()
                    .HasForeignKey(sq => sq.PostingProfile)
                    .HasPrincipalKey(cl => cl.PostingProfile)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sq => sq.SalesPool)
                    .WithMany()
                    .HasForeignKey(sq => sq.SalesPoolId)
                    .HasPrincipalKey(sp => sp.SalesPoolId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sq => sq.DeliveryAddress)
                    .WithMany()
                    .HasForeignKey(sq => sq.DeliveryPostalAddress)
                    .HasPrincipalKey(addr => addr.RecId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sq => sq.SalesResponsibleEmployee)
                    .WithMany()
                    .HasForeignKey(sq => sq.WorkerSalesResponsible)
                    .HasPrincipalKey(emp => emp.RecId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sq => sq.SalesTakerEmployee)
                    .WithMany()
                    .HasForeignKey(sq => sq.WorkerSalesTaker)
                    .HasPrincipalKey(emp => emp.RecId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- SalesQuotationLine ---
            modelBuilder.Entity<SalesQuotationLine>(entity =>
            {
                entity.HasOne(sql => sql.SalesQuotationTable)
                    .WithMany(sq => sq.QuotationLines)
                    .HasForeignKey(sql => sql.QuotationId)
                    .HasPrincipalKey(sq => sq.QuotationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sql => sql.CustAccount_CustTable)
                    .WithMany()
                    .HasForeignKey(sql => sql.CustAccount)
                    .HasPrincipalKey(c => c.AccountNum)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sql => sql.InventTable)
                    .WithMany()
                    .HasForeignKey(sql => sql.ItemId)
                    .HasPrincipalKey(it => it.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sql => sql.Currency)
                    .WithMany()
                    .HasForeignKey(sql => sql.CurrencyCode)
                    .HasPrincipalKey(curr => curr.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sql => sql.DlvModeTable)
                    .WithMany()
                    .HasForeignKey(sql => sql.DlvMode)
                    .HasPrincipalKey(dm => dm.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sql => sql.InventTransOrigin)
                    .WithMany()
                    .HasForeignKey(sql => sql.InventTransId)
                    .HasPrincipalKey(ito => ito.InventTransId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sql => sql.InventDim)
                    .WithMany()
                    .HasForeignKey(sql => sql.InventDimId)
                    .HasPrincipalKey(id => id.InventDimId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sql => sql.TaxGroupHeading)
                    .WithMany()
                    .HasForeignKey(sql => sql.TaxGroup)
                    .HasPrincipalKey(tg => tg.TaxGroup)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sql => sql.TaxItemGroupHeading)
                    .WithMany()
                    .HasForeignKey(sql => sql.TaxItemGroup)
                    .HasPrincipalKey(tig => tig.TaxItemGroup)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sql => sql.DeliveryAddress)
                    .WithMany()
                    .HasForeignKey(sql => sql.DeliveryPostalAddress)
                    .HasPrincipalKey(addr => addr.RecId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- CustConfirmJour ---
            modelBuilder.Entity<CustConfirmJour>(entity =>
            {
                entity.HasOne(ccj => ccj.SalesTable)
                    .WithMany()
                    .HasForeignKey(ccj => ccj.SalesId)
                    .HasPrincipalKey(s => s.SalesId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ccj => ccj.OrderAccount_CustTable)
                    .WithMany()
                    .HasForeignKey(ccj => ccj.OrderAccount)
                    .HasPrincipalKey(c => c.AccountNum)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ccj => ccj.InvoiceAccount_CustTable)
                    .WithMany()
                    .HasForeignKey(ccj => ccj.InvoiceAccount)
                    .HasPrincipalKey(c => c.AccountNum)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ccj => ccj.Currency)
                    .WithMany()
                    .HasForeignKey(ccj => ccj.CurrencyCode)
                    .HasPrincipalKey(curr => curr.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ccj => ccj.DlvModeTable)
                    .WithMany()
                    .HasForeignKey(ccj => ccj.DlvMode)
                    .HasPrincipalKey(dm => dm.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ccj => ccj.DlvTermTable)
                    .WithMany()
                    .HasForeignKey(ccj => ccj.DlvTerm)
                    .HasPrincipalKey(dt => dt.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ccj => ccj.CustGroupTable)
                    .WithMany()
                    .HasForeignKey(ccj => ccj.CustGroup)
                    .HasPrincipalKey(cg => cg.CustGroupId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ccj => ccj.PaymTerm)
                    .WithMany()
                    .HasForeignKey(ccj => ccj.Payment)
                    .HasPrincipalKey(pt => pt.PaymTermId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ccj => ccj.DeliveryAddress)
                    .WithMany()
                    .HasForeignKey(ccj => ccj.DeliveryPostalAddress)
                    .HasPrincipalKey(addr => addr.RecId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ccj => ccj.SalesTakerEmployee)
                    .WithMany()
                    .HasForeignKey(ccj => ccj.WorkerSalesTaker)
                    .HasPrincipalKey(emp => emp.RecId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- CustConfirmTrans ---
            modelBuilder.Entity<CustConfirmTrans>(entity =>
            {
                entity.HasOne(cct => cct.CustConfirmJour)
                    .WithMany(ccj => ccj.ConfirmLines)
                    .HasForeignKey(cct => cct.ConfirmId)
                    .HasPrincipalKey(ccj => ccj.ConfirmId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cct => cct.SalesTable)
                    .WithMany()
                    .HasForeignKey(cct => cct.SalesId)
                    .HasPrincipalKey(s => s.SalesId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cct => cct.InventTable)
                    .WithMany()
                    .HasForeignKey(cct => cct.ItemId)
                    .HasPrincipalKey(it => it.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cct => cct.Currency)
                    .WithMany()
                    .HasForeignKey(cct => cct.CurrencyCode)
                    .HasPrincipalKey(curr => curr.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cct => cct.DlvTermTable)
                    .WithMany()
                    .HasForeignKey(cct => cct.DlvTerm)
                    .HasPrincipalKey(dt => dt.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cct => cct.InventDim)
                    .WithMany()
                    .HasForeignKey(cct => cct.InventDimId)
                    .HasPrincipalKey(id => id.InventDimId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cct => cct.TaxGroupHeading)
                    .WithMany()
                    .HasForeignKey(cct => cct.TaxGroup)
                    .HasPrincipalKey(tg => tg.TaxGroup)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cct => cct.TaxItemGroupHeading)
                    .WithMany()
                    .HasForeignKey(cct => cct.TaxItemGroup)
                    .HasPrincipalKey(tig => tig.TaxItemGroup)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- CustInvoiceJour ---
            modelBuilder.Entity<CustInvoiceJour>(entity =>
            {
                entity.HasOne(cij => cij.SalesTable)
                    .WithMany()
                    .HasForeignKey(cij => cij.SalesId)
                    .HasPrincipalKey(s => s.SalesId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cij => cij.OrderAccount_CustTable)
                    .WithMany()
                    .HasForeignKey(cij => cij.OrderAccount)
                    .HasPrincipalKey(c => c.AccountNum)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cij => cij.InvoiceAccount_CustTable)
                    .WithMany()
                    .HasForeignKey(cij => cij.InvoiceAccount)
                    .HasPrincipalKey(c => c.AccountNum)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cij => cij.Currency)
                    .WithMany()
                    .HasForeignKey(cij => cij.CurrencyCode)
                    .HasPrincipalKey(curr => curr.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cij => cij.DlvModeTable)
                    .WithMany()
                    .HasForeignKey(cij => cij.DlvMode)
                    .HasPrincipalKey(dm => dm.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cij => cij.DlvTermTable)
                    .WithMany()
                    .HasForeignKey(cij => cij.DlvTerm)
                    .HasPrincipalKey(dt => dt.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cij => cij.CustGroupTable)
                    .WithMany()
                    .HasForeignKey(cij => cij.CustGroup)
                    .HasPrincipalKey(cg => cg.CustGroupId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cij => cij.CustLedger)
                    .WithMany()
                    .HasForeignKey(cij => cij.PostingProfile)
                    .HasPrincipalKey(cl => cl.PostingProfile)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cij => cij.PaymTerm)
                    .WithMany()
                    .HasForeignKey(cij => cij.Payment)
                    .HasPrincipalKey(pt => pt.PaymTermId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cij => cij.TaxGroupHeading)
                    .WithMany()
                    .HasForeignKey(cij => cij.TaxGroup)
                    .HasPrincipalKey(tg => tg.TaxGroup)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cij => cij.InventLocation)
                    .WithMany()
                    .HasForeignKey(cij => cij.InventLocationId)
                    .HasPrincipalKey(il => il.InventLocationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cij => cij.SalesTakerEmployee)
                    .WithMany()
                    .HasForeignKey(cij => cij.WorkerSalesTaker)
                    .HasPrincipalKey(emp => emp.RecId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cij => cij.DeliveryAddress)
                    .WithMany()
                    .HasForeignKey(cij => cij.DeliveryPostalAddress)
                    .HasPrincipalKey(addr => addr.RecId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cij => cij.InvoiceAddressMap)
                    .WithMany()
                    .HasForeignKey(cij => cij.InvoicePostalAddress)
                    .HasPrincipalKey(addr => addr.RecId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cij => cij.PaymentSchedule)
                    .WithMany()
                    .HasForeignKey(cij => cij.PaymentSched)
                    .HasPrincipalKey(ps => ps.Name)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- CustInvoiceTrans ---
            modelBuilder.Entity<CustInvoiceTrans>(entity =>
            {
                entity.HasOne(cit => cit.CustInvoiceJour)
                    .WithMany(cij => cij.InvoiceLines)
                    .HasForeignKey(cit => cit.InvoiceId)
                    .HasPrincipalKey(cij => cij.InvoiceId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cit => cit.SalesTable)
                    .WithMany()
                    .HasForeignKey(cit => cit.SalesId)
                    .HasPrincipalKey(s => s.SalesId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cit => cit.InventTable)
                    .WithMany()
                    .HasForeignKey(cit => cit.ItemId)
                    .HasPrincipalKey(it => it.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cit => cit.Currency)
                    .WithMany()
                    .HasForeignKey(cit => cit.CurrencyCode)
                    .HasPrincipalKey(curr => curr.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);



                entity.HasOne(cit => cit.InventDim)
                    .WithMany()
                    .HasForeignKey(cit => cit.InventDimId)
                    .HasPrincipalKey(id => id.InventDimId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cit => cit.TaxGroupHeading)
                    .WithMany()
                    .HasForeignKey(cit => cit.TaxGroup)
                    .HasPrincipalKey(tg => tg.TaxGroup)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cit => cit.TaxItemGroupHeading)
                    .WithMany()
                    .HasForeignKey(cit => cit.TaxItemGroup)
                    .HasPrincipalKey(tig => tig.TaxItemGroup)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- CustPackingSlipJour ---
            modelBuilder.Entity<CustPackingSlipJour>(entity =>
            {
                entity.HasOne(cpj => cpj.SalesTable)
                    .WithMany()
                    .HasForeignKey(cpj => cpj.SalesId)
                    .HasPrincipalKey(s => s.SalesId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cpj => cpj.OrderAccount_CustTable)
                    .WithMany()
                    .HasForeignKey(cpj => cpj.OrderAccount)
                    .HasPrincipalKey(c => c.AccountNum)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cpj => cpj.InvoiceAccount_CustTable)
                    .WithMany()
                    .HasForeignKey(cpj => cpj.InvoiceAccount)
                    .HasPrincipalKey(c => c.AccountNum)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cpj => cpj.DlvModeTable)
                    .WithMany()
                    .HasForeignKey(cpj => cpj.DlvMode)
                    .HasPrincipalKey(dm => dm.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cpj => cpj.DlvTermTable)
                    .WithMany()
                    .HasForeignKey(cpj => cpj.DlvTerm)
                    .HasPrincipalKey(dt => dt.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cpj => cpj.InventLocation)
                    .WithMany()
                    .HasForeignKey(cpj => cpj.InventLocationId)
                    .HasPrincipalKey(il => il.InventLocationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cpj => cpj.DeliveryAddress)
                    .WithMany()
                    .HasForeignKey(cpj => cpj.DeliveryPostalAddress)
                    .HasPrincipalKey(addr => addr.RecId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cpj => cpj.InvoiceAddressMap)
                    .WithMany()
                    .HasForeignKey(cpj => cpj.InvoicePostalAddress)
                    .HasPrincipalKey(addr => addr.RecId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cpj => cpj.SalesTakerEmployee)
                    .WithMany()
                    .HasForeignKey(cpj => cpj.WorkerSalesTaker)
                    .HasPrincipalKey(emp => emp.RecId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- CustPackingSlipTrans ---
            modelBuilder.Entity<CustPackingSlipTrans>(entity =>
            {
                entity.HasOne(cpt => cpt.CustPackingSlipJour)
                    .WithMany(cpj => cpj.PackingSlipLines)
                    .HasForeignKey(cpt => cpt.PackingSlipId)
                    .HasPrincipalKey(cpj => cpj.PackingSlipId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cpt => cpt.SalesTable)
                    .WithMany()
                    .HasForeignKey(cpt => cpt.SalesId)
                    .HasPrincipalKey(s => s.SalesId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cpt => cpt.InventTable)
                    .WithMany()
                    .HasForeignKey(cpt => cpt.ItemId)
                    .HasPrincipalKey(it => it.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cpt => cpt.InventDim)
                    .WithMany()
                    .HasForeignKey(cpt => cpt.InventDimId)
                    .HasPrincipalKey(id => id.InventDimId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- CustTrans ---
            modelBuilder.Entity<CustTrans>(entity =>
            {
                entity.HasOne(ct => ct.Customer)
                    .WithMany(c => c.CustTrans)
                    .HasForeignKey(ct => ct.AccountNum)
                    .HasPrincipalKey(c => c.AccountNum)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ct => ct.Currency)
                    .WithMany()
                    .HasForeignKey(ct => ct.CurrencyCode)
                    .HasPrincipalKey(curr => curr.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ct => ct.PaymTerm)
                    .WithMany()
                    .HasForeignKey(ct => ct.PaymTermId)
                    .HasPrincipalKey(pt => pt.PaymTermId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ct => ct.CustPaymModeTable)
                    .WithMany()
                    .HasForeignKey(ct => ct.PaymMode)
                    .HasPrincipalKey(cp => cp.PaymMode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ct => ct.CustLedger)
                    .WithMany()
                    .HasForeignKey(ct => ct.PostingProfile)
                    .HasPrincipalKey(cl => cl.PostingProfile)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ct => ct.PaymentSchedule)
                    .WithMany()
                    .HasForeignKey(ct => ct.PaymSchedId)
                    .HasPrincipalKey(ps => ps.Name)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ct => ct.ApproverEmployee)
                    .WithMany()
                    .HasForeignKey(ct => ct.Approver)
                    .HasPrincipalKey(emp => emp.RecId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ct => ct.OrderAccount_CustTable)
                    .WithMany()
                    .HasForeignKey(ct => ct.OrderAccount)
                    .HasPrincipalKey(c => c.AccountNum)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ct => ct.DlvModeTable)
                    .WithMany()
                    .HasForeignKey(ct => ct.DeliveryMode)
                    .HasPrincipalKey(dm => dm.Code)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- CustTransOpen ---
            modelBuilder.Entity<CustTransOpen>(entity =>
            {
                entity.HasOne(cto => cto.CustomerAccount)
                    .WithMany()
                    .HasForeignKey(cto => cto.AccountNum)
                    .HasPrincipalKey(c => c.AccountNum)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cto => cto.ParentCustomerTransaction)
                    .WithMany()
                    .HasForeignKey(cto => cto.RefRecId)
                    .HasPrincipalKey(ct => ct.RecId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- CustSettlement ---
            modelBuilder.Entity<CustSettlement>(entity =>
            {
                entity.HasOne(cs => cs.CustTable)
                    .WithMany(c => c.CustSettlement)
                    .HasForeignKey(cs => cs.AccountNum)
                    .HasPrincipalKey(c => c.AccountNum)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cs => cs.CustTrans)
                    .WithMany(ct => ct.Settlements)
                    .HasForeignKey(cs => cs.TransRecId)
                    .HasPrincipalKey(ct => ct.RecId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cs => cs.OffsetCustTrans)
                    .WithMany()
                    .HasForeignKey(cs => cs.OffsetRecId)
                    .HasPrincipalKey(ct => ct.RecId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- CustQuotationJour ---
            modelBuilder.Entity<CustQuotationJour>(entity =>
            {
                entity.HasOne(cqj => cqj.SalesQuotationTable)
                    .WithMany()
                    .HasForeignKey(cqj => cqj.QuotationId)
                    .HasPrincipalKey(sq => sq.QuotationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cqj => cqj.OrderAccount_CustTable)
                    .WithMany()
                    .HasForeignKey(cqj => cqj.OrderAccount)
                    .HasPrincipalKey(c => c.AccountNum)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cqj => cqj.InvoiceAccount_CustTable)
                    .WithMany()
                    .HasForeignKey(cqj => cqj.InvoiceAccount)
                    .HasPrincipalKey(c => c.AccountNum)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cqj => cqj.Currency)
                    .WithMany()
                    .HasForeignKey(cqj => cqj.CurrencyCode)
                    .HasPrincipalKey(c => c.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cqj => cqj.DlvModeTable)
                    .WithMany()
                    .HasForeignKey(cqj => cqj.DlvMode)
                    .HasPrincipalKey(dm => dm.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cqj => cqj.DlvTermTable)
                    .WithMany()
                    .HasForeignKey(cqj => cqj.DlvTerm)
                    .HasPrincipalKey(dt => dt.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cqj => cqj.CustGroupTable)
                    .WithMany()
                    .HasForeignKey(cqj => cqj.CustGroup)
                    .HasPrincipalKey(cg => cg.CustGroupId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cqj => cqj.PaymTermTable)
                    .WithMany()
                    .HasForeignKey(cqj => cqj.Payment)
                    .HasPrincipalKey(pt => pt.PaymTermId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- CustQuotationTrans ---
            modelBuilder.Entity<CustQuotationTrans>(entity =>
            {
                entity.HasOne(cqt => cqt.CustQuotationJour)
                    .WithMany()
                    .HasForeignKey(cqt => cqt.QuotationId)
                    .HasPrincipalKey(cqj => cqj.QuotationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cqt => cqt.InventTable)
                    .WithMany()
                    .HasForeignKey(cqt => cqt.ItemId)
                    .HasPrincipalKey(it => it.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cqt => cqt.Currency)
                    .WithMany()
                    .HasForeignKey(cqt => cqt.CurrencyCode)
                    .HasPrincipalKey(c => c.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cqt => cqt.InventDim)
                    .WithMany()
                    .HasForeignKey(cqt => cqt.InventDimId)
                    .HasPrincipalKey(id => id.InventDimId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cqt => cqt.TaxGroupHeading)
                    .WithMany()
                    .HasForeignKey(cqt => cqt.TaxGroup)
                    .HasPrincipalKey(tg => tg.TaxGroup)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cqt => cqt.TaxItemGroupHeading)
                    .WithMany()
                    .HasForeignKey(cqt => cqt.TaxItemGroup)
                    .HasPrincipalKey(tig => tig.TaxItemGroup)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- CustInvoiceTable ---
            modelBuilder.Entity<CustInvoiceTable>(entity =>
            {
                entity.HasOne(cit => cit.OrderAccount_CustTable)
                    .WithMany()
                    .HasForeignKey(cit => cit.OrderAccount)
                    .HasPrincipalKey(c => c.AccountNum)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cit => cit.InvoiceAccount_CustTable)
                    .WithMany()
                    .HasForeignKey(cit => cit.InvoiceAccount)
                    .HasPrincipalKey(c => c.AccountNum)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cit => cit.Currency)
                    .WithMany()
                    .HasForeignKey(cit => cit.CurrencyCode)
                    .HasPrincipalKey(c => c.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cit => cit.DlvTermTable)
                    .WithMany()
                    .HasForeignKey(cit => cit.DlvTerm)
                    .HasPrincipalKey(dt => dt.Code)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cit => cit.CustGroupTable)
                    .WithMany()
                    .HasForeignKey(cit => cit.CustGroup)
                    .HasPrincipalKey(cg => cg.CustGroupId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cit => cit.CustLedger)
                    .WithMany()
                    .HasForeignKey(cit => cit.PostingProfile)
                    .HasPrincipalKey(cl => cl.PostingProfile)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cit => cit.PaymTerm)
                    .WithMany()
                    .HasForeignKey(cit => cit.Payment)
                    .HasPrincipalKey(pt => pt.PaymTermId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cit => cit.CustPaymModeTable)
                    .WithMany()
                    .HasForeignKey(cit => cit.PaymMode)
                    .HasPrincipalKey(cp => cp.PaymMode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cit => cit.TaxGroupHeading)
                    .WithMany()
                    .HasForeignKey(cit => cit.TaxGroup)
                    .HasPrincipalKey(tg => tg.TaxGroup)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cit => cit.TaxItemGroupHeading)
                    .WithMany()
                    .HasForeignKey(cit => cit.TaxItemGroup)
                    .HasPrincipalKey(tig => tig.TaxItemGroup)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cit => cit.PaymentSchedule)
                    .WithMany()
                    .HasForeignKey(cit => cit.PaymentSched)
                    .HasPrincipalKey(ps => ps.Name)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- CustInvoiceLine ---
            modelBuilder.Entity<CustInvoiceLine>(entity =>
            {
                entity.HasOne(cil => cil.CustInvoiceTable)
                    .WithMany(cit => cit.InvoiceLines)
                    .HasForeignKey(cil => cil.ParentRecId)
                    .HasPrincipalKey(cit => cit.RecId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cil => cil.TaxGroupHeading)
                    .WithMany()
                    .HasForeignKey(cil => cil.TaxGroup)
                    .HasPrincipalKey(tg => tg.TaxGroup)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cil => cil.TaxItemGroupHeading)
                    .WithMany()
                    .HasForeignKey(cil => cil.TaxItemGroup)
                    .HasPrincipalKey(tig => tig.TaxItemGroup)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- CustLedgerAccounts ---
            modelBuilder.Entity<CustLedgerAccounts>(entity =>
            {
                entity.HasOne(cla => cla.CustLedger)
                    .WithMany()
                    .HasForeignKey(cla => cla.PostingProfile)
                    .HasPrincipalKey(cl => cl.PostingProfile)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ==========================================
            // 3. Configure Inventory relationships
            // ==========================================

            // --- InventItemGroupItem ---
            modelBuilder.Entity<InventItemGroupItem>(entity =>
            {
                entity.HasOne(iigi => iigi.InventTable)
                    .WithMany()
                    .HasForeignKey(iigi => iigi.ItemId)
                    .HasPrincipalKey(it => it.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(iigi => iigi.InventItemGroup)
                    .WithMany()
                    .HasForeignKey(iigi => iigi.ItemGroupId)
                    .HasPrincipalKey(iig => iig.ItemGroupId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- InventTransOrigin ---
            modelBuilder.Entity<InventTransOrigin>(entity =>
            {
                entity.HasOne(ito => ito.ReleasedProduct)
                    .WithMany()
                    .HasForeignKey(ito => ito.ItemId)
                    .HasPrincipalKey(it => it.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ito => ito.HeaderDimensions)
                    .WithMany()
                    .HasForeignKey(ito => ito.ItemInventDimId)
                    .HasPrincipalKey(id => id.InventDimId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- InventDim ---
            modelBuilder.Entity<InventDim>(entity =>
            {
                entity.HasOne(id => id.InventoryBatch)
                    .WithMany()
                    .HasForeignKey(id => id.InventBatchId)
                    .HasPrincipalKey(ib => ib.InventBatchId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(id => id.InventSite)
                    .WithMany()
                    .HasForeignKey(id => id.InventSiteId)
                    .HasPrincipalKey(isite => isite.SiteId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(id => id.InventLocation)
                    .WithMany()
                    .HasForeignKey(id => id.InventLocationId)
                    .HasPrincipalKey(il => il.InventLocationId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- InventLocation ---
            modelBuilder.Entity<InventLocation>(entity =>
            {
                entity.HasOne(il => il.AssociatedSite)
                    .WithMany()
                    .HasForeignKey(il => il.InventSiteId)
                    .HasPrincipalKey(isite => isite.SiteId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- InventBatch ---
            modelBuilder.Entity<InventBatch>(entity =>
            {
                entity.HasOne(ib => ib.InventoryItem)
                    .WithMany()
                    .HasForeignKey(ib => ib.ItemId)
                    .HasPrincipalKey(it => it.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- LedgerJournalName ---
            modelBuilder.Entity<LedgerJournalName>(entity =>
            {
                entity.HasOne(ljn => ljn.Currency)
                    .WithMany()
                    .HasForeignKey(ljn => ljn.CurrencyCode)
                    .HasPrincipalKey(c => c.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- BankAccountTable ---
            modelBuilder.Entity<BankAccountTable>(entity =>
            {
                entity.HasOne(ba => ba.BankGroup)
                    .WithMany()
                    .HasForeignKey(ba => ba.BankGroupId)
                    .HasPrincipalKey(bg => bg.BankGroupId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ba => ba.Currency)
                    .WithMany()
                    .HasForeignKey(ba => ba.CurrencyCode)
                    .HasPrincipalKey(c => c.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- Ledger ---
            modelBuilder.Entity<Ledger>(entity =>
            {
                entity.HasOne(l => l.BaseAccountingCurrency)
                    .WithMany()
                    .HasForeignKey(l => l.AccountingCurrency)
                    .HasPrincipalKey(c => c.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(l => l.BaseReportingCurrency)
                    .WithMany()
                    .HasForeignKey(l => l.ReportingCurrency)
                    .HasPrincipalKey(c => c.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- MarkupTrans ---
            modelBuilder.Entity<MarkupTrans>(entity =>
            {
                entity.HasOne(mt => mt.AssociatedMarkup)
                    .WithMany()
                    .HasForeignKey(mt => mt.MarkupCode)
                    .HasPrincipalKey(m => m.MarkupCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(mt => mt.Currency)
                    .WithMany()
                    .HasForeignKey(mt => mt.CurrencyCode)
                    .HasPrincipalKey(c => c.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- TaxTable ---
            modelBuilder.Entity<TaxTable>(entity =>
            {
                entity.HasOne(tt => tt.CurrencySetup)
                    .WithMany()
                    .HasForeignKey(tt => tt.TaxCurrencyCode)
                    .HasPrincipalKey(c => c.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- TaxData ---
            modelBuilder.Entity<TaxData>(entity =>
            {
                entity.HasOne(td => td.TaxCodeConfiguration)
                    .WithMany()
                    .HasForeignKey(td => td.TaxCode)
                    .HasPrincipalKey(t => t.TaxCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- LedgerJournalTable ---
            modelBuilder.Entity<LedgerJournalTable>(entity =>
            {
                entity.HasOne(ljt => ljt.JournalDefinition)
                    .WithMany()
                    .HasForeignKey(ljt => ljt.JournalName)
                    .HasPrincipalKey(ljn => ljn.JournalName)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ljt => ljt.Currency)
                    .WithMany()
                    .HasForeignKey(ljt => ljt.CurrencyCode)
                    .HasPrincipalKey(c => c.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- LedgerJournalTrans ---
            modelBuilder.Entity<LedgerJournalTrans>(entity =>
            {
                entity.HasOne(ljt => ljt.CoreTaxCodeSetup)
                    .WithMany()
                    .HasForeignKey(ljt => ljt.TaxCode)
                    .HasPrincipalKey(t => t.TaxCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ljt => ljt.CoreTaxItemGroupSetup)
                    .WithMany()
                    .HasForeignKey(ljt => ljt.TaxItemGroup)
                    .HasPrincipalKey(tig => tig.TaxItemGroup)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ljt => ljt.JournalTable)
                    .WithMany()
                    .HasForeignKey(ljt => ljt.JournalNum)
                    .HasPrincipalKey(lj => lj.JournalNum)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ljt => ljt.Currency)
                    .WithMany()
                    .HasForeignKey(ljt => ljt.CurrencyCode)
                    .HasPrincipalKey(c => c.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ljt => ljt.TaxGroupHeading)
                    .WithMany()
                    .HasForeignKey(ljt => ljt.TaxGroup)
                    .HasPrincipalKey(tg => tg.TaxGroup)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- BankAccountTrans ---
            modelBuilder.Entity<BankAccountTrans>(entity =>
            {
                entity.HasOne(bat => bat.BankAccountTable)
                    .WithMany()
                    .HasForeignKey(bat => bat.AccountId)
                    .HasPrincipalKey(ba => ba.AccountId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(bat => bat.TransactionCurrency)
                    .WithMany()
                    .HasForeignKey(bat => bat.CurrencyCode)
                    .HasPrincipalKey(c => c.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(bat => bat.BankTransactionCurrency)
                    .WithMany()
                    .HasForeignKey(bat => bat.BankTransCurrencyCode)
                    .HasPrincipalKey(c => c.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(bat => bat.BankTransactionTypeInfo)
                    .WithMany()
                    .HasForeignKey(bat => bat.BankTransType)
                    .HasPrincipalKey(btt => btt.BankTransactionType)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- BankAccountStatement ---
            modelBuilder.Entity<BankAccountStatement>(entity =>
            {
                entity.HasOne(bas => bas.BankAccount)
                    .WithMany()
                    .HasForeignKey(bas => bas.AccountId)
                    .HasPrincipalKey(ba => ba.AccountId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(bas => bas.Currency)
                    .WithMany()
                    .HasForeignKey(bas => bas.CurrencyCode)
                    .HasPrincipalKey(c => c.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- GeneralJournalAccountEntry ---
            modelBuilder.Entity<GeneralJournalAccountEntry>(entity =>
            {
                entity.HasOne(gjae => gjae.TransactionCurrency)
                    .WithMany()
                    .HasForeignKey(gjae => gjae.TransactionCurrencyCode)
                    .HasPrincipalKey(c => c.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- MainAccount ---
            modelBuilder.Entity<MainAccount>(entity =>
            {
                entity.HasOne(ma => ma.Currency)
                    .WithMany()
                    .HasForeignKey(ma => ma.CurrencyCode)
                    .HasPrincipalKey(c => c.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ma => ma.ParentAccount)
                    .WithMany()
                    .HasForeignKey(ma => ma.ParentMainAccount)
                    .HasPrincipalKey(p => p.RecId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ma => ma.OpeningMainAccount)
                    .WithMany()
                    .HasForeignKey(ma => ma.OpeningAccount)
                    .HasPrincipalKey(o => o.RecId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- TaxJournalTrans ---
            modelBuilder.Entity<TaxJournalTrans>(entity =>
            {
                entity.HasOne(tjt => tjt.TaxCodeConfiguration)
                    .WithMany()
                    .HasForeignKey(tjt => tjt.TaxCode)
                    .HasPrincipalKey(tt => tt.TaxCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(tjt => tjt.AppliedTaxGroup)
                    .WithMany()
                    .HasForeignKey(tjt => tjt.TaxGroup)
                    .HasPrincipalKey(tgh => tgh.TaxGroup)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(tjt => tjt.AppliedTaxItemGroup)
                    .WithMany()
                    .HasForeignKey(tjt => tjt.TaxItemGroup)
                    .HasPrincipalKey(tigh => tigh.TaxItemGroup)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(tjt => tjt.SourceCurrency)
                    .WithMany()
                    .HasForeignKey(tjt => tjt.SourceCurrencyCode)
                    .HasPrincipalKey(c => c.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- TaxGroupData ---
            modelBuilder.Entity<TaxGroupData>(entity =>
            {
                entity.HasOne(tgd => tgd.ParentTaxGroup)
                    .WithMany()
                    .HasForeignKey(tgd => tgd.TaxGroup)
                    .HasPrincipalKey(tgh => tgh.TaxGroup)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(tgd => tgd.AssociatedTaxCode)
                    .WithMany()
                    .HasForeignKey(tgd => tgd.TaxCode)
                    .HasPrincipalKey(tt => tt.TaxCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- TaxOnItem ---
            modelBuilder.Entity<TaxOnItem>(entity =>
            {
                entity.HasOne(toi => toi.ParentTaxItemGroup)
                    .WithMany()
                    .HasForeignKey(toi => toi.TaxItemGroup)
                    .HasPrincipalKey(tigh => tigh.TaxItemGroup)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(toi => toi.AssociatedTaxCode)
                    .WithMany()
                    .HasForeignKey(toi => toi.TaxCode)
                    .HasPrincipalKey(tt => tt.TaxCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- TaxTrans ---
            modelBuilder.Entity<TaxTrans>(entity =>
            {
                entity.HasOne(tt => tt.TaxCodeMaster)
                    .WithMany()
                    .HasForeignKey(tt => tt.TaxCode)
                    .HasPrincipalKey(t => t.TaxCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(tt => tt.AppliedTaxGroup)
                    .WithMany()
                    .HasForeignKey(tt => tt.TaxGroup)
                    .HasPrincipalKey(tgh => tgh.TaxGroup)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(tt => tt.AppliedTaxItemGroup)
                    .WithMany()
                    .HasForeignKey(tt => tt.TaxItemGroup)
                    .HasPrincipalKey(tigh => tigh.TaxItemGroup)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(tt => tt.TransactionCurrency)
                    .WithMany()
                    .HasForeignKey(tt => tt.CurrencyCode)
                    .HasPrincipalKey(c => c.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(tt => tt.SourceCurrency)
                    .WithMany()
                    .HasForeignKey(tt => tt.SourceCurrencyCode)
                    .HasPrincipalKey(c => c.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- InventSerial ---
            modelBuilder.Entity<InventSerial>(entity =>
            {
                entity.HasOne(invS => invS.InventTable)
                    .WithMany()
                    .HasForeignKey(invS => invS.ItemId)
                    .HasPrincipalKey(it => it.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- InventSum ---
            modelBuilder.Entity<InventSum>(entity =>
            {
                entity.HasOne(sum => sum.Dimensions)
                    .WithMany()
                    .HasForeignKey(sum => sum.InventDimId)
                    .HasPrincipalKey(id => id.InventDimId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sum => sum.InventTable)
                    .WithMany()
                    .HasForeignKey(sum => sum.ItemId)
                    .HasPrincipalKey(it => it.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sum => sum.InventSite)
                    .WithMany()
                    .HasForeignKey(sum => sum.InventSiteId)
                    .HasPrincipalKey(s => s.SiteId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sum => sum.InventLocation)
                    .WithMany()
                    .HasForeignKey(sum => sum.InventLocationId)
                    .HasPrincipalKey(il => il.InventLocationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sum => sum.InventBatch)
                    .WithMany()
                    .HasForeignKey(sum => sum.InventBatchId)
                    .HasPrincipalKey(ib => ib.InventBatchId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(sum => sum.InventSerial)
                    .WithMany()
                    .HasForeignKey(sum => sum.InventSerialId)
                    .HasPrincipalKey(iserial => iserial.InventSerialId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- InventItemLocation ---
            modelBuilder.Entity<InventItemLocation>(entity =>
            {
                entity.HasOne(iil => iil.Dimensions)
                    .WithMany()
                    .HasForeignKey(iil => iil.InventDimId)
                    .HasPrincipalKey(id => id.InventDimId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(iil => iil.InventTable)
                    .WithMany()
                    .HasForeignKey(iil => iil.ItemId)
                    .HasPrincipalKey(it => it.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(iil => iil.CountGroup)
                    .WithMany()
                    .HasForeignKey(iil => iil.CountGroupId)
                    .HasPrincipalKey(icg => icg.CountGroupId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- InventJournalTable ---
            modelBuilder.Entity<InventJournalTable>(entity =>
            {
                entity.HasOne(ijt => ijt.JournalName)
                    .WithMany()
                    .HasForeignKey(ijt => ijt.JournalNameId)
                    .HasPrincipalKey(ijn => ijn.JournalNameId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ijt => ijt.InventSite)
                    .WithMany()
                    .HasForeignKey(ijt => ijt.InventSiteId)
                    .HasPrincipalKey(s => s.SiteId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ijt => ijt.InventLocation)
                    .WithMany()
                    .HasForeignKey(ijt => ijt.InventLocationId)
                    .HasPrincipalKey(il => il.InventLocationId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- InventJournalTrans ---
            modelBuilder.Entity<InventJournalTrans>(entity =>
            {
                entity.HasOne(ijt => ijt.JournalHeader)
                    .WithMany()
                    .HasForeignKey(ijt => ijt.JournalId)
                    .HasPrincipalKey(h => h.JournalId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ijt => ijt.Dimensions)
                    .WithMany()
                    .HasForeignKey(ijt => ijt.InventDimId)
                    .HasPrincipalKey(id => id.InventDimId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ijt => ijt.ToDimensions)
                    .WithMany()
                    .HasForeignKey(ijt => ijt.ToInventDimId)
                    .HasPrincipalKey(id => id.InventDimId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ijt => ijt.InventTable)
                    .WithMany()
                    .HasForeignKey(ijt => ijt.ItemId)
                    .HasPrincipalKey(it => it.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ijt => ijt.ProjTaxGroup)
                    .WithMany()
                    .HasForeignKey(ijt => ijt.ProjTaxGroupId)
                    .HasPrincipalKey(tg => tg.TaxGroup)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(ijt => ijt.ProjTaxItemGroup)
                    .WithMany()
                    .HasForeignKey(ijt => ijt.ProjTaxItemGroupId)
                    .HasPrincipalKey(tig => tig.TaxItemGroup)
                    .OnDelete(DeleteBehavior.NoAction);


                entity.HasOne(ijt => ijt.UnitOfMeasure)
                    .WithMany()
                    .HasForeignKey(ijt => ijt.Unit)
                    .HasPrincipalKey(tig => tig.Symbol)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- InventItemPrice ---
            modelBuilder.Entity<InventItemPrice>(entity =>
            {
                entity.HasOne(iip => iip.Dimensions)
                    .WithMany()
                    .HasForeignKey(iip => iip.InventDimId)
                    .HasPrincipalKey(id => id.InventDimId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(iip => iip.InventTable)
                    .WithMany()
                    .HasForeignKey(iip => iip.ItemId)
                    .HasPrincipalKey(it => it.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(iip => iip.UnitOfMeasure)
               .WithMany()
               .HasForeignKey(iip => iip.UnitId)
               .HasPrincipalKey(it => it.Symbol)
               .OnDelete(DeleteBehavior.NoAction);
            });

            // --- InventTrans ---
            modelBuilder.Entity<InventTrans>(entity =>
            {
                entity.HasOne(it => it.Dimensions)
                    .WithMany()
                    .HasForeignKey(it => it.InventDimId)
                    .HasPrincipalKey(id => id.InventDimId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(it => it.TransactionCurrency)
                    .WithMany()
                    .HasForeignKey(it => it.CurrencyCode)
                    .HasPrincipalKey(c => c.CurrencyCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(it => it.InventTable)
                    .WithMany()
                    .HasForeignKey(it => it.ItemId)
                    .HasPrincipalKey(table => table.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(it => it.TransactionOriginLink)
                    .WithMany()
                    .HasForeignKey(it => it.InventTransOrigin)
                    .HasPrincipalKey(o => o.RecId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(it => it.MarkingRefOrigin)
                    .WithMany()
                    .HasForeignKey(it => it.MarkingRefInventTransOrigin)
                    .HasPrincipalKey(o => o.RecId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(it => it.ReturnOrigin)
                    .WithMany()
                    .HasForeignKey(it => it.ReturnInventTransOrigin)
                    .HasPrincipalKey(o => o.RecId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- InventCountJour ---
            modelBuilder.Entity<InventCountJour>(entity =>
            {
                entity.HasOne(icj => icj.Dimensions)
                    .WithMany()
                    .HasForeignKey(icj => icj.InventDimId)
                    .HasPrincipalKey(id => id.InventDimId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(icj => icj.InventTable)
                    .WithMany()
                    .HasForeignKey(icj => icj.ItemId)
                    .HasPrincipalKey(it => it.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(icj => icj.JournalHeader)
                    .WithMany()
                    .HasForeignKey(icj => icj.JournalId)
                    .HasPrincipalKey(h => h.JournalId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- InventItemBarcode ---
            modelBuilder.Entity<InventItemBarcode>(entity =>
            {
                entity.HasOne(iib => iib.Dimensions)
                    .WithMany()
                    .HasForeignKey(iib => iib.InventDimId)
                    .HasPrincipalKey(id => id.InventDimId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(iib => iib.InventTable)
                    .WithMany()
                    .HasForeignKey(iib => iib.ItemId)
                    .HasPrincipalKey(it => it.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(iib => iib.UnitOfMeasure)
                    .WithMany()
                    .HasForeignKey(iib => iib.UnitId)
                    .HasPrincipalKey(u => u.Symbol)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- InventSettlement ---
            modelBuilder.Entity<InventSettlement>(entity =>
            {
                entity.HasOne(isett => isett.InventTable)
                    .WithMany()
                    .HasForeignKey(isett => isett.ItemId)
                    .HasPrincipalKey(it => it.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(isett => isett.InventItemGroup)
                    .WithMany()
                    .HasForeignKey(isett => isett.ItemGroupId)
                    .HasPrincipalKey(iig => iig.ItemGroupId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- InventLocation ---
            modelBuilder.Entity<InventLocation>(entity =>
            {
                entity.HasOne(il => il.AssociatedSite)
                    .WithMany()
                    .HasForeignKey(il => il.InventSiteId)
                    .HasPrincipalKey(s => s.SiteId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(il => il.TransitLocation)
                    .WithMany()
                    .HasForeignKey(il => il.InventLocationIdTransit)
                    .HasPrincipalKey(l => l.InventLocationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(il => il.QuarantineLocation)
                    .WithMany()
                    .HasForeignKey(il => il.InventLocationIdQuarantine)
                    .HasPrincipalKey(l => l.InventLocationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(il => il.ReqMainLocation)
                    .WithMany()
                    .HasForeignKey(il => il.InventLocationIdReqMain)
                    .HasPrincipalKey(l => l.InventLocationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(il => il.GitLocation)
                    .WithMany()
                    .HasForeignKey(il => il.ItmInventLocationIdGit)
                    .HasPrincipalKey(l => l.InventLocationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(il => il.UnderLocation)
                    .WithMany()
                    .HasForeignKey(il => il.ItmInventLocationIdUnder)
                    .HasPrincipalKey(l => l.InventLocationId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- InventDim ---
            modelBuilder.Entity<InventDim>(entity =>
            {
                entity.HasOne(id => id.InventoryBatch)
                    .WithMany()
                    .HasForeignKey(id => id.InventBatchId)
                    .HasPrincipalKey(ib => ib.InventBatchId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(id => id.InventSite)
                    .WithMany()
                    .HasForeignKey(id => id.InventSiteId)
                    .HasPrincipalKey(s => s.SiteId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(id => id.InventLocation)
                    .WithMany()
                    .HasForeignKey(id => id.InventLocationId)
                    .HasPrincipalKey(l => l.InventLocationId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(id => id.InventSerial)
                    .WithMany()
                    .HasForeignKey(id => id.InventSerialId)
                    .HasPrincipalKey(s => s.InventSerialId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- InventTableModule ---
            modelBuilder.Entity<InventTableModule>(entity =>
            {
                entity.HasOne(itm => itm.ReleasedProduct)
                    .WithMany()
                    .HasForeignKey(itm => itm.ItemId)
                    .HasPrincipalKey(it => it.ItemId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(itm => itm.UnitOfMeasure)
                    .WithMany()
                    .HasForeignKey(itm => itm.UnitId)
                    .HasPrincipalKey(u => u.Symbol)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(itm => itm.TaxItemGroupHeading)
                    .WithMany()
                    .HasForeignKey(itm => itm.TaxItemGroupId)
                    .HasPrincipalKey(t => t.TaxItemGroup)
                    .OnDelete(DeleteBehavior.NoAction);
            });


            // --- InventTableModule ---
            modelBuilder.Entity<CustPaymModeTable>(entity =>
            {
                entity.HasOne(itm => itm.LedgerJournalName)
                    .WithMany()
                    .HasForeignKey(itm => itm.PaymJournalNameId)
                    .HasPrincipalKey(it => it.JournalName)
                    .OnDelete(DeleteBehavior.NoAction);


            });
            modelBuilder.Entity<LogisticsAddressState>(entity =>
            {
                entity.HasOne(itm => itm.CountryContext)
                    .WithMany()
                    .HasForeignKey(itm => itm.CountryRegionId)
                    .HasPrincipalKey(it => it.CountryRegionId)
                    .OnDelete(DeleteBehavior.NoAction);


            });
            modelBuilder.Entity<LogisticsPostalAddress>(entity =>
            {
                entity.HasOne(itm => itm.LogisticsAddressCountryRegion)
                    .WithMany()
                    .HasForeignKey(itm => itm.CountryRegionId)
                    .HasPrincipalKey(it => it.CountryRegionId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(itm => itm.LogisticsLocation)
                .WithMany()
                .HasForeignKey(itm => itm.Location)
                .HasPrincipalKey(it => it.RecId)
                .OnDelete(DeleteBehavior.NoAction);


                entity.HasOne(itm => itm.DirPartyTable)
                .WithMany()
                .HasForeignKey(itm => itm.PrivateForParty)
                .HasPrincipalKey(it => it.RecId)
                .OnDelete(DeleteBehavior.NoAction);



            });

            modelBuilder.Entity<LogisticsAddressCity>(entity =>
            {
                entity.HasOne(itm => itm.CountryContext)
                    .WithMany()
                    .HasForeignKey(itm => itm.CountryRegionId)
                    .HasPrincipalKey(it => it.CountryRegionId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(itm => itm.CountyContext)
                .WithMany()
                .HasForeignKey(itm => itm.CountyId)
                .HasPrincipalKey(it => it.CountyId)
                .OnDelete(DeleteBehavior.NoAction);


                entity.HasOne(itm => itm.StateContext)
           .WithMany()
           .HasForeignKey(itm => itm.StateId)
           .HasPrincipalKey(it => it.StateId)
           .OnDelete(DeleteBehavior.NoAction);

            });

            modelBuilder.Entity<LogisticsAddressCounty>(entity =>
            {
                entity.HasOne(itm => itm.CountryContext)
                    .WithMany()
                    .HasForeignKey(itm => itm.CountryRegionId)
                    .HasPrincipalKey(it => it.CountryRegionId)
                    .OnDelete(DeleteBehavior.NoAction);


                entity.HasOne(itm => itm.StateContext)
           .WithMany()
           .HasForeignKey(itm => itm.StateId)
           .HasPrincipalKey(it => it.StateId)
           .OnDelete(DeleteBehavior.NoAction);

            });

            // ==========================================
            // 4. Missing Core & Financial Relationships
            // ==========================================

            // --- CustGroup ---
            modelBuilder.Entity<CustGroup>(entity =>
            {
                entity.HasOne(cg => cg.PaymTerm)
                    .WithMany()
                    .HasForeignKey(cg => cg.PaymTermId)
                    .HasPrincipalKey(pt => pt.PaymTermId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(cg => cg.TaxGroupHeading)
                    .WithMany()
                    .HasForeignKey(cg => cg.TaxGroupId)
                    .HasPrincipalKey(tg => tg.TaxGroup)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- Currency ---
            modelBuilder.Entity<Currency>(entity =>
            {

            });

            // --- DlvTerm ---
            modelBuilder.Entity<DlvTerm>(entity =>
            {

            });

            // --- DlvMode ---
            modelBuilder.Entity<DlvMode>(entity =>
            {

            });

            // --- PaymSched ---
            modelBuilder.Entity<PaymSched>(entity =>
            {

            });

            // --- PaymSchedLine ---
            modelBuilder.Entity<PaymSchedLine>(entity =>
            {
                entity.HasOne(itm => itm.ParentPaymentSchedule)
         .WithMany()
         .HasForeignKey(itm => itm.Name)
         .HasPrincipalKey(it => it.Name)
         .OnDelete(DeleteBehavior.NoAction);


            });

            // --- SpecTrans ---
            modelBuilder.Entity<SpecTrans>(entity =>
            {

            });

            // --- SalesPool ---
            modelBuilder.Entity<SalesPool>(entity =>
            {
            });

            // --- ContactPerson ---
            modelBuilder.Entity<ContactPerson>(entity =>
            {

            });

            // --- CustLedger ---
            modelBuilder.Entity<CustLedger>(entity =>
            {

            });


            // --- InventTable ---
            modelBuilder.Entity<InventTable>(entity =>
            {
                entity.HasOne(itm => itm.FinancialDimensionSet)
                  .WithMany()
                  .HasForeignKey(itm => itm.DefaultDimension)
                  .HasPrincipalKey(it => it.RecId)
                  .OnDelete(DeleteBehavior.NoAction);
            });
                  
            // --- InventItemGroup ---
            modelBuilder.Entity<InventItemGroup>(entity =>
            {
                entity.HasOne(iig => iig.TaxItemGroupSales)
                    .WithMany()
                    .HasForeignKey(iig => iig.TaxItemGroupIdSales)
                    .HasPrincipalKey(tig => tig.TaxItemGroup)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(iig => iig.TaxItemGroupPurch)
                    .WithMany()
                    .HasForeignKey(iig => iig.TaxItemGroupIdPurch)
                    .HasPrincipalKey(tig => tig.TaxItemGroup)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // --- UnitOfMeasure ---
            modelBuilder.Entity<UnitOfMeasure>(entity =>
            {

            });

            // --- InventClosing ---
            modelBuilder.Entity<InventClosing>(entity =>
            {

            });

            // --- InventSite ---
            modelBuilder.Entity<InventSite>(entity =>
            {

            });

            // --- InventJournalName ---
            modelBuilder.Entity<InventJournalName>(entity =>
            {

            });

            // --- GeneralJournalEntry ---
            modelBuilder.Entity<GeneralJournalEntry>(entity =>
            {

            });

            // --- FiscalCalendar ---
            modelBuilder.Entity<FiscalCalendar>(entity =>
            {

            });

            // --- FiscalCalendarYear ---
            modelBuilder.Entity<FiscalCalendarYear>(entity =>
            {

            });

            // --- FiscalCalendarPeriod ---
            modelBuilder.Entity<FiscalCalendarPeriod>(entity =>
            {

            });

            // --- InventPosting ---
            modelBuilder.Entity<InventPosting>(entity =>
            {

            });

            // --- MarkupTable ---
            modelBuilder.Entity<MarkupTable>(entity =>
            {

            });

            // --- LedgerChartOfAccounts ---
            modelBuilder.Entity<LedgerChartOfAccounts>(entity =>
            {

            });

            // --- TaxGroupHeading ---
            modelBuilder.Entity<TaxGroupHeading>(entity =>
            {

            });
            modelBuilder.Entity<PaymTerm>(entity =>
            {
                entity.HasOne(itm => itm.AssociatedPaymentSchedule)
                  .WithMany()
                  .HasForeignKey(itm => itm.PaymSched)
                  .HasPrincipalKey(it => it.Name)
                  .OnDelete(DeleteBehavior.NoAction);



            });

            // --- LogisticsLocation ---
            modelBuilder.Entity<LogisticsLocation>(entity =>
            {
                entity.HasOne(ll => ll.ParentLocationStructure)
                    .WithMany()
                    .HasForeignKey(ll => ll.ParentLocation)
                    .HasPrincipalKey(parent => parent.RecId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
            */
            return modelBuilder;
        }
    }
}
