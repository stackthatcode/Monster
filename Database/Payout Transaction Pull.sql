USE [Monster]
GO
/****** Object:  Table [dbo].[usrPayoutPreferences]    Script Date: 9/18/2018 6:14:15 PM ******/
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
/****** Object:  Table [dbo].[usrShopifyPayout]    Script Date: 9/18/2018 6:14:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyPayout](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ShopifyPayoutId] [bigint] NOT NULL,
	[Json] [text] NOT NULL,
	[AllDetailRecordsCaptured] [bit] NOT NULL,
	[CreatedDate] [datetime] NULL,
	[UpdatedDate] [datetime] NULL,
	[AcumaticaRefNumber] [varchar](50) NULL,
	[AcumaticaImportDate] [datetime] NULL,
 CONSTRAINT [PK_usrShopifyPayout] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[usrShopifyPayoutTransaction]    Script Date: 9/18/2018 6:14:15 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[usrShopifyPayoutTransaction](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ShopifyPayoutId] [bigint] NOT NULL,
	[ShopifyPayoutTransId] [bigint] NOT NULL,
	[Type] [varchar](50) NULL,
	[Json] [text] NULL,
	[CreatedDate] [datetime] NULL,
 CONSTRAINT [PK_usrShopifyPayoutTransaction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
