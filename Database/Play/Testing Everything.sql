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



-- Shipments and Fulfillments
SELECT * FROM usrShopifyFulfillment;
SELECT * FROM usrAcumaticaShipment;
SELECT * FROM usrAcumaticaShipmentDetail;
SELECT * FROM usrShopAcuShipmentSync;
SELECT * FROM vw_AcumaticaUnsyncedShipments 


-- Refunds
SELECT * FROM usrShopifyRefund;


-- Transactions/Payments
SELECT * FROM usrShopifyTransaction;
SELECT * FROM usrShopifyAcuPayment;

SELECT * FROM usrShopifyORder;

SELECT * FROM usrJobExecutionLog;


SELECT * FROM usrAcumaticaSalesOrder;
SELECT * FROM usrShopAcuOrderSync;
SELECT * FROM usrAcumaticaSoShipmentInvoice;


SELECT * FROM usrShopifyOrder
SELECT * FROM usrShopifyCustomer;
SELECT * FROM usrShopAcuCustomerSync;
SELECT * FROM usrAcumaticaCustomer;


SELECT * FROM usrAcumaticaShipment;
SELECT * FROM usrAcumaticaShipmentSalesOrderRef;
SELECT * FROM usrShopAcuShipmentSync;

DELETE FROM usrQueuedJob;

SELECT * FROM usrShopifyOrder;
SELECT * FROM usrPreferences;

SELECT * FROM usrAcumaticaInvoice;

DELETE FROM usrSystemState;


