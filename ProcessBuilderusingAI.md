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

### Important Rule

Do not create or assume database structures before analyzing the existing implementation.

The first stage is to **fully reverse-engineer and understand the current Process Builder**.

Once this architecture is understood, it should be used as the foundation for converting future chats, images, PDFs, and forms into complete processes that can be created in the system.
