USE Monster0001;
GO

-- Warehouse and Location synchronization
--
SELECT * FROM vw_SyncWarehousesAndLocations;


-- Inventory synchronization
--
SELECT * FROM vw_ShopifyInventory;
SELECT * FROM vw_AcumaticaInventory;
SELECT * FROM vw_SyncWarehousesAndLocations;
SELECT * FROM vw_SyncVariantsAndStockItems;
SELECT * FROM vw_SyncInventoryLevelAndReceipts;
SELECT * FROM vw_SyncInventoryAllInclusive;		-- Identifies unmatched Stock Items-Variants





