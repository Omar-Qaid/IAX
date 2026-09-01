# Request From — Current Displayed Controls

## Screen context

- Route: `/workflow/request-from/6/1`
- Page direction shown: RTL (Arabic)
- Selected process: `PAYMENT_REQUEST`
- Displayed process name: `طلب الصرف`
- Purpose: capture the current request-entry screen and the controls visible in the details area.

## Display modes

The page supports two explicitly named display modes:

### Normal Display

- Default mode.
- Uses the current standard responsive dynamic-form layout documented below.
- Controls remain editable and the `Submit Request` action is available.
- Switching temporarily to the other display mode does not unmount the form, so the current in-memory entries are retained when returning to Normal Display.

### Print Template Display

- Uses the process's configured published print-template document and its page styling.
- Selects the default active published template; if no published template is marked as default, it uses the first published template returned for the process.
- Always loads the immutable published version. An unpublished draft is never displayed on the request-entry page.
- Request controls explicitly bound to template elements are rendered as editable controls at those exact positions, and the request can be submitted from this mode.
- Before a request exists, request-bound template elements use the current shared form values while system data such as the request date and company branding is rendered from the page context.
- If the process has no published template, the page displays an informational message instead of silently falling back to a draft.
- Loading and failure states are displayed inside the details pane.

### Control Order in Print Template Display

The concise technical rule is:

> **Normal Display uses `RequestControls` ordering; Print Template Display uses `PrintTemplate` ordering and positioning as the source of truth.**

- Normal Display continues to sort controls by `RequestControl.SortOrder`, then by request-control ID as a stable fallback.
- Print Template Display traverses the published template's header, sections, nested rows/columns, and footer in document order.
- A request control is rendered only at an explicit `requestControl` binding on a template element.
- A field binding renders the control inside that field's configured template position.
- A table binding renders the editable table at that table element's configured position.
- If the same request control is explicitly bound more than once, every configured occurrence is rendered and shares the same underlying value.
- `RequestControl.SortOrder` is never used to rearrange template-bound controls in Print Template Display.
- Active visible controls absent from the published template are appended after the complete template-body layout. They are never inserted between template-bound controls.
- A control referenced only by a visibility condition affects conditional rendering but is not treated as a configured input position; it needs its own bound template element to be editable in this mode.
- Switching modes reuses the same mounted form and the same values, errors, queued control files, score, and saved-request state.
- Visibility and validation continue to use the workflow form definition. A bound control that is not currently visible is not rendered until its workflow visibility rule is satisfied.
- The appended fallback controls retain `RequestControl.SortOrder` and the normal responsive one/two/three-column behavior.
- A fallback control disappears from the appended area as soon as that same request-control ID is explicitly mapped into the published template body.
- Both the body layout and appended-controls area use a white, full-width page background.

### Request-control layout across template areas

Print Template Display follows this composition rule:

> **Print Template Display = Template Layout + RequestControls Binding**

- The template is traversed in header → body (`sections`) → footer order.
- A request-control binding in the header is retained and rendered; it is not skipped merely because it is in the header.
- A request-control binding in the footer is retained and rendered; it is not skipped merely because it is in the footer.
- A field, table, signature, image, QR code, barcode, or attachment element is retained when its binding has `sourceType = requestControl`.
- Company, system, report, workflow, attachment-only, and repeating print-data bindings are not rendered as request inputs.
- Static text, print dates, page numbers, dividers, spacers, page breaks, workflow approvals, company fields, system fields, and other unrelated print-only elements are removed from this request-entry mode.
- Section, row, and column elements are retained only when they contain at least one retained request-control element. Empty structural containers are removed.
- Retained structural containers preserve their nesting, document order, section column count, row flow, column `span` ratios, and configured element width styles.
- The retained request-control layout is rendered into the full available request-page width. It does not use `PrintoutDocument`, A4/Letter dimensions, print margins, print header/footer slots, or a centered paper canvas.
- Template field labels are not wrapped around the editable control in request-body mode; the actual dynamic request control supplies its localized label, required marker, options, validation, and input behavior.
- Table elements render the actual editable request table at the template table position while retaining the request control's configured columns and submitted JSON contract.

