
USE Monster0001;
GO


-- Clear out Monster
DELETE FROM usrAcumaticaSOShipment;
DELETE FROM usrAcumaticaShipment;
DELETE FROM usrAcumaticaSalesOrder;
DELETE FROM usrAcumaticaCustomer;

DELETE FROM usrShopifyFulfillment;
DELETE FROM usrShopifyOrderLineItem;
DELETE FROM usrShopifyOrder;
DELETE FROM usrShopifyCustomer;

DELETE FROM usrShopifyInventoryLevels;

DELETE FROM usrAcumaticaInventoryReceipt;
DELETE FROM usrAcumaticaWarehouseDetails;
DELETE FROM usrAcumaticaStockItem;
DELETE FROM usrAcumaticaWarehouse;

DELETE FROM usrShopifyVariant;
DELETE FROM usrShopifyProduct;
DELETE FROM usrShopifyLocation;


ALTER DATABASE AcuInst2 SET SINGLE_USER WITH ROLLBACK IMMEDIATE 
ALTER DATABASE AcuInst2 SET MULTI_USER

-- TODO - DELETE DATABASE

-- TODO - RESTORE DATABASE FROM FILE

