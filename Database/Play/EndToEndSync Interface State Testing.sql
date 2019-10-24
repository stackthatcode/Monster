
USE Monster0001;
GO

SELECT * FROM SystemState;

SELECT * FROM ExclusiveJobMonitor;

SELECT * FROM Preferences;

SELECT	StartingShopifyOrderId, StartingShopifyOrderName, StartingShopifyOrderCreatedAtUtc, 
		MaxParallelAcumaticaSyncs
FROM Preferences;



-- Enable End-to-End start
--
UPDATE SystemState SET ShopifyConnState = 2;
UPDATE SystemState SET StartingShopifyOrderState = 2;

UPDATE Preferences 
SET StartingShopifyOrderId = 1778846826540,
StartingShopifyOrderName = 'LOGIC-1039',
StartingShopifyOrderCreatedAtUtc = '2019-10-22T23:06:30Z',
MaxParallelAcumaticaSyncs = 4;



-- Kill the Exclusive Job
--
DELETE FROM ExclusiveJobMonitor;



-- Disable the Config and Starting Order State
--
UPDATE SystemState SET ShopifyConnState = 1;
UPDATE SystemState SET StartingShopifyOrderState = 1;

UPDATE Preferences 
SET StartingShopifyOrderId = NULL,
StartingShopifyOrderName = NULL,
StartingShopifyOrderCreatedAtUtc = NULL,
MaxParallelAcumaticaSyncs = 4;



