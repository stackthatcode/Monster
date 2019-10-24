USE Monster0001;
GO



-- Clean Synchronization data
--
DROP PROCEDURE IF EXISTS dbo.ResetStartingShopifyOrder
GO

CREATE PROCEDURE dbo.ResetStartingShopifyOrder
AS
	UPDATE Preferences 
		SET ShopifyOrderId = NULL,
		ShopifyOrderName = NULL,
		ShopifyOrderCreatedAtUtc = NULL

	DELETE FROM AcumaticaBatchState;
	DELETE FROM ShopifyBatchState;
GO


