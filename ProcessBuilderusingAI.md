## Process Builder – Analysis and Understanding Requirements

I need you to perform a complete analysis of the existing **Process Builder** and understand exactly how processes and forms are created and stored in the system.

### 1. Analyze the Existing Process Builder

Study the current database structure and application logic to understand:

- How a new process is created.
- How forms are created and linked to a process.
- How sections, controls, fields, and options are configured.
- How validations and required fields are defined.
- How workflow steps and approvals are configured.
- How processes, forms, and controls are related to each other.
- How the system stores submitted request data.

### 2. Analyze Database Tables

Identify all tables involved in the Process Builder.

For each table, explain:

- Table name.
- Purpose of the table.
- What type of data it stores.
- Primary key.
- Important fields.
- Foreign keys and relationships.
- Which other tables depend on it.
- At what stage of process creation the table is used.
- Whether it stores configuration/master data or transaction/runtime data.
- What impact adding, updating, or deleting records from the table may have.

I need to clearly understand the complete relationship, for example:

**Process → Forms → Sections → Controls → Options → Validations → Workflow → Requests → Request Data**

The actual structure must be determined from the existing system rather than assumed.

### 3. Understand the Process Creation Flow

Document the complete technical flow required to create a process.

For example:

1. Create the process.
2. Create/configure its forms.
3. Create sections.
4. Add controls/fields.
5. Configure control properties.
6. Add options for select/radio controls.
7. Add validations.
8. Configure workflow/approval steps.
9. Activate or publish the process.

For every step, identify exactly which tables are inserted or updated and how their IDs are linked.

### 4. Build a Process Creation Map

After the analysis, create a reusable technical map showing:

**Input Requirement**\
→ **Process**\
→ **Forms**\
→ **Sections**\
→ **Controls**\
→ **Options / Validations**\
→ **Workflow Configuration**\
→ **Published Process**

This map will become the reference for automatically building future processes.

### 5. Prepare for AI-Based Process Generation

The final objective is to allow me to provide a business requirement in different formats, such as:

- Chat conversation.
- Written description.
- Screenshot/image.
- PDF.
- Existing paper or digital form.
- Attached document containing the required fields and workflow.

You should analyze the provided content and determine automatically:

- Process name and purpose.
- Required forms.
- Form sections.
- Required fields/controls.
- Appropriate control types.
- Required/optional fields.
- Dropdown/radio/select options.
- Validations.
- Default values.
- Field ordering and layout.
- Workflow/approval requirements.

Then generate the required configuration based on the **actual existing Process Builder architecture**.

### 6. Expected Output

For each generated process, provide:

- Process structure.
- Form structure.
- Fields and control types.
- Validation rules.
- Workflow structure.
- Table impact.
- Required INSERT/UPDATE operations.
- Correct relationships and generated IDs.
- SQL scripts or API payloads required to create the process.

The generated configuration must follow the existing database architecture, naming conventions, relationships, and Process Builder rules.

### 7. Mail Printout Behavior

The workflow Mail page has two intentionally separate print modes. Do not combine them:

#### Published Template Printouts

- Every named print option is loaded from the active published templates configured for the selected process.
- The layout, labels, bindings, page settings, header, footer, tables, and formatting come from the published `WfPrintTemplates` / `WfPrintTemplateVersions` document.
- Selecting a named template opens the official template viewer and renders that exact published template.
- Template request-field bindings must use the stable `WfRequestControl` ID. A stable request-control value must take precedence over any legacy control-type ID when numeric IDs collide.

#### Full Transaction Details

- This is the final print option in the Mail page menu.
- It is a generic printout and must never load or render a published/default print template.
- It must not capture or print the current page DOM/layout.
- It must build a clean printable document from the selected request and the Mail details data.
- It must include the request summary and every transaction/request field displayed in the Mail page, using the same values and localized labels.
- Dynamic fields must be ordered by their configured control order and use the appropriate renderer for text, dates, numbers, locations, tables, files, signatures, and other supported control types.
- RTL uses the Arabic label/alias when available; LTR uses the English name/label. RTL falls back to English when the Arabic value is empty.
- The presence of a default published template must not change this option's behavior.

The required command mapping is:

**Named template option** → **Published template ID** → **Official template viewer** → **Render `WfPrintTemplateVersion.TemplateJson`**

**Full Transaction Details** → **Selected workflow request** → **Mail-details data** → **Generic workflow mail printout** → **No template lookup or template rendering**

### Important Rule

Do not create or assume database structures before analyzing the existing implementation.

The first stage is to **fully reverse-engineer and understand the current Process Builder**.

Once this architecture is understood, it should be used as the foundation for converting future chats, images, PDFs, and forms into complete processes that can be created in the system.
