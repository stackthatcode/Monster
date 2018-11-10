
USE Monster0001;
GO


-- Clear out Monster

-- Synchronization
DELETE FROM usrShopAcuShipmentSync;
DELETE FROM usrShopAcuOrderSync;
DELETE FROM usrShopAcuCustomerSync;
DELETE FROM usrInventoryReceiptSync;
DELETE FROM usrShopAcuItemSync;
DELETE FROM usrShopAcuWarehouseSync;


-- Shopify
DELETE FROM usrAcumaticaShipment;
DELETE FROM usrAcumaticaSalesOrder;
DELETE FROM usrAcumaticaCustomer;

DELETE FROM usrAcumaticaInventoryReceipt;
DELETE FROM usrAcumaticaWarehouseDetails;
DELETE FROM usrAcumaticaStockItem;
DELETE FROM usrAcumaticaWarehouse;


-- Acumatica
DELETE FROM usrShopifyFulfillment;
DELETE FROM usrShopifyOrder;
DELETE FROM usrShopifyCustomer;

DELETE FROM usrShopifyInventoryLevels;
DELETE FROM usrShopifyVariant;
DELETE FROM usrShopifyProduct;
DELETE FROM usrShopifyLocation;



ALTER DATABASE AcuInst2 SET SINGLE_USER WITH ROLLBACK IMMEDIATE 
ALTER DATABASE AcuInst2 SET MULTI_USER

-- TODO - DELETE DATABASE

-- TODO - RESTORE DATABASE FROM FILE