## Page header and actions

The compact record header displays the process name, process code, and current request date. The action bar contains:

- `تقديم الطلب` — submits the request. It is disabled while saving and after a successful submission.
- Score indicator (`الدرجة`) — starts at zero and is recalculated from populated controls and selected options.
- Attachment button — queues one or more files before submission. Its badge displays the queued-file count. After submission it changes to the saved request's attachment action.
- Search, refresh, options, navigation, and standard list/details page actions.

The process list is initially hidden, leaving the full content width for the request form.

## Details-area layout

The form is displayed inside a bordered white details card. On large screens it uses a responsive three-column grid; on medium screens it uses two columns, and on small screens one column. Controls are ordered by `SortOrder`. In RTL mode the visual flow begins at the right side.

The current desktop arrangement is:

| Row | Right | Center | Left |
| --- | --- | --- | --- |
| 1 | نوع طلب الصرف | التاريخ | الموقع |
| 2 | القسم الإداري | بيانات الصرف | الإجمالي |

## Current controls

### 1. Payment request type

| Property | Current value |
| --- | --- |
| Code | `PAYMENT_REQUEST_TYPE` |
| Arabic label (`NameAlias`) | نوع طلب الصرف |
| English label (`Name`) | Payment request type |
| Control type | Radio-button list |
| Sort order | 1 |
| Required | Yes |
| Description | العهدة النقدية أو الاتفاقيات والعقود والموردين |

Available options:

| Stored value | English name | Arabic name |
| --- | --- | --- |
| `CashAdvance` | Cash advance | العهدة النقدية |
| `ContractsVendors` | Agreements, contracts and vendors | الاتفاقيات والعقود والموردين |

Only one option can be selected.

### 2. Request date

| Property | Current value |
| --- | --- |
| Code | `PAYMENT_REQUEST_DATE` |
| Arabic label (`NameAlias`) | التاريخ |
| English label (`Name`) | Request date |
| Control type | Calendar/date input |
| Sort order | 2 |
| Required | Yes |
| Description | تاريخ طلب الصرف |

The current empty state shows the browser date placeholder and a calendar-picker icon.

### 3. Location

| Property | Current value |
| --- | --- |
| Code | `PAYMENT_SITE` |
| Arabic label (`NameAlias`) | الموقع |
| English label (`Name`) | Location |
| Control type | Location selector |
| Sort order | 3 |
| Required | Yes |
| Description | موقع أو إدارة الشركة مقدمة الطلب |

The empty state displays `لم يتم تحديد موقع` and an `اختيار` action. The selected location is stored as the location control's structured value.

### 4. Administrative department

| Property | Current value |
| --- | --- |
| Code | `PAYMENT_DEPARTMENT` |
| Arabic label (`NameAlias`) | القسم الإداري |
| English label (`Name`) | Administrative department |
| Control type | Text box |
| Sort order | 4 |
| Required | Yes |
| Description | القسم الإداري مقدم الطلب |

The current empty state shows the placeholder `القسم الإداري`.

### 5. Payment details

| Property | Current value |
| --- | --- |
| Code | `PAYMENT_DETAILS` |
| Arabic label (`NameAlias`) | بيانات الصرف |
| English label (`Name`) | Payment details |
| Control type | Editable table |
| Sort order | 5 |
| Required | Yes |
| Description | تفاصيل الفواتير والتحويل أو الصرف |

The table initially displays an empty-data row (`بيانات الجدول`) and an `إضافة` action for inserting detail rows. When the available width is insufficient, the table provides horizontal scrolling.

Detail columns, in configured order:

