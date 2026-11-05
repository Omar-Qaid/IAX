# Implementation Contract

Keep module seeders as the only master insertion path. Keep JSON descriptive and validate drift before writes. Resolve company/business keys to database IDs at execution time. Capture preparation references before commit, with no posted balances in master reports. Keep transaction execution explicit and service-owned. Run FoundationSeedTests, AP seed regressions and the affected API/inventory suites. Report actual skips and failures, not a catalog entry as proof.
