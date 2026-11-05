# Execution And Rerun

Use a fresh DI scope and the intended company execution context. PrepareAsync validates definitions before mutation, invokes SeedRunner under the existing lock and serializable transaction, captures validated references before commit, and returns the report only after commit. Reruns insert missing masters and validate existing values. Transaction examples retain owning-service commit boundaries; earlier committed documents remain available after a later failure and are reused on resume.
