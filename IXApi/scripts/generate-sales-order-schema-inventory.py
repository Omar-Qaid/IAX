"""Generate a read-only sales-order schema inventory from the EF model snapshot."""

from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
SNAPSHOT = ROOT / "src/Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs"
OUTPUT = ROOT / "docs/sales-order-schema-inventory.md"

GROUPS = {
    "Setup: customer and addresses": "CustTable DirPartyTable DirPartyLocation LogisticsLocation LogisticsPostalAddress",
    "Setup: item and inventory": "InventTable InventTableModule EcoResProduct InventDimCombination InventDim InventItemSalesSetup InventSite InventLocation UnitOfMeasure UnitOfMeasureConversion",
    "Setup: pricing and charges": "PriceDiscAdmTable PriceDiscAdmTrans PriceDiscTable PriceDiscGroup MarkupTable MarkupAutoTable MarkupAutoLine",
    "Setup: tax and terms": "TaxGroupHeading TaxGroupData TaxItemGroupHeading TaxOnItem TaxTable TaxData PaymTerm DlvTerm DlvMode SalesParameters",
    "Order and inventory": "SalesTable SalesLine InventTransOrigin InventTransOriginSalesLine InventTrans InventSum MarkupTrans",
    "Confirmation and documents": "SalesParmUpdate SalesParmTable SalesParmLine CustConfirmJour CustConfirmTrans CustConfirmSalesLink DocuRef DocuValue",
}

# Only fields explicitly listed in the user's pasted D365 table answer and the
# preceding three-table request. These are reference fields, not a full D365 schema.
D365_FIELDS = {
    "SalesTable": "RecId DataAreaId SalesId SalesName CustAccount InvoiceAccount SalesType SalesStatus DocumentStatus CurrencyCode Payment PaymMode TaxGroup DlvMode DlvTerm DeliveryName DeliveryPostalAddress InventSiteId InventLocationId CustomerRef PurchOrderFormNum ReceiptDateRequested ReceiptDateConfirmed ShippingDateRequested ShippingDateConfirmed EndDisc DefaultDimension",
    "SalesLine": "RecId DataAreaId SalesId LineNum ItemId Name SalesCategory SalesType SalesStatus CustAccount CurrencyCode SalesQty SalesUnit QtyOrdered SalesPrice PriceUnit LineDisc LinePercent MultiLnDisc MultiLnPercent LineAmount InventDimId InventTransId RemainSalesPhysical RemainInventPhysical RemainSalesFinancial RemainInventFinancial TaxGroup TaxItemGroup DeliveryName DeliveryPostalAddress ReceiptDateRequested ReceiptDateConfirmed ShippingDateRequested ShippingDateConfirmed DefaultDimension",
    "InventDim": "RecId DataAreaId InventDimId InventSiteId InventLocationId ConfigId InventSizeId InventColorId InventStyleId InventBatchId InventSerialId",
    "InventTransOrigin": "RecId DataAreaId InventTransId ItemId ReferenceCategory ReferenceId",
    "InventTransOriginSalesLine": "RecId InventTransOrigin SalesLineDataAreaId SalesLineInventTransId",
    "InventTrans": "RecId DataAreaId InventTransOrigin ItemId InventDimId Qty StatusIssue StatusReceipt DateExpected DatePhysical DateFinancial DateStatus CurrencyCode CostAmountPhysical CostAmountPosted CostAmountAdjustment VoucherPhysical Voucher PackingSlipId InvoiceId",
    "InventSum": "RecId DataAreaId ItemId InventDimId PostedQty Received Deducted Registered Picked ReservPhysical ReservOrdered OnOrder Ordered Arrived PhysicalInvent AvailPhysical AvailOrdered Closed",
    "MarkupTrans": "RecId DataAreaId TransTableId TransRecId LineNum MarkupCode MarkupCategory Value CurrencyCode Txt TaxGroup TaxItemGroup Keep",
    "MarkupAutoTable": "RecId AccountCode AccountRelation DlvModeCode DlvModeRelation ItemCode ItemRelation MarkupReturn ModuleCategory ModuleType ReturnRelation RetailConcessionFeeLegacy RetailConcessionFee SHA256Hash RetailAdvancedChargesDeliveryProrate RetailChannelCode RetailChannelRelation Description DataAreaId",
    "MarkupAutoLine": "RecId CurrencyCode CustomsAssessableValue_IN FromAmount Keep LineNum MarkupCategory MarkupCode MarkupCurrencyCode MCRReturnMarkup ModuleCategory ModuleType NotionalCharges_IN NotionalPct_IN TableRecId TableTableId TaxGroup TaxItemGroup ToAmount Txt Value InventSiteId InventLocationId DataAreaId",
}
KNOWN_ALIASES = {"SalesTable": {"Payment": "PaymTerm", "TaxGroup": "TaxGroupId"}}

