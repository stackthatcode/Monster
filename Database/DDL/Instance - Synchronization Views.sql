USE Monster0001;
GO


DROP VIEW IF EXISTS ShopifyOrdersOnlySyncView
GO
CREATE VIEW ShopifyOrdersOnlySyncView
AS
SELECT	t1.MonsterId, 
		t1.ShopifyOrderId, 
		t1.Ignore, 
		t1.NeedsOrderPut,
		t1.NeedsTransactionGet,
		t2.AcumaticaOrderNbr 
FROM ShopifyOrder t1
	LEFT JOIN AcumaticaSalesOrder t2
		ON t1.MonsterId = t2.ShopifyOrderMonsterId
GO

DROP VIEW IF EXISTS ShopifyOrderPaymentsSyncStatus
GO
CREATE VIEW ShopifyOrderPaymentsSyncStatus
AS
SELECT	t1.MonsterId, 
		t1.ShopifyOrderId,
		t1.Ignore, 
		t6.AcumaticaDocType, 
		t6.AcumaticaRefNbr, 
		t6.NeedRelease
FROM ShopifyOrder t1
	INNER JOIN ShopifyTransaction t5
		ON t1.MonsterId = t5.ShopifyOrderMonsterId
		AND t5.IsSyncableToPayment = 1
	LEFT JOIN AcumaticaPayment t6
		ON t5.MonsterId = t6.ShopifyTransactionMonsterId
GO

DROP VIEW IF EXISTS ShopifyOrderAdjustmentsSyncStatus
GO
CREATE VIEW ShopifyOrderAdjustmentsSyncStatus
AS
SELECT	t1.MonsterId, 
		t1.ShopifyOrderId, 
		t1.Ignore, 
		t7.ShopifyRefundId, 
		t8.AcumaticaDocType, 
		t8.AcumaticaRefNbr, 
		t8.NeedRelease, 
		t8.NeedApplyToOrder
FROM ShopifyOrder t1
	INNER JOIN ShopifyRefund t7
		ON t1.MonsterId = t7.ShopifyOrderMonsterId
		AND t7.RequiresMemo = 1
	LEFT JOIN AcumaticaMemo t8
		ON t7.MonsterId = t8.ShopifyRefundMonsterId
GO

DROP VIEW IF EXISTS ShopifyOrderSoShipmentsSyncStatus
GO
CREATE VIEW ShopifyOrderSoShipmentsSyncStatus
AS
SELECT t1.MonsterId, 
	t1.ShopifyOrderId, 
	t1.Ignore,
	t2.AcumaticaOrderNbr,
	t3.AcumaticaShipmentNbr,
	t3.AcumaticaInvoiceNbr,
	t3.NeedShipmentAndInvoiceGet,
	t4.ShopifyFulfillmentId
FROM ShopifyOrder t1
	INNER JOIN AcumaticaSalesOrder t2
		ON t1.MonsterId = t2.ShopifyOrderMonsterId
	INNER JOIN AcumaticaSoShipment t3
		ON t2.ShopifyOrderMonsterId = t3.ShopifyOrderMonsterId
	LEFT JOIN ShopifyFulfillment t4
		ON t3.ShopifyFulfillmentMonsterId = t4.MonsterId
GO




-- Views for identifying records needing sync
--

DROP VIEW IF EXISTS ShopifyOrdersNeedingCreateSync
GO
CREATE VIEW ShopifyOrdersNeedingCreateSync
AS
SELECT * FROM ShopifyOrdersOnlySyncView
WHERE ( Ignore = 0 ) AND ( AcumaticaOrderNbr IS NULL )
GO

DROP VIEW IF EXISTS ShopifyOrdersNeedingUpdateSync
GO
CREATE VIEW ShopifyOrdersNeedingUpdateSync
AS
SELECT * FROM ShopifyOrdersOnlySyncView
WHERE ( Ignore = 0 ) AND ( AcumaticaOrderNbr IS NOT NULL ) AND ( NeedsOrderPut = 1 )
GO

DROP VIEW IF EXISTS ShopifyOrderPaymentsNeedingCreateSync
GO
CREATE VIEW ShopifyOrderPaymentsNeedingCreateSync
AS
SELECT * FROM ShopifyOrderPaymentsSyncStatus
WHERE ( Ignore = 0 )
AND ( AcumaticaRefNbr IS NULL )
GO

DROP VIEW IF EXISTS ShopifyOrderPaymentsNeedingReleaseSync
GO
CREATE VIEW ShopifyOrderPaymentsNeedingReleaseSync
AS
SELECT * FROM ShopifyOrderPaymentsSyncStatus
WHERE ( Ignore = 0 )
AND ( AcumaticaRefNbr IS NOT NULL AND NeedRelease = 1 )
GO

DROP VIEW IF EXISTS ShopifyOrderAdjustmentsNeedingSync
GO
CREATE VIEW ShopifyOrderAdjustmentsNeedingSync
AS
SELECT * FROM ShopifyOrderAdjustmentsSyncStatus
WHERE ( Ignore = 0 )
AND ( ( AcumaticaRefNbr IS NULL )
		OR ( AcumaticaRefNbr IS NOT NULL AND NeedRelease = 1 ) 
		OR ( AcumaticaRefNbr IS NOT NULL AND NeedApplyToOrder = 1 ) )
GO

DROP VIEW IF EXISTS ShopifyOrderSoShipmentsNeedingSync
GO
CREATE VIEW ShopifyOrderSoShipmentsNeedingSync
AS
SELECT * FROM ShopifyOrderSoShipmentsSyncStatus
WHERE ( Ignore = 0 )
AND ( ShopifyFulfillmentId IS NULL )
GO


DROP VIEW IF EXISTS ShopifyOrdersNeedingSyncAll
GO
CREATE VIEW ShopifyOrdersNeedingSyncAll
AS
SELECT MonsterId FROM ShopifyOrdersNeedingCreateSync
UNION
SELECT MonsterId FROM ShopifyOrdersNeedingUpdateSync
UNION
SELECT MonsterId FROM ShopifyOrderPaymentsNeedingCreateSync
UNION
SELECT MonsterId FROM ShopifyOrderPaymentsNeedingReleaseSync
UNION
SELECT MonsterId FROM ShopifyOrderAdjustmentsNeedingSync
UNION
SELECT MonsterId FROM ShopifyOrderSoShipmentsNeedingSync
GO

DROP VIEW IF EXISTS ShopifyOrdersNotNeedingSyncAll
GO
CREATE VIEW ShopifyOrdersNotNeedingSyncAll
AS
SELECT MonsterId FROM ShopifyOrder 
WHERE MonsterId NOT IN ( SELECT MonsterId FROM ShopifyOrdersNeedingSyncAll )
GO


SELECT * FROM ShopifyOrdersNeedingCreateSync
SELECT * FROM ShopifyOrdersNeedingUpdateSync
SELECT * FROM ShopifyOrderPaymentsNeedingSync
SELECT * FROM ShopifyOrderAdjustmentsNeedingSync
SELECT * FROM ShopifyOrderSoShipmentsNeedingSync

SELECT * FROM ShopifyOrdersNeedingSyncAll;

