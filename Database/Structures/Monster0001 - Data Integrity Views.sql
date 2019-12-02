USE Monster0001;
GO

-- Shopify Workers checks
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
FROM ShopifyLocation t1
	FULL OUTER JOIN AcumaticaWarehouse t2
		ON t1.ShopifyLocationName = t2.AcumaticaWarehouseId;
GO


DROP VIEW IF EXISTS vw_ShopifyInventory 
GO
CREATE VIEW vw_ShopifyInventory
AS
SELECT	t1.MonsterId, 
		t1.ShopifyProductId,
		t1.ShopifyTitle AS ProductTitle,
		t1.ShopifyVendor,
		t1.ShopifyProductType,
		t1.IsDeleted AS ProductIsDeleted, 
		t2.ShopifyVariantId, 
		t2.ShopifyInventoryItemId, 
		t2.ShopifySku, 
		t2.ShopifyTitle AS VariantTitle,
		t2.ShopifyCost, 
		t2.IsMissing AS VariantIsMissing,
		t3.ShopifyLocationId,
		t3.ShopifyAvailableQuantity,
		t1.LastUpdated AS ProductLastUpdated,
		t2.LastUpdated AS VariantLastUpdated,
		t3.LastUpdated AS InventoryLevelLastUpdate
FROM ShopifyProduct t1
	LEFT JOIN ShopifyVariant t2
		ON t2.ParentMonsterId = t1.MonsterId
	LEFT JOIN ShopifyInventoryLevel t3
		ON t3.ParentMonsterId = t2.MonsterId;
GO

DROP VIEW IF EXISTS vw_AcumaticaInventory 
GO
CREATE VIEW vw_AcumaticaInventory
AS
SELECT	t1.MonsterId, 
		t1.ItemId,
		t1.IsPriceSynced,
		t2.AcumaticaWarehouseId,
		t2.AcumaticaAvailQty,
		t2.IsInventorySynced,
		t1.LastUpdated AS StockItemLastUpdated,
		t2.LastUpdated AS WarehouseDetailsLastUpdated
FROM AcumaticaStockItem t1
	LEFT JOIN AcumaticaInventory t2
		ON t2.ParentMonsterId = t1.MonsterId
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
FROM ShopifyVariant t1
	FULL OUTER JOIN AcumaticaStockItem t3
		ON t3.ShopifyVariantMonsterId = t1.MonsterId
GO


DROP VIEW IF EXISTS vw_SyncVariantsAndStockItems_Alt
GO
CREATE VIEW vw_SyncVariantsAndStockItems_Alt
AS
	SELECT	t2.MonsterId AS MonsterVariantId, t2.ShopifyProductId, t2.ShopifyTitle AS ShopifyProductTitle, t2.ShopifyVendor, t2.ShopifyProductType,
			t1.ShopifyVariantId, t1.ShopifySku, t1.ShopifyTitle AS ShopifyVariantTitle, t4.ItemId AS AcumaticaItemId,
			t4.AcumaticaDescription
	FROM ShopifyVariant t1 
		INNER JOIN ShopifyProduct t2
			ON t1.ParentMonsterId = t2.MonsterId
		INNER JOIN AcumaticaStockItem t4
			ON t1.MonsterId = t4.ShopifyVariantMonsterId;		
GO

DROP VIEW IF EXISTS vw_SyncAcumaticaInventory
GO
CREATE VIEW vw_SyncAcumaticaInventory
AS
SELECT t1.MonsterId,
	t1.ShopifyVariantMonsterId,
	t1.ItemId AS AcumaticaItemId, 
	t3.AcumaticaWarehouseId, 
	t3.AcumaticaAvailQty, 
	t4.Id AS WarehouseSyncId,
	t1.IsPriceSynced,
	t3.IsInventorySynced
FROM AcumaticaStockItem t1
	FULL OUTER JOIN AcumaticaInventory t3
		ON t1.MonsterId = t3.ParentMonsterId
	FULL OUTER JOIN ShopAcuWarehouseSync t4
		ON t3.WarehouseMonsterId = t4.AcumaticaWarehouseMonsterId;
GO