D365_DEFAULT_TYPES = {
    "SalesTable": "str", "SalesLine": "str", "InventDim": "str",
    "InventTransOrigin": "str", "InventTransOriginSalesLine": "str",
    "InventTrans": "str", "InventSum": "str", "MarkupTrans": "str",
    "MarkupAutoTable": "string", "MarkupAutoLine": "string",
}
D365_TYPE_OVERRIDES = {
    "SalesTable": {
        "int64": "RecId DeliveryPostalAddress DefaultDimension",
        "enum": "SalesType SalesStatus DocumentStatus",
        "date": "ReceiptDateRequested ReceiptDateConfirmed ShippingDateRequested ShippingDateConfirmed",
        "real": "EndDisc",
    },
    "SalesLine": {
        "int64": "RecId SalesCategory DeliveryPostalAddress DefaultDimension",
        "enum": "SalesType SalesStatus",
        "date": "ReceiptDateRequested ReceiptDateConfirmed ShippingDateRequested ShippingDateConfirmed",
        "real": "LineNum SalesQty QtyOrdered SalesPrice PriceUnit LineDisc LinePercent MultiLnDisc MultiLnPercent LineAmount RemainSalesPhysical RemainInventPhysical RemainSalesFinancial RemainInventFinancial",
    },
    "InventDim": {"int64": "RecId"},
    "InventTransOrigin": {"int64": "RecId", "enum": "ReferenceCategory"},
    "InventTransOriginSalesLine": {"int64": "RecId InventTransOrigin"},
    "InventTrans": {
        "int64": "RecId InventTransOrigin", "enum": "StatusIssue StatusReceipt",
        "date": "DateExpected DatePhysical DateFinancial DateStatus",
        "real": "Qty CostAmountPhysical CostAmountPosted CostAmountAdjustment",
    },
    "InventSum": {
        "int64": "RecId", "enum": "Closed",
        "real": "PostedQty Received Deducted Registered Picked ReservPhysical ReservOrdered OnOrder Ordered Arrived PhysicalInvent AvailPhysical AvailOrdered",
    },
    "MarkupTrans": {"int64": "RecId TransRecId", "int": "TransTableId", "enum": "MarkupCategory Keep", "real": "LineNum Value"},
    "MarkupAutoTable": {
        "int64": "RecId",
        "int32": "AccountCode DlvModeCode ItemCode MarkupReturn ModuleCategory ModuleType RetailConcessionFeeLegacy RetailConcessionFee RetailAdvancedChargesDeliveryProrate RetailChannelCode",
    },
    "MarkupAutoLine": {
        "int64": "RecId TableRecId",
        "int32": "CustomsAssessableValue_IN Keep MarkupCategory MCRReturnMarkup ModuleCategory ModuleType NotionalCharges_IN TableTableId",
        "decimal": "FromAmount LineNum NotionalPct_IN ToAmount Value",
    },
}

def d365_type(table, field):
    if field not in D365_FIELDS.get(table, "").split():
        return "—"
    for field_type, names in D365_TYPE_OVERRIDES.get(table, {}).items():
        if field in names.split():
            return field_type
    return D365_DEFAULT_TYPES[table]

