USE Monster0001;
GO

-- Shopify Workers checks
--
DROP VIEW IF EXISTS vw_ShopifyInventory 
GO

CREATE VIEW vw_ShopifyInventory
AS
SELECT	t1.ShopifyProductId,
		t1.IsDeleted AS ProductIsDeleted, 
		t2.ShopifyVariantId, 
		t2.ShopifyInventoryItemId, 
		t2.ShopifySku, 
		t2.ShopifyCost, 
		t2.IsMissing AS VariantIsMissing,
		t3.ShopifyLocationId,
		t3.ShopifyAvailableQuantity
FROM usrShopifyProduct t1
	INNER JOIN usrShopifyVariant t2
		ON t2.ParentMonsterId = t1.MonsterId
	INNER JOIN usrShopifyInventoryLevel t3
		ON t3.ParentMonsterId = t2.MonsterId;
GO


SELECT * FROM vw_ShopifyInventory;

SELECT * FROM usrShopifyProduct 
SELECT * FROM usrShopifyVariant
SELECT * FROM usrShopifyInventoryLevel;


