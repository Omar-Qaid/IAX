# Number-sequence standard

`SysNumberSequences` is the sole runtime source of behavior. Feature pages declare only a sequence key and code field. A preview is informational and never consumes a value; the create transaction allocates and persists the authoritative value.

## Field semantics

| Field | Standard behavior |
|---|---|
| `NumberSequence` | Stable key; by convention it matches the entity/controller name. |
| `Lowest`, `Highest`, `NextRec` | Inclusive allocation range and next candidate. Invalid/out-of-range configurations fail closed. |
| `Blocked` | `1` prevents preview and create. |
| `InUse`, `IsActive`, `IsDeleted` | `0`, `false`, or `true` respectively makes a sequence unavailable. |
| `Manual` | `1` means manual-only: Code is editable and required; no value is consumed. Other values mean automatic. |
| `Format` | Supplies prefix and `#` padding, e.g. `PROC-######`. |
| `AnnotatedFormat` | Token pattern supporting `{PREFIX}`, `{SEQ}`, `{YYYY}`, `{YY}`, `{MM}`, `{DD}`. |
| `Continuous` | Allocation and entity insertion share the create transaction. This prevents failure gaps; deletion gaps remain possible. |
| `Cyclic` | Enables reset policy. Current legacy daily behavior remains pending full `CleanInterval`/timezone implementation. |
| `CleanAtAccess`, `CleanInterval`, `LatestCleanDateTime`, `LatestCleanDateTimeTzId` | Reset-policy inputs. Unsupported combinations must be rejected before expanding cyclic behavior. |
| `NoIncrement` | Automatic unique allocation is rejected to prevent duplicate codes. |
| `NumberSequenceScope` | Reserved scope identifier. Current resolution prefers current `DataAreaId`, then global configuration. |
| `AllowChangeUp`, `AllowChangeDown` | Gate administrative changes to `NextRec`. |
| `FetchAhead`, `FetchAheadQty` | Fetch-ahead is not enabled; invalid non-positive quantities fail validation. |
| `DataAreaId`, `Partition` | Company/partition ownership. Entity code uniqueness must use the applicable scope. |

## Generic lifecycle

1. `GET /v1/{sequenceKey}/number-sequence` runs through the feature controller's existing permission.
2. New-record UI reads database metadata. Automatic fields are read-only; manual fields are editable and required.
3. Automatic previews are removed from the POST payload.
4. The base controller opens a transaction, applies the centralized policy, persists the entity, and commits both operations together.
5. The response returns the persisted code and the shared frontend refreshes metadata for the next New action.

## Known follow-ups

- Add reviewed scoped unique indexes after the production duplicate audit.
- Complete timezone-aware cyclic policies before enabling non-daily cleanup configurations.
- Remove workflow service-level `EnsureCodeAsync` hooks after all non-controller import/create paths use the centralized runtime.
- Fetch-ahead remains intentionally disabled until reservation recovery and concurrency tests exist.

