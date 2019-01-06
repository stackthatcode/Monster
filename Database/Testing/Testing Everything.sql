USE Monster0001;
GO


-- 
SELECT * FROM usrTenant;
SELECT * FROM usrSystemState;

SELECT * FROM usrAcumaticaReferences
SELECT * FROM usrPreferences;

UPDATE usrTenant SET AcumaticaPassword = '';

--MYCOMPANY



-- Shopify Products & Variants
SELECT * FROM usrShopifyProduct;
SELECT * FROM usrShopifyVariant;
SELECT * FROM usrShopifyInventoryLevels;






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


