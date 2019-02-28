
SELECT * FROM usrSystemState;

SELECT * FROM usrShopAcuWarehouseSync;


UPDATE usrSystemState SET InventoryPull = 1;
UPDATE usrSystemState SET InventoryPull = 2;

UPDATE usrSystemState SET WarehouseSync = 1;
UPDATE usrSystemState SET WarehouseSync = 2;
UPDATE usrSystemState SET WarehouseSync = 3;
UPDATE usrSystemState SET WarehouseSync = 4;

UPDATE usrSystemState SET IsRandomAccessMode = 1;
UPDATE usrSystemState SET IsRandomAccessMode = 0;




SELECT * FROM vw_SyncVariantsAndStockItems;

SELECT * FROM usrShopifyVariant;

SELECT * FROM vw_SyncVariantAndStockItem;
SELECT * FROM vw_SyncShopifyInventory
SELECT * FROM vw_ShopifyInventory;

DELETE FROM usrInventoryReceiptSync;
DELETE FROM usrAcumaticaInventoryReceipt;
DELETE FROM usrShopAcuItemSync;

SELECT * FROM usrConnections;

SELECT * FROM usrShopAcuItemSync;

UPDATE usrShopAcuItemSync SET IsSyncEnabled = 0 

SELECT * FROM usrBackgroundJob;

SELECT * FROM usrAcumaticaBatchState;

UPDATE usrAcumaticaBatchState SET AcumaticaStockItemPullEnd = NULL;

SELECT * FROM usrAcumaticaWarehouseDetails ;

UPDATE usrAcumaticaWarehouseDetails SET IsInventorySynced = 0;

SELECT * FROM usrAcumaticaStockItem;


USE AcuInst6;

SELECT * FROM INItemSite;

SELECT * FROM ite

SELECT [name],[dbid] FROM [master].[dbo].[sysdatabases] ORDER BY [name]






