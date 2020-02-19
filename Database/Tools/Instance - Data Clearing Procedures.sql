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
	DELETE FROM AcumaticaMemo;
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


DROP PROCEDURE IF EXISTS dbo.DeleteBatchStateOnly
GO

CREATE PROCEDURE dbo.DeleteBatchStateOnly
AS
	DELETE FROM AcumaticaBatchState;
	DELETE FROM ShopifyBatchState;
GO


-- Clear out System tables
--
DROP PROCEDURE IF EXISTS dbo.DeleteAllSystemRecords
GO

CREATE PROCEDURE dbo.DeleteAllSystemRecords
AS
	DELETE FROM AcumaticaJsonStore;
	DELETE FROM ShopifyJsonStore;

	DELETE FROM AcumaticaBatchState;
	DELETE FROM ShopifyBatchState;

	DELETE FROM AcumaticaRefData
	DELETE FROM MonsterSettings;
	DELETE FROM PaymentGateways;

	DELETE FROM SystemState;

	DELETE FROM ExclusiveJobMonitor;
	DELETE FROM ExecutionLog;
GO



