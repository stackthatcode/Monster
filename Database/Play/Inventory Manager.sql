USE Monster0001;

/**
DELETE FROM usrPreferences;

INSERT INTO usrPreferences 
VALUES ( '2018-01-01', '2018-01-01', 00.20, 'STOCKITEM', 'SHOPIFY', '5/24/2014'  );
SELECT * FROM usrPreferences;
**/

SELECT * FROM usrPreferences;
SELECT * FROM usrBatchState;


-- Warehouses, Products, Variant, Inventory

SELECT * FROM usrShopifyProduct;
SELECT * FROM usrShopifyVariant;	 --WHERE ShopifyInventoryItemId = 13936416260194;
SELECT * FROM usrShopifyInventoryLevels;
SELECT * FROM usrShopifyLocation;

SELECT * FROM usrAcumaticaWarehouse;
SELECT * FROM usrAcumaticaStockItem;
SELECT * FROM usrAcumaticaWarehouseDetails;
SELECT * FROM usrAcumaticaInventoryReceipt;



-- Customers and Orders

SELECT * FROM usrShopifyCustomer;
SELECT * FROM usrAcumaticaCustomer;

SELECT * FROM usrShopifyOrder;
SELECT * FROM usrAcumaticaSalesOrder;

