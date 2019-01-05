USE Monster0001;
GO



-- Clean Synchronization 
--
DELETE FROM usrShopAcuRefundCM;
DELETE FROM usrShopifyAcuPayment;
DELETE FROM usrShopAcuShipmentSync;
DELETE FROM usrShopAcuOrderSync;
DELETE FROM usrShopAcuCustomerSync;

DELETE FROM usrInventoryReceiptSync;
DELETE FROM usrShopAcuItemSync;
DELETE FROM usrShopAcuWarehouseSync;



-- Acumatica data
--
DELETE FROM usrAcumaticaInvoice;
DELETE FROM usrAcumaticaShipmentSalesOrderRef;
DELETE FROM usrAcumaticaShipment;
DELETE FROM usrAcumaticaSalesOrder;
DELETE FROM usrAcumaticaCustomer;

DELETE FROM usrAcumaticaInventoryReceipt;
DELETE FROM usrAcumaticaWarehouseDetails;
DELETE FROM usrAcumaticaStockItem;
DELETE FROM usrAcumaticaWarehouse;


-- Shopify data
--
DELETE FROM usrShopifyPayout;
DELETE FROM usrShopifyPayoutTransaction;

DELETE FROM usrShopifyTransaction;
DELETE FROM usrShopifyRefund;
DELETE FROM usrShopifyFulfillment;
DELETE FROM usrShopifyOrder;
DELETE FROM usrShopifyCustomer;

DELETE FROM usrShopifyInventoryLevels;
DELETE FROM usrShopifyVariant;
DELETE FROM usrShopifyProduct;
DELETE FROM usrShopifyLocation;




-- Clear out System tables
DELETE FROM usrAcumaticaReferences
DELETE FROM usrPreferences;

DELETE FROM usrSystemState;
DELETE FROM usrBackgroundJob;
DELETE FROM usrExecutionLog;

DELETE FROM usrAcumaticaBatchState;
DELETE FROM usrShopifyBatchState;


-- Clear out User Credentials
-- DELETE FROM usrTenant


ALTER DATABASE AcuInst4 SET SINGLE_USER WITH ROLLBACK IMMEDIATE 
ALTER DATABASE AcuInst4 SET MULTI_USER


/*
DROP DATABASE AcuInst2;
GO

-- TODO - RESTORE DATABASE FROM FILE
RESTORE DATABASE AcuInst2
FROM DISK = 'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\Backup\AcuInst2.bak'
*/

