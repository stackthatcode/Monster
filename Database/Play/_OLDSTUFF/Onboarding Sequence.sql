USE Monster0001;



DELETE FROM usrPreferences;

-- TODO - replace with interactive management
DECLARE @StartingShopifyOrder int = 1035;
DECLARE @FulfillInAcumatica bit = 1;

INSERT INTO usrPreferences VALUES (
	'2018-12-06', 
	00.20, 
	@StartingShopifyOrder, 
	'Central Standard Time', 
	'NSTOCKITEM', 
	'SHOPIFY', 
	'ONLINE', 
	'102050', 
	'ONLINE', 
	'EXEMPT', 
	'ONLINE', 
	@FulfillInAcumatica );



SELECT * FROM usrTenant;

SELECT * FROM usrSystemState;

UPDATE usrSystemState SET IsRandomAccessMode = 1;

UPDATE usrSystemState SET ShopifyConnection = 2;
UPDATE usrSystemState SET AcumaticaConnection = 2;
UPDATE usrSystemState SET AcumaticaReferenceData = 2;
UPDATE usrSystemState SET PreferenceSelections = 2;
UPDATE usrSystemState SET WarehouseSync = 2;
UPDATE usrSystemState SET AcumaticaInventoryPush = 2;
UPDATE usrSystemState SET ShopifyInventoryPush = 2;


DELETE FROM usrBackgroundJob

UPDATE usrSystemState SET IsShopifyUrlFinalized = 1;
UPDATE usrSystemState SET IsAcumaticaUrlFinalized = 1;


SELECT * FROM usrSystemState;

SELECT * FROM usrBackgroundJob

SELECT * FROM usrAcumaticaReferences;

SELECT * FROM usrTenant;



