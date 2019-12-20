USE Monster0001;
GO

/*
EXEC dbo.DeleteAllSyncRecords;
EXEC dbo.DeleteAllAcumaticaOrderRecords;
EXEC dbo.DeleteAllAcumaticaInventoryRecords;
EXEC dbo.DeleteAllShopifyOrderRecords;
EXEC dbo.DeleteAllShopifyInventoryRecords;
EXEC dbo.DeleteBatchStateOnly;

EXEC dbo.DeleteAllSystemRecords;
EXEC dbo.ResetStartingShopifyOrder;
*/

/*
SELECT * FROM Monster0001..ShopifyOrder;
SELECT * FROM MonsterSys..AspNetUsers;
SELECT * FROM MonsterSys..AspNetUserLogins;
SELECT * FROM MonsterSys..Instance;
*/

-- UPDATE MonsterSys..AspNetUserLogins SET ProviderKey = 'onemoreteststorecanthurt.myshopify.com';

USE MonsterSys;

SELECT * FROM HangFire.Hash;

USE Monster0001;
GO

DELETE FROM ExclusiveJobMonitor;

SELECT * FROM SystemState;

UPDATE SystemState SET SettingsState = 2;


UPDATE MonsterSettings SET LastRecurringFrequency = 1;

