USE Monster0001;

SELECT * FROM usrTenant;

SELECT * FROM usrSystemState;

UPDATE usrSystemState SET IsShopifyUrlFinalized = 1;
UPDATE usrSystemState SET IsAcumaticaUrlFinalized = 0;
UPDATE usrSystemState SET IsRandomAccessMode = 0;
UPDATE usrSystemState SET ShopifyConnection = 1;
UPDATE usrSystemState SET AcumaticaConnection = 1;

SELECT * FROM usrBackgroundJob

UPDATE usrSystemState SET AcumaticaConnection = 1;
DELETE FROM usrBackgroundJob


