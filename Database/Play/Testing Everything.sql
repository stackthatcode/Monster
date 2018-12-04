USE Monster0001;
GO


/**
DELETE FROM usrPreferences;

INSERT INTO usrPreferences VALUES (
	'2018-01-01', 
	00.20, 
	1019, 
	'STOCKITEM', 
	'SHOPIFY', 
	'5/24/2014', 
	'America/Chicago', 
	'ONLINE', 
	'102050', 
	'ONLINE', 
	'ONLINE', 
	'ONLINE' );


SELECT * FROM usrPreferences;
**/



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
--UPDATE usrAcumaticaWarehouseDetails SET IsShopifySynced = 0;

SELECT * FROM usrInventoryReceiptSync;
SELECT * FROM usrAcumaticaInventoryReceipt;


-- Shopify Orders and Acumatica Sales Orders
SELECT * FROM usrShopifyOrder;
SELECT * FROM usrAcumaticaSalesOrder;
SELECT * FROM usrShopAcuOrderSync;


-- Shipments and Fulfillments
SELECT * FROM usrShopifyFulfillment;
SELECT * FROM usrAcumaticaShipment;
SELECT * FROM usrAcumaticaShipmentDetail;
SELECT * FROM usrShopAcuShipmentSync;
SELECT * FROM vw_AcumaticaUnsyncedShipments 




DELETE FROM usrQueuedJob;


-- Refunds
SELECT * FROM usrShopifyRefund;


-- Transactions/Payments
SELECT * FROM usrShopifyTransaction;
SELECT * FROM usrShopifyAcuPayment;


-- Payouts (ON HOLD FOR NOW)
SELECT * FROM usrShopifyPayout;

