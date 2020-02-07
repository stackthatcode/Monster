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


DROP VIEW IF EXISTS ShopifyOrderRefundsSyncStatus
GO
CREATE VIEW ShopifyOrderRefundsSyncStatus
AS
SELECT	t1.MonsterId, 
		t1.ShopifyOrderId, 
		t1.Ignore, 
		t7.ShopifyRefundId, 
		t7.NeedOriginalPaymentPut,
		t7.RequiresMemo,
		t8.AcumaticaDocType, 
		t8.AcumaticaRefNbr, 
		t8.NeedRelease, 
		t8.NeedApplyToOrder
FROM ShopifyOrder t1
	INNER JOIN ShopifyRefund t7
		ON t1.MonsterId = t7.ShopifyOrderMonsterId
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

DROP VIEW IF EXISTS ShopifyOrdersNeedingOrderCreate
GO
CREATE VIEW ShopifyOrdersNeedingOrderCreate
AS
SELECT * FROM ShopifyOrdersOnlySyncView
WHERE ( Ignore = 0 ) AND ( AcumaticaOrderNbr IS NULL )
GO

DROP VIEW IF EXISTS ShopifyOrdersNeedingOrderUpdate
GO
CREATE VIEW ShopifyOrdersNeedingOrderUpdate
AS
SELECT * FROM ShopifyOrdersOnlySyncView
WHERE ( Ignore = 0 ) AND ( AcumaticaOrderNbr IS NOT NULL AND NeedsOrderPut = 1  )
GO

DROP VIEW IF EXISTS ShopifyOrdersNeedingPaymentSync
GO
CREATE VIEW ShopifyOrdersNeedingPaymentSync
AS
SELECT * FROM ShopifyOrderPaymentsSyncStatus
WHERE ( Ignore = 0 )
AND ( ( AcumaticaRefNbr IS NULL ) OR
	( AcumaticaRefNbr IS NOT NULL AND NeedRelease = 1 ) )
GO

DROP VIEW IF EXISTS ShopifyOrdersNeedingOriginalPaymentUpdate
GO
CREATE VIEW ShopifyOrdersNeedingOriginalPaymentUpdate
AS
SELECT * FROM ShopifyOrderRefundsSyncStatus
WHERE ( Ignore = 0  AND NeedOriginalPaymentPut = 1 ) 
GO


DROP VIEW IF EXISTS ShopifyOrderNeedingRefundSync
GO
CREATE VIEW ShopifyOrderNeedingRefundSync
AS
SELECT * FROM ShopifyOrderRefundsSyncStatus
WHERE ( Ignore = 0 AND RequiresMemo = 1 )
AND ( ( AcumaticaRefNbr IS NULL )
		OR ( AcumaticaRefNbr IS NOT NULL AND NeedRelease = 1 ) 
		OR ( AcumaticaRefNbr IS NOT NULL AND NeedApplyToOrder = 1 ) )
GO

DROP VIEW IF EXISTS ShopifyOrderNeedingSoShipmentsSync
GO
CREATE VIEW ShopifyOrderNeedingSoShipmentsSync
AS
SELECT * FROM ShopifyOrderSoShipmentsSyncStatus
WHERE ( Ignore = 0 )
AND ( ShopifyFulfillmentId IS NULL )
GO





DROP VIEW IF EXISTS ShopifyOrdersNeedingSyncAll
GO
CREATE VIEW ShopifyOrdersNeedingSyncAll
AS
SELECT MonsterId, ShopifyOrderId FROM ShopifyOrdersNeedingOrderCreate
UNION
SELECT MonsterId, ShopifyOrderId FROM ShopifyOrdersNeedingOrderUpdate
UNION
SELECT MonsterId, ShopifyOrderId FROM ShopifyOrdersNeedingPaymentSync
UNION
SELECT MonsterId, ShopifyOrderId FROM ShopifyOrdersNeedingOriginalPaymentUpdate
UNION
SELECT MonsterId, ShopifyOrderId FROM ShopifyOrderNeedingRefundSync
UNION
SELECT MonsterId, ShopifyOrderId FROM ShopifyOrderNeedingSoShipmentsSync
GO

DROP VIEW IF EXISTS ShopifyOrdersNotNeedingSyncAll
GO
CREATE VIEW ShopifyOrdersNotNeedingSyncAll
AS
SELECT MonsterId, ShopifyOrderId 
FROM ShopifyOrder 
WHERE MonsterId NOT IN ( SELECT MonsterId FROM ShopifyOrdersNeedingSyncAll )
GO



SELECT * FROM ShopifyOrdersNeedingOrderCreate
SELECT * FROM ShopifyOrdersNeedingOrderUpdate
SELECT * FROM ShopifyOrdersNeedingPaymentSync
SELECT * FROM ShopifyOrdersNeedingOriginalPaymentUpdate
SELECT * FROM ShopifyOrderNeedingRefundSync
SELECT * FROM ShopifyOrderNeedingSoShipmentsSync

SELECT * FROM ShopifyOrdersNeedingSyncAll;

