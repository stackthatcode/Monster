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


