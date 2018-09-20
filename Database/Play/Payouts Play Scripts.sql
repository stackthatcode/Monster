USE Monster;

/*
DELETE FROM usrShopifyPayout;
DELETE FROM usrShopifyPayoutTransaction;
*/
SELECT table_schema AS "Database", ROUND(SUM(data_length + index_length) / 1024 / 1024, 2) AS "Size (MB)" FROM information_schema.TABLES GROUP BY table_schema;

/*
UPDATE usrShopifyPayout 
SET AcumaticaRefNumber = null, 
	AcumaticaImportDate = null;

UPDATE usrShopifyPayoutTransaction 
SET AcumaticaRecordId = null,
	AcumaticaImportDate = null;
*/

	
SELECT * FROM usrShopifyPayout;

SELECT * FROM usrShopifyPayoutTransaction
WHERE ShopifyPayoutTransId = 219876524132;


SELECT * FROM usrPayoutPreferences;

UPDATE usrShopifyPayoutTransaction
SET AcumaticaImportDate = NULL,
AcumaticaExtRefNrb = NULL 
WHERE AcumaticaExtRefNrb IN (
	'Shopify Payout Trans Id: 219121582180',
	'Shopify Payout Trans Id: 219123155044',
	'Shopify Payout Trans Id: 219146682468',
	'Shopify Payout Trans Id: 219161591908'
);




