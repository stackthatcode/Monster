
SELECT * FROM usrSystemState;

UPDATE usrSystemState SET InventoryPull = 2;

UPDATE usrSystemState SET IsRandomAccessMode = 1;

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
UPDATE usrShopAcuItemSync SET IsSyncEnabled = 0 WHERE Id = 20;


SELECT * FROM usrBackgroundJob;







