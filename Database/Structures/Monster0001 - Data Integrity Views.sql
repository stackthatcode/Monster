USE Monster0001;
GO

-- Shopify Workers checks
--

DROP VIEW IF EXISTS vw_ShopifyInventory 
GO
CREATE VIEW vw_ShopifyInventory
AS
SELECT	t1.MonsterId, 
		t1.ShopifyProductId,
		t1.IsDeleted AS ProductIsDeleted, 
		t2.ShopifyVariantId, 
		t2.ShopifyInventoryItemId, 
		t2.ShopifySku, 
		t2.ShopifyCost, 
		t2.IsMissing AS VariantIsMissing,
		t3.ShopifyLocationId,
		t3.ShopifyAvailableQuantity,
		t1.LastUpdated AS ProductLastUpdated,
		t2.LastUpdated AS VariantLastUpdated,
		t3.LastUpdated AS InventoryLevelLastUpdate
FROM usrShopifyProduct t1
	LEFT JOIN usrShopifyVariant t2
		ON t2.ParentMonsterId = t1.MonsterId
	LEFT JOIN usrShopifyInventoryLevel t3
		ON t3.ParentMonsterId = t2.MonsterId;
GO

DROP VIEW IF EXISTS vw_ShopifyOrderCustomer 
GO
CREATE VIEW vw_ShopifyOrderCustomer
AS
SELECT 
	t1.Id, 
	t1.ShopifyOrderId, 
	t1.ShopifyOrderNumber, 
	t1.ShopifyFinancialStatus, 
	t1.AreTransactionsUpdated,
	t2.ShopifyCustomerId,
	t2.ShopifyPrimaryEmail,
	t1.LastUpdated AS OrderLastUpdated,
	t2.LastUpdated AS CustomerLastUpdated
FROM usrShopifyOrder t1
	LEFT OUTER JOIN usrShopifyCustomer t2
		ON t2.Id = t1.CustomerMonsterId;
GO

DROP VIEW IF EXISTS vw_ShopifyOrderRefunds
GO
CREATE VIEW vw_ShopifyOrderRefunds
AS
SELECT 
	t1.Id, 
	t1.ShopifyOrderId, 
	t1.ShopifyOrderNumber, 
	t1.ShopifyFinancialStatus, 
	t1.AreTransactionsUpdated,
	t2.ShopifyRefundId,
	t1.LastUpdated AS OrderLastUpdated,
	t2.LastUpdated AS RefundLastUpdated
FROM usrShopifyOrder t1
	LEFT OUTER JOIN usrShopifyRefund t2
		ON t2.OrderMonsterId = t1.Id;
GO

DROP VIEW IF EXISTS vw_ShopifyOrderFulfillments
GO
CREATE VIEW vw_ShopifyOrderFulfillments
AS
SELECT 
	t1.Id, 
	t1.ShopifyOrderId, 
	t1.ShopifyOrderNumber, 
	t1.ShopifyFinancialStatus, 
	t1.AreTransactionsUpdated,
	t2.ShopifyFulfillmentId,
	t2.ShopifyStatus,
	t1.LastUpdated AS OrderLastUpdated,
	t2.LastUpdated AS FulfillmentLastUpdated
FROM usrShopifyOrder t1
	LEFT OUTER JOIN usrShopifyFulfillment t2
		ON t2.OrderMonsterId = t1.Id;
GO



DROP VIEW IF EXISTS vw_ShopifyOrderTransactions
GO
CREATE VIEW vw_ShopifyOrderTransactions
AS
SELECT 
	t1.Id, 
	t1.ShopifyOrderId, 
	t1.ShopifyOrderNumber, 
	t1.ShopifyFinancialStatus, 
	t1.AreTransactionsUpdated,
	t2.ShopifyTransactionId,
	t2.ShopifyKind,
	t2.ShopifyStatus,
	t1.LastUpdated AS OrderLastUpdated,
	t2.LastUpdated AS TransactionLastUpdated
FROM usrShopifyOrder t1
	LEFT OUTER JOIN usrShopifyTransaction t2
		ON t2.OrderMonsterId = t1.Id;
GO


-- Acumatica Workers checks
--

DROP VIEW IF EXISTS vw_AcumaticaInventory 
GO
CREATE VIEW vw_AcumaticaInventory
AS
SELECT	t1.MonsterId, 
		t1.ItemId,
		t2.AcumaticaWarehouseId,
		t2.AcumaticaQtyOnHand,
		t2.IsShopifySynced,
		t1.LastUpdated AS StockItemLastUpdated,
		t2.LastUpdated AS WarehouseDetailsLastUpdated
