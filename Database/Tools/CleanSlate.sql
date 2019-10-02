USE Monster0001;
GO



-- Clean Synchronization 
--
DELETE FROM ShopAcuRefundCM;
DELETE FROM ShopifyAcuPayment;
DELETE FROM ShopAcuShipmentSync;
DELETE FROM ShopAcuOrderSync;
DELETE FROM ShopAcuCustomerSync;

DELETE FROM InventoryReceiptSync;
DELETE FROM ShopAcuItemSync;
DELETE FROM ShopAcuWarehouseSync;



-- Acumatica Sales Order data
--
DELETE FROM AcumaticaCreditMemo;
DELETE FROM AcumaticaShipmentSalesOrderRef;
DELETE FROM AcumaticaShipment;
DELETE FROM AcumaticaSoShipmentInvoice;
DELETE FROM AcumaticaSalesOrder;
DELETE FROM AcumaticaCustomer;



-- Acumatica Inventory data
--
DELETE FROM AcumaticaInventoryReceipt;
DELETE FROM AcumaticaWarehouseDetails;
DELETE FROM AcumaticaStockItem;
DELETE FROM AcumaticaWarehouse;



-- Shopify data
--
DELETE FROM ShopifyPayout;
DELETE FROM ShopifyPayoutTransaction;

DELETE FROM ShopifyTransaction;
DELETE FROM ShopifyRefund;
DELETE FROM ShopifyFulfillment;
DELETE FROM ShopifyOrder;
DELETE FROM ShopifyCustomer;

DELETE FROM ShopifyInventoryLevel;
DELETE FROM ShopifyVariant;
DELETE FROM ShopifyProduct;
DELETE FROM ShopifyLocation;



-- Clear out System tables
--
DELETE FROM AcumaticaRefData
DELETE FROM Preferences;

DELETE FROM AcumaticaBatchState;
DELETE FROM ShopifyBatchState;

DELETE FROM JobMonitor;
DELETE FROM ExecutionLog;

DELETE FROM SystemState;
-- DELETE FROM Tenant




ALTER DATABASE Monster0001 SET SINGLE_USER WITH ROLLBACK IMMEDIATE 
ALTER DATABASE Monster0001 SET MULTI_USER

ALTER DATABASE AcuInst0001 SET SINGLE_USER WITH ROLLBACK IMMEDIATE 
ALTER DATABASE AcuInst0001 SET MULTI_USER


/*
DROP DATABASE AcuInst6;
GO

-- TODO - RESTORE DATABASE FROM FILE
RESTORE DATABASE AcuInst6
FROM DISK = 'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\Backup\AcuInst2.bak'
*/

UPDATE AcuInst0001..Users SET Password = '123456'


