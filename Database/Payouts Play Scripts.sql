
--DELETE FROM usrShopifyPayout;
--DELETE FROM usrShopifyPayoutTransaction;


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