DROP VIEW IF EXISTS vw_SyncShopifyInventory
GO
CREATE VIEW vw_SyncShopifyInventory
AS
SELECT	t1.MonsterId AS MonsterVariantId,
		t1.ShopifySku, 
		t1.ShopifyVariantId,
		t1.IsMissing,
		t2.ShopifyAvailableQuantity, 
		t2.ShopifyLocationId,
		t5.ShopifyLocationName,
		t3.Id AS LocationSyncId
FROM ShopifyVariant t1
	FULL OUTER JOIN ShopifyInventoryLevel t2
		ON t1.MonsterId = t2.ParentMonsterId
	FULL OUTER JOIN ShopAcuWarehouseSync t3
		ON t2.LocationMonsterId = t3.ShopifyLocationMonsterId
	INNER JOIN ShopifyLocation t5
		ON t2.LocationMonsterId = t5.MonsterId;
GO

DROP VIEW IF EXISTS vw_SyncInventoryAllInclusive
GO
CREATE VIEW vw_SyncInventoryAllInclusive
AS 
SELECT t1.ShopifySku, 
	t2.AcumaticaItemId,
	t1.ShopifyVariantId,
	t1.IsMissing,
	t1.ShopifyLocationId,
	t1.ShopifyLocationName,
	t2.AcumaticaWarehouseId,
	t1.ShopifyAvailableQuantity,
	t2.AcumaticaAvailQty
FROM vw_SyncShopifyInventory t1
	FULL OUTER JOIN vw_SyncAcumaticaInventory t2
		ON t1.MonsterVariantId = t2.ShopifyVariantMonsterId
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
	FROM ShopifyInventoryLevel t1
		FULL OUTER JOIN InventoryReceiptSync t2
			ON t2.ShopifyInventoryMonsterId = t1.MonsterId
		FULL OUTER JOIN AcumaticaInventoryReceipt t3
			ON t3.MonsterId = t2.AcumaticaInvReceiptMonsterId
		FULL OUTER JOIN ShopifyVariant t4
			ON t4.MonsterId = t1.ParentMonsterId
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
	t1.NeedsTransactionGet,
	t1.NeedsOrderPut,
	t2.ShopifyCustomerId,
	t2.ShopifyPrimaryEmail,
	t2.NeedsCustomerPut,
	t1.LastUpdated AS OrderLastUpdated,
	t2.LastUpdated AS CustomerLastUpdated
FROM ShopifyOrder t1
	LEFT OUTER JOIN ShopifyCustomer t2
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
	t1.NeedsTransactionGet,
	t1.NeedsOrderPut,
	t2.ShopifyRefundId,
	t1.LastUpdated AS OrderLastUpdated,
	t2.LastUpdated AS RefundLastUpdated
FROM ShopifyOrder t1
	LEFT OUTER JOIN ShopifyRefund t2
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
	t1.NeedsTransactionGet,
	t1.NeedsOrderPut,
	t2.ShopifyFulfillmentId,
	t2.ShopifyStatus,
	t1.LastUpdated AS OrderLastUpdated,
	t2.LastUpdated AS FulfillmentLastUpdated
FROM ShopifyOrder t1
	LEFT OUTER JOIN ShopifyFulfillment t2
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
	t1.NeedsTransactionGet,
	t1.NeedsOrderPut,
	t2.ShopifyTransactionId,
	t2.ShopifyKind,
	t2.ShopifyStatus,
	t1.LastUpdated AS OrderLastUpdated,
	t2.LastUpdated AS TransactionLastUpdated
FROM ShopifyOrder t1
	LEFT OUTER JOIN ShopifyTransaction t2
		ON t2.OrderMonsterId = t1.Id;
GO


-- Acumatica Workers checks
--

DROP VIEW IF EXISTS vw_AcumaticaSalesOrderAndCustomer
GO
CREATE VIEW vw_AcumaticaSalesOrderAndCustomer
AS
	SELECT t1.ShopifyOrderMonsterId, 
		t1.AcumaticaOrderNbr, 
		t1.AcumaticaStatus,
		t2.AcumaticaCustomerId,
		t1.LastUpdated AS SalesOrderLastUpdated,
		t2.LastUpdated AS CustomerLastUpdated
	FROM AcumaticaSalesOrder t1
		LEFT OUTER JOIN AcumaticaCustomer t2
			ON t2.ShopifyCustomerMonsterId = t1.ShopifyCustomerMonsterId;
