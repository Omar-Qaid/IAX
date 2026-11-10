# Counting variants fixture-length failure

Original evidence: package-20261110-counting-variants.trx, 7 failed / 1 passed / 0 skipped. Preparation refused
EcoResProduct.DisplayProductNumber exceeding its mapped maximum of 20 characters. No scenario posted: the master
transaction rejected the new fixture and rolled back. The passing test was the injected rollback case.

Cause: the product key prefixes the year-scoped item with the company; COUNTPLUS/COUNTMATCH suffixes were too long
after adding that prefix. Repair uses CPLUS/CMATCH. For company dat in 2026, product keys are dat-IV2-2026-CPLUS
and dat-IV2-2026-CMATCH, within the model limit. No mapped length, integrity validation, or existing fixture was relaxed.

The isolated retest is package-20261110-counting-variants-fixed.log/.trx. Check its actual result before recording
CNT-02 as passed. Historical failure evidence is retained.
