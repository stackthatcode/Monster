USE [Monster0001]
GO
/****** Object:  Table [dbo].[usrAcumaticaStockItem]    Script Date: 10/19/2018 6:28:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrAcumaticaStockItem](
	[MonsterId] [bigint] IDENTITY(1,1) NOT NULL,
	[ItemId] [varchar](100) NULL,
	[AcumaticaJson] [nvarchar](max) NULL,
	[ShopifyVariantMonsterId] [bigint] NULL,
	[DateCreated] [datetime] NULL,
	[LastUpdated] [datetime] NULL,
 CONSTRAINT [PK_usrAcumaticaStockItem] PRIMARY KEY CLUSTERED 
(
	[MonsterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrAcumaticaWarehouse]    Script Date: 10/19/2018 6:28:39 PM ******/
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
/****** Object:  Table [dbo].[usrBatchState]    Script Date: 10/19/2018 6:28:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrBatchState](
	[Id] [bigint] NOT NULL,
	[ShopifyProductsPullEnd] [datetime] NULL,
	[ShopifyOrdersPullEnd] [datetime] NULL,
	[AcumaticaProductsPullEnd] [datetime] NULL,
 CONSTRAINT [PK_usrShopifyBatchState] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrPayoutPreferences]    Script Date: 10/19/2018 6:28:39 PM ******/
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
/****** Object:  Table [dbo].[usrPreferences]    Script Date: 10/19/2018 6:28:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrPreferences](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyOrderPullStart] [datetime] NULL,
	[AcumaticaDefaultItemClass] [varchar](50) NULL,
 CONSTRAINT [PK_usrPreferences] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopifyLocation]    Script Date: 10/19/2018 6:28:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyLocation](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyLocationId] [bigint] NOT NULL,
	[ShopifyJson] [nvarchar](max) NOT NULL,
	[ShopifyLocationName] [varchar](50) NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyLocation_1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopifyPayout]    Script Date: 10/19/2018 6:28:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyPayout](
	[ShopifyPayoutId] [bigint] NOT NULL,
	[ShopifyLastStatus] [varchar](50) NOT NULL,
	[Json] [text] NOT NULL,
	[AllShopifyTransDownloaded] [bit] NOT NULL,
	[CreatedDate] [datetime] NULL,
	[UpdatedDate] [datetime] NULL,
	[AcumaticaCashAccount] [varchar](50) NULL,
	[AcumaticaRefNumber] [varchar](50) NULL,
	[AcumaticaImportDate] [datetime] NULL,
 CONSTRAINT [PK_usrShopifyPayout_1] PRIMARY KEY CLUSTERED 
(
	[ShopifyPayoutId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopifyPayoutTransaction]    Script Date: 10/19/2018 6:28:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyPayoutTransaction](
	[ShopifyPayoutId] [bigint] NOT NULL,
	[ShopifyPayoutTransId] [bigint] NOT NULL,
	[Type] [varchar](50) NULL,
	[Json] [text] NULL,
	[CreatedDate] [datetime] NULL,
	[AcumaticaImportDate] [datetime] NULL,
	[AcumaticaExtRefNbr] [varchar](50) NULL,
 CONSTRAINT [PK_usrShopifyPayoutTransaction_1] PRIMARY KEY CLUSTERED 
(
	[ShopifyPayoutId] ASC,
	[ShopifyPayoutTransId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopifyProduct]    Script Date: 10/19/2018 6:28:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyProduct](
	[MonsterId] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyProductId] [bigint] NOT NULL,
	[ShopifyJson] [nvarchar](max) NULL,
	[IsDeleted] [bit] NULL,
	[DateCreated] [datetime] NULL,
	[LastUpdated] [datetime] NULL,
 CONSTRAINT [PK_usrShopifyProduct] PRIMARY KEY CLUSTERED 
(
	[MonsterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopifyVariant]    Script Date: 10/19/2018 6:28:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyVariant](
	[MonsterId] [bigint] IDENTITY(1,1) NOT NULL,
	[ParentMonsterId] [bigint] NULL,
	[ShopifyVariantId] [bigint] NULL,
	[ShopifySku] [varchar](100) NULL,
	[IsMissing] [bit] NULL,
	[ShopifyInventoryItemId] [bigint] NULL,
	[ShopifyInventoryJson] [nvarchar](max) NULL,
	[DateCreated] [datetime] NULL,
	[LastUpdated] [datetime] NULL,
 CONSTRAINT [PK_usrShopifyVariant_1] PRIMARY KEY CLUSTERED 
(
	[MonsterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrTenant]    Script Date: 10/19/2018 6:28:39 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrTenant](
	[CompanyID] [bigint] NOT NULL,
	[ShopifyDomain] [varchar](500) NULL,
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
	[CompanyID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[usrAcumaticaStockItem]  WITH CHECK ADD  CONSTRAINT [FK_usrAcumaticaStockItem_usrShopifyVariant] FOREIGN KEY([ShopifyVariantMonsterId])
REFERENCES [dbo].[usrShopifyVariant] ([MonsterId])
GO
ALTER TABLE [dbo].[usrAcumaticaStockItem] CHECK CONSTRAINT [FK_usrAcumaticaStockItem_usrShopifyVariant]
GO
ALTER TABLE [dbo].[usrShopifyPayoutTransaction]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyPayoutTransaction_usrShopifyPayout] FOREIGN KEY([ShopifyPayoutId])
REFERENCES [dbo].[usrShopifyPayout] ([ShopifyPayoutId])
GO
ALTER TABLE [dbo].[usrShopifyPayoutTransaction] CHECK CONSTRAINT [FK_usrShopifyPayoutTransaction_usrShopifyPayout]
GO
ALTER TABLE [dbo].[usrShopifyVariant]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyVariant_usrShopifyProduct] FOREIGN KEY([ParentMonsterId])
REFERENCES [dbo].[usrShopifyProduct] ([MonsterId])
GO
ALTER TABLE [dbo].[usrShopifyVariant] CHECK CONSTRAINT [FK_usrShopifyVariant_usrShopifyProduct]
GO
