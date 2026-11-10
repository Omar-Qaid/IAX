# Movement build failure — no acceptance tests executed

package-20261110-movement.log records a fatal System.AccessViolationException in Roslyn while compiling
IAX.IXApi.Infrastructure, in SubstitutedMethodSymbol.get_ReturnTypeWithAnnotations / LocalRewriter.MakeCall.
No TRX/test outcome was produced. This is a compiler-process failure, not a failed inventory assertion.

No test/compiler process remained active at inspection. Retrying the unchanged implementation with one MSBuild
worker and UseSharedCompilation=false, preserving the original log. Inspect package-20261110-movement-retry.log
and .trx before another run. MOV-01 remains unverified until actual test results exist.
