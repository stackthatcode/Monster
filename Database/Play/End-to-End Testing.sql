

SELECT * FROM SystemState;
UPDATE SystemState SET InventoryRefreshState = 1;

SELECT * FROM AcumaticaBatchState;
DELETE AcumaticaBatchState;

/*
EXEC dbo.DeleteAllSyncRecords;
EXEC dbo.DeleteAllAcumaticaOrderRecords;
EXEC dbo.DeleteAllAcumaticaInventoryRecords;
EXEC dbo.DeleteAllShopifyOrderRecords;
EXEC dbo.DeleteAllShopifyInventoryRecords;
EXEC dbo.DeleteAllSystemRecords;
*/

/*EXEC dbo.ResetStartingShopifyOrder*/


UPDATE AcumaticaSoShipment SET NeedShipmentGet = 1;

SELECT * FROM ShopifyFulfillment;

SELECT * FROM AcumaticaSoShipment;

UPDATE AcumaticaSoShipment SET ShopifyFulfillmentMonsterId = 2
WHERE ID = 2;

SELECT * FROM AcumaticaInventory;