| Order | Stored field | English heading | Arabic heading | Intended content |
| ---: | --- | --- | --- | --- |
| 1 | `sequence` | No. | م | Row sequence number |
| 2 | `beneficiary` | Beneficiary name | اسم المستفيد | Person or organization receiving payment |
| 3 | `invoice_number` | Invoice number | رقم الفاتورة | Related invoice/reference number |
| 4 | `invoice_amount` | Invoice amount | قيمة الفاتورة | Invoice amount before or alongside VAT |
| 5 | `vat` | VAT | الضريبة | VAT amount |
| 6 | `total` | Total | الإجمالي | Total amount for the detail row |
| 7 | `payment_statement` | Transfer / payment statement | بيان التحويل / الصرف | Transfer or payment explanation |

Each added row is serialized as part of the table control value when the request is submitted.

### 6. Grand total

| Property | Current value |
| --- | --- |
| Code | `PAYMENT_GRAND_TOTAL` |
| Arabic label (`NameAlias`) | الإجمالي |
| English label (`Name`) | Grand total |
| Control type | Text box (using the workflow `number` control definition) |
| Sort order | 6 |
| Required | Yes |
| Description | إجمالي مبلغ طلب الصرف |

The control holds the overall payment-request amount. In the current screen it is a separate input and is not visibly auto-calculated from the payment-details rows.

## Validation and submission behavior

- All six current controls are configured as required.
- Validation runs when the user selects `تقديم الطلب`.
- Arabic validation uses `ErrorMessageAlias`; for example: `حقل التاريخ مطلوب.`
- An invalid control displays its error directly below the control.
- Only visible, non-label controls are included in the submitted values.
- The submit button is available while the form contains input controls and no request has yet been saved.
- After successful submission, the request ID is retained, queued attachments are uploaded, and the form switches to its saved/preview state.

## Localization behavior

- Arabic/RTL displays `NameAlias` and option aliases when present.
- English/LTR displays `Name` and English option names.
- Search matches the process code, `Name`, `NameAlias`, and description.

## Current visual observations

- The payment-details grid is narrower than the complete seven-column definition, so horizontal scrolling is required to reach every column.
- The form occupies only the upper part of the available details pane because there are currently six controls; the remaining pane stays empty.
- The desktop details pane itself scrolls vertically when future controls or expanded option-dependent content exceed the available viewport height.

## Exact current value contract

The request form submits one string value for every visible, non-label control. The current payment request uses these formats:

| Control code | Submitted string format | Example |
| --- | --- | --- |
| `PAYMENT_REQUEST_TYPE` | Selected option's stable `Value` | `CashAdvance` |
| `PAYMENT_REQUEST_DATE` | Browser date value in `YYYY-MM-DD` format | `2026-09-01` |
| `PAYMENT_SITE` | Location control's serialized structured value | Implementation-generated location value |
| `PAYMENT_DEPARTMENT` | Plain text | `Finance` |
| `PAYMENT_DETAILS` | JSON array of row objects | `[{"sequence":"1","beneficiary":"Vendor A"}]` |
| `PAYMENT_GRAND_TOTAL` | Plain text representing the entered amount | `1250.00` |

The table keys are derived from the option `Value`, normalized to lowercase underscore-separated keys. The current option values are already print-safe field names (`sequence`, `beneficiary`, `invoice_number`, `invoice_amount`, `vat`, `total`, and `payment_statement`). These values must remain stable after requests have been saved.

## Current print-template mapping

The active payment print template is:

| Property | Value |
| --- | --- |
| Code | `PAYMENT_REQUEST_AR` |
| English name | Payment Request Form |
| Arabic name | نموذج طلب الصرف |
| Language/direction | Arabic / RTL |
| Page | A4 portrait |
| Missing-field behavior | Render an empty value |

Request controls are bound by both `RequestControlId` and `ControlId` with `SourceType = requestControl`. The database record ID is therefore part of the print contract; a seeded control should be reconciled in place rather than deleted and recreated.

Current form-to-print coverage:

