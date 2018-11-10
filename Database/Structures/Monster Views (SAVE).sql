USE Monster0001;
GO

DROP VIEW vw_AcumaticaUnsyncedShipments
GO

CREATE VIEW vw_AcumaticaUnsyncedShipments
AS

SELECT t1.* 
FROM usrAcumaticaShipment t1
	LEFT OUTER JOIN usrAcumaticaSOShipment t2
		ON t1.Id = t2.AcumaticaShipmentId
	LEFT OUTER JOIN usrAcumaticaSalesOrder t3
		ON t2.AcumaticaSalesOrderId = t3.Id
WHERE t3.ShopifyOrderMonsterId IS NOT NULL
AND t1.ShopifyFulfillmentMonsterId IS NULL
GO

		
SELECT * FROM vw_AcumaticaUnsyncedShipments

SELECT * FROM usrAcumaticaShipment;
