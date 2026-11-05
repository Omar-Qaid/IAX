# Master Data

Data/references.json describes 13 stable Foundation reference keys. Data/tables.json inventories the 68 masters and three transaction types. Module seeders own inserts; manifests describe and validate them. Foundation uses FNV2 keys and reuses APV2 shared records. Workers, contacts, customer defaults, dimensions and tax authority references resolve actual RecIds within the selected company.