FROM usrAcumaticaStockItem t1
	LEFT JOIN usrAcumaticaWarehouseDetails t2
		ON t2.ParentMonsterId = t1.MonsterId
GO

DROP VIEW IF EXISTS vw_AcumaticaSalesOrderAndCustomer
GO
CREATE VIEW vw_AcumaticaSalesOrderAndCustomer
AS
	SELECT t1.Id, 
		t1.AcumaticaOrderNbr, 
		t1.AcumaticaStatus,
		t2.AcumaticaCustomerId,
		t1.LastUpdated AS SalesOrderLastUpdated,
		t2.LastUpdated AS CustomerLastUpdated
	FROM usrAcumaticaSalesOrder t1
		LEFT OUTER JOIN usrAcumaticaCustomer t2
			ON t2.Id = t1.CustomerMonsterId;
GO

DROP VIEW IF EXISTS vw_AcumaticaSalesOrderAndShipmentInvoices
GO

CREATE VIEW vw_AcumaticaSalesOrderAndShipmentInvoices
AS
	SELECT t1.Id, 
		t1.AcumaticaOrderNbr, 
		t1.AcumaticaStatus,
		t2.AcumaticaShipmentNbr,
		t2.AcumaticaInvoiceNbr,
		t1.LastUpdated AS SalesOrderLastUpdated,
		t2.LastUpdated AS ShipmentLastUpdated
	FROM usrAcumaticaSalesOrder t1
		LEFT OUTER JOIN usrAcumaticaSoShipmentInvoice t2
			ON t2.SalesOrderMonsterId = t1.Id;
GO

DROP VIEW IF EXISTS vw_AcumaticaSalesOrderAndShipments
GO

CREATE VIEW vw_AcumaticaSalesOrderAndShipments
AS
	SELECT t1.Id,
		t1.AcumaticaShipmentNbr, 
		t1.AcumaticaStatus,
		t2.AcumaticaOrderNbr,
		t1.LastUpdated AS ShipmentLastUpdated,
		t2.LastUpdated AS SalesOrderRefLastUpdated
	FROM usrAcumaticaShipment t1
		LEFT OUTER JOIN usrAcumaticaShipmentSalesOrderRef t2
			ON t1.Id = t2.ShipmentMonsterId
GO


-- Sync Workers checks
--


DROP VIEW IF EXISTS vw_SyncWarehousesAndLocations
GO

CREATE VIEW vw_SyncWarehousesAndLocations
AS
SELECT t1.MonsterId AS ShopifyLocationMonsterId, 
		t1.ShopifyLocationName,
		t1.ShopifyLocationId,
		t2.Id AS AcumaticaWarehouseMonsterId,
		t2.AcumaticaWarehouseId
FROM usrShopifyLocation t1
	FULL OUTER JOIN usrAcumaticaWarehouse t2
		ON t1.ShopifyLocationName = t2.AcumaticaWarehouseId;
GO


DROP VIEW IF EXISTS vw_SyncVariantsAndStockItems
GO
CREATE VIEW vw_SyncVariantsAndStockItems
AS
SELECT t1.ShopifyVariantId,
	t1.ShopifyInventoryItemId,
	t1.ShopifySku,
	t1.ShopifyCost,
	t1.ShopifyIsTracked,
	t1.IsMissing,
	t3.ItemId,
	t1.LastUpdated AS VariantLastUpdated,
	t3.LastUpdated AS StockItemLastUpdated
FROM usrShopifyVariant t1
	FULL OUTER JOIN usrShopAcuItemSync t2
		ON t2.ShopifyVariantMonsterId = t1.MonsterId
	FULL OUTER JOIN usrAcumaticaStockItem t3
		ON t3.MonsterId = t2.AcumaticaItemMonsterId
GO


DROP VIEW IF EXISTS vw_SyncAcumaticaInventory
GO
CREATE VIEW vw_SyncAcumaticaInventory
AS
SELECT t1.ItemId AS AcumaticaItemId, 
	t3.AcumaticaWarehouseId, 
	t3.AcumaticaQtyOnHand, 
	t4.Id AS WarehouseSyncId,
	t2.Id AS ItemSyncId
FROM usrAcumaticaStockItem t1
	FULL OUTER JOIN usrShopAcuItemSync t2
		ON t1.MonsterId = t2.AcumaticaItemMonsterId
	FULL OUTER JOIN usrAcumaticaWarehouseDetails t3
		ON t1.MonsterId = t3.ParentMonsterId
	FULL OUTER JOIN usrShopAcuWarehouseSync t4
		ON t3.WarehouseMonsterId = t4.AcumaticaWarehouseMonsterId;