source = SNAPSHOT.read_text(encoding="utf-8-sig")
models = {}
for match in re.finditer(r'modelBuilder\.Entity\("([^"]+)", b =>\s*\{(.*?)\n\s*\}\);', source, re.S):
    block = match.group(2)
    table = re.search(r'b\.ToTable\("([^"]+)"', block)
    if not table:
        continue
    columns = []
    for prop in re.finditer(r'(?m)^[ \t]*b\.Property<([^>]+)>\("([^"]+)"\)(.*?);', block, re.S):
        clr, name, settings = prop.groups()
        sql = re.search(r'\.HasColumnType\("([^"]+)"\)', settings)
        db_name = re.search(r'\.HasColumnName\("([^"]+)"\)', settings)
        columns.append((name, db_name.group(1) if db_name else name, clr, sql.group(1) if sql else "(provider inferred)"))
    if columns:
        models[table.group(1)] = (match.group(1), columns)

lines = [
    "# Sales order cycle schema inventory",
    "",
    "Source: `ApplicationDbContextModelSnapshot.cs` in this checkout, including the uncommitted three-table migration from the preceding task. This is the IXApi EF model, not a live database inspection.",
    "D365 reference fields and types come only from the user's pasted material and are illustrative, not a complete or version-specific D365 schema. The attachment uses X++ base types for eight tables and published CDM data formats for MarkupAutoTable/MarkupAutoLine; these are labeled as supplied, not converted into SQL precision. `RecId` maps to IXApi's `RECID` SQL column.",
    "SQL types shown below are the model's configured column types. Custom fields, migrations not applied to a database, and D365 extensions require live metadata verification.",
    "Official published references for selected tables: [SalesLine](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/salesandmarketing/worksheetline/salesline), [MarkupAutoTable](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/procurementandsourcing/group/markupautotable), [MarkupAutoLine](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/procurementandsourcing/group/markupautoline), [InventTransOriginSalesLine](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/inventory/transaction/inventtransoriginsalesline).",
    "",
    "| Group | Tables in cycle | IXApi model | Missing in IXApi model |",
    "| --- | ---: | ---: | ---: |",
]
for group, names in GROUPS.items():
    tables = names.split()
    present = sum(name in models for name in tables)
    lines.append(f"| {group} | {len(tables)} | {present} | {len(tables) - present} |")
lines.append("")

for group, names in GROUPS.items():
    lines.extend([f"## {group}", ""])
    for name in names.split():
        if name not in models:
            class_files = list((ROOT / "src").rglob(f"{name}.cs"))
            class_note = " Class file exists: " + ", ".join(f"`{path.relative_to(ROOT).as_posix()}`" for path in class_files) + "." if class_files else " No matching class file found."
            lines.extend([f"### {name}", "", "**No IXApi EF table mapping found in the current snapshot.**" + class_note, ""])
            continue
        entity, columns = models[name]
        reference = set(D365_FIELDS.get(name, "").split())
        model_names = {column[0] for column in columns}
        lines.extend([f"### {name}", "", f"IXApi entity: `{entity}`. {len(columns)} mapped columns.", ""])
        if reference:
            aliases = KNOWN_ALIASES.get(name, {})
            missing = sorted(reference - model_names - set(aliases))
            lines.append("D365 fields listed in the attachment but absent in IXApi: " + (", ".join(f"`{item}`" for item in missing) if missing else "none") + ".")
            if aliases:
                lines.append("\nNaming differences: " + ", ".join(f"D365 `{old}` → IXApi `{new}`" for old, new in aliases.items()) + ".")
            lines.append("")
        lines.extend(["| IXApi property | SQL column | CLR type | SQL type | D365 type in pasted material |", "| --- | --- | --- | --- | --- |"])
        for prop, column, clr, sql in columns:
            d365_name = prop if prop in reference else next((old for old, new in KNOWN_ALIASES.get(name, {}).items() if new == prop), None)
            supplied_type = d365_type(name, d365_name) if d365_name else "—"
            supplied = f"`{supplied_type}`" + (f" (`{d365_name}`)" if d365_name != prop else "") if supplied_type != "—" else "—"
            lines.append(f"| `{prop}` | `{column}` | `{clr}` | `{sql}` | {supplied} |")
        lines.append("")

OUTPUT.parent.mkdir(parents=True, exist_ok=True)
OUTPUT.write_text("\n".join(lines), encoding="utf-8")
print(f"Wrote {OUTPUT}: {len(models)} mapped EF tables in snapshot; {sum(len(x.split()) for x in GROUPS.values())} cycle table references")
