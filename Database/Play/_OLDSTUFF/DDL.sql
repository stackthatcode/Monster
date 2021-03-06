USE [Bundler]
GO
/****** Object:  Table [dbo].[ExclusionConstraints]    Script Date: 5/1/2018 12:31:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ExclusionConstraints](
	[SourceProductVariantId] [int] NOT NULL,
	[TargetProductVariantId] [int] NOT NULL,
 CONSTRAINT [PK_ExclusionConstraints] PRIMARY KEY CLUSTERED 
(
	[SourceProductVariantId] ASC,
	[TargetProductVariantId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ProductTypes]    Script Date: 5/1/2018 12:31:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductTypes](
	[Id] [int] NOT NULL,
	[Description] [varchar](50) NOT NULL,
 CONSTRAINT [PK_ProductType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ProductVariants]    Script Date: 5/1/2018 12:31:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProductVariants](
	[Id] [int] NOT NULL,
	[ProductTypeId] [int] NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[ShopifyProductHandle] [varchar](100) NOT NULL,
	[ShopifySku] [varchar](100) NOT NULL,
 CONSTRAINT [PK_ProductVariant] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[ExclusionConstraints]  WITH CHECK ADD  CONSTRAINT [FK_ExclusionConstraints_ProductVariants] FOREIGN KEY([SourceProductVariantId])
REFERENCES [dbo].[ProductVariants] ([Id])
GO
ALTER TABLE [dbo].[ExclusionConstraints] CHECK CONSTRAINT [FK_ExclusionConstraints_ProductVariants]
GO
ALTER TABLE [dbo].[ExclusionConstraints]  WITH CHECK ADD  CONSTRAINT [FK_ExclusionConstraints_ProductVariants1] FOREIGN KEY([TargetProductVariantId])
REFERENCES [dbo].[ProductVariants] ([Id])
GO
ALTER TABLE [dbo].[ExclusionConstraints] CHECK CONSTRAINT [FK_ExclusionConstraints_ProductVariants1]
GO
ALTER TABLE [dbo].[ProductVariants]  WITH CHECK ADD  CONSTRAINT [FK_ProductVariant_ProductType] FOREIGN KEY([ProductTypeId])
REFERENCES [dbo].[ProductTypes] ([Id])
GO
ALTER TABLE [dbo].[ProductVariants] CHECK CONSTRAINT [FK_ProductVariant_ProductType]
GO
