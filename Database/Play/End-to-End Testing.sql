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



/*
UPDATE ShopifyOrder SET PutErrorCount = 1 WHERE ShopifyOrderId = 1886864834604
UPDATE ShopifyTransaction SET PutErrorCount = 1 WHERE ShopifyTransactionId = 2427856748588
UPDATE AcumaticaSoShipment SET PutErrorCount = 0;
*/

SELECT Id, ShopifyTransactionId, ShopifyOrderId, ShopifyStatus, ShopifyKind, NeedsPaymentPut, PutErrorCount
FROM ShopifyTransaction 
WHERE Id IN (SELECT ShopifyTransactionMonsterId FROM AcumaticaPayment WHERE AcumaticaRefNbr = 'UNKNOWN');

UPDATE ShopifyTransaction SET PutErrorCount = 0 WHERE ShopifyTransactionId = 2427870183468;



