
/****** Object:  Table [dbo].[AcumaticaBatchState]    Script Date: 1/10/2020 10:53:41 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AcumaticaBatchState](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AcumaticaStockItemGetEnd] [datetime] NULL,
	[AcumaticaInventoryStatusGetEnd] [datetime] NULL,
	[AcumaticaCustomersGetEnd] [datetime] NULL,
	[AcumaticaOrdersGetEnd] [datetime] NULL,
	[AcumaticaShipmentsGetEnd] [datetime] NULL,
 CONSTRAINT [PK_usrShopifyBatchState] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AcumaticaCustomer]    Script Date: 1/10/2020 10:53:41 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AcumaticaCustomer](
	[ShopifyCustomerMonsterId] [bigint] NOT NULL,
	[AcumaticaCustomerId] [varchar](50) NOT NULL,
	[AcumaticaMainContactEmail] [varchar](100) NULL,
	[AcumaticaJson] [nvarchar](max) NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_AcumaticaCustomer] PRIMARY KEY CLUSTERED 
(
	[ShopifyCustomerMonsterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AcumaticaInventory]    Script Date: 1/10/2020 10:53:41 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AcumaticaInventory](
	[MonsterId] [bigint] IDENTITY(1,1) NOT NULL,
	[ParentMonsterId] [bigint] NULL,
	[AcumaticaWarehouseId] [nvarchar](50) NOT NULL,
	[AcumaticaAvailQty] [float] NOT NULL,
	[WarehouseMonsterId] [bigint] NOT NULL,
	[IsInventorySynced] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrAcumaticaWarehouseDetails] PRIMARY KEY CLUSTERED 
(
	[MonsterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AcumaticaInventoryReceipt]    Script Date: 1/10/2020 10:53:41 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AcumaticaInventoryReceipt](
	[MonsterId] [bigint] IDENTITY(1,1) NOT NULL,
	[AcumaticaRefNumber] [nvarchar](50) NOT NULL,
	[AcumaticaJson] [nvarchar](max) NOT NULL,
	[IsReleased] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdate] [datetime] NOT NULL,
 CONSTRAINT [PK_usrAcumaticaInventoryReceipt] PRIMARY KEY CLUSTERED 
(
	[MonsterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AcumaticaPayment]    Script Date: 1/10/2020 10:53:41 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AcumaticaPayment](
	[ShopifyTransactionMonsterId] [bigint] NOT NULL,
	[AcumaticaRefNbr] [varchar](50) NOT NULL,
	[AcumaticaDocType] [varchar](25) NOT NULL,
	[AcumaticaAmount] [money] NOT NULL,
	[AcumaticaAppliedToOrder] [money] NOT NULL,
	[IsReleased] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyAcuPayment] PRIMARY KEY CLUSTERED 
(
	[ShopifyTransactionMonsterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AcumaticaRefData]    Script Date: 1/10/2020 10:53:41 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AcumaticaRefData](
	[Id] [bigint] NOT NULL,
	[ItemClass] [nvarchar](max) NULL,
	[PaymentMethod] [nvarchar](max) NULL,
	[TaxZone] [nvarchar](max) NULL,
	[TaxCategory] [nvarchar](max) NULL,
	[TaxId] [nvarchar](max) NULL,
 CONSTRAINT [PK_usrAcumaticaReferences] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AcumaticaSalesOrder]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AcumaticaSalesOrder](
	[ShopifyOrderMonsterId] [bigint] NOT NULL,
	[AcumaticaShipmentDetailsJson] [nvarchar](max) NULL,
	[AcumaticaOrderNbr] [varchar](50) NOT NULL,
	[AcumaticaStatus] [varchar](25) NOT NULL,
	[AcumaticaIsTaxValid] [bit] NOT NULL,
	[ShopifyCustomerMonsterId] [bigint] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_AcumaticaSalesOrder] PRIMARY KEY CLUSTERED 
(
	[ShopifyOrderMonsterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AcumaticaSoShipment]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AcumaticaSoShipment](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyOrderMonsterId] [bigint] NOT NULL,
	[AcumaticaShipmentNbr] [varchar](50) NOT NULL,
	[AcumaticaInvoiceType] [varchar](10) NOT NULL,
	[AcumaticaInvoiceNbr] [varchar](50) NOT NULL,
	[AcumaticaStatus] [varchar](50) NOT NULL,
	[AcumaticaShipmentJson] [nvarchar](max) NULL,
	[AcumaticaTrackingNbr] [varchar](200) NULL,
	[AcumaticaInvoiceAmount] [money] NULL,
	[AcumaticaInvoiceTax] [money] NULL,
	[NeedShipmentGet] [bit] NOT NULL,
	[PutErrorCount] [int] NOT NULL,
	[ShopifyFulfillmentMonsterId] [bigint] NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrAcumaticaSoShipmentInvoice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AcumaticaStockItem]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AcumaticaStockItem](
	[MonsterId] [bigint] IDENTITY(1,1) NOT NULL,
	[ItemId] [varchar](100) NOT NULL,
	[AcumaticaJson] [nvarchar](max) NOT NULL,
	[AcumaticaTaxCategory] [varchar](50) NOT NULL,
	[AcumaticaDescription] [varchar](200) NULL,
	[AcumaticaDefaultPrice] [money] NOT NULL,
	[AcumaticaLastCost] [money] NOT NULL,
	[IsVariantSynced] [bit] NOT NULL,
	[ShopifyVariantMonsterId] [bigint] NULL,
	[IsSyncEnabled] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrAcumaticaStockItem] PRIMARY KEY CLUSTERED 
(
	[MonsterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AcumaticaWarehouse]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AcumaticaWarehouse](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AcumaticaWarehouseId] [varchar](50) NOT NULL,
	[AcumaticaJson] [nvarchar](max) NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrAcumaticaWarehouse] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Connections]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Connections](
	[Id] [bigint] NOT NULL,
	[ShopifyDomain] [varchar](500) NULL,
	[ShopifyAuthCodeHash] [varchar](500) NULL,
	[ShopifyAccessToken] [varchar](500) NULL,
	[ShopifyApiKey] [varchar](500) NULL,
	[ShopifyApiPassword] [varchar](500) NULL,
	[ShopifyApiSecret] [varchar](500) NULL,
	[AcumaticaInstanceUrl] [varchar](500) NULL,
	[AcumaticaBranch] [varchar](500) NULL,
	[AcumaticaCompanyName] [varchar](500) NULL,
	[AcumaticaUsername] [varchar](500) NULL,
	[AcumaticaPassword] [varchar](500) NULL,
 CONSTRAINT [PK_usrTenantContext] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExclusiveJobMonitor]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExclusiveJobMonitor](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[BackgroundJobType] [int] NOT NULL,
	[HangFireJobId] [varchar](100) NULL,
	[ReceivedKillSignal] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrJobMonitor] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ExecutionLog]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExecutionLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[LogLevel] [int] NOT NULL,
	[LogContent] [varchar](250) NOT NULL,
	[DateCreated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrJobExecutionLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InventoryReceiptSync]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InventoryReceiptSync](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyInventoryMonsterId] [bigint] NOT NULL,
	[AcumaticaInvReceiptMonsterId] [bigint] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrInventoryReceiptSync] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MonsterSettings]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MonsterSettings](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AcumaticaTimeZone] [varchar](50) NULL,
	[AcumaticaDefaultItemClass] [varchar](50) NULL,
	[AcumaticaDefaultPostingClass] [varchar](50) NULL,
	[AcumaticaTaxZone] [varchar](50) NULL,
	[AcumaticaTaxExemptCategory] [varchar](50) NULL,
	[AcumaticaTaxableCategory] [varchar](50) NULL,
	[AcumaticaLineItemTaxId] [varchar](50) NULL,
	[AcumaticaFreightTaxId] [varchar](50) NULL,
	[SyncOrdersEnabled] [bit] NOT NULL,
	[SyncInventoryEnabled] [bit] NOT NULL,
	[SyncFulfillmentsEnabled] [bit] NOT NULL,
	[SyncRefundsEnabled] [bit] NOT NULL,
	[ShopifyOrderId] [bigint] NULL,
	[ShopifyOrderName] [varchar](50) NULL,
	[ShopifyOrderCreatedAtUtc] [datetime] NULL,
	[MaxParallelAcumaticaSyncs] [int] NOT NULL,
	[MaxNumberOfOrders] [int] NOT NULL,
	[InventorySyncPrice] [bit] NOT NULL,
	[InventorySyncWeight] [bit] NOT NULL,
	[InventorySyncAvailableQty] [bit] NOT NULL,
	[LastRecurringSchedule] [int] NOT NULL,
 CONSTRAINT [PK_usrPreferences] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PaymentGateways]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentGateways](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ShopifyGatewayId] [varchar](50) NOT NULL,
	[AcumaticaPaymentMethod] [varchar](25) NOT NULL,
	[AcumaticaCashAccount] [varchar](25) NOT NULL,
 CONSTRAINT [PK_PaymentGateways] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PayoutPreferences]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PayoutPreferences](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AcumaticaCashAccount] [varchar](50) NULL,
 CONSTRAINT [PK_usrPayoutPreferences] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShopAcuWarehouseSync]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShopAcuWarehouseSync](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyLocationMonsterId] [bigint] NOT NULL,
	[AcumaticaWarehouseMonsterId] [bigint] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopAcuWarehouseSync] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShopifyBatchState]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShopifyBatchState](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyProductsGetEnd] [datetime] NULL,
	[ShopifyOrdersGetEnd] [datetime] NULL,
	[ShopifyCustomersGetEnd] [datetime] NULL,
	[ShopifyPayoutGetEnd] [date] NULL,
 CONSTRAINT [PK_usrShopifyBatchState_1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShopifyCustomer]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShopifyCustomer](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyJson] [nvarchar](max) NOT NULL,
	[ShopifyCustomerId] [bigint] NOT NULL,
	[ShopifyPrimaryEmail] [varchar](100) NULL,
	[NeedsCustomerPut] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyCustomer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShopifyFulfillment]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShopifyFulfillment](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyFulfillmentId] [bigint] NOT NULL,
	[ShopifyOrderId] [bigint] NOT NULL,
	[ShopifyStatus] [varchar](50) NOT NULL,
	[ShopifyTrackingNumber] [varchar](200) NULL,
	[OrderMonsterId] [bigint] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyFulfillment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShopifyInventoryLevel]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShopifyInventoryLevel](
	[MonsterId] [bigint] IDENTITY(1,1) NOT NULL,
	[ParentMonsterId] [bigint] NOT NULL,
	[ShopifyInventoryItemId] [bigint] NOT NULL,
	[ShopifyLocationId] [bigint] NOT NULL,
	[ShopifyAvailableQuantity] [int] NOT NULL,
	[LocationMonsterId] [bigint] NULL,
	[InventoryReceiptMonsterId] [bigint] NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyInventoryLevels] PRIMARY KEY CLUSTERED 
(
	[MonsterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShopifyLocation]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShopifyLocation](
	[MonsterId] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyLocationId] [bigint] NOT NULL,
	[ShopifyJson] [nvarchar](max) NOT NULL,
	[ShopifyLocationName] [varchar](50) NOT NULL,
	[ShopifyActive] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyLocation_1] PRIMARY KEY CLUSTERED 
(
	[MonsterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShopifyOrder]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShopifyOrder](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyJson] [nvarchar](max) NOT NULL,
	[ShopifyOrderId] [bigint] NOT NULL,
	[ShopifyOrderNumber] [varchar](25) NOT NULL,
	[ShopifyFinancialStatus] [varchar](25) NOT NULL,
	[IsEmptyOrCancelled] [bit] NOT NULL,
	[NeedsTransactionGet] [bit] NOT NULL,
	[NeedsOrderPut] [bit] NOT NULL,
	[IsBlocked] [bit] NOT NULL,
	[PutErrorCount] [int] NOT NULL,
	[CustomerMonsterId] [bigint] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShopifyPayout]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShopifyPayout](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyPayoutId] [bigint] NOT NULL,
	[ShopifyLastStatus] [varchar](50) NOT NULL,
	[Json] [text] NOT NULL,
	[AllTransDownloaded] [bit] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyPayout] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShopifyPayoutTransaction]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShopifyPayoutTransaction](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[MonsterParentId] [bigint] NOT NULL,
	[ShopifyPayoutId] [bigint] NOT NULL,
	[ShopifyPayoutTransId] [bigint] NOT NULL,
	[ShopifyOrderId] [bigint] NULL,
	[ShopifyCustomerId] [bigint] NULL,
	[Type] [varchar](50) NOT NULL,
	[Json] [text] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyPayoutTransaction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShopifyProduct]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShopifyProduct](
	[MonsterId] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyProductId] [bigint] NOT NULL,
	[ShopifyTitle] [varchar](100) NOT NULL,
	[ShopifyVendor] [varchar](100) NOT NULL,
	[ShopifyProductType] [varchar](100) NOT NULL,
	[ShopifyJson] [nvarchar](max) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyProduct] PRIMARY KEY CLUSTERED 
(
	[MonsterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShopifyRefund]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShopifyRefund](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyRefundId] [bigint] NOT NULL,
	[ShopifyOrderId] [bigint] NOT NULL,
	[OrderMonsterId] [bigint] NOT NULL,
	[CreditAdjustment] [money] NOT NULL,
	[DebitAdjustment] [money] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyRefund] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShopifyTransaction]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShopifyTransaction](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyTransactionId] [bigint] NOT NULL,
	[ShopifyOrderId] [bigint] NOT NULL,
	[ShopifyStatus] [varchar](25) NOT NULL,
	[ShopifyKind] [varchar](25) NOT NULL,
	[ShopifyJson] [nvarchar](max) NOT NULL,
	[ShopifyGateway] [varchar](50) NOT NULL,
	[ShopifyAmount] [money] NOT NULL,
	[ShopifyRefundId] [bigint] NULL,
	[Ignore] [bit] NOT NULL,
	[NeedsPaymentPut] [bit] NOT NULL,
	[PutErrorCount] [int] NOT NULL,
	[OrderMonsterId] [bigint] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyTransaction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ShopifyVariant]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ShopifyVariant](
	[MonsterId] [bigint] IDENTITY(1,1) NOT NULL,
	[ParentMonsterId] [bigint] NOT NULL,
	[ShopifyVariantId] [bigint] NOT NULL,
	[ShopifyInventoryItemId] [bigint] NOT NULL,
	[ShopifyVariantJson] [nvarchar](max) NOT NULL,
	[ShopifySku] [varchar](100) NOT NULL,
	[ShopifyTitle] [varchar](200) NOT NULL,
	[ShopifyIsTracked] [bit] NOT NULL,
	[ShopifyPrice] [money] NOT NULL,
	[ShopifyCost] [money] NOT NULL,
	[ShopifyIsTaxable] [bit] NOT NULL,
	[IsMissing] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyVariant_1] PRIMARY KEY CLUSTERED 
(
	[MonsterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SystemState]    Script Date: 1/10/2020 10:53:42 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SystemState](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[IsRandomAccessMode] [bit] NOT NULL,
	[ShopifyConnState] [int] NOT NULL,
	[AcumaticaConnState] [int] NOT NULL,
	[AcumaticaRefDataState] [int] NOT NULL,
	[SettingsState] [int] NOT NULL,
	[SettingsTaxesState] [int] NOT NULL,
	[WarehouseSyncState] [int] NOT NULL,
	[InventoryRefreshState] [int] NOT NULL,
	[StartingShopifyOrderState] [int] NOT NULL,
	[ShopifyOrderEtcGetState] [int] NOT NULL,
	[AcumaticaOrderEtcGetState] [int] NOT NULL,
	[AcumaticaOrderEtcPutState] [int] NOT NULL,
	[AcumaticaRefundPutState] [int] NOT NULL,
	[ShopifyFulfillmentPutState] [int] NOT NULL,
	[ShopifyInventoryPutState] [int] NOT NULL,
 CONSTRAINT [PK_usrSystemState] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AcumaticaCustomer]  WITH CHECK ADD  CONSTRAINT [FK_AcumaticaCustomer_ShopifyCustomer] FOREIGN KEY([ShopifyCustomerMonsterId])
REFERENCES [dbo].[ShopifyCustomer] ([Id])
GO
ALTER TABLE [dbo].[AcumaticaCustomer] CHECK CONSTRAINT [FK_AcumaticaCustomer_ShopifyCustomer]
GO
ALTER TABLE [dbo].[AcumaticaInventory]  WITH CHECK ADD  CONSTRAINT [FK_usrAcumaticaWarehouseDetails_usrAcumaticaStockItem] FOREIGN KEY([ParentMonsterId])
REFERENCES [dbo].[AcumaticaStockItem] ([MonsterId])
GO
ALTER TABLE [dbo].[AcumaticaInventory] CHECK CONSTRAINT [FK_usrAcumaticaWarehouseDetails_usrAcumaticaStockItem]
GO
ALTER TABLE [dbo].[AcumaticaInventory]  WITH CHECK ADD  CONSTRAINT [FK_usrAcumaticaWarehouseDetails_usrAcumaticaWarehouse] FOREIGN KEY([WarehouseMonsterId])
REFERENCES [dbo].[AcumaticaWarehouse] ([Id])
GO
ALTER TABLE [dbo].[AcumaticaInventory] CHECK CONSTRAINT [FK_usrAcumaticaWarehouseDetails_usrAcumaticaWarehouse]
GO
ALTER TABLE [dbo].[AcumaticaPayment]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyAcuPayment_usrShopifyTransaction] FOREIGN KEY([ShopifyTransactionMonsterId])
REFERENCES [dbo].[ShopifyTransaction] ([Id])
GO
ALTER TABLE [dbo].[AcumaticaPayment] CHECK CONSTRAINT [FK_usrShopifyAcuPayment_usrShopifyTransaction]
GO
ALTER TABLE [dbo].[AcumaticaSalesOrder]  WITH CHECK ADD  CONSTRAINT [FK_AcumaticaSalesOrder_AcumaticaCustomer] FOREIGN KEY([ShopifyCustomerMonsterId])
REFERENCES [dbo].[AcumaticaCustomer] ([ShopifyCustomerMonsterId])
GO
ALTER TABLE [dbo].[AcumaticaSalesOrder] CHECK CONSTRAINT [FK_AcumaticaSalesOrder_AcumaticaCustomer]
GO
ALTER TABLE [dbo].[AcumaticaSalesOrder]  WITH CHECK ADD  CONSTRAINT [FK_AcumaticaSalesOrder_ShopifyOrder] FOREIGN KEY([ShopifyOrderMonsterId])
REFERENCES [dbo].[ShopifyOrder] ([Id])
GO
ALTER TABLE [dbo].[AcumaticaSalesOrder] CHECK CONSTRAINT [FK_AcumaticaSalesOrder_ShopifyOrder]
GO
ALTER TABLE [dbo].[AcumaticaSoShipment]  WITH CHECK ADD  CONSTRAINT [FK_AcumaticaSoShipment_AcumaticaSalesOrder] FOREIGN KEY([ShopifyOrderMonsterId])
REFERENCES [dbo].[AcumaticaSalesOrder] ([ShopifyOrderMonsterId])
GO
ALTER TABLE [dbo].[AcumaticaSoShipment] CHECK CONSTRAINT [FK_AcumaticaSoShipment_AcumaticaSalesOrder]
GO
ALTER TABLE [dbo].[AcumaticaSoShipment]  WITH CHECK ADD  CONSTRAINT [FK_AcumaticaSoShipment_ShopifyFulfillment] FOREIGN KEY([ShopifyFulfillmentMonsterId])
REFERENCES [dbo].[ShopifyFulfillment] ([Id])
GO
ALTER TABLE [dbo].[AcumaticaSoShipment] CHECK CONSTRAINT [FK_AcumaticaSoShipment_ShopifyFulfillment]
GO
ALTER TABLE [dbo].[AcumaticaStockItem]  WITH CHECK ADD  CONSTRAINT [FK_AcumaticaStockItem_ShopifyVariant] FOREIGN KEY([ShopifyVariantMonsterId])
REFERENCES [dbo].[ShopifyVariant] ([MonsterId])
GO
ALTER TABLE [dbo].[AcumaticaStockItem] CHECK CONSTRAINT [FK_AcumaticaStockItem_ShopifyVariant]
GO
ALTER TABLE [dbo].[InventoryReceiptSync]  WITH CHECK ADD  CONSTRAINT [FK_usrInventoryReceiptSync_usrAcumaticaInventoryReceipt] FOREIGN KEY([AcumaticaInvReceiptMonsterId])
REFERENCES [dbo].[AcumaticaInventoryReceipt] ([MonsterId])
GO
ALTER TABLE [dbo].[InventoryReceiptSync] CHECK CONSTRAINT [FK_usrInventoryReceiptSync_usrAcumaticaInventoryReceipt]
GO
ALTER TABLE [dbo].[InventoryReceiptSync]  WITH CHECK ADD  CONSTRAINT [FK_usrInventoryReceiptSync_usrShopifyInventoryLevels] FOREIGN KEY([ShopifyInventoryMonsterId])
REFERENCES [dbo].[ShopifyInventoryLevel] ([MonsterId])
GO
ALTER TABLE [dbo].[InventoryReceiptSync] CHECK CONSTRAINT [FK_usrInventoryReceiptSync_usrShopifyInventoryLevels]
GO
ALTER TABLE [dbo].[ShopAcuWarehouseSync]  WITH CHECK ADD  CONSTRAINT [FK_usrShopAcuWarehouseSync_usrAcumaticaWarehouse] FOREIGN KEY([AcumaticaWarehouseMonsterId])
REFERENCES [dbo].[AcumaticaWarehouse] ([Id])
GO
ALTER TABLE [dbo].[ShopAcuWarehouseSync] CHECK CONSTRAINT [FK_usrShopAcuWarehouseSync_usrAcumaticaWarehouse]
GO
ALTER TABLE [dbo].[ShopAcuWarehouseSync]  WITH CHECK ADD  CONSTRAINT [FK_usrShopAcuWarehouseSync_usrShopifyLocation] FOREIGN KEY([ShopifyLocationMonsterId])
REFERENCES [dbo].[ShopifyLocation] ([MonsterId])
GO
ALTER TABLE [dbo].[ShopAcuWarehouseSync] CHECK CONSTRAINT [FK_usrShopAcuWarehouseSync_usrShopifyLocation]
GO
ALTER TABLE [dbo].[ShopifyFulfillment]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyFulfillment_usrShopifyOrder] FOREIGN KEY([OrderMonsterId])
REFERENCES [dbo].[ShopifyOrder] ([Id])
GO
ALTER TABLE [dbo].[ShopifyFulfillment] CHECK CONSTRAINT [FK_usrShopifyFulfillment_usrShopifyOrder]
GO
ALTER TABLE [dbo].[ShopifyInventoryLevel]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyInventoryLevels_usrShopifyLocation] FOREIGN KEY([LocationMonsterId])
REFERENCES [dbo].[ShopifyLocation] ([MonsterId])
GO
ALTER TABLE [dbo].[ShopifyInventoryLevel] CHECK CONSTRAINT [FK_usrShopifyInventoryLevels_usrShopifyLocation]
GO
ALTER TABLE [dbo].[ShopifyInventoryLevel]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyInventoryLevels_usrShopifyVariant] FOREIGN KEY([ParentMonsterId])
REFERENCES [dbo].[ShopifyVariant] ([MonsterId])
GO
ALTER TABLE [dbo].[ShopifyInventoryLevel] CHECK CONSTRAINT [FK_usrShopifyInventoryLevels_usrShopifyVariant]
GO
ALTER TABLE [dbo].[ShopifyOrder]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyOrder_usrShopifyCustomer] FOREIGN KEY([CustomerMonsterId])
REFERENCES [dbo].[ShopifyCustomer] ([Id])
GO
ALTER TABLE [dbo].[ShopifyOrder] CHECK CONSTRAINT [FK_usrShopifyOrder_usrShopifyCustomer]
GO
ALTER TABLE [dbo].[ShopifyPayoutTransaction]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyPayoutTransaction_usrShopifyPayout] FOREIGN KEY([MonsterParentId])
REFERENCES [dbo].[ShopifyPayout] ([Id])
GO
ALTER TABLE [dbo].[ShopifyPayoutTransaction] CHECK CONSTRAINT [FK_usrShopifyPayoutTransaction_usrShopifyPayout]
GO
ALTER TABLE [dbo].[ShopifyRefund]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyRefund_usrShopifyOrder] FOREIGN KEY([OrderMonsterId])
REFERENCES [dbo].[ShopifyOrder] ([Id])
GO
ALTER TABLE [dbo].[ShopifyRefund] CHECK CONSTRAINT [FK_usrShopifyRefund_usrShopifyOrder]
GO
ALTER TABLE [dbo].[ShopifyTransaction]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyTransaction_usrShopifyOrder] FOREIGN KEY([OrderMonsterId])
REFERENCES [dbo].[ShopifyOrder] ([Id])
GO
ALTER TABLE [dbo].[ShopifyTransaction] CHECK CONSTRAINT [FK_usrShopifyTransaction_usrShopifyOrder]
GO
ALTER TABLE [dbo].[ShopifyVariant]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyVariant_usrShopifyProduct] FOREIGN KEY([ParentMonsterId])
REFERENCES [dbo].[ShopifyProduct] ([MonsterId])
GO
ALTER TABLE [dbo].[ShopifyVariant] CHECK CONSTRAINT [FK_usrShopifyVariant_usrShopifyProduct]
GO
