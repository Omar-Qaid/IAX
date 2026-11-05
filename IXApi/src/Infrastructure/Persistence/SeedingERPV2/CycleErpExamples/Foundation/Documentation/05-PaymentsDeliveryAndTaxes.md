# Payments Delivery And Taxes

FNV2 has two installments totaling 100 percent, linked by the FNV2-60 payment term to the demo customer. The customer references delivery mode FNV2 and its default dimension set. The tax code APV2-15 uses the Monthly period with twelve reporting intervals. Tax configuration references existing input-tax ledger combinations; example transaction amounts are computed by business services, not by these manifests.

Fresh tax reporting intervals use version 0 (unsettled). Existing settled or closed intervals are not reset by a rerun; conflicting state is rejected and preserved.
