# Full regression — first result

Follow-up completed: InventoryCycle.Tests passed all 146 tests (0 failures/skips), and all 3 SecurityConfigurationTests
passed after the path repair. See `package-20261110-cycle.trx` and `package-20261110-security-path-fixed.trx`.
The original full-suite run remains 925 passed / 1 failed / 18 skipped; it has not been relabeled as a clean run.

`package-20261110-full.trx`: 925 passed, 1 failed, 18 skipped, total 944; execution duration 11m48s. The skipped tests include opt-in SQL Server/concurrency checks and do not count as verified.

Failure: `SecurityConfigurationTests.Committed_application_settings_do_not_contain_runtime_secrets` threw DirectoryNotFoundException from FindApiRoot. The isolated output lives under the workspace's `.validation` folder; the helper only searched ancestors for IXApi.csproj. No settings assertion was reached, so this is not evidence of exposed secrets.

Repair: the helper also checks each ancestor's IXApi child directory for IXApi.csproj. It still reads the actual application settings, with no bypass of the security assertions. The existing file had no user changes before this small patch.

The targeted SecurityConfigurationTests run is queued behind the already-running InventoryCycle.Tests job, avoiding simultaneous output changes. Retest exec session: 8666. Actual outcome belongs in `package-20261110-security-path-fixed.trx` and `.exit`; do not mark fixed-and-passed until those exist. Original full-suite failure is preserved.
