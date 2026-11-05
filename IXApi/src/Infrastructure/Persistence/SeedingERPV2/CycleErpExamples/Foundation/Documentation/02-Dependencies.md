# Dependencies

Use SeedDependencyResolver, not a second ordering list. AddFoundationPhaseOneSeeding composes the existing GL, Tax, Inventory, Cash/Bank and AP fixtures with Foundation extensions. Currency/calendar/company precede ledger setup; ledger combinations precede posting groups; workers follow parties and lookups; assignments follow units, roles and positions. Missing dependencies and cycles fail before writes.
