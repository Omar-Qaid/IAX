# Localized record names

Display a record's `nameAlias` in RTL and `name` in LTR. Empty aliases fall back to `name`. Keep both fetched values intact: localization belongs in display selectors, not API response mutation. Create/edit fields bind separately to the original properties.

## Module coverage

| Area | Contract and display behavior |
| --- | --- |
| Organization / HCM | Roles, units, workers, positions and hierarchy records use `nameAlias`; their editors keep both name fields. Hierarchy lookup appears above tree search. |
| Legal entities | The existing contract calls the Arabic field `arabicName`; lists and read-only headers use it in RTL. Both inputs remain available. |
| Workflow | Master lists, record headers and shared lookups select localized names. Setup grids retain separate editable name and alias columns. Request process/category selection and report templates already support aliases. |
| Process Builder | Process, step, activity and variable aliases survive load/save and have separate settings inputs. Trees, diagrams, rule labels and selections use localized names. Variable identifiers, operator resolution and expressions keep their original values. Controls retain their existing separate labels. |
| Finance customers | The response calls the Arabic name `nameAr`; customer lists, record headers and sales-order customer selections display it in RTL. Customer writes continue mapping it to `nameAlias`. |
| Shared lookups | Dropdown, dialog and grid lookups select localized names and preserve the original selected record. Grid fields default to `nameAlias`; explicitly configured legacy label fields remain supported. |

## Contract limits

Not every displayed string is a bilingual record name. Currency `txt`, exchange-rate type descriptions, posting profiles, number-sequence descriptions, background-job/task descriptions, document/file names, and workflow performer contracts do not expose a corresponding `nameAlias` through their current IXApp APIs. This change does not create unsavable alias inputs for those contracts. Customer groups currently use local sample data rather than persisted bilingual records. Dashboard, authentication and code-only views have no bilingual record-name editor.

Supporting independently editable aliases for these remaining records requires a matching API/storage contract. Static interface labels continue using translation keys; identifiers and user-entered content are not translated automatically.

## Verification

Regression coverage checks Arabic/English lookup display, empty-alias fallback, preservation of the selected bilingual record, organization-role grid display, separate list-details editing, and Process Builder alias load/save/clear behavior. Automated checks do not establish authenticated live API/database acceptance.