GO


DROP VIEW IF EXISTS vw_SyncShopifyInventory
GO
CREATE VIEW vw_SyncShopifyInventory
AS
SELECT t1.ShopifySku, 
		t1.ShopifyVariantId, 
		t2.ShopifyAvailableQuantity, 
		t2.ShopifyLocationId,
		t5.ShopifyLocationName,
		t3.Id AS LocationSyncId, 
		t4.Id AS VariantSyncId
FROM usrShopifyVariant t1
	FULL OUTER JOIN usrShopifyInventoryLevel t2
		ON t1.MonsterId = t2.ParentMonsterId
	FULL OUTER JOIN usrShopAcuWarehouseSync t3
		ON t2.LocationMonsterId = t3.ShopifyLocationMonsterId
	FULL OUTER JOIN usrShopAcuItemSync t4
		ON t1.MonsterId = t4.ShopifyVariantMonsterId 
	FULL OUTER JOIN usrShopifyLocation t5
		ON t2.LocationMonsterId = t5.MonsterId;
GO


DROP VIEW IF EXISTS vw_SyncInventoryAllInclusive
GO
CREATE VIEW vw_SyncInventoryAllInclusive
AS 
SELECT t1.ShopifySku, 
	t2.AcumaticaItemId,
	t1.ShopifyVariantId,
	t1.ShopifyLocationId,
	t1.ShopifyLocationName,
	t2.AcumaticaWarehouseId,
	t1.ShopifyAvailableQuantity,
	t2.AcumaticaQtyOnHand
FROM vw_SyncShopifyInventory t1
	FULL OUTER JOIN vw_SyncAcumaticaInventory t2
		ON t1.LocationSyncId = t2.WarehouseSyncId
		AND t1.VariantSyncId = t2.ItemSyncId
GO



DROP VIEW IF EXISTS vw_SyncInventoryLevelAndReceipts
GO
CREATE VIEW vw_SyncInventoryLevelAndReceipts
AS
	SELECT 
		t4.ShopifySku,
		t1.MonsterId,
		t1.ShopifyInventoryItemId,
		t1.ShopifyLocationId,
		t1.ShopifyAvailableQuantity,
		t3.AcumaticaRefNumber,
		t3.IsReleased,
		t1.LastUpdated AS InventoryLevelLastUpdated,
		t3.LastUpdate AS InventoryReceiptLastUpdated
	FROM usrShopifyInventoryLevel t1
		FULL OUTER JOIN usrInventoryReceiptSync t2
			ON t2.ShopifyInventoryMonsterId = t1.MonsterId
		FULL OUTER JOIN usrAcumaticaInventoryReceipt t3
			ON t3.MonsterId = t2.AcumaticaInvReceiptMonsterId
		FULL OUTER JOIN usrShopifyVariant t4
			ON t4.MonsterId = t1.ParentMonsterId
GO




DROP VIEW IF EXISTS vw_SyncCustomerWithCustomers
GO
CREATE VIEW vw_SyncCustomerWithCustomers
AS
SELECT  
	t1.ShopifyCustomerId, 
	t1.ShopifyPrimaryEmail,
	t1.IsUpdatedInAcumatica,
	t1.LastUpdated AS ShopifyLastUpdated,
	t3.AcumaticaCustomerId,
	t3.AcumaticaMainContactEmail,
	t3.LastUpdated AS AcumaticaLastUpdated
FROM usrShopifyCustomer t1
	FULL OUTER JOIN usrShopAcuCustomerSync t2
		ON t1.Id = t2.ShopifyCustomerMonsterId
	FULL OUTER JOIN usrAcumaticaCustomer t3
		ON t2.AcumaticaCustomerMonsterId = t3.Id
GO



DROP VIEW IF EXISTS vw_SyncOrdersAndSalesOrders
GO
CREATE VIEW vw_SyncOrdersAndSalesOrders
AS
	SELECT t1.ShopifyOrderId, 
		t1.ShopifyOrderNumber, 
		t1.ShopifyIsCancelled, 
		t1.ShopifyFinancialStatus,
		t1.AreTransactionsUpdated, 
		t2.IsTaxLoadedToAcumatica,
		t3.AcumaticaOrderNbr,
		t3.AcumaticaStatus,
		t4.AcumaticaInvoiceNbr,
		t4.AcumaticaShipmentNbr,
		t1.LastUpdated AS ShopifyLastUpdated,
		t3.LastUpdated AS AcumaticaLastUpdated
	FROM usrShopifyOrder t1
		FULL OUTER JOIN usrShopAcuOrderSync t2
			ON t1.Id = t2.ShopifyOrderMonsterId
		FULL OUTER JOIN usrAcumaticaSalesOrder t3
			ON t2.AcumaticaSalesOrderMonsterId = t3.Id
		FULL OUTER JOIN usrAcumaticaSoShipmentInvoice t4
			ON t3.Id = t4.SalesOrderMonsterId
