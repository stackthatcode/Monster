
USE Monster0001;
GO

EXEC dbo.DeleteAllAcumaticaOrderRecords

SELECT * FROM Connections;

UPDATE Connections SET AcumaticaInstanceUrl = 'http://localhost/Acu19R100021';

EXEC ResetStartingShopifyOrder;



