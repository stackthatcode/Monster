USE Monster0001;
GO



-- Clean Synchronization data
--
DROP PROCEDURE IF EXISTS dbo.DeleteAllSyncRecords
GO

CREATE PROCEDURE dbo.DeleteAllSyncRecords
AS
	DELETE FROM InventoryReceiptSync;
	DELETE FROM ShopAcuWarehouseSync;
GO



-- Acumatica Sales Order data
--
DROP PROCEDURE IF EXISTS dbo.DeleteAllAcumaticaOrderRecords
GO

CREATE PROCEDURE dbo.DeleteAllAcumaticaOrderRecords
AS
	DELETE FROM AcumaticaPayment;
	DELETE FROM AcumaticaSoShipment;
	DELETE FROM AcumaticaSalesOrder;
	DELETE FROM AcumaticaCustomer;
GO



-- Acumatica Inventory data
--
DROP PROCEDURE IF EXISTS dbo.DeleteAllAcumaticaInventoryRecords
GO

CREATE PROCEDURE dbo.DeleteAllAcumaticaInventoryRecords
AS
	DELETE FROM AcumaticaInventoryReceipt;
	DELETE FROM AcumaticaInventory;
	DELETE FROM AcumaticaStockItem;
	DELETE FROM AcumaticaWarehouse;
GO



-- Delete all Shopify Order data
--
DROP PROCEDURE IF EXISTS dbo.DeleteAllShopifyOrderRecords
GO

CREATE PROCEDURE dbo.DeleteAllShopifyOrderRecords
AS
	DELETE FROM ShopifyPayout;
	DELETE FROM ShopifyPayoutTransaction;

	DELETE FROM ShopifyTransaction;
	DELETE FROM ShopifyRefund;
	DELETE FROM ShopifyFulfillment;
	DELETE FROM ShopifyOrder;
	DELETE FROM ShopifyCustomer;
GO

-- Delete all Shopify Inventory data
--
DROP PROCEDURE IF EXISTS dbo.DeleteAllShopifyInventoryRecords
GO

CREATE PROCEDURE dbo.DeleteAllShopifyInventoryRecords
AS
	DELETE FROM ShopifyInventoryLevel;
	DELETE FROM ShopifyVariant;
	DELETE FROM ShopifyProduct;
	DELETE FROM ShopifyLocation;
GO


-- Clear out System tables
--
DROP PROCEDURE IF EXISTS dbo.DeleteAllSystemRecords
GO

CREATE PROCEDURE dbo.DeleteAllSystemRecords
AS
	DELETE FROM AcumaticaBatchState;
	DELETE FROM ShopifyBatchState;

	DELETE FROM AcumaticaRefData
	DELETE FROM MonsterSettings;
	DELETE FROM PaymentGateways;

	DELETE FROM SystemState;

	DELETE FROM ExclusiveJobMonitor;
	DELETE FROM ExecutionLog;
GO



EXEC dbo.DeleteAllSyncRecords;
EXEC dbo.DeleteAllAcumaticaOrderRecords;
EXEC dbo.DeleteAllAcumaticaInventoryRecords;
EXEC dbo.DeleteAllShopifyOrderRecords;
EXEC dbo.DeleteAllShopifyInventoryRecords;
EXEC dbo.DeleteAllSystemRecords;




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


