USE Monster0001;

SELECT * FROM usrTenant;

SELECT * FROM usrSystemState;

UPDATE usrSystemState SET IsShopifyUrlFinalized = 0;
UPDATE usrSystemState SET ShopifyConnection = 1;

UPDATE usrSystemState SET IsAcumaticaUrlFinalized = 0;
UPDATE usrSystemState SET AcumaticaConnection = 1;

UPDATE usrSystemState SET IsRandomAccessMode = 0;


UPDATE usrSystemState SET AcumaticaConnection = 2;
DELETE FROM usrBackgroundJob


SELECT * FROM usrBackgroundJob

SELECT * FROM usrAcumaticaReferences;

SELECT * FROM usrTenant;


