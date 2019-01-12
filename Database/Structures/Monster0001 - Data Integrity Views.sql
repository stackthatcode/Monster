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
		t1.AcumaticaReleasedInvoiceNbr,
		t2.AcumaticaOrderNbr,
		t1.LastUpdated AS ShipmentLastUpdated,
		t2.LastUpdated AS SalesOrderRefLastUpdated
	FROM usrAcumaticaShipment t1
		LEFT OUTER JOIN usrAcumaticaShipmentSalesOrderRef t2
			ON t1.Id = t2.ShipmentMonsterId
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

