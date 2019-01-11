USE [Monster0001]
GO
/****** Object:  Table [dbo].[usrAcumaticaBatchState]    Script Date: 1/11/2019 3:15:54 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrAcumaticaBatchState](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AcumaticaProductsPullEnd] [datetime] NULL,
	[AcumaticaCustomersPullEnd] [datetime] NULL,
	[AcumaticaOrdersPullEnd] [datetime] NULL,
	[AcumaticaShipmentsPullEnd] [datetime] NULL,
	[AcumaticaInvoicesPullEnd] [datetime] NULL,
 CONSTRAINT [PK_usrShopifyBatchState] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrAcumaticaCreditMemo]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrAcumaticaCreditMemo](
	[Id] [bigint] NOT NULL,
	[SalesOrderMonsterId] [bigint] NOT NULL,
	[AcumaticaCreditMemoId] [varchar](50) NOT NULL,
	[AcumaticaJson] [nchar](10) NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrAcumaticaCreditMemo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrAcumaticaCustomer]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrAcumaticaCustomer](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AcumaticaCustomerId] [varchar](50) NOT NULL,
	[AcumaticaMainContactEmail] [varchar](100) NULL,
	[AcumaticaJson] [nvarchar](max) NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrAcumaticaCustomer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrAcumaticaInventoryReceipt]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrAcumaticaInventoryReceipt](
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
/****** Object:  Table [dbo].[usrAcumaticaInvoice]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrAcumaticaInvoice](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AcumaticaInvoiceRefNbr] [varchar](50) NOT NULL,
	[AcumaticaStatus] [varchar](50) NULL,
	[Json] [nvarchar](max) NULL,
	[IsPulledFromAcumatica] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrAcumaticaInvoice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrAcumaticaReferences]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrAcumaticaReferences](
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
/****** Object:  Table [dbo].[usrAcumaticaSalesOrder]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrAcumaticaSalesOrder](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[DetailsJson] [nvarchar](max) NOT NULL,
	[ShipmentsJson] [nvarchar](max) NULL,
	[AcumaticaOrderNbr] [varchar](50) NOT NULL,
	[AcumaticaStatus] [varchar](25) NOT NULL,
	[CustomerMonsterId] [bigint] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrAcumaticaSalesOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrAcumaticaShipment]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrAcumaticaShipment](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AcumaticaJson] [nvarchar](max) NULL,
	[AcumaticaShipmentNbr] [varchar](50) NOT NULL,
	[AcumaticaStatus] [varchar](25) NULL,
	[AcumaticaReleasedInvoiceNbr] [varchar](50) NULL,
	[IsCreatedByMonster] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrAcumaticaShipment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrAcumaticaShipmentSalesOrderRef]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrAcumaticaShipmentSalesOrderRef](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShipmentMonsterId] [bigint] NOT NULL,
	[AcumaticaShipmentNbr] [varchar](50) NOT NULL,
	[AcumaticaOrderNbr] [varchar](50) NOT NULL,
	[DateCreated] [datetime] NULL,
	[LastUpdated] [datetime] NULL,
 CONSTRAINT [PK_usrAcumaticaShipmentSO] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrAcumaticaSoShipmentInvoice]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrAcumaticaSoShipmentInvoice](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[SalesOrderMonsterId] [bigint] NOT NULL,
	[AcumaticaShipmentNbr] [varchar](50) NOT NULL,
	[AcumaticaInvoiceNbr] [varchar](50) NULL,
	[IsLatestPulled] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrAcumaticaSoShipmentInvoice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrAcumaticaStockItem]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrAcumaticaStockItem](
	[MonsterId] [bigint] IDENTITY(1,1) NOT NULL,
	[ItemId] [varchar](100) NOT NULL,
	[AcumaticaJson] [nvarchar](max) NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrAcumaticaStockItem] PRIMARY KEY CLUSTERED 
(
	[MonsterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrAcumaticaWarehouse]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrAcumaticaWarehouse](
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
/****** Object:  Table [dbo].[usrAcumaticaWarehouseDetails]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrAcumaticaWarehouseDetails](
	[MonsterId] [bigint] IDENTITY(1,1) NOT NULL,
	[ParentMonsterId] [bigint] NULL,
	[AcumaticaJson] [nvarchar](max) NOT NULL,
	[AcumaticaWarehouseId] [nvarchar](50) NOT NULL,
	[AcumaticaQtyOnHand] [float] NOT NULL,
	[WarehouseMonsterId] [bigint] NOT NULL,
	[IsShopifySynced] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrAcumaticaWarehouseDetails] PRIMARY KEY CLUSTERED 
(
	[MonsterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrBackgroundJob]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrBackgroundJob](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[BackgroundJobType] [int] NULL,
	[HangFireJobId] [varchar](50) NULL,
	[DateCreated] [datetime] NULL,
	[LastUpdated] [datetime] NULL,
 CONSTRAINT [PK_usrJobMonitor] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrConnections]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrConnections](
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
/****** Object:  Table [dbo].[usrExecutionLog]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrExecutionLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[LogContent] [varchar](250) NOT NULL,
	[DateCreated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrJobExecutionLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrInventoryReceiptSync]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrInventoryReceiptSync](
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
/****** Object:  Table [dbo].[usrPayoutPreferences]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrPayoutPreferences](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AcumaticaCashAccount] [varchar](50) NULL,
 CONSTRAINT [PK_usrPayoutPreferences] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrPreferences]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrPreferences](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyOrderDateStart] [date] NULL,
	[ShopifyOrderNumberStart] [int] NULL,
	[AcumaticaTimeZone] [varchar](50) NULL,
	[DefaultCoGSMargin] [float] NULL,
	[AcumaticaDefaultItemClass] [varchar](50) NULL,
	[AcumaticaDefaultPostingClass] [varchar](50) NULL,
	[AcumaticaPaymentMethod] [varchar](50) NULL,
	[AcumaticaPaymentCashAccount] [varchar](50) NULL,
	[AcumaticaTaxZone] [varchar](50) NULL,
	[AcumaticaTaxCategory] [varchar](50) NULL,
	[AcumaticaTaxId] [varchar](50) NULL,
	[FulfillmentInAcumatica] [bit] NULL,
 CONSTRAINT [PK_usrPreferences] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopAcuCustomerSync]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopAcuCustomerSync](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AcumaticaCustomerMonsterId] [bigint] NOT NULL,
	[ShopifyCustomerMonsterId] [bigint] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopAcuCustomerSync] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopAcuItemSync]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopAcuItemSync](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyVariantMonsterId] [bigint] NOT NULL,
	[AcumaticaItemMonsterId] [bigint] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopAcuItemSync] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopAcuOrderSync]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopAcuOrderSync](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AcumaticaSalesOrderMonsterId] [bigint] NOT NULL,
	[ShopifyOrderMonsterId] [bigint] NOT NULL,
	[AcumaticaTaxDetailId] [varchar](128) NOT NULL,
	[IsTaxLoadedToAcumatica] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopAcuOrderSync] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopAcuRefundCM]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopAcuRefundCM](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyRefundMonsterId] [bigint] NOT NULL,
	[AcumaticaCreditMemoNbr] [varchar](50) NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopAcuRefundWithInv] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopAcuShipmentSync]    Script Date: 1/11/2019 3:15:55 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopAcuShipmentSync](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[AcumaticaShipDetailMonsterId] [bigint] NOT NULL,
	[ShopifyFulfillmentMonsterId] [bigint] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopAcuShipmentSync] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopAcuWarehouseSync]    Script Date: 1/11/2019 3:15:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopAcuWarehouseSync](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyLocationMonsterId] [bigint] NOT NULL,
	[AcumaticaWarehouseMonsterId] [bigint] NOT NULL,
	[IsNameMismatched] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopAcuWarehouseSync] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopifyAcuPayment]    Script Date: 1/11/2019 3:15:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyAcuPayment](
	[ShopifyTransactionMonsterId] [bigint] NOT NULL,
	[ShopifyPaymentNbr] [varchar](50) NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyAcuPayment] PRIMARY KEY CLUSTERED 
(
	[ShopifyTransactionMonsterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopifyBatchState]    Script Date: 1/11/2019 3:15:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyBatchState](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyProductsPullEnd] [datetime] NULL,
	[ShopifyOrdersPullEnd] [datetime] NULL,
	[ShopifyCustomersPullEnd] [datetime] NULL,
	[ShopifyPayoutPullEnd] [date] NULL,
 CONSTRAINT [PK_usrShopifyBatchState_1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopifyCustomer]    Script Date: 1/11/2019 3:15:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyCustomer](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyJson] [nvarchar](max) NOT NULL,
	[ShopifyCustomerId] [bigint] NOT NULL,
	[ShopifyPrimaryEmail] [varchar](100) NOT NULL,
	[IsUpdatedInAcumatica] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyCustomer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopifyFulfillment]    Script Date: 1/11/2019 3:15:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyFulfillment](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyFulfillmentId] [bigint] NOT NULL,
	[ShopifyOrderId] [bigint] NOT NULL,
	[ShopifyStatus] [varchar](50) NOT NULL,
	[OrderMonsterId] [bigint] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyFulfillment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopifyInventoryLevel]    Script Date: 1/11/2019 3:15:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyInventoryLevel](
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
/****** Object:  Table [dbo].[usrShopifyLocation]    Script Date: 1/11/2019 3:15:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyLocation](
	[MonsterId] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyLocationId] [bigint] NOT NULL,
	[ShopifyJson] [nvarchar](max) NOT NULL,
	[ShopifyLocationName] [varchar](50) NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyLocation_1] PRIMARY KEY CLUSTERED 
(
	[MonsterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopifyOrder]    Script Date: 1/11/2019 3:15:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyOrder](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyJson] [nvarchar](max) NOT NULL,
	[ShopifyOrderId] [bigint] NOT NULL,
	[ShopifyOrderNumber] [int] NOT NULL,
	[ShopifyIsCancelled] [bit] NOT NULL,
	[ShopifyFinancialStatus] [varchar](25) NOT NULL,
	[AreTransactionsUpdated] [bit] NOT NULL,
	[CustomerMonsterId] [bigint] NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopifyPayout]    Script Date: 1/11/2019 3:15:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyPayout](
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
/****** Object:  Table [dbo].[usrShopifyPayoutTransaction]    Script Date: 1/11/2019 3:15:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyPayoutTransaction](
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
/****** Object:  Table [dbo].[usrShopifyProduct]    Script Date: 1/11/2019 3:15:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyProduct](
	[MonsterId] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyProductId] [bigint] NOT NULL,
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
/****** Object:  Table [dbo].[usrShopifyRefund]    Script Date: 1/11/2019 3:15:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyRefund](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyRefundId] [bigint] NOT NULL,
	[ShopifyOrderId] [bigint] NOT NULL,
	[OrderMonsterId] [bigint] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyRefund] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopifyTransaction]    Script Date: 1/11/2019 3:15:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyTransaction](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyTransactionId] [bigint] NOT NULL,
	[ShopifyOrderId] [bigint] NOT NULL,
	[ShopifyStatus] [varchar](25) NOT NULL,
	[ShopifyKind] [varchar](25) NOT NULL,
	[ShopifyJson] [nvarchar](max) NOT NULL,
	[OrderMonsterId] [bigint] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyTransaction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopifyVariant]    Script Date: 1/11/2019 3:15:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyVariant](
	[MonsterId] [bigint] IDENTITY(1,1) NOT NULL,
	[ParentMonsterId] [bigint] NOT NULL,
	[ShopifyVariantId] [bigint] NOT NULL,
	[ShopifyInventoryItemId] [bigint] NOT NULL,
	[ShopifyVariantJson] [nvarchar](max) NOT NULL,
	[ShopifySku] [varchar](100) NOT NULL,
	[ShopifyCost] [money] NOT NULL,
	[ShopifyIsTracked] [bit] NOT NULL,
	[IsMissing] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyVariant_1] PRIMARY KEY CLUSTERED 
(
	[MonsterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrSystemState]    Script Date: 1/11/2019 3:15:56 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrSystemState](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyConnection] [int] NOT NULL,
	[AcumaticaConnection] [int] NOT NULL,
	[AcumaticaReferenceData] [int] NOT NULL,
	[PreferenceSelections] [int] NOT NULL,
	[WarehouseSync] [int] NOT NULL,
	[AcumaticaInventoryPush] [int] NOT NULL,
	[ShopifyInventoryPush] [int] NOT NULL,
	[IsShopifyUrlFinalized] [bit] NOT NULL,
	[IsAcumaticaUrlFinalized] [bit] NOT NULL,
	[IsRandomAccessMode] [bit] NOT NULL,
	[RealTimeHangFireJobId] [varchar](250) NULL,
 CONSTRAINT [PK_usrSystemState] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[usrAcumaticaSalesOrder]  WITH CHECK ADD  CONSTRAINT [FK_usrAcumaticaSalesOrder_usrAcumaticaCustomer] FOREIGN KEY([CustomerMonsterId])
REFERENCES [dbo].[usrAcumaticaCustomer] ([Id])
GO
ALTER TABLE [dbo].[usrAcumaticaSalesOrder] CHECK CONSTRAINT [FK_usrAcumaticaSalesOrder_usrAcumaticaCustomer]
GO
ALTER TABLE [dbo].[usrAcumaticaShipmentSalesOrderRef]  WITH CHECK ADD  CONSTRAINT [FK_usrAcumaticaShipmentSO_usrAcumaticaShipment] FOREIGN KEY([ShipmentMonsterId])
REFERENCES [dbo].[usrAcumaticaShipment] ([Id])
GO
ALTER TABLE [dbo].[usrAcumaticaShipmentSalesOrderRef] CHECK CONSTRAINT [FK_usrAcumaticaShipmentSO_usrAcumaticaShipment]
GO
ALTER TABLE [dbo].[usrAcumaticaSoShipmentInvoice]  WITH CHECK ADD  CONSTRAINT [FK_usrAcumaticaSoShipmentInvoice_usrAcumaticaSalesOrder] FOREIGN KEY([SalesOrderMonsterId])
REFERENCES [dbo].[usrAcumaticaSalesOrder] ([Id])
GO
ALTER TABLE [dbo].[usrAcumaticaSoShipmentInvoice] CHECK CONSTRAINT [FK_usrAcumaticaSoShipmentInvoice_usrAcumaticaSalesOrder]
GO
ALTER TABLE [dbo].[usrAcumaticaWarehouseDetails]  WITH CHECK ADD  CONSTRAINT [FK_usrAcumaticaWarehouseDetails_usrAcumaticaStockItem] FOREIGN KEY([ParentMonsterId])
REFERENCES [dbo].[usrAcumaticaStockItem] ([MonsterId])
GO
ALTER TABLE [dbo].[usrAcumaticaWarehouseDetails] CHECK CONSTRAINT [FK_usrAcumaticaWarehouseDetails_usrAcumaticaStockItem]
GO
ALTER TABLE [dbo].[usrAcumaticaWarehouseDetails]  WITH CHECK ADD  CONSTRAINT [FK_usrAcumaticaWarehouseDetails_usrAcumaticaWarehouse] FOREIGN KEY([WarehouseMonsterId])
REFERENCES [dbo].[usrAcumaticaWarehouse] ([Id])
GO
ALTER TABLE [dbo].[usrAcumaticaWarehouseDetails] CHECK CONSTRAINT [FK_usrAcumaticaWarehouseDetails_usrAcumaticaWarehouse]
GO
ALTER TABLE [dbo].[usrInventoryReceiptSync]  WITH CHECK ADD  CONSTRAINT [FK_usrInventoryReceiptSync_usrAcumaticaInventoryReceipt] FOREIGN KEY([AcumaticaInvReceiptMonsterId])
REFERENCES [dbo].[usrAcumaticaInventoryReceipt] ([MonsterId])
GO
ALTER TABLE [dbo].[usrInventoryReceiptSync] CHECK CONSTRAINT [FK_usrInventoryReceiptSync_usrAcumaticaInventoryReceipt]
GO
ALTER TABLE [dbo].[usrInventoryReceiptSync]  WITH CHECK ADD  CONSTRAINT [FK_usrInventoryReceiptSync_usrShopifyInventoryLevels] FOREIGN KEY([ShopifyInventoryMonsterId])
REFERENCES [dbo].[usrShopifyInventoryLevel] ([MonsterId])
GO
ALTER TABLE [dbo].[usrInventoryReceiptSync] CHECK CONSTRAINT [FK_usrInventoryReceiptSync_usrShopifyInventoryLevels]
GO
ALTER TABLE [dbo].[usrShopAcuCustomerSync]  WITH CHECK ADD  CONSTRAINT [FK_usrShopAcuCustomerSync_usrAcumaticaCustomer] FOREIGN KEY([AcumaticaCustomerMonsterId])
REFERENCES [dbo].[usrAcumaticaCustomer] ([Id])
GO
ALTER TABLE [dbo].[usrShopAcuCustomerSync] CHECK CONSTRAINT [FK_usrShopAcuCustomerSync_usrAcumaticaCustomer]
GO
ALTER TABLE [dbo].[usrShopAcuCustomerSync]  WITH CHECK ADD  CONSTRAINT [FK_usrShopAcuCustomerSync_usrShopifyCustomer] FOREIGN KEY([ShopifyCustomerMonsterId])
REFERENCES [dbo].[usrShopifyCustomer] ([Id])
GO
ALTER TABLE [dbo].[usrShopAcuCustomerSync] CHECK CONSTRAINT [FK_usrShopAcuCustomerSync_usrShopifyCustomer]
GO
ALTER TABLE [dbo].[usrShopAcuItemSync]  WITH CHECK ADD  CONSTRAINT [FK_usrShopAcuItemSync_usrAcumaticaStockItem] FOREIGN KEY([AcumaticaItemMonsterId])
REFERENCES [dbo].[usrAcumaticaStockItem] ([MonsterId])
GO
ALTER TABLE [dbo].[usrShopAcuItemSync] CHECK CONSTRAINT [FK_usrShopAcuItemSync_usrAcumaticaStockItem]
GO
ALTER TABLE [dbo].[usrShopAcuItemSync]  WITH CHECK ADD  CONSTRAINT [FK_usrShopAcuItemSync_usrShopifyVariant] FOREIGN KEY([ShopifyVariantMonsterId])
REFERENCES [dbo].[usrShopifyVariant] ([MonsterId])
GO
ALTER TABLE [dbo].[usrShopAcuItemSync] CHECK CONSTRAINT [FK_usrShopAcuItemSync_usrShopifyVariant]
GO
ALTER TABLE [dbo].[usrShopAcuOrderSync]  WITH CHECK ADD  CONSTRAINT [FK_usrShopAcuOrderSync_usrAcumaticaSalesOrder] FOREIGN KEY([AcumaticaSalesOrderMonsterId])
REFERENCES [dbo].[usrAcumaticaSalesOrder] ([Id])
GO
ALTER TABLE [dbo].[usrShopAcuOrderSync] CHECK CONSTRAINT [FK_usrShopAcuOrderSync_usrAcumaticaSalesOrder]
GO
ALTER TABLE [dbo].[usrShopAcuOrderSync]  WITH CHECK ADD  CONSTRAINT [FK_usrShopAcuOrderSync_usrShopifyOrder] FOREIGN KEY([ShopifyOrderMonsterId])
REFERENCES [dbo].[usrShopifyOrder] ([Id])
GO
ALTER TABLE [dbo].[usrShopAcuOrderSync] CHECK CONSTRAINT [FK_usrShopAcuOrderSync_usrShopifyOrder]
GO
ALTER TABLE [dbo].[usrShopAcuRefundCM]  WITH CHECK ADD  CONSTRAINT [FK_usrShopAcuRefundWithInv_usrShopifyRefund] FOREIGN KEY([ShopifyRefundMonsterId])
REFERENCES [dbo].[usrShopifyRefund] ([Id])
GO
ALTER TABLE [dbo].[usrShopAcuRefundCM] CHECK CONSTRAINT [FK_usrShopAcuRefundWithInv_usrShopifyRefund]
GO
ALTER TABLE [dbo].[usrShopAcuShipmentSync]  WITH CHECK ADD  CONSTRAINT [FK_usrShopAcuShipmentSync_usrAcumaticaShipmentSO] FOREIGN KEY([AcumaticaShipDetailMonsterId])
REFERENCES [dbo].[usrAcumaticaShipmentSalesOrderRef] ([Id])
GO
ALTER TABLE [dbo].[usrShopAcuShipmentSync] CHECK CONSTRAINT [FK_usrShopAcuShipmentSync_usrAcumaticaShipmentSO]
GO
ALTER TABLE [dbo].[usrShopAcuShipmentSync]  WITH CHECK ADD  CONSTRAINT [FK_usrShopAcuShipmentSync_usrShopifyFulfillment] FOREIGN KEY([ShopifyFulfillmentMonsterId])
REFERENCES [dbo].[usrShopifyFulfillment] ([Id])
GO
ALTER TABLE [dbo].[usrShopAcuShipmentSync] CHECK CONSTRAINT [FK_usrShopAcuShipmentSync_usrShopifyFulfillment]
GO
ALTER TABLE [dbo].[usrShopAcuWarehouseSync]  WITH CHECK ADD  CONSTRAINT [FK_usrShopAcuWarehouseSync_usrAcumaticaWarehouse] FOREIGN KEY([AcumaticaWarehouseMonsterId])
REFERENCES [dbo].[usrAcumaticaWarehouse] ([Id])
GO
ALTER TABLE [dbo].[usrShopAcuWarehouseSync] CHECK CONSTRAINT [FK_usrShopAcuWarehouseSync_usrAcumaticaWarehouse]
GO
ALTER TABLE [dbo].[usrShopAcuWarehouseSync]  WITH CHECK ADD  CONSTRAINT [FK_usrShopAcuWarehouseSync_usrShopifyLocation] FOREIGN KEY([ShopifyLocationMonsterId])
REFERENCES [dbo].[usrShopifyLocation] ([MonsterId])
GO
ALTER TABLE [dbo].[usrShopAcuWarehouseSync] CHECK CONSTRAINT [FK_usrShopAcuWarehouseSync_usrShopifyLocation]
GO
ALTER TABLE [dbo].[usrShopifyAcuPayment]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyAcuPayment_usrShopifyTransaction] FOREIGN KEY([ShopifyTransactionMonsterId])
REFERENCES [dbo].[usrShopifyTransaction] ([Id])
GO
ALTER TABLE [dbo].[usrShopifyAcuPayment] CHECK CONSTRAINT [FK_usrShopifyAcuPayment_usrShopifyTransaction]
GO
ALTER TABLE [dbo].[usrShopifyFulfillment]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyFulfillment_usrShopifyOrder] FOREIGN KEY([OrderMonsterId])
REFERENCES [dbo].[usrShopifyOrder] ([Id])
GO
ALTER TABLE [dbo].[usrShopifyFulfillment] CHECK CONSTRAINT [FK_usrShopifyFulfillment_usrShopifyOrder]
GO
ALTER TABLE [dbo].[usrShopifyInventoryLevel]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyInventoryLevels_usrShopifyLocation] FOREIGN KEY([LocationMonsterId])
REFERENCES [dbo].[usrShopifyLocation] ([MonsterId])
GO
ALTER TABLE [dbo].[usrShopifyInventoryLevel] CHECK CONSTRAINT [FK_usrShopifyInventoryLevels_usrShopifyLocation]
GO
ALTER TABLE [dbo].[usrShopifyInventoryLevel]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyInventoryLevels_usrShopifyVariant] FOREIGN KEY([ParentMonsterId])
REFERENCES [dbo].[usrShopifyVariant] ([MonsterId])
GO
ALTER TABLE [dbo].[usrShopifyInventoryLevel] CHECK CONSTRAINT [FK_usrShopifyInventoryLevels_usrShopifyVariant]
GO
ALTER TABLE [dbo].[usrShopifyOrder]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyOrder_usrShopifyCustomer] FOREIGN KEY([CustomerMonsterId])
REFERENCES [dbo].[usrShopifyCustomer] ([Id])
GO
ALTER TABLE [dbo].[usrShopifyOrder] CHECK CONSTRAINT [FK_usrShopifyOrder_usrShopifyCustomer]
GO
ALTER TABLE [dbo].[usrShopifyPayoutTransaction]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyPayoutTransaction_usrShopifyPayout] FOREIGN KEY([MonsterParentId])
REFERENCES [dbo].[usrShopifyPayout] ([Id])
GO
ALTER TABLE [dbo].[usrShopifyPayoutTransaction] CHECK CONSTRAINT [FK_usrShopifyPayoutTransaction_usrShopifyPayout]
GO
ALTER TABLE [dbo].[usrShopifyRefund]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyRefund_usrShopifyOrder] FOREIGN KEY([OrderMonsterId])
REFERENCES [dbo].[usrShopifyOrder] ([Id])
GO
ALTER TABLE [dbo].[usrShopifyRefund] CHECK CONSTRAINT [FK_usrShopifyRefund_usrShopifyOrder]
GO
ALTER TABLE [dbo].[usrShopifyTransaction]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyTransaction_usrShopifyOrder] FOREIGN KEY([OrderMonsterId])
REFERENCES [dbo].[usrShopifyOrder] ([Id])
GO
ALTER TABLE [dbo].[usrShopifyTransaction] CHECK CONSTRAINT [FK_usrShopifyTransaction_usrShopifyOrder]
GO
ALTER TABLE [dbo].[usrShopifyVariant]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyVariant_usrShopifyProduct] FOREIGN KEY([ParentMonsterId])
REFERENCES [dbo].[usrShopifyProduct] ([MonsterId])
GO
ALTER TABLE [dbo].[usrShopifyVariant] CHECK CONSTRAINT [FK_usrShopifyVariant_usrShopifyProduct]
GO
