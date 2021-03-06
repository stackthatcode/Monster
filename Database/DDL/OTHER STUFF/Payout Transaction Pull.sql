USE [Monster]
GO
/****** Object:  Table [dbo].[usrPayoutPreferences]    Script Date: 9/19/2018 7:46:19 PM ******/
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
/****** Object:  Table [dbo].[usrShopifyPayout]    Script Date: 9/19/2018 7:46:19 PM ******/
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
/****** Object:  Table [dbo].[usrShopifyPayoutTransaction]    Script Date: 9/19/2018 7:46:19 PM ******/
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
ALTER TABLE [dbo].[usrShopifyPayoutTransaction]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyPayoutTransaction_usrShopifyPayout] FOREIGN KEY([ShopifyPayoutId])
REFERENCES [dbo].[usrShopifyPayout] ([ShopifyPayoutId])
GO
ALTER TABLE [dbo].[usrShopifyPayoutTransaction] CHECK CONSTRAINT [FK_usrShopifyPayoutTransaction_usrShopifyPayout]
GO
