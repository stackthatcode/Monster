USE Monster0001;
GO


-- Clear out Monster

DELETE FROM usrBatchState;

-- Synchronization
DELETE FROM usrShopAcuRefundCM;
DELETE FROM usrShopAcuShipmentSync;
DELETE FROM usrShopAcuOrderSync;
DELETE FROM usrShopAcuCustomerSync;
DELETE FROM usrInventoryReceiptSync;
DELETE FROM usrShopAcuItemSync;
DELETE FROM usrShopAcuWarehouseSync;


-- Shopify
DELETE FROM usrAcumaticaShipmentDetail;
DELETE FROM usrAcumaticaShipment;
DELETE FROM usrAcumaticaSalesOrder;
DELETE FROM usrAcumaticaCustomer;

DELETE FROM usrAcumaticaInventoryReceipt;
DELETE FROM usrAcumaticaWarehouseDetails;
DELETE FROM usrAcumaticaStockItem;
DELETE FROM usrAcumaticaWarehouse;


-- Acumatica
DELETE FROM usrShopifyRefund;
DELETE FROM usrShopifyFulfillment;
DELETE FROM usrShopifyOrder;
DELETE FROM usrShopifyCustomer;

DELETE FROM usrShopifyInventoryLevels;
DELETE FROM usrShopifyVariant;
DELETE FROM usrShopifyProduct;
DELETE FROM usrShopifyLocation;



ALTER DATABASE AcuInst2 SET SINGLE_USER WITH ROLLBACK IMMEDIATE 
ALTER DATABASE AcuInst2 SET MULTI_USER


/*
DROP DATABASE AcuInst2;
GO

-- TODO - RESTORE DATABASE FROM FILE
RESTORE DATABASE AcuInst2
FROM DISK = 'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\Backup\AcuInst2.bak'
*/

