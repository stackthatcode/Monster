
USE [MonsterSys]
GO
/****** Object:  Table [dbo].[PaymentGateways]    Script Date: 12/20/2019 12:40:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PaymentGateways](
	[Id] [varchar](50) NOT NULL,
	[Name] [varchar](50) NULL,
 CONSTRAINT [PK_PaymentGateways] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO




DELETE FROM PaymentGateways;

INSERT INTO PaymentGateways VALUES ( 'shopify_payments', 'Shopify Payments' );
INSERT INTO PaymentGateways VALUES ( 'paypal', 'PayPal' );
INSERT INTO PaymentGateways VALUES ( 'amazon_payments', 'Amazon' );
INSERT INTO PaymentGateways VALUES ( 'bogus', 'Bogus Gateway (TEST)' );


