
-- Preferences, Batch State
SELECT * FROM usrPreferences;
SELECT * FROM usrBatchState;


-- Warehouse and Location
SELECT * FROM usrAcumaticaWarehouse;
SELECT * FROM usrShopifyLocation;
SELECT * FROM usrShopAcuWarehouseSync;


-- Products/Variants and Stock Items
SELECT * FROM usrShopifyProduct;
SELECT * FROM usrShopifyVariant;
SELECT * FROM usrAcumaticaStockItem;
SELECT * FROM usrShopAcuItemSync;


-- Inventory Levels and Warehouse Details
SELECT * FROM usrShopifyInventoryLevels;
SELECT * FROM usrAcumaticaWarehouseDetails;
SELECT * FROM usrInventoryReceiptSync;


-- Shopify Orders and Acumatica Sales Orders
SELECT * FROM usrShopifyOrder;
SELECT * FROM usrAcumaticaSalesOrder;
SELECT * FROM usrShopAcuOrderSync;


SELECT * FROM usrShopifyFulfillment;
SELECT * FROM usrAcumaticaShipment;
SELECT * FROM usrShopAcuShipmentSync;

SELECT * FROM vw_AcumaticaUnsyncedShipments 
