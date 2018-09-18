USE Monster;

/*
DELETE FROM usrShopifyPayout;
DELETE FROM usrShopifyPayoutTransaction;
*/

SELECT * FROM usrShopifyPayout

UPDATE usrShopifyPayout 
SET AcumaticaHeaderId = null, 
	AcumaticaRefNumber = null, 
	AcumaticaImportDate = null;

UPDATE usrShopifyPayoutTransaction 
SET AcumaticaRecordId = null,
	AcumaticaImportDate = null;

	
SELECT * FROM usrShopifyPayout;

SELECT * FROM usrShopifyPayoutTransaction;

SELECT * FROM usrPayoutPreferences;


