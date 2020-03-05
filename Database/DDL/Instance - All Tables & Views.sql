USE [Monster0001]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopifyVariant]') AND type in (N'U'))
ALTER TABLE [dbo].[ShopifyVariant] DROP CONSTRAINT IF EXISTS [FK_usrShopifyVariant_usrShopifyProduct]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopifyTransaction]') AND type in (N'U'))
ALTER TABLE [dbo].[ShopifyTransaction] DROP CONSTRAINT IF EXISTS [FK_usrShopifyTransaction_usrShopifyOrder]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopifyRefund]') AND type in (N'U'))
ALTER TABLE [dbo].[ShopifyRefund] DROP CONSTRAINT IF EXISTS [FK_usrShopifyRefund_usrShopifyOrder]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopifyPayoutTransaction]') AND type in (N'U'))
ALTER TABLE [dbo].[ShopifyPayoutTransaction] DROP CONSTRAINT IF EXISTS [FK_usrShopifyPayoutTransaction_usrShopifyPayout]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopifyOrder]') AND type in (N'U'))
ALTER TABLE [dbo].[ShopifyOrder] DROP CONSTRAINT IF EXISTS [FK_usrShopifyOrder_usrShopifyCustomer]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopifyInventoryLevel]') AND type in (N'U'))
ALTER TABLE [dbo].[ShopifyInventoryLevel] DROP CONSTRAINT IF EXISTS [FK_usrShopifyInventoryLevels_usrShopifyVariant]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopifyInventoryLevel]') AND type in (N'U'))
ALTER TABLE [dbo].[ShopifyInventoryLevel] DROP CONSTRAINT IF EXISTS [FK_usrShopifyInventoryLevels_usrShopifyLocation]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopifyFulfillment]') AND type in (N'U'))
ALTER TABLE [dbo].[ShopifyFulfillment] DROP CONSTRAINT IF EXISTS [FK_usrShopifyFulfillment_usrShopifyOrder]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopAcuWarehouseSync]') AND type in (N'U'))
ALTER TABLE [dbo].[ShopAcuWarehouseSync] DROP CONSTRAINT IF EXISTS [FK_usrShopAcuWarehouseSync_usrShopifyLocation]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopAcuWarehouseSync]') AND type in (N'U'))
ALTER TABLE [dbo].[ShopAcuWarehouseSync] DROP CONSTRAINT IF EXISTS [FK_usrShopAcuWarehouseSync_usrAcumaticaWarehouse]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InventoryReceiptSync]') AND type in (N'U'))
ALTER TABLE [dbo].[InventoryReceiptSync] DROP CONSTRAINT IF EXISTS [FK_usrInventoryReceiptSync_usrShopifyInventoryLevels]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InventoryReceiptSync]') AND type in (N'U'))
ALTER TABLE [dbo].[InventoryReceiptSync] DROP CONSTRAINT IF EXISTS [FK_usrInventoryReceiptSync_usrAcumaticaInventoryReceipt]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AcumaticaStockItem]') AND type in (N'U'))
ALTER TABLE [dbo].[AcumaticaStockItem] DROP CONSTRAINT IF EXISTS [FK_AcumaticaStockItem_ShopifyVariant]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AcumaticaSoShipment]') AND type in (N'U'))
ALTER TABLE [dbo].[AcumaticaSoShipment] DROP CONSTRAINT IF EXISTS [FK_AcumaticaSoShipment_ShopifyFulfillment]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AcumaticaSoShipment]') AND type in (N'U'))
ALTER TABLE [dbo].[AcumaticaSoShipment] DROP CONSTRAINT IF EXISTS [FK_AcumaticaSoShipment_AcumaticaSalesOrder]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AcumaticaSalesOrder]') AND type in (N'U'))
ALTER TABLE [dbo].[AcumaticaSalesOrder] DROP CONSTRAINT IF EXISTS [FK_AcumaticaSalesOrder_ShopifyOrder]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AcumaticaSalesOrder]') AND type in (N'U'))
ALTER TABLE [dbo].[AcumaticaSalesOrder] DROP CONSTRAINT IF EXISTS [FK_AcumaticaSalesOrder_AcumaticaCustomer]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AcumaticaPayment]') AND type in (N'U'))
ALTER TABLE [dbo].[AcumaticaPayment] DROP CONSTRAINT IF EXISTS [FK_usrShopifyAcuPayment_usrShopifyTransaction]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AcumaticaMemo]') AND type in (N'U'))
ALTER TABLE [dbo].[AcumaticaMemo] DROP CONSTRAINT IF EXISTS [FK_AcumaticaMemo_ShopifyRefund]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AcumaticaInventory]') AND type in (N'U'))
ALTER TABLE [dbo].[AcumaticaInventory] DROP CONSTRAINT IF EXISTS [FK_usrAcumaticaWarehouseDetails_usrAcumaticaWarehouse]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AcumaticaInventory]') AND type in (N'U'))
ALTER TABLE [dbo].[AcumaticaInventory] DROP CONSTRAINT IF EXISTS [FK_usrAcumaticaWarehouseDetails_usrAcumaticaStockItem]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AcumaticaCustomer]') AND type in (N'U'))
ALTER TABLE [dbo].[AcumaticaCustomer] DROP CONSTRAINT IF EXISTS [FK_AcumaticaCustomer_ShopifyCustomer]
GO
/****** Object:  Table [dbo].[SystemState]    Script Date: 3/2/2020 6:30:11 PM ******/
DROP TABLE IF EXISTS [dbo].[SystemState]
GO
/****** Object:  Table [dbo].[ShopifyVariant]    Script Date: 3/2/2020 6:30:11 PM ******/
DROP TABLE IF EXISTS [dbo].[ShopifyVariant]
GO
/****** Object:  Table [dbo].[ShopifyProduct]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[ShopifyProduct]
GO
/****** Object:  Table [dbo].[ShopifyPayoutTransaction]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[ShopifyPayoutTransaction]
GO
/****** Object:  Table [dbo].[ShopifyPayout]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[ShopifyPayout]
GO
/****** Object:  Table [dbo].[ShopifyLocation]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[ShopifyLocation]
GO
/****** Object:  Table [dbo].[ShopifyJsonStore]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[ShopifyJsonStore]
GO
/****** Object:  Table [dbo].[ShopifyInventoryLevel]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[ShopifyInventoryLevel]
GO
/****** Object:  Table [dbo].[ShopifyCustomer]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[ShopifyCustomer]
GO
/****** Object:  Table [dbo].[ShopifyBatchState]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[ShopifyBatchState]
GO
/****** Object:  Table [dbo].[ShopAcuWarehouseSync]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[ShopAcuWarehouseSync]
GO
/****** Object:  Table [dbo].[PayoutPreferences]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[PayoutPreferences]
GO
/****** Object:  Table [dbo].[PaymentGateways]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[PaymentGateways]
GO
/****** Object:  Table [dbo].[MonsterSettings]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[MonsterSettings]
GO
/****** Object:  Table [dbo].[InventoryReceiptSync]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[InventoryReceiptSync]
GO
/****** Object:  Table [dbo].[ExecutionLog]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[ExecutionLog]
GO
/****** Object:  Table [dbo].[ExclusiveJobMonitor]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[ExclusiveJobMonitor]
GO
/****** Object:  Table [dbo].[Connections]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[Connections]
GO
/****** Object:  Table [dbo].[AcumaticaWarehouse]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[AcumaticaWarehouse]
GO
/****** Object:  Table [dbo].[AcumaticaStockItem]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[AcumaticaStockItem]
GO
/****** Object:  Table [dbo].[AcumaticaRefData]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[AcumaticaRefData]
GO
/****** Object:  Table [dbo].[AcumaticaJsonStore]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[AcumaticaJsonStore]
GO
/****** Object:  Table [dbo].[AcumaticaInventoryReceipt]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[AcumaticaInventoryReceipt]
GO
/****** Object:  Table [dbo].[AcumaticaInventory]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[AcumaticaInventory]
GO
/****** Object:  Table [dbo].[AcumaticaCustomer]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[AcumaticaCustomer]
GO
/****** Object:  Table [dbo].[AcumaticaBatchState]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[AcumaticaBatchState]
GO
/****** Object:  View [dbo].[ShopifyOrdersNotNeedingSyncAll]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP VIEW IF EXISTS [dbo].[ShopifyOrdersNotNeedingSyncAll]
GO
/****** Object:  View [dbo].[ShopifyOrdersNeedingSyncAll]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP VIEW IF EXISTS [dbo].[ShopifyOrdersNeedingSyncAll]
GO
/****** Object:  View [dbo].[ShopifyOrdersNeedingOriginalPaymentUpdate]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP VIEW IF EXISTS [dbo].[ShopifyOrdersNeedingOriginalPaymentUpdate]
GO
/****** Object:  View [dbo].[ShopifyOrdersNeedingOrderCreate]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP VIEW IF EXISTS [dbo].[ShopifyOrdersNeedingOrderCreate]
GO
/****** Object:  View [dbo].[ShopifyOrderNeedingSoShipmentsSync]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP VIEW IF EXISTS [dbo].[ShopifyOrderNeedingSoShipmentsSync]
GO
/****** Object:  View [dbo].[ShopifyOrderSoShipmentsSyncStatus]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP VIEW IF EXISTS [dbo].[ShopifyOrderSoShipmentsSyncStatus]
GO
/****** Object:  Table [dbo].[AcumaticaSoShipment]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[AcumaticaSoShipment]
GO
/****** Object:  Table [dbo].[ShopifyFulfillment]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[ShopifyFulfillment]
GO
/****** Object:  View [dbo].[ShopifyOrderNeedingRefundSync]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP VIEW IF EXISTS [dbo].[ShopifyOrderNeedingRefundSync]
GO
/****** Object:  View [dbo].[ShopifyOrdersNeedingPaymentSync]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP VIEW IF EXISTS [dbo].[ShopifyOrdersNeedingPaymentSync]
GO
/****** Object:  View [dbo].[ShopifyOrderRefundsSyncStatus]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP VIEW IF EXISTS [dbo].[ShopifyOrderRefundsSyncStatus]
GO
/****** Object:  Table [dbo].[ShopifyRefund]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[ShopifyRefund]
GO
/****** Object:  Table [dbo].[AcumaticaMemo]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[AcumaticaMemo]
GO
/****** Object:  View [dbo].[ShopifyOrderPaymentsSyncStatus]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP VIEW IF EXISTS [dbo].[ShopifyOrderPaymentsSyncStatus]
GO
/****** Object:  Table [dbo].[AcumaticaPayment]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[AcumaticaPayment]
GO
/****** Object:  Table [dbo].[ShopifyTransaction]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[ShopifyTransaction]
GO
/****** Object:  View [dbo].[ShopifyOrdersNeedingOrderUpdate]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP VIEW IF EXISTS [dbo].[ShopifyOrdersNeedingOrderUpdate]
GO
/****** Object:  View [dbo].[ShopifyOrdersOnlySyncView]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP VIEW IF EXISTS [dbo].[ShopifyOrdersOnlySyncView]
GO
/****** Object:  Table [dbo].[ShopifyOrder]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[ShopifyOrder]
GO
/****** Object:  Table [dbo].[AcumaticaSalesOrder]    Script Date: 3/2/2020 6:30:12 PM ******/
DROP TABLE IF EXISTS [dbo].[AcumaticaSalesOrder]
GO
