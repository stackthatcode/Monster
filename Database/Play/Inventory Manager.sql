USE Monster0001;

/**
DELETE FROM usrPreferences;

INSERT INTO usrPreferences 
VALUES ( '2018-01-01', '2018-01-01', 00.20, 'STOCKITEM', 'SHOPIFY', '5/24/2014'  );
SELECT * FROM usrPreferences;
**/


/*
DELETE FROM usrShopifyInventoryLevels;

DELETE FROM usrAcumaticaInventoryReceipt;
DELETE FROM usrAcumaticaWarehouseDetails;
DELETE FROM usrAcumaticaStockItem;
DELETE FROM usrAcumaticaWarehouse;

DELETE FROM usrShopifyVariant;
DELETE FROM usrShopifyProduct;
DELETE FROM usrShopifyLocation;

*/


SELECT * FROM usrShopifyProduct;
SELECT * FROM usrShopifyVariant;	 --WHERE ShopifyInventoryItemId = 13936416260194;
SELECT * FROM usrShopifyInventoryLevels;
SELECT * FROM usrShopifyLocation;

SELECT * FROM usrAcumaticaWarehouse;
SELECT * FROM usrAcumaticaStockItem;
SELECT * FROM usrAcumaticaWarehouseDetails;
SELECT * FROM usrAcumaticaInventoryReceipt;

SELECT * FROM usrPreferences;
SELECT * FROM usrBatchState;