GO


DROP VIEW IF EXISTS vw_SyncFulfillmentsAndShipments
GO
CREATE VIEW vw_SyncFulfillmentsAndShipments
AS
SELECT 
	t0.ShopifyOrderId,
	t0.ShopifyOrderNumber,
	t1.ShopifyFulfillmentId,
	t1.ShopifyStatus, 
	t3.AcumaticaOrderNbr,
	t3.AcumaticaShipmentNbr,
	t4.AcumaticaStatus AS AcumaticaShipmentStatus,
	t1.LastUpdated AS FulfillmentLastUpdated
FROM usrShopifyOrder t0
	FULL OUTER JOIN usrShopifyFulfillment t1
		ON t0.Id = t1.OrderMonsterId
	FULL OUTER JOIN usrShopAcuShipmentSync t2
		ON t1.Id = t2.ShopifyFulfillmentMonsterId
	FULL OUTER JOIN usrAcumaticaShipmentSalesOrderRef t3
		ON t2.AcumaticaShipDetailMonsterId = t3.Id
	FULL OUTER JOIN usrAcumaticaShipment t4
		ON t3.ShipmentMonsterId = t4.Id
GO



DROP VIEW IF EXISTS vw_SyncRefundAndCreditMemo
GO
CREATE VIEW vw_SyncRefundAndCreditMemo
AS
SELECT 
	t1.ShopifyOrderId,
	t1.ShopifyOrderNumber,
	t2.ShopifyRefundId,
	t3.AcumaticaCreditMemoNbr,
	t2.LastUpdated AS ShopifyRefundLastUpdated,
	t3.LastUpdated AS CreditMemoSyncLastUpdated
FROM usrShopifyOrder t1
	FULL OUTER JOIN usrShopifyRefund t2
		ON t1.Id = t2.OrderMonsterId
	FULL OUTER JOIN usrShopAcuRefundCM t3
		ON t2.Id = t3.ShopifyRefundMonsterId
GO

DROP VIEW IF EXISTS vw_SyncTransactionAndPayment
GO
CREATE VIEW vw_SyncTransactionAndPayment
AS
SELECT 
	t1.ShopifyOrderId,
	t1.ShopifyOrderNumber,
	t2.ShopifyTransactionId,
	t2.ShopifyStatus,
	t2.ShopifyKind,
	t3.ShopifyPaymentNbr,
	t2.LastUpdated AS ShopifyRefundLastUpdated,
	t3.LastUpdated AS PaymentSyncLastUpdated
FROM usrShopifyOrder t1
	FULL OUTER JOIN usrShopifyTransaction t2
		ON t1.Id = t2.OrderMonsterId
	FULL OUTER JOIN usrShopifyAcuPayment t3
		ON t2.Id = t3.ShopifyTransactionMonsterId
GO


SELECT * FROM vw_ShopifyInventory;
SELECT * FROM vw_ShopifyOrderCustomer;
SELECT * FROM vw_ShopifyOrderRefunds;
SELECT * FROM vw_ShopifyOrderFulfillments;
SELECT * FROM vw_ShopifyOrderTransactions;

SELECT * FROM vw_AcumaticaInventory;
SELECT * FROM vw_AcumaticaSalesOrderAndCustomer;
SELECT * FROM vw_AcumaticaSalesOrderAndShipmentInvoices;
SELECT * FROM vw_AcumaticaSalesOrderAndShipments;

SELECT * FROM vw_SyncWarehousesAndLocations;
SELECT * FROM vw_SyncVariantsAndStockItems;
SELECT * FROM vw_SyncInventoryLevelAndReceipts;
SELECT * FROM vw_SyncInventoryAllInclusive;		-- Identifies unmatched Stock Items-Variants

SELECT * FROM vw_SyncCustomerWithCustomers;
SELECT * FROM vw_SyncOrdersAndSalesOrders;		-- Identifies unsynced Orders-Sales Orders
SELECT * FROM vw_SyncFulfillmentsAndShipments;	-- Bi-directional view of Fulfillments and Shipments
SELECT * FROM vw_SyncRefundAndCreditMemo;		-- Shows Orders with/out Refunds, and with/wout Credit Memo sync
SELECT * FROM vw_SyncTransactionAndPayment;		-- Shows Transactions with/out Payment syncs


