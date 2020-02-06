USE Monster0001;
GO

-- Warehouse and Location synchronization
--
SELECT * FROM vw_SyncWarehousesAndLocations;

-- Inventory synchronization - ALL
--
SELECT * FROM vw_SyncWarehousesAndLocations;
SELECT * FROM vw_ShopifyInventory;
SELECT * FROM vw_AcumaticaInventory;
SELECT * FROM vw_SyncVariantsAndStockItems;
SELECT * FROM vw_SyncInventoryLevelAndReceipts;
SELECT * FROM vw_SyncInventoryAllInclusive;		-- Identifies unmatched Stock Items-Variants

-- Order synchronization - Shopify
--
SELECT * FROM vw_ShopifyOrderCustomer;
SELECT * FROM vw_ShopifyOrderRefunds;
SELECT * FROM vw_ShopifyOrderFulfillments;
SELECT * FROM vw_ShopifyOrderTransactions;

-- Order synchronization - Acumatica
--
SELECT * FROM vw_AcumaticaSalesOrderAndCustomer;
SELECT * FROM vw_AcumaticaSalesOrderAndShipmentInvoices;
SELECT * FROM vw_AcumaticaSalesOrderAndShipments;	-- Obviously, you guys need to be tuned!

-- Order synchronization - ALL
--
SELECT * FROM vw_SyncCustomerWithCustomers;
SELECT * FROM vw_SyncOrdersAndSalesOrders;		-- Identifies unsynced Orders-Sales Orders
SELECT * FROM vw_SyncFulfillmentsAndShipments;	-- Bi-directional view of Fulfillments and Shipments
SELECT * FROM vw_SyncRefundAndCreditMemo;		-- Shows Orders with/out Refunds, and with/wout Credit Memo sync
SELECT * FROM vw_SyncTransactionAndPayment;		-- Shows Transactions with/out Payment syncs


