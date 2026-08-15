# Shared fields

`src/shared/components/fields` contains compact Material UI wrappers: text, number/currency, date/date-time, boolean, select, enum, display, bilingual, generated-code, simple lookup, and grid lookup fields.

## Binding modes

Fields support controlled values where their props expose `value`/`onChange`. React Hook Form support varies by implementation:

- `AppTextField` can resolve `control` from `useFormContext` when `name` is provided.
- `AppSelectField` uses React Hook Form only when an explicit `control` and `name` are supplied; otherwise it is controlled.
- Specialized fields generally delegate to one of these wrappers.
- `AppLookupGridField` delegates to `LookupGridField`, whose public types define form and standalone variants.

Do not assume every field automatically reads `FormProvider`; inspect the field contract. Validation errors from `Controller` are rendered as helper text. `readOnly` behavior also varies by underlying input: text uses input read-only state, while select disables interaction.

Use `AppGeneratedCodeField` with number-sequence metadata, lookup fields for relational selections, and `AppBilingualField` only when the underlying model genuinely has two language values. The workflow option model now has one `Name`, so it should not use a bilingual field.
