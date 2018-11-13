USE Monster0001;
GO

DROP VIEW vw_AcumaticaUnsyncedShipments
GO

CREATE VIEW vw_AcumaticaUnsyncedShipments
AS

SELECT t1.* 
FROM usrAcumaticaShipment t1
	LEFT OUTER JOIN usrAcumaticaSalesOrder t2
		ON t1.AcumaticaSalesOrderId = t2.AcumaticaSalesOrderId

WHERE t3.ShopifyOrderMonsterId IS NOT NULL
AND t1.ShopifyFulfillmentMonsterId IS NULL
GO

		
SELECT * FROM vw_AcumaticaUnsyncedShipments

SELECT * FROM usrAcumaticaShipment;
