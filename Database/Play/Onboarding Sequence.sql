USE Monster0001;

SELECT * FROM usrTenant;

SELECT * FROM usrSystemState;

UPDATE usrSystemState SET IsShopifyUrlFinalized = 1;
UPDATE usrSystemState SET IsRandomAccessMode = 1;
UPDATE usrSystemState SET ShopifyConnection = 3;

