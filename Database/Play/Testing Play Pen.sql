USE Monster0001;
GO


-- Testing Play Pen (!!!)

SELECT * FROM usrAcumaticaReferences;
SELECT * FROM usrPreferences;
SELECT * FROM usrShopifyOrder;

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
SELECT * FROM vw_SyncInventoryLevelAndReceipts;
SELECT * FROM vw_SyncInventoryAllInclusive;		-- Identifies unmatched Stock Items-Variants

SELECT * FROM vw_SyncCustomerWithCustomers;
SELECT * FROM vw_SyncOrdersAndSalesOrders;		-- Identifies unsynced Orders-Sales Orders
SELECT * FROM vw_SyncFulfillmentsAndShipments;	-- Bi-directional view of Fulfillments and Shipments
SELECT * FROM vw_SyncRefundAndCreditMemo;		-- Shows Orders with/out Refunds, and with/wout Credit Memo sync
SELECT * FROM vw_SyncTransactionAndPayment;		-- Shows Transactions with/out Payment syncs


SELECT * FROM usrShopifyBatchState;

DELETE FROM usrShopifyBatchState;

