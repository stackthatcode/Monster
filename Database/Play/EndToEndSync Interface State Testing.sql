
USE Monster0001;
GO

SELECT * FROM SystemState;
SELECT * FROM ExclusiveJobMonitor;
SELECT * FROM Preferences;


SELECT	ShopifyOrderId, ShopifyOrderName, ShopifyOrderCreatedAtUtc, MaxParallelAcumaticaSyncs
FROM Preferences;



-- Enable End-to-End start
--
UPDATE SystemState SET ShopifyConnState = 2;
UPDATE SystemState SET StartingShopifyOrderState = 2;

UPDATE Preferences 
SET ShopifyOrderId = 1778846826540,
ShopifyOrderName = 'LOGIC-1039',
ShopifyOrderCreatedAtUtc = '2019-10-22T23:06:30Z',
MaxParallelAcumaticaSyncs = 4;



-- Kill the Exclusive Job
--
DELETE FROM ExclusiveJobMonitor;



-- Disable the Config and Starting Order State
--
UPDATE SystemState SET ShopifyConnState = 1;
UPDATE SystemState SET ShopifyOrderState = 1;

UPDATE Preferences 
SET ShopifyOrderId = NULL,
ShopifyOrderName = NULL,
ShopifyOrderCreatedAtUtc = NULL,
MaxParallelAcumaticaSyncs = 1;


EXEC dbo.ResetStartingShopifyOrder;