| Form control | Current print output |
| --- | --- |
| `PAYMENT_REQUEST_TYPE` | Two conditional header lines. `CashAdvance` checks العهدة النقدية; `ContractsVendors` checks الاتفاقيات والعقود والموردين. |
| `PAYMENT_REQUEST_DATE` | Request field labeled التاريخ, formatted as a date. |
| `PAYMENT_SITE` | Request field labeled الموقع / إدارة الشركة. |
| `PAYMENT_DEPARTMENT` | Request field labeled القسم الإداري. |
| `PAYMENT_DETAILS` | Repeating seven-column print table using the exact JSON row keys documented above. |
| `PAYMENT_GRAND_TOTAL` | Currency field labeled الإجمالي, formatted as SAR with two decimals and grouping. |

The print template also includes system-sourced values that are not editable request controls: company logo, request number, submitter name, approval/signature areas, and page number.

## Adding another option that reveals follow-up controls

An option can display additional controls through its feature configuration. The relevant configuration is:

```json
{
  "showOtherControls": true,
  "visibleControlIds": [123, 124],
  "requireFileUpload": false,
  "sendAlertMessage": false,
  "alertMessage": "",
  "performerIds": []
}
```

The IDs in `visibleControlIds` are request-control record IDs, not master `ControlId` values or control codes.

Current display behavior for option-controlled fields:

1. A child control is excluded from the top-level form grid.
2. It becomes visible only while the owning option is selected and its parent control is visible.
3. One small child control is rendered directly below the selected option in a lightly tinted block with a blue inline border.
4. Two or more children, or any table/long-text/textarea child, use a separate full-width option-details block below the parent row.
5. A full-width child block uses the same responsive one/two/three-column layout as the main form.
6. Nested option dependencies are supported. Cycles are guarded during rendering and must not be configured intentionally.
7. A revealed required control participates in validation and submission only while visible.
8. Deselecting the option hides its children. The UI retains their in-memory values during the current editing session, but hidden controls are excluded from the submitted values.

An option label displays small status icons when it sends a notification, requires an attachment, or reveals other controls.

### Required implementation data for each new option

Document and seed all of the following:

- Stable option `Value`; use an English code-like value and never use the translated label as the contract.
- English option `Name` and Arabic `NameAlias`.
- Sort order and score.
- The child request-control IDs in `visibleControlIds`.
- English and Arabic names for every child control.
- Control type, sort order, column span, required state, default value, descriptions, options, and validations.
- Any attachment, alert, or performer behavior.
- The exact submitted value format expected by the print runtime.

## Keeping a new option synchronized with the print template

Adding an option to the request form does not automatically add its label or follow-up controls to the print document. The print template must be updated in the same change.

For every new parent option:

1. Add the option to the seeded request-control options with a stable `Value`, `Name`, and `NameAlias`.
2. Add or reconcile its follow-up request controls; do not replace existing records if their IDs are already referenced by print bindings.
3. Configure `showOtherControls` and `visibleControlIds` for the option.
4. Add a print visibility condition against the parent request control and the exact stored option value.
5. Add print fields/tables for every child control that should appear when that option is selected.
6. Use the same condition on the option heading and its associated print section so unrelated empty sections do not appear.
7. For a child table, use option `Value` strings as JSON field keys and repeat those exact keys in the print table's `Field` definitions.
8. Reconcile and publish a new print-template version after changing the template document.
9. Verify both language labels in the form and the Arabic labels in the current Arabic print template.

Example print condition for a third option whose stored value is `Other`:

```csharp
VisibleWhen = RequestValueCondition(
    controls["PAYMENT_REQUEST_TYPE"],
    "Other")
```

The same stable value must appear in the request option seed, the saved request detail, and the print condition.

## Acceptance checklist for the future option

- The new option appears in the correct order in Arabic and English.
- Selecting it reveals only its configured follow-up controls.
- Deselecting it removes those controls from validation and submission.
- Required child controls block submission and show localized messages.
- Mobile, medium, and desktop layouts remain one, two, and three columns respectively.
- Table children scroll horizontally without widening the details pane.
- Saved values use stable codes/keys rather than translated labels.
- The printed option state matches the selected radio option.
- All visible child values appear in the correct conditional print section.
- Other options do not display the new section in print.
- Existing `CashAdvance` and `ContractsVendors` form and print output remain unchanged.
- A new published print-template version is active after seeding.
