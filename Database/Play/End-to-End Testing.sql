USE Monster0001;


/*
EXEC dbo.DeleteAllSyncRecords;
EXEC dbo.DeleteAllAcumaticaOrderRecords;
EXEC dbo.DeleteAllAcumaticaInventoryRecords;
EXEC dbo.DeleteAllShopifyOrderRecords;
EXEC dbo.DeleteAllShopifyInventoryRecords;
EXEC dbo.DeleteBatchStateOnly;
EXEC dbo.DeleteAllSystemRecords;
EXEC dbo.ResetStartingShopifyOrder;
*/

USE MonsterSys;
SELECT * FROM AspNetUsers;


USE Monster0001;
GO

SELECT * FROM vw_SyncWarehousesAndLocations
SELECT * FROM vw_ShopifyInventory 
SELECT * FROM vw_AcumaticaInventory 
SELECT * FROM vw_SyncVariantsAndStockItems
SELECT * FROM vw_SyncVariantsAndStockItems_Alt
SELECT * FROM vw_SyncAcumaticaInventory
SELECT * FROM vw_SyncShopifyInventory
SELECT * FROM vw_SyncInventoryAllInclusive
SELECT * FROM vw_SyncInventoryLevelAndReceipts

SELECT * FROM vw_ShopifyOrderCustomer 
SELECT * FROM vw_ShopifyOrderRefunds
SELECT * FROM vw_ShopifyOrderFulfillments
SELECT * FROM vw_ShopifyOrderTransactions
SELECT * FROM vw_AcumaticaSalesOrderAndCustomer
SELECT * FROM vw_AcumaticaSalesOrderAndShipmentInvoices
SELECT * FROM vw_SyncCustomerWithCustomers
SELECT * FROM vw_SyncOrdersAndSalesOrders
SELECT * FROM vw_SyncFulfillmentsAndShipments
SELECT * FROM vw_SyncTransactionAndPayment



SELECT * FROM ShopifyOrder;
SELECT * FROM ShopifyTransaction;
SELECT * FROM AcumaticaSalesOrder;
SELECT * FROM AcumaticaPayment;
SELECT * FROM AcumaticaSoShipment;

/*
UPDATE ShopifyOrder SET PutErrorCount = 1 WHERE ShopifyOrderId = 1886864834604
UPDATE ShopifyTransaction SET PutErrorCount = 1 WHERE ShopifyTransactionId = 2427856748588
UPDATE AcumaticaSoShipment SET PutErrorCount = 0;
*/

SELECT ShopifyOrderId, ShopifyOrderNumber, NeedsOrderPut, PutErrorCount FROM ShopifyORder;
SELECT ShopifyTransactionId, ShopifyOrderId, ShopifySTatus, ShopifyKind, NeedsPaymentPut, PutErrorCount FROM ShopifyTransaction;

SELECT AcumaticaShipmentNbr, AcumaticaInvoiceNbr, AcumaticaInvoiceType, PutErrorCount FROM AcumaticaSoShipment;

SELECT ShopifyOrderId, ShopifyOrderNumber, NeedsOrderPut, PutErrorCount 
FROM ShopifyORder WHERE ShopifyOrderId = 1886864834604;

SELECT * FROM AcumaticaSalesOrder;


SELECT Id, ShopifyTransactionId, ShopifyOrderId, ShopifyStatus, ShopifyKind, NeedsPaymentPut, PutErrorCount
FROM ShopifyTransaction 
WHERE Id IN (SELECT ShopifyTransactionMonsterId FROM AcumaticaPayment WHERE AcumaticaRefNbr = 'UNKNOWN');

UPDATE ShopifyTransaction SET PutErrorCount = 0 WHERE ShopifyTransactionId = 2427870183468;


