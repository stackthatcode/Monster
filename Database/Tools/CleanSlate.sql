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



-- Acumatica Sales Order data
--
DELETE FROM usrAcumaticaCreditMemo;

DELETE FROM usrAcumaticaShipmentSalesOrderRef;
DELETE FROM usrAcumaticaShipment;

DELETE FROM usrAcumaticaSoShipmentInvoice;
DELETE FROM usrAcumaticaSalesOrder;

DELETE FROM usrAcumaticaCustomer;



-- Acumatica Inventory data
--
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

DELETE FROM usrShopifyInventoryLevel;
DELETE FROM usrShopifyVariant;
DELETE FROM usrShopifyProduct;
DELETE FROM usrShopifyLocation;



-- Clear out System tables
--
DELETE FROM AcumaticaRefData
DELETE FROM Preferences;

DELETE FROM AcumaticaBatchState;
DELETE FROM ShopifyBatchState;

DELETE FROM BackgroundJob;
DELETE FROM ExecutionLog;

DELETE FROM SystemState;
-- DELETE FROM Tenant




ALTER DATABASE Monster0001 SET SINGLE_USER WITH ROLLBACK IMMEDIATE 
ALTER DATABASE Monster0001 SET MULTI_USER

ALTER DATABASE AcuInst6 SET SINGLE_USER WITH ROLLBACK IMMEDIATE 
ALTER DATABASE AcuInst6 SET MULTI_USER


/*
DROP DATABASE AcuInst6;
GO

-- TODO - RESTORE DATABASE FROM FILE
RESTORE DATABASE AcuInst6
FROM DISK = 'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\Backup\AcuInst2.bak'
*/

UPDATE AcuInst6..Users SET Password = '123456'


