USE Monster0001;
GO

-- Identifies 

DROP VIEW vw_AcumaticaUnsyncedShipments
GO

CREATE VIEW vw_AcumaticaUnsyncedShipments
AS

SELECT t1.* 
FROM usrAcumaticaShipmentSo t1
	LEFT OUTER JOIN usrAcumaticaSalesOrder t2
		ON t1.AcumaticaOrderNbr = t2.AcumaticaOrderNbr
	LEFT OUTER JOIN usrShopAcuOrderSync t3
		ON t2.Id = t3.AcumaticaSalesOrderMonsterId
WHERE t3.AcumaticaSalesOrderMonsterId IS NOT NULL;
GO


SELECT * FROM vw_AcumaticaUnsyncedShipments


