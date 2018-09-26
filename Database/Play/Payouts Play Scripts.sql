USE Monster;

/*
DELETE FROM usrShopifyPayoutTransaction;
DELETE FROM usrShopifyPayout;
*/

SELECT * FROM usrShopifyPayout

/*
UPDATE usrShopifyPayout 
SET AcumaticaHeaderId = null, 
	AcumaticaRefNumber = null, 
	AcumaticaImportDate = null;

UPDATE usrShopifyPayoutTransaction 
SET AcumaticaRecordId = null,
	AcumaticaImportDate = null;
*/
	
SELECT * FROM usrShopifyPayout;

SELECT * FROM usrShopifyPayoutTransaction WHERE Json LIKE '%dispute%';




SELECT name, recovery_model_desc  
   FROM sys.databases  
      WHERE name = 'Monster' ;  
GO

USE master ;  
ALTER DATABASE Monster SET RECOVERY SIMPLE ; 


SELECT * FROM usrPayoutPreferences;


