USE Monster0001;
GO


DROP VIEW vw_AcumaticaUnsyncedShipmentIds
GO

CREATE VIEW vw_AcumaticaUnsyncedShipmentIds
AS

SELECT DISTINCT(t1.ShipmentMonsterId)
FROM usrAcumaticaShipmentSo t1
	LEFT OUTER JOIN usrShopAcuShipmentSync t2
		ON t1.Id = t2.AcumaticaShipmentSOMonsterId
WHERE t2.Id IS NULL
GO



SELECT * FROM vw_AcumaticaUnsyncedShipmentIds


