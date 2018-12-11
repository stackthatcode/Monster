USE Monster0001;
GO


DELETE FROM usrPreferences;

-- TODO - replace with interactive management
DECLARE @StartingShopifyOrder int = 1035;
DECLARE @FulfillInAcumatica bit = 1;

INSERT INTO usrPreferences VALUES (
	'2018-12-06', 
	00.20, 
	@StartingShopifyOrder, 
	'STOCKITEM', 
	'SHOPIFY', 
	'5/24/2014', 
	'America/Chicago', 
	'ONLINE', 
	'102050', 
	'ONLINE', 
	'ONLINE', 
	'ONLINE', 
	NULL,
	@FulfillInAcumatica );

SELECT * FROM usrTenant;

SELECT * FROM usrPreferences;

DELETE FROM usrShopifyBatchState;


UPDATE usrAcumaticaBatchState SET AcumaticaOrdersPullEnd = NULL;

UPDATE usrAcumaticaBatchState SET AcumaticaShipmentsPullEnd = NULL;


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
SELECT TOP 3 * FROM usrShopifyOrder ORDER BY ShopifyOrderNumber DESC
SELECT TOP 3 * FROM usrAcumaticaSalesOrder ORDER BY AcumaticaOrderNbr DESC
SELECT * FROM usrShopAcuOrderSync;

DELETE FROM usrShopAcuOrderSync;


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


SELECT * FROM usrShopifyTransaction;


DELETE FROM usrShopifyRefund;
DELETE FROM usrShopifyFulfillment;
DELETE FROM usrShopifyBatchState;
DELETE FROM usrShopifyTransaction;
DELETE FROM usrShopifyOrder;
DELETE FROM usrShopifyCustomer;


SELECT * FROM usrShopifyORder;


SELECT * FROM usrShopifyCustomer;



SELECT * FROM usrJobExecutionLog;


-- Payouts (ON HOLD FOR NOW)
SELECT * FROM usrShopifyPayout;


INSERT INTO usrJobExecutionLog VALUES('Test 1',  GETUTCDATE())
INSERT INTO usrJobExecutionLog VALUES('Test 2',  GETUTCDATE())
INSERT INTO usrJobExecutionLog VALUES('Test 3',  GETUTCDATE())

DELETE FROM usrJobExecutionLog;

SELECT * FROM usrQueuedJob;

DELETE FROM usrQueuedJob;

SELECT * FROM usrPreferences;
SELECT * FROM usrShopifyORder;
SELECT * FROM usrShopAcuOrderSync;




