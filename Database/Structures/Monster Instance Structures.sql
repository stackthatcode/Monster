USE [master]
GO
/****** Object:  Database [Monster0003]    Script Date: 12/2/2019 3:45:15 PM ******/
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'Monster0003')
BEGIN
CREATE DATABASE [Monster0003]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Monster0003', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\Monster0003.mdf' , SIZE = 73728KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Monster0003_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\Monster0003_log.ldf' , SIZE = 270336KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
END
GO
ALTER DATABASE [Monster0003] SET COMPATIBILITY_LEVEL = 140
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Monster0003].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Monster0003] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Monster0003] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Monster0003] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Monster0003] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Monster0003] SET ARITHABORT OFF 
GO
ALTER DATABASE [Monster0003] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [Monster0003] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Monster0003] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Monster0003] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Monster0003] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Monster0003] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Monster0003] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Monster0003] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Monster0003] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Monster0003] SET  DISABLE_BROKER 
GO
ALTER DATABASE [Monster0003] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Monster0003] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Monster0003] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Monster0003] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Monster0003] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Monster0003] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [Monster0003] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Monster0003] SET RECOVERY FULL 
GO
ALTER DATABASE [Monster0003] SET  MULTI_USER 
GO
ALTER DATABASE [Monster0003] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Monster0003] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Monster0003] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Monster0003] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [Monster0003] SET DELAYED_DURABILITY = DISABLED 
GO
EXEC sys.sp_db_vardecimal_storage_format N'Monster0003', N'ON'
GO
ALTER DATABASE [Monster0003] SET QUERY_STORE = OFF
GO
USE [Monster0003]
GO
/****** Object:  User [IIS APPPOOL\DefaultAppPool]    Script Date: 12/2/2019 3:45:16 PM ******/
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'IIS APPPOOL\DefaultAppPool')
CREATE USER [IIS APPPOOL\DefaultAppPool] FOR LOGIN [IIS APPPOOL\DefaultAppPool] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [IIS APPPOOL\DefaultAppPool]
GO
/****** Object:  Table [dbo].[ShopifyCustomer]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopifyCustomer]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ShopifyCustomer](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyJson] [nvarchar](max) NOT NULL,
	[ShopifyCustomerId] [bigint] NOT NULL,
	[ShopifyPrimaryEmail] [varchar](100) NOT NULL,
	[NeedsCustomerPut] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyCustomer] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[ShopifyOrder]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopifyOrder]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ShopifyOrder](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ShopifyJson] [nvarchar](max) NOT NULL,
	[ShopifyOrderId] [bigint] NOT NULL,
	[ShopifyOrderNumber] [varchar](25) NOT NULL,
	[ShopifyIsCancelled] [bit] NOT NULL,
	[ShopifyFinancialStatus] [varchar](25) NOT NULL,
	[NeedsTransactionGet] [bit] NOT NULL,
	[NeedsOrderPut] [bit] NOT NULL,
	[IsBlocked] [bit] NOT NULL,
	[CustomerMonsterId] [bigint] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyOrder] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  View [dbo].[vw_ShopifyOrderCustomer]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_ShopifyOrderCustomer]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vw_ShopifyOrderCustomer]
AS
SELECT 
	t1.Id, 
	t1.ShopifyOrderId, 
	t1.ShopifyOrderNumber, 
	t1.ShopifyFinancialStatus, 
	t1.NeedsTransactionGet,
	t1.NeedsOrderPut,
	t2.ShopifyCustomerId,
	t2.ShopifyPrimaryEmail,
	t2.NeedsCustomerPut,
	t1.LastUpdated AS OrderLastUpdated,
	t2.LastUpdated AS CustomerLastUpdated
FROM ShopifyOrder t1
	LEFT OUTER JOIN ShopifyCustomer t2
		ON t2.Id = t1.CustomerMonsterId;
' 
GO
/****** Object:  Table [dbo].[ShopifyRefund]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopifyRefund]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  View [dbo].[vw_ShopifyOrderRefunds]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_ShopifyOrderRefunds]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vw_ShopifyOrderRefunds]
AS
SELECT 
	t1.Id, 
	t1.ShopifyOrderId, 
	t1.ShopifyOrderNumber, 
	t1.ShopifyFinancialStatus, 
	t1.NeedsTransactionGet,
	t1.NeedsOrderPut,
	t2.ShopifyRefundId,
	t1.LastUpdated AS OrderLastUpdated,
	t2.LastUpdated AS RefundLastUpdated
FROM ShopifyOrder t1
	LEFT OUTER JOIN ShopifyRefund t2
		ON t2.OrderMonsterId = t1.Id;
' 
GO
/****** Object:  Table [dbo].[ShopifyFulfillment]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopifyFulfillment]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  View [dbo].[vw_ShopifyOrderFulfillments]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_ShopifyOrderFulfillments]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vw_ShopifyOrderFulfillments]
AS
SELECT 
	t1.Id, 
	t1.ShopifyOrderId, 
	t1.ShopifyOrderNumber, 
	t1.ShopifyFinancialStatus, 
	t1.NeedsTransactionGet,
	t1.NeedsOrderPut,
	t2.ShopifyFulfillmentId,
	t2.ShopifyStatus,
	t1.LastUpdated AS OrderLastUpdated,
	t2.LastUpdated AS FulfillmentLastUpdated
FROM ShopifyOrder t1
	LEFT OUTER JOIN ShopifyFulfillment t2
		ON t2.OrderMonsterId = t1.Id;
' 
GO
/****** Object:  Table [dbo].[ShopifyTransaction]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopifyTransaction]') AND type in (N'U'))
BEGIN
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
	[OrderMonsterId] [bigint] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrShopifyTransaction] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  View [dbo].[vw_ShopifyOrderTransactions]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_ShopifyOrderTransactions]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vw_ShopifyOrderTransactions]
AS
SELECT 
	t1.Id, 
	t1.ShopifyOrderId, 
	t1.ShopifyOrderNumber, 
	t1.ShopifyFinancialStatus, 
	t1.NeedsTransactionGet,
	t1.NeedsOrderPut,
	t2.ShopifyTransactionId,
	t2.ShopifyKind,
	t2.ShopifyStatus,
	t1.LastUpdated AS OrderLastUpdated,
	t2.LastUpdated AS TransactionLastUpdated
FROM ShopifyOrder t1
	LEFT OUTER JOIN ShopifyTransaction t2
		ON t2.OrderMonsterId = t1.Id;
' 
GO
/****** Object:  Table [dbo].[AcumaticaSalesOrder]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AcumaticaSalesOrder]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AcumaticaSalesOrder](
	[ShopifyOrderMonsterId] [bigint] NOT NULL,
	[AcumaticaShipmentDetailsJson] [nvarchar](max) NULL,
	[AcumaticaOrderNbr] [varchar](50) NOT NULL,
	[AcumaticaStatus] [varchar](25) NOT NULL,
	[ShopifyCustomerMonsterId] [bigint] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_AcumaticaSalesOrder] PRIMARY KEY CLUSTERED 
(
	[ShopifyOrderMonsterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[AcumaticaCustomer]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AcumaticaCustomer]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  View [dbo].[vw_AcumaticaSalesOrderAndCustomer]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_AcumaticaSalesOrderAndCustomer]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vw_AcumaticaSalesOrderAndCustomer]
AS
	SELECT t1.ShopifyOrderMonsterId, 
		t1.AcumaticaOrderNbr, 
		t1.AcumaticaStatus,
		t2.AcumaticaCustomerId,
		t1.LastUpdated AS SalesOrderLastUpdated,
		t2.LastUpdated AS CustomerLastUpdated
	FROM AcumaticaSalesOrder t1
		LEFT OUTER JOIN AcumaticaCustomer t2
			ON t2.ShopifyCustomerMonsterId = t1.ShopifyCustomerMonsterId;
' 
GO
/****** Object:  Table [dbo].[AcumaticaSoShipment]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AcumaticaSoShipment]') AND type in (N'U'))
BEGIN
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
	[ShopifyFulfillmentMonsterId] [bigint] NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrAcumaticaSoShipmentInvoice] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  View [dbo].[vw_AcumaticaSalesOrderAndShipmentInvoices]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_AcumaticaSalesOrderAndShipmentInvoices]'))
EXEC dbo.sp_executesql @statement = N'
CREATE VIEW [dbo].[vw_AcumaticaSalesOrderAndShipmentInvoices]
AS
	SELECT t1.ShopifyOrderMonsterId, 
		t1.AcumaticaOrderNbr, 
		t1.AcumaticaStatus,
		t2.AcumaticaShipmentNbr,
		t2.AcumaticaInvoiceNbr,
		t2.AcumaticaTrackingNbr,
		t1.LastUpdated AS SalesOrderLastUpdated,
		t2.LastUpdated AS ShipmentLastUpdated
	FROM AcumaticaSalesOrder t1
		LEFT OUTER JOIN AcumaticaSoShipment t2
			ON t2.ShopifyOrderMonsterId = t1.ShopifyOrderMonsterId;
' 
GO
/****** Object:  View [dbo].[vw_SyncCustomerWithCustomers]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_SyncCustomerWithCustomers]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vw_SyncCustomerWithCustomers]
AS
SELECT  
	t1.ShopifyCustomerId, 
	t1.ShopifyPrimaryEmail,
	t1.NeedsCustomerPut,
	t1.LastUpdated AS ShopifyLastUpdated,
	t2.AcumaticaMainContactEmail,
	t2.LastUpdated AS AcumaticaLastUpdated
FROM ShopifyCustomer t1
	FULL OUTER JOIN AcumaticaCustomer t2
		ON t1.Id = t2.ShopifyCustomerMonsterId
' 
GO
/****** Object:  View [dbo].[vw_SyncOrdersAndSalesOrders]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_SyncOrdersAndSalesOrders]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vw_SyncOrdersAndSalesOrders]
AS
	SELECT t1.ShopifyOrderId, 
		t1.ShopifyOrderNumber, 
		t1.ShopifyIsCancelled, 
		t1.ShopifyFinancialStatus,
		t1.NeedsOrderPut,
		t1.NeedsTransactionGet,
		t2.AcumaticaOrderNbr,
		t2.AcumaticaStatus,
		t3.AcumaticaInvoiceNbr,
		t3.AcumaticaShipmentNbr,
		t1.LastUpdated AS ShopifyLastUpdated,
		t2.LastUpdated AS AcumaticaLastUpdated
	FROM ShopifyOrder t1
		FULL OUTER JOIN AcumaticaSalesOrder t2
			ON t2.ShopifyOrderMonsterId = t1.Id
		FULL OUTER JOIN AcumaticaSoShipment t3
			ON t2.ShopifyOrderMonsterId = t3.ShopifyOrderMonsterId
' 
GO
/****** Object:  View [dbo].[vw_SyncFulfillmentsAndShipments]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_SyncFulfillmentsAndShipments]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vw_SyncFulfillmentsAndShipments]
AS
SELECT 
	t0.ShopifyOrderId,
	t0.ShopifyOrderNumber,
	t1.ShopifyFulfillmentId,
	t1.ShopifyStatus, 
	t2.AcumaticaShipmentNbr,
	t2.AcumaticaInvoiceNbr,
	t1.LastUpdated AS FulfillmentLastUpdated
FROM ShopifyOrder t0
	FULL OUTER JOIN ShopifyFulfillment t1
		ON t0.Id = t1.OrderMonsterId
	FULL OUTER JOIN AcumaticaSoShipment t2
		ON t1.Id = t2.ShopifyFulfillmentMonsterId
' 
GO
/****** Object:  Table [dbo].[AcumaticaPayment]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AcumaticaPayment]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  View [dbo].[vw_SyncTransactionAndPayment]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_SyncTransactionAndPayment]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vw_SyncTransactionAndPayment]
AS
SELECT 
	t1.ShopifyOrderId,
	t1.ShopifyOrderNumber,
	t1.NeedsTransactionGet,
	t2.ShopifyTransactionId,
	t2.ShopifyStatus,
	t2.ShopifyKind,
	t2.Ignore,
	t2.NeedsPaymentPut,
	t3.AcumaticaRefNbr,
	t3.AcumaticaDocType,
	t3.IsReleased,
	t2.LastUpdated AS ShopifyRefundLastUpdated,
	t3.LastUpdated AS PaymentSyncLastUpdated
FROM ShopifyOrder t1
	FULL OUTER JOIN ShopifyTransaction t2
		ON t1.Id = t2.OrderMonsterId
	FULL OUTER JOIN AcumaticaPayment t3
		ON t2.Id = t3.ShopifyTransactionMonsterId
' 
GO
/****** Object:  Table [dbo].[AcumaticaStockItem]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AcumaticaStockItem]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AcumaticaStockItem](
	[MonsterId] [bigint] IDENTITY(1,1) NOT NULL,
	[ItemId] [varchar](100) NOT NULL,
	[AcumaticaJson] [nvarchar](max) NOT NULL,
	[AcumaticaTaxCategory] [varchar](50) NOT NULL,
	[AcumaticaDescription] [varchar](200) NULL,
	[AcumaticaDefaultPrice] [money] NOT NULL,
	[AcumaticaLastCost] [money] NOT NULL,
	[IsPriceSynced] [bit] NOT NULL,
	[ShopifyVariantMonsterId] [bigint] NULL,
	[IsSyncEnabled] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrAcumaticaStockItem] PRIMARY KEY CLUSTERED 
(
	[MonsterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[AcumaticaInventory]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AcumaticaInventory]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  View [dbo].[vw_AcumaticaInventory]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_AcumaticaInventory]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vw_AcumaticaInventory]
AS
SELECT	t1.MonsterId, 
		t1.ItemId,
		t1.IsPriceSynced,
		t2.AcumaticaWarehouseId,
		t2.AcumaticaAvailQty,
		t2.IsInventorySynced,
		t1.LastUpdated AS StockItemLastUpdated,
		t2.LastUpdated AS WarehouseDetailsLastUpdated
FROM AcumaticaStockItem t1
	LEFT JOIN AcumaticaInventory t2
		ON t2.ParentMonsterId = t1.MonsterId
' 
GO
/****** Object:  Table [dbo].[ShopifyVariant]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopifyVariant]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  View [dbo].[vw_SyncVariantsAndStockItems]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_SyncVariantsAndStockItems]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vw_SyncVariantsAndStockItems]
AS
SELECT t1.ShopifyVariantId,
	t1.ShopifyInventoryItemId,
	t1.ShopifySku,
	t1.ShopifyCost,
	t1.ShopifyIsTracked,
	t1.IsMissing,
	t3.ItemId,
	t1.LastUpdated AS VariantLastUpdated,
	t3.LastUpdated AS StockItemLastUpdated
FROM ShopifyVariant t1
	FULL OUTER JOIN AcumaticaStockItem t3
		ON t3.ShopifyVariantMonsterId = t1.MonsterId
' 
GO
/****** Object:  Table [dbo].[ShopifyProduct]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopifyProduct]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  View [dbo].[vw_SyncVariantsAndStockItems_Alt]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_SyncVariantsAndStockItems_Alt]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vw_SyncVariantsAndStockItems_Alt]
AS
	SELECT	t2.MonsterId AS MonsterVariantId, t2.ShopifyProductId, t2.ShopifyTitle AS ShopifyProductTitle, t2.ShopifyVendor, t2.ShopifyProductType,
			t1.ShopifyVariantId, t1.ShopifySku, t1.ShopifyTitle AS ShopifyVariantTitle, t4.ItemId AS AcumaticaItemId,
			t4.AcumaticaDescription
	FROM ShopifyVariant t1 
		INNER JOIN ShopifyProduct t2
			ON t1.ParentMonsterId = t2.MonsterId
		INNER JOIN AcumaticaStockItem t4
			ON t1.MonsterId = t4.ShopifyVariantMonsterId;		
' 
GO
/****** Object:  Table [dbo].[ShopifyLocation]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopifyLocation]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [dbo].[ShopAcuWarehouseSync]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopAcuWarehouseSync]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [dbo].[ShopifyInventoryLevel]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopifyInventoryLevel]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  View [dbo].[vw_SyncShopifyInventory]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_SyncShopifyInventory]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vw_SyncShopifyInventory]
AS
SELECT	t1.MonsterId AS MonsterVariantId,
		t1.ShopifySku, 
		t1.ShopifyVariantId,
		t1.IsMissing,
		t2.ShopifyAvailableQuantity, 
		t2.ShopifyLocationId,
		t5.ShopifyLocationName,
		t3.Id AS LocationSyncId
FROM ShopifyVariant t1
	FULL OUTER JOIN ShopifyInventoryLevel t2
		ON t1.MonsterId = t2.ParentMonsterId
	FULL OUTER JOIN ShopAcuWarehouseSync t3
		ON t2.LocationMonsterId = t3.ShopifyLocationMonsterId
	INNER JOIN ShopifyLocation t5
		ON t2.LocationMonsterId = t5.MonsterId;
' 
GO
/****** Object:  View [dbo].[vw_SyncAcumaticaInventory]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_SyncAcumaticaInventory]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vw_SyncAcumaticaInventory]
AS
SELECT t1.MonsterId,
	t1.ShopifyVariantMonsterId,
	t1.ItemId AS AcumaticaItemId, 
	t3.AcumaticaWarehouseId, 
	t3.AcumaticaAvailQty, 
	t4.Id AS WarehouseSyncId,
	t1.IsPriceSynced,
	t3.IsInventorySynced
FROM AcumaticaStockItem t1
	FULL OUTER JOIN AcumaticaInventory t3
		ON t1.MonsterId = t3.ParentMonsterId
	FULL OUTER JOIN ShopAcuWarehouseSync t4
		ON t3.WarehouseMonsterId = t4.AcumaticaWarehouseMonsterId;
' 
GO
/****** Object:  View [dbo].[vw_SyncInventoryAllInclusive]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_SyncInventoryAllInclusive]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vw_SyncInventoryAllInclusive]
AS 
SELECT t1.ShopifySku, 
	t2.AcumaticaItemId,
	t1.ShopifyVariantId,
	t1.IsMissing,
	t1.ShopifyLocationId,
	t1.ShopifyLocationName,
	t2.AcumaticaWarehouseId,
	t1.ShopifyAvailableQuantity,
	t2.AcumaticaAvailQty
FROM vw_SyncShopifyInventory t1
	FULL OUTER JOIN vw_SyncAcumaticaInventory t2
		ON t1.MonsterVariantId = t2.ShopifyVariantMonsterId
' 
GO
/****** Object:  Table [dbo].[InventoryReceiptSync]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InventoryReceiptSync]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [dbo].[AcumaticaInventoryReceipt]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AcumaticaInventoryReceipt]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  View [dbo].[vw_SyncInventoryLevelAndReceipts]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_SyncInventoryLevelAndReceipts]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vw_SyncInventoryLevelAndReceipts]
AS
	SELECT 
		t4.ShopifySku,
		t1.MonsterId,
		t1.ShopifyInventoryItemId,
		t1.ShopifyLocationId,
		t1.ShopifyAvailableQuantity,
		t3.AcumaticaRefNumber,
		t3.IsReleased,
		t1.LastUpdated AS InventoryLevelLastUpdated,
		t3.LastUpdate AS InventoryReceiptLastUpdated
	FROM ShopifyInventoryLevel t1
		FULL OUTER JOIN InventoryReceiptSync t2
			ON t2.ShopifyInventoryMonsterId = t1.MonsterId
		FULL OUTER JOIN AcumaticaInventoryReceipt t3
			ON t3.MonsterId = t2.AcumaticaInvReceiptMonsterId
		FULL OUTER JOIN ShopifyVariant t4
			ON t4.MonsterId = t1.ParentMonsterId
' 
GO
/****** Object:  Table [dbo].[AcumaticaWarehouse]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AcumaticaWarehouse]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  View [dbo].[vw_SyncWarehousesAndLocations]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_SyncWarehousesAndLocations]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vw_SyncWarehousesAndLocations]
AS
SELECT t1.MonsterId AS ShopifyLocationMonsterId, 
		t1.ShopifyLocationName,
		t1.ShopifyLocationId,
		t2.Id AS AcumaticaWarehouseMonsterId,
		t2.AcumaticaWarehouseId
FROM ShopifyLocation t1
	FULL OUTER JOIN AcumaticaWarehouse t2
		ON t1.ShopifyLocationName = t2.AcumaticaWarehouseId;
' 
GO
/****** Object:  View [dbo].[vw_ShopifyInventory]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vw_ShopifyInventory]'))
EXEC dbo.sp_executesql @statement = N'CREATE VIEW [dbo].[vw_ShopifyInventory]
AS
SELECT	t1.MonsterId, 
		t1.ShopifyProductId,
		t1.ShopifyTitle AS ProductTitle,
		t1.ShopifyVendor,
		t1.ShopifyProductType,
		t1.IsDeleted AS ProductIsDeleted, 
		t2.ShopifyVariantId, 
		t2.ShopifyInventoryItemId, 
		t2.ShopifySku, 
		t2.ShopifyTitle AS VariantTitle,
		t2.ShopifyCost, 
		t2.IsMissing AS VariantIsMissing,
		t3.ShopifyLocationId,
		t3.ShopifyAvailableQuantity,
		t1.LastUpdated AS ProductLastUpdated,
		t2.LastUpdated AS VariantLastUpdated,
		t3.LastUpdated AS InventoryLevelLastUpdate
FROM ShopifyProduct t1
	LEFT JOIN ShopifyVariant t2
		ON t2.ParentMonsterId = t1.MonsterId
	LEFT JOIN ShopifyInventoryLevel t3
		ON t3.ParentMonsterId = t2.MonsterId;
' 
GO

/****** Object:  Table [dbo].[AcumaticaBatchState]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AcumaticaBatchState]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [dbo].[AcumaticaRefData]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AcumaticaRefData]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [dbo].[Connections]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Connections]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [dbo].[ExclusiveJobMonitor]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExclusiveJobMonitor]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ExclusiveJobMonitor](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[BackgroundJobType] [int] NOT NULL,
	[IsRecurring] [bit] NOT NULL,
	[HangFireJobId] [varchar](100) NULL,
	[ReceivedKillSignal] [bit] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_usrJobMonitor] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[ExecutionLog]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExecutionLog]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [dbo].[MonsterSettings]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MonsterSettings]') AND type in (N'U'))
BEGIN
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
 CONSTRAINT [PK_usrPreferences] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[PaymentGateways]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PaymentGateways]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [dbo].[PayoutPreferences]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PayoutPreferences]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[PayoutPreferences](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AcumaticaCashAccount] [varchar](50) NULL,
 CONSTRAINT [PK_usrPayoutPreferences] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[ShopifyBatchState]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopifyBatchState]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [dbo].[ShopifyPayout]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopifyPayout]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [dbo].[ShopifyPayoutTransaction]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShopifyPayoutTransaction]') AND type in (N'U'))
BEGIN
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
END
GO
/****** Object:  Table [dbo].[SystemState]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SystemState]') AND type in (N'U'))
BEGIN
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
END
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_AcumaticaCustomer_ShopifyCustomer]') AND parent_object_id = OBJECT_ID(N'[dbo].[AcumaticaCustomer]'))
ALTER TABLE [dbo].[AcumaticaCustomer]  WITH CHECK ADD  CONSTRAINT [FK_AcumaticaCustomer_ShopifyCustomer] FOREIGN KEY([ShopifyCustomerMonsterId])
REFERENCES [dbo].[ShopifyCustomer] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_AcumaticaCustomer_ShopifyCustomer]') AND parent_object_id = OBJECT_ID(N'[dbo].[AcumaticaCustomer]'))
ALTER TABLE [dbo].[AcumaticaCustomer] CHECK CONSTRAINT [FK_AcumaticaCustomer_ShopifyCustomer]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrAcumaticaWarehouseDetails_usrAcumaticaStockItem]') AND parent_object_id = OBJECT_ID(N'[dbo].[AcumaticaInventory]'))
ALTER TABLE [dbo].[AcumaticaInventory]  WITH CHECK ADD  CONSTRAINT [FK_usrAcumaticaWarehouseDetails_usrAcumaticaStockItem] FOREIGN KEY([ParentMonsterId])
REFERENCES [dbo].[AcumaticaStockItem] ([MonsterId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrAcumaticaWarehouseDetails_usrAcumaticaStockItem]') AND parent_object_id = OBJECT_ID(N'[dbo].[AcumaticaInventory]'))
ALTER TABLE [dbo].[AcumaticaInventory] CHECK CONSTRAINT [FK_usrAcumaticaWarehouseDetails_usrAcumaticaStockItem]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrAcumaticaWarehouseDetails_usrAcumaticaWarehouse]') AND parent_object_id = OBJECT_ID(N'[dbo].[AcumaticaInventory]'))
ALTER TABLE [dbo].[AcumaticaInventory]  WITH CHECK ADD  CONSTRAINT [FK_usrAcumaticaWarehouseDetails_usrAcumaticaWarehouse] FOREIGN KEY([WarehouseMonsterId])
REFERENCES [dbo].[AcumaticaWarehouse] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrAcumaticaWarehouseDetails_usrAcumaticaWarehouse]') AND parent_object_id = OBJECT_ID(N'[dbo].[AcumaticaInventory]'))
ALTER TABLE [dbo].[AcumaticaInventory] CHECK CONSTRAINT [FK_usrAcumaticaWarehouseDetails_usrAcumaticaWarehouse]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrShopifyAcuPayment_usrShopifyTransaction]') AND parent_object_id = OBJECT_ID(N'[dbo].[AcumaticaPayment]'))
ALTER TABLE [dbo].[AcumaticaPayment]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyAcuPayment_usrShopifyTransaction] FOREIGN KEY([ShopifyTransactionMonsterId])
REFERENCES [dbo].[ShopifyTransaction] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrShopifyAcuPayment_usrShopifyTransaction]') AND parent_object_id = OBJECT_ID(N'[dbo].[AcumaticaPayment]'))
ALTER TABLE [dbo].[AcumaticaPayment] CHECK CONSTRAINT [FK_usrShopifyAcuPayment_usrShopifyTransaction]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_AcumaticaSalesOrder_AcumaticaCustomer]') AND parent_object_id = OBJECT_ID(N'[dbo].[AcumaticaSalesOrder]'))
ALTER TABLE [dbo].[AcumaticaSalesOrder]  WITH CHECK ADD  CONSTRAINT [FK_AcumaticaSalesOrder_AcumaticaCustomer] FOREIGN KEY([ShopifyCustomerMonsterId])
REFERENCES [dbo].[AcumaticaCustomer] ([ShopifyCustomerMonsterId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_AcumaticaSalesOrder_AcumaticaCustomer]') AND parent_object_id = OBJECT_ID(N'[dbo].[AcumaticaSalesOrder]'))
ALTER TABLE [dbo].[AcumaticaSalesOrder] CHECK CONSTRAINT [FK_AcumaticaSalesOrder_AcumaticaCustomer]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_AcumaticaSalesOrder_ShopifyOrder]') AND parent_object_id = OBJECT_ID(N'[dbo].[AcumaticaSalesOrder]'))
ALTER TABLE [dbo].[AcumaticaSalesOrder]  WITH CHECK ADD  CONSTRAINT [FK_AcumaticaSalesOrder_ShopifyOrder] FOREIGN KEY([ShopifyOrderMonsterId])
REFERENCES [dbo].[ShopifyOrder] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_AcumaticaSalesOrder_ShopifyOrder]') AND parent_object_id = OBJECT_ID(N'[dbo].[AcumaticaSalesOrder]'))
ALTER TABLE [dbo].[AcumaticaSalesOrder] CHECK CONSTRAINT [FK_AcumaticaSalesOrder_ShopifyOrder]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_AcumaticaSoShipment_AcumaticaSalesOrder]') AND parent_object_id = OBJECT_ID(N'[dbo].[AcumaticaSoShipment]'))
ALTER TABLE [dbo].[AcumaticaSoShipment]  WITH CHECK ADD  CONSTRAINT [FK_AcumaticaSoShipment_AcumaticaSalesOrder] FOREIGN KEY([ShopifyOrderMonsterId])
REFERENCES [dbo].[AcumaticaSalesOrder] ([ShopifyOrderMonsterId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_AcumaticaSoShipment_AcumaticaSalesOrder]') AND parent_object_id = OBJECT_ID(N'[dbo].[AcumaticaSoShipment]'))
ALTER TABLE [dbo].[AcumaticaSoShipment] CHECK CONSTRAINT [FK_AcumaticaSoShipment_AcumaticaSalesOrder]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_AcumaticaSoShipment_ShopifyFulfillment]') AND parent_object_id = OBJECT_ID(N'[dbo].[AcumaticaSoShipment]'))
ALTER TABLE [dbo].[AcumaticaSoShipment]  WITH CHECK ADD  CONSTRAINT [FK_AcumaticaSoShipment_ShopifyFulfillment] FOREIGN KEY([ShopifyFulfillmentMonsterId])
REFERENCES [dbo].[ShopifyFulfillment] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_AcumaticaSoShipment_ShopifyFulfillment]') AND parent_object_id = OBJECT_ID(N'[dbo].[AcumaticaSoShipment]'))
ALTER TABLE [dbo].[AcumaticaSoShipment] CHECK CONSTRAINT [FK_AcumaticaSoShipment_ShopifyFulfillment]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_AcumaticaStockItem_ShopifyVariant]') AND parent_object_id = OBJECT_ID(N'[dbo].[AcumaticaStockItem]'))
ALTER TABLE [dbo].[AcumaticaStockItem]  WITH CHECK ADD  CONSTRAINT [FK_AcumaticaStockItem_ShopifyVariant] FOREIGN KEY([ShopifyVariantMonsterId])
REFERENCES [dbo].[ShopifyVariant] ([MonsterId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_AcumaticaStockItem_ShopifyVariant]') AND parent_object_id = OBJECT_ID(N'[dbo].[AcumaticaStockItem]'))
ALTER TABLE [dbo].[AcumaticaStockItem] CHECK CONSTRAINT [FK_AcumaticaStockItem_ShopifyVariant]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrInventoryReceiptSync_usrAcumaticaInventoryReceipt]') AND parent_object_id = OBJECT_ID(N'[dbo].[InventoryReceiptSync]'))
ALTER TABLE [dbo].[InventoryReceiptSync]  WITH CHECK ADD  CONSTRAINT [FK_usrInventoryReceiptSync_usrAcumaticaInventoryReceipt] FOREIGN KEY([AcumaticaInvReceiptMonsterId])
REFERENCES [dbo].[AcumaticaInventoryReceipt] ([MonsterId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrInventoryReceiptSync_usrAcumaticaInventoryReceipt]') AND parent_object_id = OBJECT_ID(N'[dbo].[InventoryReceiptSync]'))
ALTER TABLE [dbo].[InventoryReceiptSync] CHECK CONSTRAINT [FK_usrInventoryReceiptSync_usrAcumaticaInventoryReceipt]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrInventoryReceiptSync_usrShopifyInventoryLevels]') AND parent_object_id = OBJECT_ID(N'[dbo].[InventoryReceiptSync]'))
ALTER TABLE [dbo].[InventoryReceiptSync]  WITH CHECK ADD  CONSTRAINT [FK_usrInventoryReceiptSync_usrShopifyInventoryLevels] FOREIGN KEY([ShopifyInventoryMonsterId])
REFERENCES [dbo].[ShopifyInventoryLevel] ([MonsterId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrInventoryReceiptSync_usrShopifyInventoryLevels]') AND parent_object_id = OBJECT_ID(N'[dbo].[InventoryReceiptSync]'))
ALTER TABLE [dbo].[InventoryReceiptSync] CHECK CONSTRAINT [FK_usrInventoryReceiptSync_usrShopifyInventoryLevels]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrShopAcuWarehouseSync_usrAcumaticaWarehouse]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShopAcuWarehouseSync]'))
ALTER TABLE [dbo].[ShopAcuWarehouseSync]  WITH CHECK ADD  CONSTRAINT [FK_usrShopAcuWarehouseSync_usrAcumaticaWarehouse] FOREIGN KEY([AcumaticaWarehouseMonsterId])
REFERENCES [dbo].[AcumaticaWarehouse] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrShopAcuWarehouseSync_usrAcumaticaWarehouse]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShopAcuWarehouseSync]'))
ALTER TABLE [dbo].[ShopAcuWarehouseSync] CHECK CONSTRAINT [FK_usrShopAcuWarehouseSync_usrAcumaticaWarehouse]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrShopAcuWarehouseSync_usrShopifyLocation]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShopAcuWarehouseSync]'))
ALTER TABLE [dbo].[ShopAcuWarehouseSync]  WITH CHECK ADD  CONSTRAINT [FK_usrShopAcuWarehouseSync_usrShopifyLocation] FOREIGN KEY([ShopifyLocationMonsterId])
REFERENCES [dbo].[ShopifyLocation] ([MonsterId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrShopAcuWarehouseSync_usrShopifyLocation]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShopAcuWarehouseSync]'))
ALTER TABLE [dbo].[ShopAcuWarehouseSync] CHECK CONSTRAINT [FK_usrShopAcuWarehouseSync_usrShopifyLocation]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrShopifyFulfillment_usrShopifyOrder]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShopifyFulfillment]'))
ALTER TABLE [dbo].[ShopifyFulfillment]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyFulfillment_usrShopifyOrder] FOREIGN KEY([OrderMonsterId])
REFERENCES [dbo].[ShopifyOrder] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrShopifyFulfillment_usrShopifyOrder]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShopifyFulfillment]'))
ALTER TABLE [dbo].[ShopifyFulfillment] CHECK CONSTRAINT [FK_usrShopifyFulfillment_usrShopifyOrder]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrShopifyInventoryLevels_usrShopifyLocation]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShopifyInventoryLevel]'))
ALTER TABLE [dbo].[ShopifyInventoryLevel]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyInventoryLevels_usrShopifyLocation] FOREIGN KEY([LocationMonsterId])
REFERENCES [dbo].[ShopifyLocation] ([MonsterId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrShopifyInventoryLevels_usrShopifyLocation]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShopifyInventoryLevel]'))
ALTER TABLE [dbo].[ShopifyInventoryLevel] CHECK CONSTRAINT [FK_usrShopifyInventoryLevels_usrShopifyLocation]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrShopifyInventoryLevels_usrShopifyVariant]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShopifyInventoryLevel]'))
ALTER TABLE [dbo].[ShopifyInventoryLevel]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyInventoryLevels_usrShopifyVariant] FOREIGN KEY([ParentMonsterId])
REFERENCES [dbo].[ShopifyVariant] ([MonsterId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrShopifyInventoryLevels_usrShopifyVariant]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShopifyInventoryLevel]'))
ALTER TABLE [dbo].[ShopifyInventoryLevel] CHECK CONSTRAINT [FK_usrShopifyInventoryLevels_usrShopifyVariant]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrShopifyOrder_usrShopifyCustomer]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShopifyOrder]'))
ALTER TABLE [dbo].[ShopifyOrder]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyOrder_usrShopifyCustomer] FOREIGN KEY([CustomerMonsterId])
REFERENCES [dbo].[ShopifyCustomer] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrShopifyOrder_usrShopifyCustomer]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShopifyOrder]'))
ALTER TABLE [dbo].[ShopifyOrder] CHECK CONSTRAINT [FK_usrShopifyOrder_usrShopifyCustomer]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrShopifyPayoutTransaction_usrShopifyPayout]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShopifyPayoutTransaction]'))
ALTER TABLE [dbo].[ShopifyPayoutTransaction]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyPayoutTransaction_usrShopifyPayout] FOREIGN KEY([MonsterParentId])
REFERENCES [dbo].[ShopifyPayout] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrShopifyPayoutTransaction_usrShopifyPayout]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShopifyPayoutTransaction]'))
ALTER TABLE [dbo].[ShopifyPayoutTransaction] CHECK CONSTRAINT [FK_usrShopifyPayoutTransaction_usrShopifyPayout]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrShopifyRefund_usrShopifyOrder]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShopifyRefund]'))
ALTER TABLE [dbo].[ShopifyRefund]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyRefund_usrShopifyOrder] FOREIGN KEY([OrderMonsterId])
REFERENCES [dbo].[ShopifyOrder] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrShopifyRefund_usrShopifyOrder]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShopifyRefund]'))
ALTER TABLE [dbo].[ShopifyRefund] CHECK CONSTRAINT [FK_usrShopifyRefund_usrShopifyOrder]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrShopifyTransaction_usrShopifyOrder]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShopifyTransaction]'))
ALTER TABLE [dbo].[ShopifyTransaction]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyTransaction_usrShopifyOrder] FOREIGN KEY([OrderMonsterId])
REFERENCES [dbo].[ShopifyOrder] ([Id])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrShopifyTransaction_usrShopifyOrder]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShopifyTransaction]'))
ALTER TABLE [dbo].[ShopifyTransaction] CHECK CONSTRAINT [FK_usrShopifyTransaction_usrShopifyOrder]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrShopifyVariant_usrShopifyProduct]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShopifyVariant]'))
ALTER TABLE [dbo].[ShopifyVariant]  WITH CHECK ADD  CONSTRAINT [FK_usrShopifyVariant_usrShopifyProduct] FOREIGN KEY([ParentMonsterId])
REFERENCES [dbo].[ShopifyProduct] ([MonsterId])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_usrShopifyVariant_usrShopifyProduct]') AND parent_object_id = OBJECT_ID(N'[dbo].[ShopifyVariant]'))
ALTER TABLE [dbo].[ShopifyVariant] CHECK CONSTRAINT [FK_usrShopifyVariant_usrShopifyProduct]
GO
/****** Object:  StoredProcedure [dbo].[DeleteAllAcumaticaInventoryRecords]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteAllAcumaticaInventoryRecords]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[DeleteAllAcumaticaInventoryRecords] AS' 
END
GO

ALTER PROCEDURE [dbo].[DeleteAllAcumaticaInventoryRecords]
AS
	DELETE FROM AcumaticaInventoryReceipt;
	DELETE FROM AcumaticaInventory;
	DELETE FROM AcumaticaStockItem;
	DELETE FROM AcumaticaWarehouse;
GO
/****** Object:  StoredProcedure [dbo].[DeleteAllAcumaticaOrderRecords]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteAllAcumaticaOrderRecords]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[DeleteAllAcumaticaOrderRecords] AS' 
END
GO

ALTER PROCEDURE [dbo].[DeleteAllAcumaticaOrderRecords]
AS
	DELETE FROM AcumaticaPayment;
	DELETE FROM AcumaticaSoShipment;
	DELETE FROM AcumaticaSalesOrder;
	DELETE FROM AcumaticaCustomer;
GO
/****** Object:  StoredProcedure [dbo].[DeleteAllShopifyInventoryRecords]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteAllShopifyInventoryRecords]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[DeleteAllShopifyInventoryRecords] AS' 
END
GO

ALTER PROCEDURE [dbo].[DeleteAllShopifyInventoryRecords]
AS
	DELETE FROM ShopifyInventoryLevel;
	DELETE FROM ShopifyVariant;
	DELETE FROM ShopifyProduct;
	DELETE FROM ShopifyLocation;
GO
/****** Object:  StoredProcedure [dbo].[DeleteAllShopifyOrderRecords]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteAllShopifyOrderRecords]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[DeleteAllShopifyOrderRecords] AS' 
END
GO

ALTER PROCEDURE [dbo].[DeleteAllShopifyOrderRecords]
AS
	DELETE FROM ShopifyPayout;
	DELETE FROM ShopifyPayoutTransaction;

	DELETE FROM ShopifyTransaction;
	DELETE FROM ShopifyRefund;
	DELETE FROM ShopifyFulfillment;
	DELETE FROM ShopifyOrder;
	DELETE FROM ShopifyCustomer;
GO
/****** Object:  StoredProcedure [dbo].[DeleteAllSyncRecords]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteAllSyncRecords]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[DeleteAllSyncRecords] AS' 
END
GO

ALTER PROCEDURE [dbo].[DeleteAllSyncRecords]
AS
	DELETE FROM InventoryReceiptSync;
	DELETE FROM ShopAcuWarehouseSync;
GO
/****** Object:  StoredProcedure [dbo].[DeleteAllSystemRecords]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeleteAllSystemRecords]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[DeleteAllSystemRecords] AS' 
END
GO

ALTER PROCEDURE [dbo].[DeleteAllSystemRecords]
AS
	DELETE FROM AcumaticaBatchState;
	DELETE FROM ShopifyBatchState;

	DELETE FROM AcumaticaRefData
	DELETE FROM MonsterSettings;
	DELETE FROM PaymentGateways;

	DELETE FROM SystemState;

	DELETE FROM ExclusiveJobMonitor;
	DELETE FROM ExecutionLog;
GO
/****** Object:  StoredProcedure [dbo].[ResetStartingShopifyOrder]    Script Date: 12/2/2019 3:45:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ResetStartingShopifyOrder]') AND type in (N'P', N'PC'))
BEGIN
EXEC dbo.sp_executesql @statement = N'CREATE PROCEDURE [dbo].[ResetStartingShopifyOrder] AS' 
END
GO

ALTER PROCEDURE [dbo].[ResetStartingShopifyOrder]
AS
	UPDATE MonsterSettings
		SET ShopifyOrderId = NULL,
		ShopifyOrderName = NULL,
		ShopifyOrderCreatedAtUtc = NULL;
		
	UPDATE SystemState SET StartingShopifyOrderState = 1;
		
	DELETE FROM AcumaticaBatchState;
	DELETE FROM ShopifyBatchState;
GO
USE [master]
GO
ALTER DATABASE [Monster0003] SET  READ_WRITE 
GO
