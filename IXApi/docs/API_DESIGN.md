# API Design & Communication

This manual outlines the API endpoint structures, response formats, validation mechanisms, and real-time communication details.

---

## 1. Controller Architecture & Base Elements

To ensure uniformity across all endpoints, the system utilizes a generic base controller:

```csharp
public abstract class BaseController<TEntity, TDto> : ControllerBase
    where TEntity : class
    where TDto : class
```

### Supported Base Operations
*   `GET api/v1/[controller]`: Lists paged, filtered, and sorted records.
*   `GET api/v1/[controller]/{id}`: Retrieves a single record by primary key.
*   `POST api/v1/[controller]`: Creates a new record.
*   `PUT api/v1/[controller]/{id}`: Updates an existing record.
*   `DELETE api/v1/[controller]/{id}`: Soft-deletes a record.

---

## 2. API Response & Exception Structures

### Successful Payload Envelope (`APIResponse<T>`)
Every successful API action wraps the returned resource in a consistent standard container:

```json
{
  "success": true,
  "message": "Resource retrieved successfully",
  "data": {
    "id": 123,
    "name": "General Ledger Account"
  },
  "pagination": {
    "currentPage": 1,
    "pageSize": 20,
    "totalCount": 100,
    "totalPages": 5
  }
}
```

### Error Payload Envelope (RFC 7807 Problem Details)
In compliance with RFC 7807, unhandled exceptions and validation errors return a `problem+json` payload handled globally:

#### Standard Validation Failure (HTTP 400)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "traceId": "00-5cfa28d904c0a1a0db6192db87d3a0c5-e2fba3d1c920f01a-00",
  "errors": {
    "Email": [
      "The Email field is not a valid e-mail address."
    ]
  }
}
```

#### Resource Not Found (HTTP 404)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "The requested resource key 'org-entity-99' was not found.",
  "traceId": "00-5cfa28d904c0a1a0db6192db87d3a0c5-e2fba3d1c920f01a-00"
}
```

---

## 3. Data Transfer & Mapping (Mapster)

Entities and DTOs are mapped automatically using **Mapster** configs.
*   **Inbound DTO Mapping**: Ignores identity-generated values, auditing columns, or foreign keys that should only be set by the server.
*   **Outbound DTO Mapping**: Includes navigation properties and lookup lists for rendering in client dashboards.

---

## 4. Real-time Communication (SignalR)

The system supports real-time communication channels using SignalR Hubs located in the `Infrastructure/Realtime/` folder:

*   **`SysNotificationHub`**: Dispatches instant notifications, workflow assignment alerts, and system broadcast messages to active users.
*   **`SysChatHub`**: Handles real-time messaging, chatroom history, and peer-to-peer communication within the Organization.
