USE [Monster0001]
GO

ALTER TABLE [dbo].[usrShopifyVariant] DROP CONSTRAINT [FK_usrShopifyVariant_usrShopifyProduct]
GO
ALTER TABLE [dbo].[usrShopifyPayoutTransaction] DROP CONSTRAINT [FK_usrShopifyPayoutTransaction_usrShopifyPayout]
GO


DROP TABLE [dbo].[usrShopifyVariant]
GO

DROP TABLE [dbo].[usrShopifyProduct]
GO
/****** Object:  Table [dbo].[usrShopifyPayoutTransaction]    Script Date: 10/2/2018 6:08:37 PM ******/
DROP TABLE [dbo].[usrShopifyPayoutTransaction]
GO
/****** Object:  Table [dbo].[usrShopifyPayout]    Script Date: 10/2/2018 6:08:37 PM ******/
DROP TABLE [dbo].[usrShopifyPayout]
GO
/****** Object:  Table [dbo].[usrShopifyLocation]    Script Date: 10/2/2018 6:08:37 PM ******/
DROP TABLE [dbo].[usrShopifyLocation]
GO
/****** Object:  Table [dbo].[usrPreferences]    Script Date: 10/2/2018 6:08:37 PM ******/
DROP TABLE [dbo].[usrPreferences]
GO
/****** Object:  Table [dbo].[usrPayoutPreferences]    Script Date: 10/2/2018 6:08:37 PM ******/
DROP TABLE [dbo].[usrPayoutPreferences]
GO
/****** Object:  Table [dbo].[usrPayoutPreferences]    Script Date: 10/2/2018 6:08:37 PM ******/
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
/****** Object:  Table [dbo].[usrPreferences]    Script Date: 10/2/2018 6:08:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrPreferences](
	[DefaultItemClass] [varchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopifyLocation]    Script Date: 10/2/2018 6:08:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyLocation](
	[ShopifyLocationId] [bigint] NOT NULL,
	[ShopifyJson] [nvarchar](max) NULL,
	[DateCreated] [datetime] NULL,
	[LastUpdated] [datetime] NULL,
 CONSTRAINT [PK_usrShopifyLocation] PRIMARY KEY CLUSTERED 
(
	[ShopifyLocationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopifyPayout]    Script Date: 10/2/2018 6:08:38 PM ******/
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
/****** Object:  Table [dbo].[usrShopifyPayoutTransaction]    Script Date: 10/2/2018 6:08:38 PM ******/
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
/****** Object:  Table [dbo].[usrShopifyProduct]    Script Date: 10/2/2018 6:08:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyProduct](
	[ShopifyProductId] [bigint] NOT NULL,
	[ShopifyJson] [nvarchar](max) NULL,
	[DateCreated] [datetime] NULL,
	[LastUpdated] [datetime] NULL,
 CONSTRAINT [PK_usrShopifyProduct] PRIMARY KEY CLUSTERED 
(
	[ShopifyProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopifyVariant]    Script Date: 10/2/2018 6:08:38 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyVariant](
	[ShopifyVariantId] [bigint] NOT NULL,
	[ShopifyProductId] [bigint] NULL,
	[ShopifyJson] [nvarchar](max) NULL,
	[ShopifySku] [varchar](100) NULL,
	[DateCreated] [datetime] NULL,
	[LastUpdated] [datetime] NULL,
 CONSTRAINT [PK_usrShopifyVariant] PRIMARY KEY CLUSTERED 
(
	[ShopifyVariantId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[usrShopifyPayoutTransaction]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyPayoutTransaction_usrShopifyPayout] FOREIGN KEY([ShopifyPayoutId])
REFERENCES [dbo].[usrShopifyPayout] ([ShopifyPayoutId])
GO
ALTER TABLE [dbo].[usrShopifyPayoutTransaction] CHECK CONSTRAINT [FK_usrShopifyPayoutTransaction_usrShopifyPayout]
GO
ALTER TABLE [dbo].[usrShopifyVariant]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyVariant_usrShopifyProduct] FOREIGN KEY([ShopifyProductId])
REFERENCES [dbo].[usrShopifyProduct] ([ShopifyProductId])
GO
ALTER TABLE [dbo].[usrShopifyVariant] CHECK CONSTRAINT [FK_usrShopifyVariant_usrShopifyProduct]
GO