GO

DROP VIEW IF EXISTS vw_AcumaticaSalesOrderAndShipmentInvoices
GO

CREATE VIEW vw_AcumaticaSalesOrderAndShipmentInvoices
AS
	SELECT t1.ShopifyOrderMonsterId, 
		t1.AcumaticaOrderNbr, 
		t1.AcumaticaStatus,
		t2.AcumaticaShipmentNbr,
		t2.AcumaticaInvoiceNbr,
		t2.AcumaticaTrackingNbr,
		t1.LastUpdated AS SalesOrderLastUpdated,
		t2.LastUpdated AS ShipmentLastUpdated
	FROM AcumaticaSalesOrder t1
		LEFT OUTER JOIN AcumaticaSoShipment t2
			ON t2.ShopifyOrderMonsterId = t1.ShopifyOrderMonsterId;
GO



-- Sync Workers checks
--


DROP VIEW IF EXISTS vw_SyncCustomerWithCustomers
GO
CREATE VIEW vw_SyncCustomerWithCustomers
AS
SELECT  
	t1.ShopifyCustomerId, 
	t1.ShopifyPrimaryEmail,
	t1.NeedsCustomerPut,
	t1.LastUpdated AS ShopifyLastUpdated,
	t2.AcumaticaMainContactEmail,
	t2.LastUpdated AS AcumaticaLastUpdated
FROM ShopifyCustomer t1
	FULL OUTER JOIN AcumaticaCustomer t2
		ON t1.Id = t2.ShopifyCustomerMonsterId
GO



DROP VIEW IF EXISTS vw_SyncOrdersAndSalesOrders
GO
CREATE VIEW vw_SyncOrdersAndSalesOrders
AS
	SELECT t1.ShopifyOrderId, 
		t1.ShopifyOrderNumber, 
		t1.ShopifyIsCancelled, 
		t1.ShopifyFinancialStatus,
		t1.NeedsOrderPut,
		t1.NeedsTransactionGet,
		t2.AcumaticaOrderNbr,
		t2.AcumaticaStatus,
		t3.AcumaticaInvoiceNbr,
		t3.AcumaticaShipmentNbr,
		t1.LastUpdated AS ShopifyLastUpdated,
		t2.LastUpdated AS AcumaticaLastUpdated
	FROM ShopifyOrder t1
		FULL OUTER JOIN AcumaticaSalesOrder t2
			ON t2.ShopifyOrderMonsterId = t1.Id
		FULL OUTER JOIN AcumaticaSoShipment t3
			ON t2.ShopifyOrderMonsterId = t3.ShopifyOrderMonsterId
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
	t2.AcumaticaShipmentNbr,
	t2.AcumaticaInvoiceNbr,
	t1.LastUpdated AS FulfillmentLastUpdated
FROM ShopifyOrder t0
	FULL OUTER JOIN ShopifyFulfillment t1
		ON t0.Id = t1.OrderMonsterId
	FULL OUTER JOIN AcumaticaSoShipment t2
		ON t1.Id = t2.ShopifyFulfillmentMonsterId
GO



DROP VIEW IF EXISTS vw_SyncTransactionAndPayment
GO
CREATE VIEW vw_SyncTransactionAndPayment
AS
SELECT 
	t1.ShopifyOrderId,
	t1.ShopifyOrderNumber,
	t1.NeedsTransactionGet,
	t2.ShopifyTransactionId,
	t2.ShopifyStatus,
	t2.ShopifyKind,
	t2.Ignore,
	t2.NeedsPaymentPut,
	t3.AcumaticaRefNbr,
	t3.AcumaticaDocType,
	t3.IsReleased,
	t2.LastUpdated AS ShopifyRefundLastUpdated,
	t3.LastUpdated AS PaymentSyncLastUpdated
FROM ShopifyOrder t1
	FULL OUTER JOIN ShopifyTransaction t2
		ON t1.Id = t2.OrderMonsterId
	FULL OUTER JOIN AcumaticaPayment t3
		ON t2.Id = t3.ShopifyTransactionMonsterId
GO



